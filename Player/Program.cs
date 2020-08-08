using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Keras.Models;
using Numpy;
using Trainer;

namespace Player
{
    class Program
    {
        
        static void PrintBoard(float[] board)
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

        static int ArgMax(NDarray array)
        {
            var max = 0f;
            var key = 0;
            var maxKey = 0;
            foreach (var output in array.GetData<float>())
            {
                if (output > max)
                {
                    max = output;
                    maxKey = key;
                }

                key++;
            }

            return maxKey;
        }
        
        static void Main(string[] args)
        {
            var loadedModel =
                Sequential.ModelFromJson(
                    File.ReadAllText(@"C:\code\github\codingbeard\naughts.net\Naughts.Net\Trainer\model.json"));
            loadedModel.LoadWeight(@"C:\code\github\codingbeard\naughts.net\Naughts.Net\Trainer\model.h5");
            
            var random = new Random();
            var floatBoard = new float[] {0, 0, 0, 0, 0, 0, 0, 0, 0,};
            var turn = 1;
            var turns = 0;
            var aiRole = 1;
            Console.Write("Choose who goes first (me or ai): ");
            var first = Console.In.ReadLine();
            if (first != null && first.Length == 2)
            {
                if (first == "me")
                {
                    aiRole = 2;
                }
            }

            PrintBoard(floatBoard);
            while (true)
            {
                turns++;
                if (turns > 10)
                {
                    break;
                }
                
                if (turn == aiRole)
                {
                    Console.WriteLine("AI's turn");
                    if (turns == 1)
                    {
                        floatBoard[random.Next(8)] = turn;
                    }
                    else
                    {
                        var predicitons = loadedModel.Predict(
                            np.array(new List<NDarray>() {np.array(floatBoard)}.ToArray()),
                            verbose: 0
                        );
                        floatBoard[ArgMax(predicitons)] = turn;
                    }
                }
                else
                {
                    Console.Out.WriteLine(" 0 1 2\n 3 4 5\n 6 7 8");
                    Console.Write("Your turn, enter a move (0-8): ");
                    var move = Console.In.ReadLine();
                    if (move != null && move.Length == 1)
                    {
                        var moveInt = Int32.Parse(move);
                        floatBoard[moveInt] = turn;
                    }
                }
                
                PrintBoard(floatBoard);
                
                if (turn == 1)
                    turn = 2;
                else
                    turn = 1;
                
                var moves = Board.GetMoves(floatBoard);
                var state = Board.GetBoardState(floatBoard);
                if (state == 0 && moves.Length == 0)
                {
                    Console.WriteLine("Draw");
                    break;
                }
                if (state == 1)
                {
                    Console.WriteLine("Naughts Win");
                    break;
                }
                if (state == 2)
                {
                    Console.WriteLine("Crosses Win");
                    break;
                }
            }
        }
    }
}