using System;
using System.Collections.Generic;
using System.Linq;
using peggame.History;

namespace peggame
{
    class InteractiveWithHintsModel : InteractiveModel
    {
        int startingPegIndex = 0;
        Dictionary<string, List<GameRecord>> history = new Dictionary<string, List<GameRecord>>();
        List<(string Pegs, GameRecord GameRecord)> activeGameRecords;
        Dictionary<string, List<GameRecord>> allGameRecords = new Dictionary<string, List<GameRecord>>();

        public override bool RemoveStartingPeg(Dictionary<char, bool> pegs)
        {
            // Create a new set of active game records
            activeGameRecords = new List<(string Pegs, GameRecord GameRecord)>();

            GameInterface.RemovePeg(pegs, GameInterface.PegChars[startingPegIndex]);

            return true;
        }

        public override bool PerformNextJump(Dictionary<char, bool> pegs)
        {
            var remainingPegsBefore = new String(GameInterface.GetRemainingPegs(pegs));
            var jumps = GameInterface.GetPossibleJumps(pegs);

            // We start by choosing the last possible jump
            var jumpIndex = jumps.Length - 1;

            Console.WriteLine($"Calculating statistics... Starting peg: {GameInterface.PegChars[startingPegIndex]}");

            if (history.ContainsKey(remainingPegsBefore)) {
                var attempts = history[remainingPegsBefore];
                var wins = attempts.FindAll(x => x.PegsRemaining.Length == 1);
                var lastAttempt = attempts[attempts.Count - 1];

                // Find the deepest jump that can be decremented
                var decrementIndex = lastAttempt.JumpList.FindLastIndex(x => x.JumpIndex > 0);

                if (decrementIndex == -1) {
                    // If no jumps can be decremented, then we have exhausted
                    // this scenario, so we abort.
                    return false;
                } else if (decrementIndex == 0) {
                    // No deeper jumps can be decremented; decrement this one
                    jumpIndex = lastAttempt.JumpList[0].JumpIndex - 1;
                } else {
                    // A deeper jump will be decremented
                    jumpIndex = lastAttempt.JumpList[0].JumpIndex;
                }
            }

            var jump = jumps[jumpIndex];
            GameInterface.PerformJump(pegs, jump);
            var remainingPegsAfter = new String(GameInterface.GetRemainingPegs(pegs));

            activeGameRecords.Add((Pegs: remainingPegsBefore, GameRecord: new GameRecord(new JumpList(), remainingPegsBefore.ToCharArray())));

            foreach (var gameRecord in activeGameRecords) {
                gameRecord.GameRecord.JumpList.Add(new JumpRecord(jump, jumpIndex));
                gameRecord.GameRecord.PegsRemaining = remainingPegsAfter.ToCharArray();
            }

            return true;
        }

        public override bool PlayAgain(Dictionary<char, bool> pegs)
        {
            foreach (var record in activeGameRecords) {
                if (!history.ContainsKey(record.Pegs)) {
                    history.Add(record.Pegs, new List<GameRecord>());
                }

                history[record.Pegs].Add(record.GameRecord);
            }

            var remainingPegs = new String(Array.FindAll(GameInterface.PegChars, peg => peg != GameInterface.PegChars[startingPegIndex]));
            var attempts = history[remainingPegs];
            var lastAttempt = attempts[attempts.Count - 1];

            bool hasRemainingPaths = lastAttempt.JumpList.Exists(x => x.JumpIndex > 0);

            if (!hasRemainingPaths) {
                if (GameInterface.PegChars.Length <= startingPegIndex + 1) {
                    ReplayGame();

                    return false;
                }

                startingPegIndex++;
            }

            return true;
        }

        private void ReplayGame()
        {
            Dictionary<char, bool> pegs;

            do
            {
                pegs = GameInterface.InitializePegs();
                GameInterface.PrintPegs(pegs);

                if (!base.RemoveStartingPeg(pegs)) {
                    base.PrintStats();
                    return;
                }

                GameInterface.PrintPegs(pegs);

                do {
                    base.PrintStats();

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

                    Console.WriteLine();

                    if (!PerformNextInteractiveJump(pegs, false)) {
                        break;
                    }

                    GameInterface.PrintPegs(pegs);
                }
                while (GameInterface.GetPossibleJumps(pegs).Length > 0);
            }
            while(base.PlayAgain(pegs));
        }

