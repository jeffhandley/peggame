using System;
using System.Collections.Generic;
using System.Linq;
using peggame.History;

namespace peggame
{
    class RemainingPegsModel : IGameModel
    {
        int startingPegIndex = 0;
        Dictionary<string, List<GameRecord>> history = new Dictionary<string, List<GameRecord>>();
        List<(string Pegs, GameRecord GameRecord)> activeGameRecords;

        public bool RemoveStartingPeg(Dictionary<char, bool> pegs)
        {
            // Create a new set of active game records
            activeGameRecords = new List<(string Pegs, GameRecord GameRecord)>();

            GameInterface.RemovePeg(pegs, GameInterface.PegChars[startingPegIndex]);

            return true;
        }

        public bool PerformNextJump(Dictionary<char, bool> pegs)
        {
            var remainingPegsBefore = new String(GameInterface.GetRemainingPegs(pegs));
            var jumps = GameInterface.GetPossibleJumps(pegs);

            // We start by choosing the last possible jump
            var jumpIndex = jumps.Length - 1;

            Console.WriteLine($"Starting Peg: {GameInterface.PegChars[startingPegIndex]}.");
            Console.WriteLine($"Pegs Remaining: {new String(remainingPegsBefore)}");

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

        public bool PlayAgain(Dictionary<char, bool> pegs)
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
            var model = new InteractiveModel();

            Dictionary<char, bool> pegs;

            do
            {
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

                    var gameRecords = GetAllGameRecords(GameInterface.GetRemainingPegs(pegs));
                    var stats = GetGameStats(pegs, gameRecords);

                    foreach (var stat in stats.Keys) {
                        Console.WriteLine($"{stat}: {stats[stat]}");
                    }

                    Console.WriteLine();

                    if (!model.PerformNextJump(pegs)) {
                        break;
                    }

                    GameInterface.PrintPegs(pegs);
                }
                while (GameInterface.GetPossibleJumps(pegs).Length > 0);
            }
            while(model.PlayAgain(pegs));
        }

        public void PrintStats()
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
            var records = new List<GameRecord>();

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

            return records;
        }

        public Dictionary<string, string> GetGameStats(Dictionary<char, bool> pegs, List<GameRecord> gameRecords)
        {
            var stats = new Dictionary<string, string>();

            stats.Add("Possibilities", gameRecords.Count.ToString("N0"));

            var wins = gameRecords.FindAll(x => x.PegsRemaining.Length == 1);
            stats.Add("Wins", wins.Count.ToString("N0"));

            if (wins.Count > 0) {
                var jumps = GameInterface.GetPossibleJumps(pegs);
                var jumpOdds = wins.GroupBy(win => win.JumpList[0].JumpIndex).Select(x => new { JumpIndex = x.Key, Wins = x.Count() });

                var winCounts = new Dictionary<int, int>();

                foreach (var jump in jumpOdds) {
                    winCounts.Add(jump.JumpIndex, jump.Wins);
                }

                for (var jumpIndex = 0; jumpIndex < jumps.Length; jumpIndex++) {
                    stats.Add($"  {jumpIndex + 1}. From {jumps[jumpIndex].From} over {jumps[jumpIndex].Over}", winCounts.ContainsKey(jumpIndex) ? winCounts[jumpIndex].ToString("N0") : "0");
                }
            }

            return stats;
        }
    }
}
