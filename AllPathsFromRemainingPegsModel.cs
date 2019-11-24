using System;
using System.Collections.Generic;
using peggame.History;

namespace peggame
{
    class AllPathsFromRemainingPegsModel : IGameModel
    {
        char startingPeg = '1';
        Dictionary<char[], List<GameRecord>> history = new Dictionary<char[], List<GameRecord>>();
        List<(char[] Pegs, GameRecord GameRecord)> activeGameRecords;

        public char? ChooseStartingPeg(Dictionary<char, bool> pegs)
        {
            activeGameRecords = new List<(char[] Pegs, GameRecord GameRecord)>();

            return startingPeg;
        }

        public bool PerformNextJump(Dictionary<char, bool> pegs)
        {
            var remainingPegsBefore = GameInterface.GetRemainingPegs(pegs);
            var jumps = GameInterface.GetPossibleJumps(pegs);
            var lastAttemptJumpIndex = jumps.Length;

            Console.WriteLine($"Pegs Remaining: {new String(remainingPegsBefore)}");
            Console.WriteLine($"Attempted Before: {history.ContainsKey(remainingPegsBefore)}");

            if (history.ContainsKey(remainingPegsBefore)) {
                Console.ReadKey(true);

                var attempts = history[remainingPegsBefore];
                var lastAttempt = attempts[attempts.Count - 1];

                lastAttemptJumpIndex = lastAttempt.JumpList[lastAttempt.JumpList.Count - 1].JumpIndex;
            }

            if (lastAttemptJumpIndex == 0) {
                return false;
            }

            var jump = jumps[lastAttemptJumpIndex - 1];
            GameInterface.PerformJump(pegs, jump);
            var remainingPegsAfter = GameInterface.GetRemainingPegs(pegs);

            activeGameRecords.Add((Pegs: remainingPegsBefore, GameRecord: new GameRecord(new JumpList(), remainingPegsBefore)));

            foreach (var gameRecord in activeGameRecords) {
                gameRecord.GameRecord.JumpList.Add(new JumpRecord(jump, lastAttemptJumpIndex - 1));
                gameRecord.GameRecord.PegsRemaining = remainingPegsAfter;
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

            foreach (var pegsRemaining in history.Keys) {
                Console.WriteLine($"Pegs: {new String(pegsRemaining)} - Records: {new String(history[pegsRemaining][0].PegsRemaining)}");
            }

            return false;
        }

        public void PrintStats()
        {

        }
    }
}
