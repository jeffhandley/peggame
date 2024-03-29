using System;
using System.Collections.Generic;
using System.Linq;
using peggame.History;

namespace peggame
{
    enum DifficultyLevel
    {
        Easy = 1,
        Medium = 2,
        Hard = 3
    }

    class InteractiveWithHintsModel : InteractiveModel
    {
        DifficultyLevel difficulty = DifficultyLevel.Medium;
        Dictionary<string, List<GameRecord>> allGameRecords = new Dictionary<string, List<GameRecord>>();
        Dictionary<string, List<GameRecord>> history = new Dictionary<string, List<GameRecord>>();
        List<string> historyCompleted = new List<string>();
        List<(string Pegs, GameRecord GameRecord)> activeGameRecords;

        public InteractiveWithHintsModel(string[] args)
        {
            if (Array.IndexOf(args, "-easy") >= 0) {
                difficulty = DifficultyLevel.Easy;
            } else if (Array.IndexOf(args, "-hard") >= 0) {
                difficulty = DifficultyLevel.Hard;
            }
        }

        public override bool PerformNextJump(Dictionary<char, bool> pegs)
        {
            return PerformNextJump(pegs, false);
        }

        protected bool PerformNextJump(Dictionary<char, bool> pegs, bool showHints)
        {
            var jumps = GameInterface.GetPossibleJumps(pegs);
            var hintsReady = false;

            if (showHints || difficulty == DifficultyLevel.Easy) {
                hintsReady = CalculateGameStats(pegs, true);
                GameInterface.PrintPegs(pegs);
            }

            Console.Write("Choose the peg to jump with: ");

            var left = Console.CursorLeft;
            var top = Console.CursorTop;

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            if (hintsReady) {
                var hints = GetHints(pegs);
                var output = new System.Text.StringBuilder();

                output.Append($"Possible Jumps:   (Possibilities: {hints.Possibilities.ToString("N0").PadLeft(9)} - Best/Worst Score: {hints.BestScore.ToString("N0").PadLeft(2)}/{hints.WorstScore.ToString("N0").PadRight(2)} - Wins: {hints.Wins.ToString("N0").PadLeft(7)} - Win Rate: {hints.WinRate.ToString("P2").PadLeft(7)})\n");

                for (var j = 0; j < jumps.Length; j++) {
                    var jump = jumps[j];
                    output.Append($"  - Jump {jump.From} over {jump.Over} (Possibilities: {hints.JumpHints[j].Possibilities.ToString("N0").PadLeft(9)} - Best/Worst Score: {hints.JumpHints[j].BestScore.ToString("N0").PadLeft(2)}/{hints.JumpHints[j].WorstScore.ToString("N0").PadRight(2)} - Wins: {hints.JumpHints[j].Wins.ToString("N0").PadLeft(7)} - Win Rate: {hints.JumpHints[j].WinRate.ToString("P2").PadLeft(7)})\n");
                }

                Console.WriteLine(output);
            } else {
                GameInterface.PrintJumps(jumps);
                Console.WriteLine("Press 'H' for hints");
            }

            Console.SetCursorPosition(left, top);

            // Calculate game stats in the background, which will
            // abort if a key becomes available
            hintsReady = CalculateGameStats(pegs, false);

            Func<char, bool> CanJumpFrom = (char selectedPeg) => selectedPeg == 'H' || CanJump(jumps, selectedPeg);
            var from = ReadPeg(CanJumpFrom);
            Console.WriteLine(from);

            if (from == null) {
                // ESC was pressed, aborting the game
                GameInterface.PrintPegs(pegs);

                return false;
            } else if (from == 'H') {
                GameInterface.PrintPegs(pegs);

                return PerformNextJump(pegs, true);
            }

            Console.Write("Choose the peg to jump over: ");

            // Calculate game stats in the background, which will
            // abort if a key becomes available
            hintsReady = CalculateGameStats(pegs, false);

            Func<char, bool> CanJumpOver = (char selectedPeg) => selectedPeg == 'H' || CanJump(jumps, from.Value, selectedPeg);
            var over = ReadPeg(CanJumpOver);
            Console.WriteLine(over);

            if (over == 'H') {
                // ESC was pressed, aborting the jump; restart the jump
                GameInterface.PrintPegs(pegs);

                return PerformNextJump(pegs, true);
            } else if (over != null) {
                for (var jumpIndex = 0; jumpIndex < jumps.Length; jumpIndex++) {
                    var jump = jumps[jumpIndex];

                    if (jump.From == from && jump.Over == over) {
                        if (hintsReady && (int)difficulty < (int)DifficultyLevel.Hard) {
                            var hints = GetHints(pegs);

                            if (hints.Wins > 0 && hints.JumpHints[jumpIndex].Wins == 0) {
                                Console.Write("This might not be the best jump. Press ESC to choose another jump or any other key to continue. ");

                                var advice = Console.ReadKey(true);

                                if (advice.Key == ConsoleKey.Escape) {
                                    GameInterface.PrintPegs(pegs);

                                    return PerformNextJump(pegs, showHints);
                                }
                            }
                        }

                        GameInterface.PerformJump(pegs, jump);

                        return true;
                    }
                }
            }

            GameInterface.PrintPegs(pegs);

            // Over selection was aborted, ask for the From selection again
            return PerformNextJump(pegs, showHints);
        }

        public GameHints GetHints(Dictionary<char, bool> pegs)
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
            hints.WorstScore = gameRecords[gameRecords.Count - 1].PegsRemaining.Length;

            for (var jumpIndex = 0; jumpIndex < jumps.Length; jumpIndex++) {
                var jumpPossibilities = gameRecords.FindAll(x => x.JumpList[0].JumpIndex == jumpIndex);
                jumpPossibilities.Sort(delegate(GameRecord record1, GameRecord record2) {
                    return record1.PegsRemaining.Length.CompareTo(record2.PegsRemaining.Length);
                });

                var jumpHints = new Hints();
                jumpHints.Possibilities = jumpPossibilities.Count;
                jumpHints.Wins = jumpPossibilities.FindAll(x => x.PegsRemaining.Length == 1).Count;
                jumpHints.BestScore = jumpPossibilities[0].PegsRemaining.Length;
                jumpHints.WorstScore = jumpPossibilities[jumpPossibilities.Count - 1].PegsRemaining.Length;

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

        public bool CalculateGameStats(Dictionary<char, bool> pegs, bool showProgress)
        {
            if (historyCompleted.Contains(new String(GameInterface.GetRemainingPegs(pegs)))) {
                return true;
            }

            do
            {
                var simulationPegs = new Dictionary<char, bool>(pegs);
                BeginSimulation(simulationPegs);

                do {
                    if (showProgress) {
                        GameInterface.PrintPegs(simulationPegs);
                        Console.WriteLine("Calculating hints... Press a key to abort.");
                    }

                    if (Console.KeyAvailable == true) {
                        return false;
                    }

                    if (!SimulateNextJump(simulationPegs)) {
                        break;
                    }
                }
                while (GameInterface.GetPossibleJumps(simulationPegs).Length > 0);
            }
            while(FinishSimulation(pegs));

            return true;
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
                historyCompleted.Add(remainingPegs);

                return false;
            }

            return true;
        }
    }
}
