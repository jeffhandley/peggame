using System;
using System.Collections.Generic;

namespace peggame
{
    class FirstWinFromAllPathsModel : AllPathsModel
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
            }

            if (GameInterface.PegChars.Length > nextStartingPeg) {
                return true;
            }

            var output = new System.Text.StringBuilder();

            output.Append("Wins:\n");

            foreach (var peg in wins.Keys) {
                output.Append($"Starting Peg: {peg}\n\n");

                if (wins[peg].Count > 0) {
                    foreach (var jump in wins[peg][0].JumpList) {
                        output.Append($"  Jumped {jump.From} over {jump.Over}.\n");
                    }

                    output.Append("\n");
                } else {
                    output.Append("No wins\n\n");
                }

            }

            Console.WriteLine(output);

            return false;
        }
    }
}
