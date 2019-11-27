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
                model = new AllPathsModel(args);
            } else if (Array.IndexOf(args, "-first") >= 0) {
                model = new FirstWinFromAllPathsModel();
            } else if (Array.IndexOf(args, "-nohints") >= 0) {
                model = new InteractiveModel();
            } else {
                model = new InteractiveWithHintsModel(args);
            }

            Dictionary<char, bool> pegs;

            do {
                pegs = GameInterface.InitializePegs();
                GameInterface.PrintPegs(pegs);

                if (!model.RemoveStartingPeg(pegs)) {
                    model.PrintStats();
                    return;
                }

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
    }
}
