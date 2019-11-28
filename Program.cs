using System;
using System.Collections.Generic;

namespace peggame
{
    class Program
    {
        static void Main(string[] args)
        {

            if (Array.IndexOf(args, "-stats") >= 0) {
                ShowStats();
                return;
            }

            IGameModel model;

            if (Array.IndexOf(args, "-paths") >= 0) {
                model = new AllPathsModel(args);
            } else if (Array.IndexOf(args, "-first") >= 0) {
                model = new FirstWinFromAllPathsModel();
            } else if (Array.IndexOf(args, "-expert") >= 0) {
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

        static void ShowStats() {
            var model = new InteractiveWithHintsModel(new string[0]);

            Console.Clear();
            Console.WriteLine("Calculating game stats for each starting peg.\n");

            var totalHints = new Hints();

            foreach (var startingPeg in GameInterface.PegChars) {
                Console.Write($"Peg {startingPeg}: ");

                var pegs = GameInterface.InitializePegs();
                GameInterface.RemovePeg(pegs, startingPeg);

                var finished = model.CalculateGameStats(pegs, false);

                if (finished) {
                    var hints = model.GetHints(pegs);
                    Console.WriteLine($"Possibilities: {hints.Possibilities.ToString("N0").PadLeft(9)} - Best/Worst Score: {hints.BestScore.ToString("N0").PadLeft(2)}/{hints.WorstScore.ToString("N0").PadRight(2)} - Wins: {hints.Wins.ToString("N0").PadLeft(7)} - Win Rate: {hints.WinRate.ToString("P2").PadLeft(7)}");

                    totalHints.Possibilities += hints.Possibilities;
                    totalHints.Wins += hints.Wins;
                    totalHints.BestScore = Math.Min(totalHints.BestScore, hints.BestScore);
                    totalHints.WorstScore = Math.Max(totalHints.WorstScore, hints.WorstScore);
                } else {
                    Console.WriteLine("Aborted");

                    if (Console.ReadKey(true).Key == ConsoleKey.Escape) {
                        break;
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Total: Possibilities: {totalHints.Possibilities.ToString("N0").PadLeft(9)} - Best/Worst Score: {totalHints.BestScore.ToString("N0").PadLeft(2)}/{totalHints.WorstScore.ToString("N0").PadRight(2)} - Wins: {totalHints.Wins.ToString("N0").PadLeft(7)} - Win Rate: {totalHints.WinRate.ToString("P2").PadLeft(7)}");
        }
    }
}
