using System;
using System.Collections.Generic;

namespace peggame
{
    class Program
    {
        static void Main(string[] args)
        {
            IGameModel model;

            if (Array.IndexOf(args, "-a") >= 0) {
                model = new AllPathsFromStartingPegModel(args);
            } else if (Array.IndexOf(args, "-1") >= 0) {
                model = new FirstWinFromStartingPegModel();
            } else {
                model = new InteractiveGameModel();
            }

            Dictionary<char, bool> pegs;

            do {
                pegs = new Dictionary<char, bool>();

                InitializePegs(pegs);
                GameInterface.PrintPegs(pegs);

                var peg = model.ChooseStartingPeg(pegs);

                if (peg == null) {
                    return;
                }

                pegs[peg.Value] = false;

                GameInterface.PrintPegs(pegs);

                var jumps = GetPossibleJumps(pegs);

                do {
                    model.PrintStats();
                    GameInterface.PrintJumps(jumps);

                    if (Console.KeyAvailable == true) {
                        Console.WriteLine("Game paused. Press a key to continue.");

                        while (Console.KeyAvailable) {
                            Console.ReadKey(true);
                        }

                        var unpause = Console.ReadKey(true).Key;

                        if (unpause == ConsoleKey.Escape) {
                            return;
                        }
                    }

                    var jump = model.ChooseNextJump(jumps);

                    if (jump == null) {
                        break;
                    }

                    pegs[jump.Value.From] = false;
                    pegs[jump.Value.Over] = false;
                    pegs[jump.Value.To] = true;

                    GameInterface.PrintPegs(pegs);

                    jumps = GetPossibleJumps(pegs);
                }
                while (jumps.Length > 0);
            }
            while (model.PlayAgain(pegs));
        }

        static void InitializePegs(Dictionary<char, bool> pegs) {
            for (var i = 0; i < GameInterface.PegChars.Length; i++) {
                pegs[GameInterface.PegChars[i]] = true;
            }
        }

        static Jump[] GetPossibleJumps(Dictionary<char, bool> pegs) {
            var jumps = new Jump[] {
                new Jump {From = '1', To = '4', Over = '2'},
                new Jump {From = '1', To = '6', Over = '3'},

                new Jump {From = '2', To = '7', Over = '4'},
                new Jump {From = '2', To = '9', Over = '5'},

                new Jump {From = '3', To = '8', Over = '5'},
                new Jump {From = '3', To = '0', Over = '6'},

                new Jump {From = '4', To = '1', Over = '2'},
                new Jump {From = '4', To = '6', Over = '5'},
                new Jump {From = '4', To = 'A', Over = '7'},
                new Jump {From = '4', To = 'C', Over = '8'},

                new Jump {From = '5', To = 'B', Over = '8'},
                new Jump {From = '5', To = 'D', Over = '9'},

                new Jump {From = '6', To = '1', Over = '3'},
                new Jump {From = '6', To = '4', Over = '5'},
                new Jump {From = '6', To = 'C', Over = '9'},
                new Jump {From = '6', To = 'E', Over = '0'},

                new Jump {From = '7', To = '2', Over = '4'},
                new Jump {From = '7', To = '9', Over = '8'},

                new Jump {From = '8', To = '3', Over = '5'},
                new Jump {From = '8', To = '0', Over = '9'},

                new Jump {From = '9', To = '2', Over = '5'},
                new Jump {From = '9', To = '7', Over = '8'},

                new Jump {From = '0', To = '3', Over = '6'},
                new Jump {From = '0', To = '8', Over = '9'},

                new Jump {From = 'A', To = '4', Over = '7'},
                new Jump {From = 'A', To = 'C', Over = 'B'},

                new Jump {From = 'B', To = '5', Over = '8'},
                new Jump {From = 'B', To = 'D', Over = 'C'},

                new Jump {From = 'C', To = '4', Over = '8'},
                new Jump {From = 'C', To = '6', Over = '9'},
                new Jump {From = 'C', To = 'A', Over = 'B'},
                new Jump {From = 'C', To = 'E', Over = 'D'},

                new Jump {From = 'D', To = '5', Over = '9'},
                new Jump {From = 'D', To = 'B', Over = 'C'},

                new Jump {From = 'E', To = '6', Over = '0'},
                new Jump {From = 'E', To = 'C', Over = 'D'}
            };

            return Array.FindAll(jumps, jump =>
                pegs[jump.From] == true &&
                pegs[jump.Over] == true &&
                pegs[jump.To] == false
            );
        }
    }
}
