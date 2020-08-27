using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Keras;
using Keras.Layers;
using Keras.Models;
using Numpy;

namespace Trainer
{
    public class Board
    {
        public static void PrintBoard(float[] board)
        {
            var output = "";
            var row = "";
            for (int i = 0; i < 9; i++)
            {
                if (i == 0)
                {
                    row = "";
                }
                else if (i % 3 == 0)
                {
                    output = output + row + "\n";
                    row = "";
                }

                if (board[i] == 0)
                {
                    row = row + " -";
                }
                else if (board[i] == 1)
                {
                    row = row + " o";
                }
                else if (board[i] == 2)
                {
                    row = row + " x";
                }
            }

            output = output + row + "\n";
            Console.Out.WriteLine(output);
        }

        public static int GetBoardState(float[] board)
        {
            //horizontal
            if (board[0] == 1 && board[1] == 1 && board[2] == 1)
                return 1;
            if (board[3] == 1 && board[4] == 1 && board[5] == 1)
                return 1;
            if (board[6] == 1 && board[7] == 1 && board[8] == 1)
                return 1;
            if (board[0] == 2 && board[1] == 2 && board[2] == 2)
                return 2;
            if (board[3] == 2 && board[4] == 2 && board[5] == 2)
                return 2;
            if (board[6] == 2 && board[7] == 2 && board[8] == 2)
                return 2;
            //vertical
            if (board[0] == 1 && board[3] == 1 && board[6] == 1)
                return 1;
            if (board[1] == 1 && board[4] == 1 && board[7] == 1)
                return 1;
            if (board[2] == 1 && board[5] == 1 && board[8] == 1)
                return 1;
            if (board[0] == 2 && board[3] == 2 && board[6] == 2)
                return 2;
            if (board[1] == 2 && board[4] == 2 && board[7] == 2)
                return 2;
            if (board[2] == 2 && board[5] == 2 && board[8] == 2)
                return 2;
            //diagonal
            if (board[0] == 1 && board[4] == 1 && board[8] == 1)
                return 1;
            if (board[2] == 1 && board[4] == 1 && board[6] == 1)
                return 1;
            if (board[0] == 2 && board[4] == 2 && board[8] == 2)
                return 2;
            if (board[2] == 2 && board[4] == 2 && board[6] == 2)
                return 2;
            return 0;
        }

        public static int[] GetMoves(float[] board)
        {
            var moves = new List<int>();

            for (int i = 0; i < 9; i++)
            {
                if (board[i] == 0)
                {
                    moves.Add(i);
                }
            }

            return moves.ToArray();
        }

