using System;
using System.Collections.Generic;
using System.Linq;
using peggame.History;

namespace peggame
{
    class InteractiveWithHintsModel : InteractiveModel
    {
        Dictionary<string, List<GameRecord>> allGameRecords = new Dictionary<string, List<GameRecord>>();
        Dictionary<string, List<GameRecord>> history = new Dictionary<string, List<GameRecord>>();
        List<(string Pegs, GameRecord GameRecord)> activeGameRecords;

        public override bool PerformNextJump(Dictionary<char, bool> pegs)
        {
            return PerformNextJump(pegs, false);
        }

        protected bool PerformNextJump(Dictionary<char, bool> pegs, bool showHints)
        {
            var jumps = GameInterface.GetPossibleJumps(pegs);

            if (showHints) {
                var hints = GetHints(pegs);

                Console.Write("Choose the peg to jump with: ");

                var left = Console.CursorLeft;
                var top = Console.CursorTop;

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();

                var output = new System.Text.StringBuilder();

                output.Append($"Possible Jumps:   (Possibilities: {hints.Possibilities.ToString("N0").PadLeft(7)} - Best Score: {hints.BestScore} - Wins: {hints.Wins.ToString("N0").PadLeft(6)} - Win Rate: {hints.WinRate.ToString("P2").PadLeft(7)})\n");

                for (var j = 0; j < jumps.Length; j++) {
                    var jump = jumps[j];
                    output.Append($"  - Jump {jump.From} over {jump.Over} (Possibilities: {hints.JumpHints[j].Possibilities.ToString("N0").PadLeft(7)} - Best Score: {hints.JumpHints[j].BestScore} - Wins: {hints.JumpHints[j].Wins.ToString("N0").PadLeft(6)} - Win Rate: {hints.JumpHints[j].WinRate.ToString("P2").PadLeft(7)})\n");
                }

                Console.WriteLine(output);

                Console.SetCursorPosition(left, top);
            } else {
                Console.Write("Choose the peg to jump with: ");

                var left = Console.CursorLeft;
                var top = Console.CursorTop;

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
                GameInterface.PrintJumps(jumps);
                Console.WriteLine("Press 'H' for hints");

                Console.SetCursorPosition(left, top);
            }

            Func<char, bool> CanJumpFrom = (char selectedPeg) => selectedPeg == 'H' || CanJump(jumps, selectedPeg);

            var from = ReadPeg(CanJumpFrom);
            Console.WriteLine(from);

            if (from == null) {
                GameInterface.PrintPegs(pegs);

                return false;
            }

            if (from == 'H') {
                GameInterface.PrintPegs(pegs);

                return PerformNextJump(pegs, true);
            }

            Console.Write("Choose the peg to jump over: ");

            Func<char, bool> CanJumpTo = (char selectedPeg) => selectedPeg == 'H' || CanJump(jumps, from.Value, selectedPeg);

            var over = ReadPeg(CanJumpTo);
            Console.WriteLine(over);

            if (over == 'H') {
                GameInterface.PrintPegs(pegs);

                return PerformNextJump(pegs, true);
            } else if (over != null) {
                foreach (var jump in jumps) {
                    if (jump.From == from && jump.Over == over) {
                        GameInterface.PerformJump(pegs, jump);

                        return true;
                    }
                }
            }

            GameInterface.PrintPegs(pegs);

            // Over selection was aborted, ask for the From selection again
            return PerformNextJump(pegs, showHints);
        }