        public override void PrintStats()
        {
            var output = new System.Text.StringBuilder();

            foreach (var peg in GameInterface.PegChars) {
                var remainingPegs = new String(Array.FindAll(GameInterface.PegChars, x => x != peg));

                if (history.ContainsKey(remainingPegs)) {
                    output.Append($"Starting Peg: {peg}. Games: {history[remainingPegs].Count.ToString("N0")}.\n");
                }
            }

            output.Append($"\nUnique Setups: {history.Keys.Count.ToString("N0")}.\n");
        }

        public List<GameRecord> GetAllGameRecords(char[] remainingPegs)
        {
            List<GameRecord> records;

            if (!allGameRecords.TryGetValue(new String(remainingPegs), out records)) {
                records = new List<GameRecord>();

                foreach (var gameRecord in history[new String(remainingPegs)]) {
                    if (history.ContainsKey(new String(gameRecord.PegsRemaining))) {
                        var childRecords = GetAllGameRecords(gameRecord.PegsRemaining);

                        foreach (var child in childRecords) {
                            var mergedJumps = new JumpList();
                            mergedJumps.AddRange(gameRecord.JumpList);
                            mergedJumps.AddRange(child.JumpList);

                            records.Add(new GameRecord(mergedJumps, child.PegsRemaining));
                        }
                    } else {
                        records.Add(gameRecord);
                    }
                }

                allGameRecords.Add(new String(remainingPegs), records);
            }

            return records;
        }

        public Dictionary<string, string> GetGameStats(Dictionary<char, bool> pegs, List<GameRecord> gameRecords)
        {
            var stats = new Dictionary<string, string>();
            var jumps = GameInterface.GetPossibleJumps(pegs);
            var wins = gameRecords.FindAll(x => x.PegsRemaining.Length == 1);
            var winPercent = (decimal)wins.Count / (decimal)gameRecords.Count;

            stats.Add("Possible Jumps", $"    Possibilities: {gameRecords.Count.ToString("N0").PadLeft(7)} - Wins: {wins.Count.ToString("N0").PadLeft(6)} ({winPercent.ToString("P0")})");

            for (var jumpIndex = 0; jumpIndex < jumps.Length; jumpIndex++) {
                var jumpPossibilities = gameRecords.FindAll(x => x.JumpList[0].JumpIndex == jumpIndex);
                var jumpWins = jumpPossibilities.FindAll(x => x.PegsRemaining.Length == 1);
                var jumpWinPercent = (decimal)jumpWins.Count / (decimal)jumpPossibilities.Count;

                stats.Add($"  {jumpIndex + 1}. From {jumps[jumpIndex].From} over {jumps[jumpIndex].Over}", $"Possibilities: {jumpPossibilities.Count.ToString("N0").PadLeft(7)} - Wins: {jumpWins.Count.ToString("N0").PadLeft(6)} ({jumpWinPercent.ToString("P0")})");
            }

            return stats;
        }

        private bool PerformNextInteractiveJump(Dictionary<char, bool> pegs, bool showHints)
        {
            var jumps = GameInterface.GetPossibleJumps(pegs);

            if (showHints) {
                var gameRecords = GetAllGameRecords(GameInterface.GetRemainingPegs(pegs));
                var stats = GetGameStats(pegs, gameRecords);

                Console.WriteLine();

                foreach (var stat in stats.Keys) {
                    Console.WriteLine($"{stat}: {stats[stat]}");
                }

                Console.WriteLine();
                Console.Write("Choose the peg to jump with: ");
            } else {
                GameInterface.PrintJumps(jumps);
                Console.Write("Choose the peg to jump with (press 'H' for hints): ");
            }

            Func<char, bool> CanJumpFrom = (char selectedPeg) => selectedPeg == 'H' || CanJump(jumps, selectedPeg);

            var from = ReadPeg(CanJumpFrom);
            Console.WriteLine(from);

            if (from == null) {
                return false;
            }

            if (from == 'H') {
                GameInterface.PrintPegs(pegs);

                return PerformNextInteractiveJump(pegs, true);
            }

            if (showHints) {
                Console.Write("Choose the peg to jump over (press 'H' for hints): ");
            } else {
                Console.Write("Choose the peg to jump over: ");
            }

            Func<char, bool> CanJumpTo = (char selectedPeg) => selectedPeg == 'H' || CanJump(jumps, from.Value, selectedPeg);

            var over = ReadPeg(CanJumpTo);
            Console.WriteLine(over);

            if (over == 'H') {
                GameInterface.PrintPegs(pegs);

                return PerformNextInteractiveJump(pegs, true);
            } else if (over != null) {
                foreach (var jump in jumps) {
                    if (jump.From == from && jump.Over == over) {
                        GameInterface.PerformJump(pegs, jump);

                        return true;
                    }
                }
            }

            return PerformNextJump(pegs);
        }
    }
}
