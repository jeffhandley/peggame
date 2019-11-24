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

                do {
                    model.PrintStats();

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

                    var jump = model.ChooseNextJump(pegs);

                    if (jump == null) {
                        break;
                    }

                    pegs[jump.Value.From] = false;
                    pegs[jump.Value.Over] = false;
                    pegs[jump.Value.To] = true;

                    GameInterface.PrintPegs(pegs);
                }
                while (GameInterface.GetPossibleJumps(pegs).Length > 0);
            }
            while (model.PlayAgain(pegs));
        }

        static void InitializePegs(Dictionary<char, bool> pegs) {
            for (var i = 0; i < GameInterface.PegChars.Length; i++) {
                pegs[GameInterface.PegChars[i]] = true;
            }
        }
    }
}
