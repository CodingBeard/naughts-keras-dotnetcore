using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Keras;
using Keras.Layers;
using Keras.Models;
using Numpy;
using Trainer;

namespace SelfTrainer
{
    class Program
    {
        static int ArgMax(NDarray array)
        {
            var random = new Random();
            var max = 0f;
            var key = 0;
            var maxKeys = new List<int>();
            foreach (var output in array.GetData<float>())
            {
                if (output > max)
                {
                    max = output;
                }
            }
            foreach (var output in array.GetData<float>())
            {
                if (output == max)
                {
                    maxKeys.Add(key);
                }

                key++;
            }

            if (maxKeys.Count > 0)
            {
                return maxKeys[random.Next(maxKeys.Count)];
            }

            return 0;
        }

        static bool IsValidMove(int[] moves, int move)
        {
            bool valid = false;
            foreach (var validMove in moves)
            {
                if (move == validMove)
                {
                    valid = true;
                    break;
                }
            }

            return valid;
        }

        static int[] GetStats(Sequential model, int games)
        {
            var wins = 0;
            var losses = 0;
            var draws = 0;
            var invalid = 0;
            var aiRole = 1;

            for (int i = 0; i < games; i++)
            {
                var random = new Random();
                var floatBoard = new float[] {0, 0, 0, 0, 0, 0, 0, 0, 0,};
                var turn = 1;
                while (true)
                {
                    var validMoves = Board.GetMoves(floatBoard);
                    int move;
                    
                    if (turn != aiRole)
                    {
                        // move = ArgMax(Board.GetMoveRewards(floatBoard));
                        move = validMoves[random.Next(validMoves.Length)];
                    }
                    else
                    {
                        var predicitons = model.Predict(
                            np.array(new List<NDarray>() {np.array(floatBoard)}.ToArray()),
                            verbose: 0
                        );
                        move = ArgMax(predicitons);
                    }

                    if (!IsValidMove(validMoves, move))
                    {
                        invalid++;
                        break;
                    }

                    floatBoard[move] = turn;

                    var moves = Board.GetMoves(floatBoard);
                    var state = Board.GetBoardState(floatBoard);
                    if (state == 0 && moves.Length == 0)
                    {
                        draws++;
                        break;
                    }
                    else if (state == 1)
                    {
                        if (state == aiRole)
                            wins++;
                        else
                            losses++;
                        break;
                    }
                    else if (state == 2)
                    {
                        if (state == aiRole)
                            wins++;
                        else
                            losses++;
                        break;
                    }

                    if (turn == 1)
                        turn = 2;
                    else
                        turn = 1;
                }

                if (aiRole == 1)
                    aiRole = 2;
                else
                    aiRole = 1;
            }

            Console.WriteLine(
                "Wins: {0}, Losses: {1}, Draws: {2}, Invalids: {3}",
                wins,
                losses,
                draws,
                invalid
            );

            return new[] {wins, losses, draws, invalid};
        }

        static void Reward(List<float[]> boardRewards, float min, float max)
        {
            float moveCount = 1;
            for (int j = 0; j < boardRewards.Count; j++)
            {
                for (int k = 0; k < 9; k++)
                {
                    if (boardRewards[j][k] == -1)
                    {
                        moveCount++;
                    }
                }
            }
            for (int j = 0; j < boardRewards.Count; j++)
            {
                for (int k = 0; k < 9; k++)
                {
                    if (boardRewards[j][k] == -1)
                    {
                        boardRewards[j][k] = max;
                        continue;
                        boardRewards[j][k] = min + ((max - min) * (4/moveCount));
                        continue;
                        var reward = (max - (max / moveCount));
                        if (j == boardRewards.Count - 1)
                            reward = max;
                            
                        boardRewards[j][k] = Math.Max(
                            min, 
                            reward
                        );
                        moveCount++;
                    }
                }
            }
        }

