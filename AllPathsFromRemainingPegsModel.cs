using System;
using System.Collections.Generic;
using peggame.History;

namespace peggame
{
    class AllPathsFromRemainingPegsModel : IGameModel
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

                Console.WriteLine($"Attempts: {attempts.Count}. Wins: {wins.Count}. Last Attempt: {lastAttempt.JumpList[0].JumpIndex}. Decrement Index: {decrementIndex}.");

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
            } else {
                Console.WriteLine("Attempts: 0.");
            }

            Console.WriteLine($"Choosing Jump Index: {jumpIndex}");

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

                    var interactive = GameInterface.InitializePegs();


                    return false;
                }

                startingPegIndex++;
            }

            return true;
        }

        public void PrintStats()
        {
            foreach (var peg in GameInterface.PegChars) {
                var remainingPegs = new String(Array.FindAll(GameInterface.PegChars, x => x != peg));

                if (history.ContainsKey(remainingPegs)) {
                    Console.WriteLine($"Starting Peg: {peg}. Games: {history[remainingPegs].Count.ToString("N0")}.");
                }
            }

            Console.WriteLine($"Unique Setups: {history.Keys.Count.ToString("N0")}.");
        }
    }
}