        public static float[] GetMoveRewards(float[] board)
        {
            var moves = new float[] {0, 0, 0, 0, 0, 0, 0, 0, 0};
            var naughts = 0;
            var crosses = 0;
            var turn = 1f;
            var oppositeTurn = 2f;
            
            var validMoveReward = 0.2f;
            var invalidMoveReward = 0f;
            var firstMoveReward = 0.8f;
            var secondMoveReward = 0.8f;
            var winningMoveReward = 0.7f;
            var avoidingLossMoveReward = 0.8f;
            var twoInARowReward = 0.1f;
            
            for (int i = 0; i < 9; i++)
            {
                if (board[i] == 0)
                {
                    moves[i] = validMoveReward;
                }
                else
                {
                    moves[i] = invalidMoveReward;
                }

                if (board[i] == 1)
                {
                    naughts++;
                }

                if (board[i] == 2)
                {
                    crosses++;
                }
            }

            if (naughts == 0 && crosses == 0)
            {
                moves[4] = firstMoveReward;
            }
            
            if ((naughts == 0 && crosses == 1) || (naughts == 1 && crosses == 0))
            {
                if (board[4] == 0)
                {
                    moves[4] = secondMoveReward;
                }
                else
                {
                    moves[0] = secondMoveReward;
                    moves[2] = secondMoveReward;
                    moves[6] = secondMoveReward;
                    moves[8] = secondMoveReward;
                }
            }

            if (naughts > crosses)
            {
                turn = 2f;
                oppositeTurn = 1f;
            }

            //horizontal 3rd free on turn
            if (board[0] == turn && board[1] == turn &&
                board[2] == 0)
                moves[2] += winningMoveReward;
            if (board[3] == turn && board[4] == turn &&
                board[5] == 0)
                moves[5] += winningMoveReward;
            if (board[6] == turn && board[7] == turn &&
                board[8] == 0)
                moves[8] += winningMoveReward;
            //horizontal 2nd free on turn
            if (board[0] == turn && board[1] == 0 &&
                board[2] == turn)
                moves[1] += winningMoveReward;
            if (board[3] == turn && board[4] == 0 &&
                board[5] == turn)
                moves[4] += winningMoveReward;
            if (board[6] == turn && board[7] == 0 &&
                board[8] == turn)
                moves[7] += winningMoveReward;
            //horizontal 1st free on turn
            if (board[0] == 0 && board[1] == turn &&
                board[2] == turn)
                moves[0] += winningMoveReward;
            if (board[3] == 0 && board[4] == turn &&
                board[5] == turn)
                moves[3] += winningMoveReward;
            if (board[6] == 0 && board[7] == turn &&
                board[8] == turn)
                moves[6] += winningMoveReward;
            //horizontal 3rd free off turn
            if (board[0] == oppositeTurn && board[1] == oppositeTurn &&
                board[2] == 0)
                moves[2] += avoidingLossMoveReward;
            if (board[3] == oppositeTurn && board[4] == oppositeTurn &&
                board[5] == 0)
                moves[5] += avoidingLossMoveReward;
            if (board[6] == oppositeTurn && board[7] == oppositeTurn &&
                board[8] == 0)
                moves[8] += avoidingLossMoveReward;
            //horizontal 2nd free off oppositeTurn
            if (board[0] == oppositeTurn && board[1] == 0 &&
                board[2] == oppositeTurn)
                moves[1] += avoidingLossMoveReward;
            if (board[3] == oppositeTurn && board[4] == 0 &&
                board[5] == oppositeTurn)
                moves[4] += avoidingLossMoveReward;
            if (board[6] == oppositeTurn && board[7] == 0 &&
                board[8] == oppositeTurn)
                moves[7] += avoidingLossMoveReward;
            //horizontal 1st free off oppositeTurn
            if (board[0] == 0 && board[1] == oppositeTurn &&
                board[2] == oppositeTurn)
                moves[0] += avoidingLossMoveReward;
            if (board[3] == 0 && board[4] == oppositeTurn &&
                board[5] == oppositeTurn)
                moves[3] += avoidingLossMoveReward;
            if (board[6] == 0 && board[7] == oppositeTurn &&
                board[8] == oppositeTurn)
                moves[6] += avoidingLossMoveReward;
            //vertical 3rd free on turn
            if (board[0] == turn && board[3] == turn &&
                board[6] == 0)
                moves[6] += winningMoveReward;
            if (board[1] == turn && board[4] == turn &&
                board[7] == 0)
                moves[7] += winningMoveReward;
            if (board[2] == turn && board[5] == turn &&
                board[8] == 0)
                moves[8] += winningMoveReward;
            //vertical 2nd free on turn
            if (board[0] == turn && board[3] == 0 &&
                board[6] == turn)
                moves[3] += winningMoveReward;
            if (board[1] == turn && board[4] == 0 &&
                board[7] == turn)
                moves[4] += winningMoveReward;
            if (board[2] == turn && board[5] == 0 &&
                board[8] == turn)
                moves[5] += winningMoveReward;
            //vertical 1st free on turn
            if (board[0] == 0 && board[3] == turn &&
                board[6] == turn)
                moves[0] += winningMoveReward;
            if (board[1] == 0 && board[4] == turn &&
                board[7] == turn)
                moves[1] += winningMoveReward;
            if (board[2] == 0 && board[5] == turn &&
                board[8] == turn)
                moves[2] += winningMoveReward;
            //vertical 3rd free off turn
            if (board[0] == oppositeTurn && board[3] == oppositeTurn &&
                board[6] == 0)
                moves[6] += avoidingLossMoveReward;
            if (board[1] == oppositeTurn && board[4] == oppositeTurn &&
                board[7] == 0)
                moves[7] += avoidingLossMoveReward;
            if (board[2] == oppositeTurn && board[5] == oppositeTurn &&
                board[8] == 0)
                moves[8] += avoidingLossMoveReward;
            //vertical 2nd free off oppositeTurn
            if (board[0] == oppositeTurn && board[3] == 0 &&
                board[6] == oppositeTurn)
                moves[3] += avoidingLossMoveReward;
            if (board[1] == oppositeTurn && board[4] == 0 &&
                board[7] == oppositeTurn)
                moves[4] += avoidingLossMoveReward;
            if (board[2] == oppositeTurn && board[5] == 0 &&
                board[8] == oppositeTurn)
                moves[5] += avoidingLossMoveReward;
            //vertical 1st free off oppositeTurn
            if (board[0] == 0 && board[3] == oppositeTurn &&
                board[6] == oppositeTurn)
                moves[0] += avoidingLossMoveReward;
            if (board[1] == 0 && board[4] == oppositeTurn &&
                board[7] == oppositeTurn)
                moves[1] += avoidingLossMoveReward;
            if (board[2] == 0 && board[5] == oppositeTurn &&
                board[8] == oppositeTurn)
                moves[2] += avoidingLossMoveReward;

            //diagonal 3rd free on turn
            if (board[0] == turn && board[4] == turn &&
                board[8] == 0)
                moves[8] += winningMoveReward;
            if (board[2] == turn && board[4] == turn &&
                board[6] == 0)
                moves[6] += winningMoveReward;
            //diagonal 2nd free on turn
            if (board[0] == turn && board[4] == 0 &&
                board[8] == turn)
                moves[4] += winningMoveReward;
            if (board[2] == turn && board[4] == 0 &&
                board[6] == turn)
                moves[4] += winningMoveReward;
            //diagonal 1st free on turn
            if (board[0] == 0 && board[4] == turn &&
                board[8] == turn)
                moves[0] += winningMoveReward;
            if (board[2] == 0 && board[4] == turn &&
                board[6] == turn)
                moves[2] += winningMoveReward;

            //diagonal 3rd free off turn
            if (board[0] == oppositeTurn && board[4] == oppositeTurn &&
                board[8] == 0)
                moves[8] += avoidingLossMoveReward;
            if (board[2] == oppositeTurn && board[4] == oppositeTurn &&
                board[6] == 0)
                moves[6] += avoidingLossMoveReward;
            //diagonal 2nd free off turn
            if (board[0] == oppositeTurn && board[4] == 0 &&
                board[8] == oppositeTurn)
                moves[4] += avoidingLossMoveReward;
            if (board[2] == oppositeTurn && board[4] == 0 &&
                board[6] == oppositeTurn)
                moves[4] += avoidingLossMoveReward;
            //diagonal 1st free off turn
            if (board[0] == 0 && board[4] == oppositeTurn &&
                board[8] == oppositeTurn)
                moves[0] += avoidingLossMoveReward;
            if (board[2] == 0 && board[4] == oppositeTurn &&
                board[6] == oppositeTurn)
                moves[2] += avoidingLossMoveReward;

            //horizontal 2nd, 3rd free on turn
            if (board[0] == turn && board[1] == 0 && board[2] == 0)
            {
                moves[1] += twoInARowReward;
                moves[2] += twoInARowReward;
            }

            if (board[3] == turn && board[4] == 0 && board[5] == 0)
            {
                moves[4] += twoInARowReward;
                moves[5] += twoInARowReward;
            }

            if (board[6] == turn && board[7] == 0 && board[8] == 0)
            {
                moves[7] += twoInARowReward;
                moves[8] += twoInARowReward;
            }

            //horizontal 1st, 2nd free on turn
            if (board[0] == 0 && board[1] == 0 && board[2] == turn)
            {
                moves[0] += twoInARowReward;
                moves[1] += twoInARowReward;
            }

            if (board[3] == 0 && board[4] == 0 && board[5] == turn)
            {
                moves[3] += twoInARowReward;
                moves[4] += twoInARowReward;
            }

            if (board[6] == 0 && board[7] == 0 && board[8] == turn)
            {
                moves[6] += twoInARowReward;
                moves[7] += twoInARowReward;
            }

            //horizontal 1st, 3rd free on turn
            if (board[0] == 0 && board[1] == turn && board[2] == 0)
            {
                moves[0] += twoInARowReward;
                moves[2] += twoInARowReward;
            }

            if (board[3] == 0 && board[4] == turn && board[5] == 0)
            {
                moves[3] += twoInARowReward;
                moves[5] += twoInARowReward;
            }

            if (board[6] == 0 && board[7] == turn && board[8] == 0)
            {
                moves[6] += twoInARowReward;
                moves[8] += twoInARowReward;
            }

            //vertical 2nd, 3rd free on turn
            if (board[0] == turn && board[3] == 0 && board[6] == 0)
            {
                moves[3] += twoInARowReward;
                moves[6] += twoInARowReward;
            }

            if (board[1] == turn && board[4] == 0 && board[7] == 0)
            {
                moves[4] += twoInARowReward;
                moves[7] += twoInARowReward;
            }

            if (board[2] == turn && board[5] == 0 && board[8] == 0)
            {
                moves[5] += twoInARowReward;
                moves[8] += twoInARowReward;
            }

            //vertical 1st, 2nd free on turn
            if (board[0] == 0 && board[3] == 0 && board[6] == turn)
            {
                moves[0] += twoInARowReward;
                moves[3] += twoInARowReward;
            }

            if (board[1] == 0 && board[4] == 0 && board[7] == turn)
            {
                moves[1] += twoInARowReward;
                moves[4] += twoInARowReward;
            }

            if (board[2] == 0 && board[5] == 0 && board[8] == turn)
            {
                moves[2] += twoInARowReward;
                moves[5] += twoInARowReward;
            }

            //vertical 1st, 3rd free on turn
            if (board[0] == 0 && board[3] == turn && board[6] == 0)
            {
                moves[0] += twoInARowReward;
                moves[6] += twoInARowReward;
            }

            if (board[1] == 0 && board[4] == turn && board[7] == 0)
            {
                moves[1] += twoInARowReward;
                moves[7] += twoInARowReward;
            }

            if (board[2] == 0 && board[5] == turn && board[8] == 0)
            {
                moves[2] += twoInARowReward;
                moves[8] += twoInARowReward;
            }

            //diagonal 2nd, 3rd free on turn
            if (board[0] == turn && board[4] == 0 && board[8] == 0)
            {
                moves[4] += twoInARowReward;
                moves[8] += twoInARowReward;
            }

            if (board[2] == turn && board[4] == 0 && board[6] == 0)
            {
                moves[4] += twoInARowReward;
                moves[6] += twoInARowReward;
            }

            //diagonal 1st, 2nd free on turn
            if (board[0] == 0 && board[4] == 0 && board[8] == turn)
            {
                moves[0] += twoInARowReward;
                moves[4] += twoInARowReward;
            }

            if (board[2] == 0 && board[4] == 0 && board[6] == turn)
            {
                moves[2] += twoInARowReward;
                moves[4] += twoInARowReward;
            }

            //diagonal 1st, 3rd free on turn
            if (board[0] == 0 && board[4] == turn && board[8] == 0)
            {
                moves[0] += twoInARowReward;
                moves[8] += twoInARowReward;
            }

            if (board[2] == 0 && board[4] == turn && board[6] == 0)
            {
                moves[2] += twoInARowReward;
                moves[6] += twoInARowReward;
            }

            var key = 0;
            foreach (var move in moves)
            {
                if (move > 1f)
                {
                    moves[key] = 1f;
                }
                key++;
            }

            return moves;
        }

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
        