        static void RewardReverse(List<float[]> boardRewards, float min, float max)
        {
            float moveCount = 1;
            for (int j = boardRewards.Count - 1; j >= 0; j--)
            {
                for (int k = 0; k < 9; k++)
                {
                    if (boardRewards[j][k] == -1)
                    {
                        boardRewards[j][k] = min;
                        continue;
                        var reward = (max - (max / moveCount));
                        if (j == 0)
                            reward = min;
                            
                        boardRewards[j][k] = Math.Max(
                            min, 
                            reward
                        );
                        moveCount++;
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            var model = new Sequential();
            model.Add(new Input(new Shape(9)));
            model.Add(new Dense(200, activation: "relu"));
            model.Add(new Dense(400, activation: "relu"));
            model.Add(new Dense(400, activation: "relu"));
            model.Add(new Dense(200, activation: "relu"));
            model.Add(new Dense(200, activation: "relu"));
            model.Add(new Dense(9, activation: "relu"));
            model.Compile("adam", "mean_squared_error", new string[] {"accuracy"});

            var gammaStart = 1000;
            float iterations = 100;
            var gamesPerSession = 100;
            var aiRole = 1;
            int gamma;
            var validMoveReward = 0.4f;
            var drawReward = 1f;
            var winReward = 1f;
            var loseReward = 0.4f;
            var batchSize = 25;
            var epochs = 5;

            for (float iteration = 0; iteration < iterations; iteration++)
            {
                gamma = (int) (gammaStart / Math.Exp(iteration / (iterations / 2)));
                if (iteration == iterations - 1)
                {
                    gamma = 0;
                }

                var moveBoards = new List<float[]>();
                var boardRewards = new List<float[]>();

                for (int i = 0; i < gamesPerSession; i++)
                {
                    var random = new Random();
                    var floatBoard = new float[] {0, 0, 0, 0, 0, 0, 0, 0, 0,};
                    var turn = 1;
                    var turns = 0;
                    while (true)
                    {
                        if (turn == aiRole)
                        {
                            var moveBoard = new float[9];
                            floatBoard.CopyTo(moveBoard, 0);
                            moveBoards.Add(moveBoard);
                        }

                        var validMoves = Board.GetMoves(floatBoard);
                        int move;

                        if (turn != aiRole)
                        {
                            if (random.Next(gammaStart) < gamma)
                            {
                                move = validMoves[random.Next(validMoves.Length)];
                            }
                            else
                            {
                                move = ArgMax(Board.GetMoveRewards(floatBoard));
                            }
                        }
                        else
                        {
                            if (random.Next(gammaStart) < gamma)
                            {
                                // move = validMoves[random.Next(validMoves.Length)];
                                move = ArgMax(Board.GetMoveRewards(floatBoard));
                            }
                            else
                            {
                                var predicitons = model.Predict(
                                    np.array(new List<NDarray>() {np.array(floatBoard)}.ToArray()),
                                    verbose: 0
                                );
                                move = ArgMax(predicitons);
                            }
                        }

                        if (!IsValidMove(validMoves, move))
                        {
                            Console.Write("\r{0}: {1}     ", iteration, i);
                            for (int j = 0; j < boardRewards.Count; j++)
                            {
                                for (int k = 0; k < 9; k++)
                                {
                                    if (boardRewards[j][k] == -1)
                                    {
                                        boardRewards[j][k] = validMoveReward;
                                    }
                                }
                            }

                            var rewards = new float[] {0, 0, 0, 0, 0, 0, 0, 0, 0,};
                            foreach (var validMove in validMoves)
                            {
                                rewards[validMove] = validMoveReward;
                            }

                            boardRewards.Add(rewards);
                            break;
                        }

                        floatBoard[move] = turn;

                        var moves = Board.GetMoves(floatBoard);
                        var state = Board.GetBoardState(floatBoard);
                        if (state == 0 && moves.Length == 0)
                        {
                            Console.Write("\r{0}: {1}     ", iteration, i);
                            if (turn == aiRole)
                            {
                                var rewards = new float[] {0, 0, 0, 0, 0, 0, 0, 0, 0,};
                                foreach (var validMove in validMoves)
                                {
                                    rewards[validMove] = validMoveReward;
                                }

                                rewards[move] = -1;

                                boardRewards.Add(rewards);
                            }

                            Reward(boardRewards, validMoveReward, drawReward);

                            break;
                        }
                        else if (state == 1)
                        {
                            Console.Write("\r{0}: {1}     ", iteration, i);
                            if (turn == aiRole)
                            {
                                var rewards = new float[] {0, 0, 0, 0, 0, 0, 0, 0, 0,};
                                foreach (var validMove in validMoves)
                                {
                                    rewards[validMove] = validMoveReward;
                                }

                                rewards[move] = -1;

                                boardRewards.Add(rewards);
                            }
                            
                            if (state == aiRole)
                                Reward(boardRewards, validMoveReward, winReward);
                            else
                                RewardReverse(boardRewards, loseReward, validMoveReward);


                            break;
                        }
                        else if (state == 2)
                        {
                            if (turn == aiRole)
                            {
                                var rewards = new float[] {0, 0, 0, 0, 0, 0, 0, 0, 0,};
                                foreach (var validMove in validMoves)
                                {
                                    rewards[validMove] = validMoveReward;
                                }

                                rewards[move] = -1;

                                boardRewards.Add(rewards);
                            }

                            Console.Write("\r{0}: {1}     ", iteration, i);
                            
                            if (state == aiRole)
                                Reward(boardRewards, validMoveReward, winReward);
                            else
                                RewardReverse(boardRewards, loseReward, validMoveReward);

                            break;
                        }
                        else
                        {

                            if (turn == aiRole)
                            {
                                var rewards = new float[] {0, 0, 0, 0, 0, 0, 0, 0, 0,};
                                foreach (var validMove in validMoves)
                                {
                                    rewards[validMove] = validMoveReward;
                                }
                                
                                rewards[move] = -1;
                                
                                boardRewards.Add(rewards);
                            }
                            
                        }

                        if (turn == 1)
                            turn = 2;
                        else
                            turn = 1;
                        
                        turns++;
                        if (turns > 10)
                        {
                            break;
                        }
                    }

                    if (aiRole == 1)
                        aiRole = 2;
                    else
                        aiRole = 1;
                }

                var moveBoardsNdArray = new List<NDarray>();
                foreach (var moveBoard in moveBoards)
                {
                    moveBoardsNdArray.Add(np.array(moveBoard));
                }

                var boardRewardsNdArray = new List<NDarray>();
                foreach (var boardReward in boardRewards)
                {
                    boardRewardsNdArray.Add(np.array(boardReward));
                }


                model.Fit(
                    np.array(moveBoardsNdArray.ToArray()),
                    np.array(boardRewardsNdArray.ToArray()),
                    batch_size: batchSize,
                    verbose: 0,
                    epochs: epochs
                );

                if (iteration % 20 == 0)
                    GetStats(model, 30);
            }

            GetStats(model, 300);
            
            string json = model.ToJson();
            File.WriteAllText(@"C:\code\github\codingbeard\naughts.net\Naughts.Net\Trainer\model.json", json);
            model.SaveWeight(@"C:\code\github\codingbeard\naughts.net\Naughts.Net\Trainer\model.h5");
        }
    }
}