        private GameHints GetHints(Dictionary<char, bool> pegs)
        {
            var gameRecords = GetAllGameRecords(pegs);
            gameRecords.Sort(delegate(GameRecord record1, GameRecord record2) {
                return record1.PegsRemaining.Length.CompareTo(record2.PegsRemaining.Length);
            });

            var hints = new GameHints();
            var jumps = GameInterface.GetPossibleJumps(pegs);
            var wins = gameRecords.FindAll(x => x.PegsRemaining.Length == 1);

            hints.Possibilities = gameRecords.Count;
            hints.Wins = wins.Count;
            hints.BestScore = gameRecords[0].PegsRemaining.Length;

            for (var jumpIndex = 0; jumpIndex < jumps.Length; jumpIndex++) {
                var jumpPossibilities = gameRecords.FindAll(x => x.JumpList[0].JumpIndex == jumpIndex);
                jumpPossibilities.Sort(delegate(GameRecord record1, GameRecord record2) {
                    return record1.PegsRemaining.Length.CompareTo(record2.PegsRemaining.Length);
                });

                var jumpHints = new Hints();
                jumpHints.Possibilities = jumpPossibilities.Count;
                jumpHints.Wins = jumpPossibilities.FindAll(x => x.PegsRemaining.Length == 1).Count;
                jumpHints.BestScore = jumpPossibilities[0].PegsRemaining.Length;

                hints.JumpHints.Add(jumpIndex, jumpHints);
            }

            return hints;
        }

        public List<GameRecord> GetAllGameRecords(Dictionary<char, bool> pegs)
        {
            var remainingPegs = GameInterface.GetRemainingPegs(pegs);

            return GetAllGameRecords(pegs, remainingPegs);
        }

        public List<GameRecord> GetAllGameRecords(Dictionary<char, bool> pegs, char[] pegsForRecords)
        {
            List<GameRecord> records;

            if (!allGameRecords.TryGetValue(new String(pegsForRecords), out records)) {
                var historyKey = new String(pegsForRecords);

                if (!history.ContainsKey(historyKey)) {
                    CalculateGameStats(pegs);

                    GameInterface.PrintPegs(pegs);
                }

                records = new List<GameRecord>();

                foreach (var gameRecord in history[historyKey]) {
                    if (history.ContainsKey(new String(gameRecord.PegsRemaining))) {
                        var childRecords = GetAllGameRecords(pegs, gameRecord.PegsRemaining);

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

                allGameRecords.Add(historyKey, records);
            }

            return records;
        }

        private void CalculateGameStats(Dictionary<char, bool> pegs)
        {
            do
            {
                var simulationPegs = new Dictionary<char, bool>(pegs);
                BeginSimulation(simulationPegs);

                do {
                    GameInterface.PrintPegs(simulationPegs);
                    Console.WriteLine("Calculating hints... Press a key to abort.");

                    if (Console.KeyAvailable == true) {
                        while (Console.KeyAvailable) {
                            Console.ReadKey(true);
                        }

                        return;
                    }

                    if (!SimulateNextJump(simulationPegs)) {
                        break;
                    }
                }
                while (GameInterface.GetPossibleJumps(simulationPegs).Length > 0);
            }
            while(FinishSimulation(pegs));
        }

        private void BeginSimulation(Dictionary<char, bool> pegs)
        {
            // Create a new set of active game records
            activeGameRecords = new List<(string Pegs, GameRecord GameRecord)>();
        }

        private bool SimulateNextJump(Dictionary<char, bool> pegs)
        {
            var remainingPegsBefore = new String(GameInterface.GetRemainingPegs(pegs));
            var jumps = GameInterface.GetPossibleJumps(pegs);

            // We start by choosing the last possible jump
            var jumpIndex = jumps.Length - 1;

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

        private bool FinishSimulation(Dictionary<char, bool> pegs)
        {
            foreach (var record in activeGameRecords) {
                if (!history.ContainsKey(record.Pegs)) {
                    history.Add(record.Pegs, new List<GameRecord>());
                }

                history[record.Pegs].Add(record.GameRecord);
            }

            var remainingPegs = new String(GameInterface.GetRemainingPegs(pegs));
            var attempts = history[remainingPegs];
            var lastAttempt = attempts[attempts.Count - 1];

            bool hasRemainingPaths = lastAttempt.JumpList.Exists(x => x.JumpIndex > 0);

            if (!hasRemainingPaths) {
                return false;
            }

            return true;
        }
    }
}
