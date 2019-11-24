using System;
using System.Collections.Generic;

namespace peggame
{
    class AllPathsFromStartingPegModel : InteractiveGameModel
    {
        protected Dictionary<char, List<History.GameRecord>> history;
        protected int nextStartingPeg = 0;
        protected int lastStartingPeg = 14;
        protected char? startingPeg;
        protected History.JumpList currentPath;
        protected History.JumpList lastPath;
        protected Dictionary<char, List<History.GameRecord>> wins;
        private int? maxAttemptsPerPeg;
        private bool quietOutput = false;

        public AllPathsFromStartingPegModel()
        {
            history = new Dictionary<char, List<History.GameRecord>>();
            wins = new Dictionary<char, List<History.GameRecord>>();
        }

        public AllPathsFromStartingPegModel(string[] args) : this()
        {
            var maxArg = Array.IndexOf(args, "-m");

            if (maxArg >= 0 && args.Length > maxArg + 1) {
                int maxAttempts = 0;
                int.TryParse(args[maxArg + 1], out maxAttempts);

                if (maxAttempts > 0) {
                    maxAttemptsPerPeg = maxAttempts;
                }
            }

            if (Array.IndexOf(args, "-q") >= 0) {
                quietOutput = true;
            }


            var fromArg = Array.IndexOf(args, "-from");

            if (fromArg >= 0 && args.Length > fromArg + 1) {
                nextStartingPeg = Array.IndexOf(GameInterface.PegChars, args[fromArg + 1][0]);
            }

            var toArg = Array.IndexOf(args, "-to");

            if (toArg >= 0 && args.Length > toArg + 1) {
                lastStartingPeg = Array.IndexOf(GameInterface.PegChars, args[toArg + 1][0]);
            }
        }

        public override char? ChooseStartingPeg(Dictionary<char, bool> pegs)
        {
            startingPeg = nextStartingPeg < GameInterface.PegChars.Length ? GameInterface.PegChars[nextStartingPeg] : (char?)null;

            if (startingPeg.HasValue) {
                history.TryAdd(startingPeg.Value, new List<History.GameRecord>());
                wins.TryAdd(startingPeg.Value, new List<History.GameRecord>());

                currentPath = new History.JumpList();

                if (history.ContainsKey(startingPeg.Value)) {
                    var paths = history[startingPeg.Value];
                    lastPath = paths.Count > 0 ? paths[paths.Count - 1].JumpList : (History.JumpList)null;
                }
            } else {
                currentPath = (History.JumpList)null;
                lastPath = (History.JumpList)null;
            }

            return startingPeg;
        }

        public override Jump? ChooseNextJump(Dictionary<char, bool> pegs)
        {
            var jumps = GameInterface.GetPossibleJumps(pegs);
            History.JumpRecord thisJump;

            // If the last path got as far as our previous jump
            if (lastPath != null && lastPath.Count > currentPath.Count - 1) {
                // If the paths have been the sae so far, then we need to
                // decide what to do on this jump based on the last path
                if (PathsEqual(lastPath, currentPath)) {
                    // The previous jumps were the same (or this is the first jump)
                    // Now we need to look ahead at the next jump to determine if
                    // this jump should be the same
                    var lastPathThisJumpIndex = lastPath[currentPath.Count].JumpIndex;
                    var hasRemainingDecrements = false;

                    for (var i = currentPath.Count + 1; i < lastPath.Count; i++) {
                        if (lastPath[i].JumpIndex > 0) {
                            hasRemainingDecrements = true;
                            break;
                        }
                    }

                    if (hasRemainingDecrements == true) {
                        // There are remaining jumps to decrement,
                        // so we will keep this jump the same
                        thisJump = new History.JumpRecord(jumps[lastPathThisJumpIndex], lastPathThisJumpIndex);
                    } else {
                        // There are no remaining jumps to decrement,
                        // so we need to decrement this one if possible

                        if (lastPathThisJumpIndex > 0) {
                            thisJump = new History.JumpRecord(jumps[lastPathThisJumpIndex - 1], lastPathThisJumpIndex - 1);
                        } else {
                            throw new Exception($"Cannot decrement this jump. Jump number: {currentPath.Count}. Has remaining decrements: {hasRemainingDecrements}.");
                        }
                    }
                } else {
                    // The previous jumps were different
                    // We will choose the last option for this jump
                    thisJump = new History.JumpRecord(jumps[jumps.Length - 1], jumps.Length - 1);
                }
            } else {
                // The last path didn't get as far as we have gotten
                // We will choose the last option for this jump
                thisJump = new History.JumpRecord(jumps[jumps.Length - 1], jumps.Length - 1);
            }

            currentPath.Add(thisJump);

            return thisJump.Jump;
        }

        public override bool PlayAgain(Dictionary<char, bool> pegs) {
            var pegsRemaining = Array.FindAll(GameInterface.PegChars, p => pegs[p] == true);
            var gameRecord = new History.GameRecord(startingPeg.Value, currentPath, pegsRemaining);

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

            if (!hasMoreMoves || (maxAttemptsPerPeg.HasValue && history[startingPeg.Value].Count >= maxAttemptsPerPeg.Value)) {
                nextStartingPeg++;
                lastPath = null;
                GameInterface.WriteWins(wins);
            }

            return Math.Min(GameInterface.PegChars.Length, lastStartingPeg + 1) > nextStartingPeg;
        }

        public override void PrintStats() {
            if (!quietOutput) {
                Console.WriteLine("".PadRight(65) + "Last Path:".PadRight(35) + "This Path:");
            }

            var pathLength = !quietOutput ? Math.Max(Math.Max(lastPath != null ? lastPath.Count : 0, currentPath.Count), history.Keys.Count) : history.Keys.Count;
            var pegStats = new List<string>();

            foreach (var peg in history.Keys) {
                decimal winPercent = history[peg].Count > 0 ? ((decimal)wins[peg].Count / (decimal)history[peg].Count) : (decimal)0.00;
                pegStats.Add($"Starting Peg: {peg}. Attempts: {history[peg].Count.ToString("N0")}. Wins: {wins[peg].Count.ToString("N0")} ({winPercent.ToString("P2")}).");
            }

            for (var i = 0; i < pathLength; i++) {
                var pegStat = pegStats.Count > i ? pegStats[i] : "";
                var lastJump = !quietOutput && lastPath != null && lastPath.Count > i ? $"Jumped {lastPath[i].From} over {lastPath[i].Over}. Jump index: {lastPath[i].JumpIndex}." : "";
                var currentJump = !quietOutput && currentPath.Count > i ? $"Jumped {currentPath[i].From} over {currentPath[i].Over}. Jump index: {currentPath[i].JumpIndex}." : "";

                if (quietOutput) {
                    Console.WriteLine(pegStat);
                } else {
                    Console.WriteLine(pegStat.PadRight(65) + lastJump.PadRight(35) + currentJump);
                }
            }

            Console.WriteLine();
        }

        static bool PathsEqual(History.JumpList pastPath, History.JumpList currentPath) {
            if (pastPath.Count < currentPath.Count) {
                return false;
            }

            for (var j = 0; j < currentPath.Count; j++) {
                if (!currentPath[j].Jump.Equals(pastPath[j].Jump)) {
                    return false;
                }
            }

            return true;
        }
    }
}
