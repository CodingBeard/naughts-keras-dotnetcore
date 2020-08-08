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
            var turn = 1;
            var oppositeTurn = 2;
            for (int i = 0; i < 9; i++)
            {
                if (board[i] == 0)
                {
                    moves[i] = 0.5f;
                }
                else
                {
                    moves[i] = 0f;
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

            if (naughts > crosses)
            {
                turn = 2;
                oppositeTurn = 1;
            }

            //horizontal 3rd free on turn
            if (board[0] == turn && board[1] == turn &&
                board[2] == 0)
                moves[2] += 0.3f;
            if (board[3] == turn && board[4] == turn &&
                board[5] == 0)
                moves[5] += 0.3f;
            if (board[6] == turn && board[7] == turn &&
                board[8] == 0)
                moves[8] += 0.3f;
            //horizontal 2nd free on turn
            if (board[0] == turn && board[1] == 0 &&
                board[2] == turn)
                moves[1] += 0.3f;
            if (board[3] == turn && board[4] == 0 &&
                board[5] == turn)
                moves[4] += 0.3f;
            if (board[6] == turn && board[7] == 0 &&
                board[8] == turn)
                moves[7] += 0.3f;
            //horizontal 1st free on turn
            if (board[0] == 0 && board[1] == turn &&
                board[2] == turn)
                moves[0] += 0.3f;
            if (board[3] == 0 && board[4] == turn &&
                board[5] == turn)
                moves[3] += 0.3f;
            if (board[6] == 0 && board[7] == turn &&
                board[8] == turn)
                moves[6] += 0.3f;
            //horizontal 3rd free off turn
            if (board[0] == oppositeTurn && board[1] == oppositeTurn &&
                board[2] == 0)
                moves[2] += 0.3f;
            if (board[3] == oppositeTurn && board[4] == oppositeTurn &&
                board[5] == 0)
                moves[5] += 0.3f;
            if (board[6] == oppositeTurn && board[7] == oppositeTurn &&
                board[8] == 0)
                moves[8] += 0.3f;
            //horizontal 2nd free off oppositeTurn
            if (board[0] == oppositeTurn && board[1] == 0 &&
                board[2] == oppositeTurn)
                moves[1] += 0.3f;
            if (board[3] == oppositeTurn && board[4] == 0 &&
                board[5] == oppositeTurn)
                moves[4] += 0.3f;
            if (board[6] == oppositeTurn && board[7] == 0 &&
                board[8] == oppositeTurn)
                moves[7] += 0.3f;
            //horizontal 1st free off oppositeTurn
            if (board[0] == 0 && board[1] == oppositeTurn &&
                board[2] == oppositeTurn)
                moves[0] += 0.3f;
            if (board[3] == 0 && board[4] == oppositeTurn &&
                board[5] == oppositeTurn)
                moves[3] += 0.3f;
            if (board[6] == 0 && board[7] == oppositeTurn &&
                board[8] == oppositeTurn)
                moves[6] += 0.3f;
            //vertical 3rd free on turn
            if (board[0] == turn && board[3] == turn &&
                board[6] == 0)
                moves[6] += 0.3f;
            if (board[1] == turn && board[4] == turn &&
                board[7] == 0)
                moves[7] += 0.3f;
            if (board[2] == turn && board[5] == turn &&
                board[8] == 0)
                moves[8] += 0.3f;
            //vertical 2nd free on turn
            if (board[0] == turn && board[3] == 0 &&
                board[6] == turn)
                moves[3] += 0.3f;
            if (board[1] == turn && board[4] == 0 &&
                board[7] == turn)
                moves[4] += 0.3f;
            if (board[2] == turn && board[5] == 0 &&
                board[8] == turn)
                moves[5] += 0.3f;
            //vertical 1st free on turn
            if (board[0] == 0 && board[3] == turn &&
                board[6] == turn)
                moves[0] += 0.3f;
            if (board[1] == 0 && board[4] == turn &&
                board[7] == turn)
                moves[1] += 0.3f;
            if (board[2] == 0 && board[5] == turn &&
                board[8] == turn)
                moves[2] += 0.3f;
            //vertical 3rd free off turn
            if (board[0] == oppositeTurn && board[3] == oppositeTurn &&
                board[6] == 0)
                moves[6] += 0.3f;
            if (board[1] == oppositeTurn && board[4] == oppositeTurn &&
                board[7] == 0)
                moves[7] += 0.3f;
            if (board[2] == oppositeTurn && board[5] == oppositeTurn &&
                board[8] == 0)
                moves[8] += 0.3f;
            //vertical 2nd free off oppositeTurn
            if (board[0] == oppositeTurn && board[3] == 0 &&
                board[6] == oppositeTurn)
                moves[3] += 0.3f;
            if (board[1] == oppositeTurn && board[4] == 0 &&
                board[7] == oppositeTurn)
                moves[4] += 0.3f;
            if (board[2] == oppositeTurn && board[5] == 0 &&
                board[8] == oppositeTurn)
                moves[5] += 0.3f;
            //vertical 1st free off oppositeTurn
            if (board[0] == 0 && board[3] == oppositeTurn &&
                board[6] == oppositeTurn)
                moves[0] += 0.3f;
            if (board[1] == 0 && board[4] == oppositeTurn &&
                board[7] == oppositeTurn)
                moves[1] += 0.3f;
            if (board[2] == 0 && board[5] == oppositeTurn &&
                board[8] == oppositeTurn)
                moves[2] += 0.3f;

            //diagonal 3rd free on turn
            if (board[0] == turn && board[4] == turn &&
                board[8] == 0)
                moves[8] += 0.3f;
            if (board[2] == turn && board[4] == turn &&
                board[6] == 0)
                moves[6] += 0.3f;
            //diagonal 2nd free on turn
            if (board[0] == turn && board[4] == 0 &&
                board[8] == turn)
                moves[4] += 0.3f;
            if (board[2] == turn && board[4] == 0 &&
                board[6] == turn)
                moves[4] += 0.3f;
            //diagonal 1st free on turn
            if (board[0] == 0 && board[4] == turn &&
                board[8] == turn)
                moves[0] += 0.3f;
            if (board[2] == 0 && board[4] == turn &&
                board[6] == turn)
                moves[2] += 0.3f;

            //diagonal 3rd free off turn
            if (board[0] == oppositeTurn && board[4] == oppositeTurn &&
                board[8] == 0)
                moves[8] += 0.3f;
            if (board[2] == oppositeTurn && board[4] == oppositeTurn &&
                board[6] == 0)
                moves[6] += 0.3f;
            //diagonal 2nd free off turn
            if (board[0] == oppositeTurn && board[4] == 0 &&
                board[8] == oppositeTurn)
                moves[4] += 0.3f;
            if (board[2] == oppositeTurn && board[4] == 0 &&
                board[6] == oppositeTurn)
                moves[4] += 0.3f;
            //diagonal 1st free off turn
            if (board[0] == 0 && board[4] == oppositeTurn &&
                board[8] == oppositeTurn)
                moves[0] += 0.3f;
            if (board[2] == 0 && board[4] == turn &&
                board[6] == turn)
                moves[2] += 0.3f;

            //horizontal 2nd, 3rd free on turn
            if (board[0] == turn && board[1] == 0 && board[2] == 0)
            {
                moves[1] += 0.1f;
                moves[2] += 0.1f;
            }

            if (board[3] == turn && board[4] == 0 && board[5] == 0)
            {
                moves[4] += 0.1f;
                moves[5] += 0.1f;
            }

            if (board[6] == turn && board[7] == 0 && board[8] == 0)
            {
                moves[7] += 0.1f;
                moves[8] += 0.1f;
            }

            //horizontal 1st, 2nd free on turn
            if (board[0] == 0 && board[1] == 0 && board[2] == turn)
            {
                moves[0] += 0.1f;
                moves[1] += 0.1f;
            }

            if (board[3] == 0 && board[4] == 0 && board[5] == turn)
            {
                moves[3] += 0.1f;
                moves[4] += 0.1f;
            }

            if (board[6] == 0 && board[7] == 0 && board[8] == turn)
            {
                moves[6] += 0.1f;
                moves[7] += 0.1f;
            }

            //horizontal 1st, 3rd free on turn
            if (board[0] == 0 && board[1] == turn && board[2] == 0)
            {
                moves[0] += 0.1f;
                moves[2] += 0.1f;
            }

            if (board[3] == 0 && board[4] == turn && board[5] == 0)
            {
                moves[3] += 0.1f;
                moves[5] += 0.1f;
            }

            if (board[6] == 0 && board[7] == turn && board[8] == 0)
            {
                moves[6] += 0.1f;
                moves[8] += 0.1f;
            }

            //vertical 2nd, 3rd free on turn
            if (board[0] == turn && board[3] == 0 && board[6] == 0)
            {
                moves[3] += 0.1f;
                moves[6] += 0.1f;
            }

            if (board[1] == turn && board[4] == 0 && board[7] == 0)
            {
                moves[4] += 0.1f;
                moves[7] += 0.1f;
            }

            if (board[2] == turn && board[5] == 0 && board[8] == 0)
            {
                moves[5] += 0.1f;
                moves[8] += 0.1f;
            }

            //vertical 1st, 2nd free on turn
            if (board[0] == 0 && board[3] == 0 && board[6] == turn)
            {
                moves[0] += 0.1f;
                moves[3] += 0.1f;
            }

            if (board[1] == 0 && board[4] == 0 && board[7] == turn)
            {
                moves[1] += 0.1f;
                moves[4] += 0.1f;
            }

            if (board[2] == 0 && board[5] == 0 && board[8] == turn)
            {
                moves[2] += 0.1f;
                moves[5] += 0.1f;
            }

            //vertical 1st, 3rd free on turn
            if (board[0] == 0 && board[3] == turn && board[6] == 0)
            {
                moves[0] += 0.1f;
                moves[6] += 0.1f;
            }

            if (board[1] == 0 && board[4] == turn && board[7] == 0)
            {
                moves[1] += 0.1f;
                moves[7] += 0.1f;
            }

            if (board[2] == 0 && board[5] == turn && board[8] == 0)
            {
                moves[2] += 0.1f;
                moves[8] += 0.1f;
            }

            //diagonal 2nd, 3rd free on turn
            if (board[0] == turn && board[4] == 0 && board[8] == 0)
            {
                moves[4] += 0.1f;
                moves[8] += 0.1f;
            }

            if (board[2] == turn && board[4] == 0 && board[6] == 0)
            {
                moves[4] += 0.1f;
                moves[6] += 0.1f;
            }

            //diagonal 1st, 2nd free on turn
            if (board[0] == 0 && board[4] == 0 && board[8] == turn)
            {
                moves[0] += 0.1f;
                moves[4] += 0.1f;
            }

            if (board[2] == 0 && board[4] == 0 && board[6] == turn)
            {
                moves[2] += 0.1f;
                moves[4] += 0.1f;
            }

            //diagonal 1st, 3rd free on turn
            if (board[0] == 0 && board[4] == turn && board[8] == 0)
            {
                moves[0] += 0.1f;
                moves[8] += 0.1f;
            }

            if (board[2] == 0 && board[4] == turn && board[6] == 0)
            {
                moves[2] += 0.1f;
                moves[6] += 0.1f;
            }

            return moves;
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
            
            string json = model.ToJson();
            File.WriteAllText(@"C:\code\github\codingbeard\naughts.net\Naughts.Net\Trainer\model.json", json);
            model.SaveWeight(@"C:\code\github\codingbeard\naughts.net\Naughts.Net\Trainer\model.h5");
        }
    }
}