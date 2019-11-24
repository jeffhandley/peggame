using System;
using System.Collections.Generic;

namespace peggame
{
    class Program
    {
        static void Main(string[] args)
        {
            IGameModel model;

            if (Array.IndexOf(args, "-paths") >= 0) {
                model = new AllPathsFromStartingPegModel(args);
            } else if (Array.IndexOf(args, "-first") >= 0) {
                model = new FirstWinFromStartingPegModel();
            } else if (Array.IndexOf(args, "-pegs") >= 0) {
                model = new AllPathsFromRemainingPegsModel();
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

                    if (!model.PerformNextJump(pegs)) {
                        break;
                    }

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