        public static int[] GetStats(Sequential model, int games)
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
                losses + invalid,
                draws,
                invalid
            );

            return new[] {wins, losses, draws, invalid};
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var random = new Random();
            var moveBoards = new List<float[]>();
            Object moveBoardsLock = new Object();
            var boardRewards = new List<float[]>();
            Object boardRewardsLock = new Object();
            Parallel.For(0, 10000, i =>
            {
                var board = new float[] {0, 0, 0, 0, 0, 0, 0, 0, 0};
                var turn = 1;
                while (true)
                {
                    var moves = Board.GetMoves(board);
                    if (moves.Length > 0)
                    {
                        foreach (var j in moves)
                        {
                            var moveBoard = new float[9];
                            board.CopyTo(moveBoard, 0);
                            moveBoard[j] = turn;
                            lock (moveBoardsLock)
                            {
                                moveBoards.Add(moveBoard);
                            }

                            lock (boardRewardsLock)
                            {
                                boardRewards.Add(Board.GetMoveRewards(moveBoard));
                            }
                        }
                    }

                    if (moves.Length - 1 > 0)
                    {
                        var move = moves[random.Next(moves.Length - 1)];
                        board[move] = turn;
                        if (turn == 1)
                        {
                            turn = 2;
                        }
                        else
                        {
                            turn = 1;
                        }
                    }
                    else if (moves.Length == 1)
                    {
                        var move = moves[0];
                        board[move] = turn;
                        if (turn == 1)
                        {
                            turn = 2;
                        }
                        else
                        {
                            turn = 1;
                        }
                    }

                    var state = Board.GetBoardState(board);
                    if (state == 0 && moves.Length == 0)
                    {
                        Console.Write("\r{0}   ", i);
                        break;
                    }

                    if (state == 1)
                    {
                        Console.Write("\r{0}   ", i);
                        break;
                    }

                    if (state == 2)
                    {
                        Console.Write("\r{0}   ", i);
                        break;
                    }
                }
            });

            Console.WriteLine("Total boards: {0}", moveBoards.Count);

            var model = new Sequential();
            model.Add(new Input(new Shape(9)));
            model.Add(new Dense(100, activation: "relu"));
            model.Add(new Dense(100, activation: "relu"));
            model.Add(new Dense(100, activation: "relu"));
            model.Add(new Dense(9, activation: "relu"));
            model.Compile("adam", "mean_squared_error", new string[] {"accuracy"});

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
                batch_size: 50,
                verbose: 1,
                epochs: 10
            );

            Board.GetStats(model, 100);

            string json = model.ToJson();
            File.WriteAllText(@"C:\code\github\codingbeard\naughts.net\Naughts.Net\Trainer\model.json", json);
            model.SaveWeight(@"C:\code\github\codingbeard\naughts.net\Naughts.Net\Trainer\model.h5");
        }
    }
}