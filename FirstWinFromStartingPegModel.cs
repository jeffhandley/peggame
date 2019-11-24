using System;
using System.Collections.Generic;

namespace peggame
{
    class FirstWinFromStartingPegModel : AllPathsFromStartingPegModel
    {
        public override bool PlayAgain(Dictionary<char, bool> pegs) {
            var pegsRemaining = GameInterface.GetRemainingPegs(pegs);
            var gameRecord = new History.GameRecord(currentPath, pegsRemaining);

            history[startingPeg.Value].Add(gameRecord);

            if (pegsRemaining.Length == 1) {
                wins[startingPeg.Value].Add(gameRecord);
            }

            PrintStats();

            bool hasMoreMoves = false;

            foreach (var jump in history[startingPeg.Value][history[startingPeg.Value].Count - 1].JumpList) {
                if (jump.JumpIndex > 0) {
                    hasMoreMoves = true;
                }
            }

            if (!hasMoreMoves || wins[startingPeg.Value].Count >= 1) {
                nextStartingPeg++;
                lastPath = null;
                GameInterface.WriteWins(wins);
            }

            if (GameInterface.PegChars.Length > nextStartingPeg) {
                return true;
            }

            Console.WriteLine("Wins:");

            foreach (var peg in wins.Keys) {
                Console.WriteLine($"Starting Peg: {peg}");
                Console.WriteLine();

                if (wins[peg].Count > 0) {
                    foreach (var jump in wins[peg][0].JumpList) {
                        Console.WriteLine($"Jumped {jump.From} over {jump.Over}.");
                        Console.WriteLine();
                    }
                } else {
                    Console.WriteLine("No wins");
                    Console.WriteLine();
                }

            }

            return false;
        }
    }
}
