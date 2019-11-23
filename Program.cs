using System;
using System.Collections.Generic;

namespace peggame
{
    class GameInterface
    {
        public static char[] PegChars = {'1', '2', '3', '4', '5', '6', '7', '8', '9', '0', 'A', 'B', 'C', 'D', 'E'};

        public static void PrintPegs(Dictionary<char, bool> pegs) {
            Console.Clear();
            Console.WriteLine("             {0}    ", ShowPeg(pegs, 0));
            Console.WriteLine("            {0} {1}   ", ShowPeg(pegs, 1), ShowPeg(pegs, 2));
            Console.WriteLine("           {0} {1} {2}  ", ShowPeg(pegs, 3), ShowPeg(pegs, 4), ShowPeg(pegs, 5));
            Console.WriteLine("          {0} {1} {2} {3} ", ShowPeg(pegs, 6), ShowPeg(pegs, 7), ShowPeg(pegs, 8), ShowPeg(pegs, 9));
            Console.WriteLine("         {0} {1} {2} {3} {4}", ShowPeg(pegs, 10), ShowPeg(pegs, 11), ShowPeg(pegs, 12), ShowPeg(pegs, 13), ShowPeg(pegs, 14));
            Console.WriteLine();
        }

        public static void PrintJumps(Jump[] jumps) {
            Console.WriteLine("Possible Jumps:");

            for (var j = 0; j < jumps.Length; j++) {
                var jump = jumps[j];
                Console.WriteLine($"  {j + 1}. Jump {jump.From} over {jump.Over}.");
            }

            Console.WriteLine();
        }

        static char ShowPeg(Dictionary<char, bool> pegs, int index) {
            char pegChar = GameInterface.PegChars[index];
            bool hasPeg = pegs[pegChar];

            if (hasPeg) {
                return pegChar;
            }

            return '∘';
        }

        public static void WriteWins(Dictionary<char, List<History.GameRecord>> wins)
        {
            var winData = new List<History.GameRecord>();

            foreach (var peg in wins.Keys)
            {
                winData.AddRange(wins[peg]);
            }

            var options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            };

            string winsJson = System.Text.Json.JsonSerializer.Serialize(winData, options);
            System.IO.File.WriteAllText("wins.json", winsJson);
        }
    }

    struct Jump
    {
        public char From {get; set;}
        public char To {get; set;}
        public char Over {get; set;}

        public override bool Equals(object obj) {
            if (!(obj is Jump)) {
                return false;
            }

            Jump jump = (Jump)obj;

            if (jump.From == this.From && jump.Over == this.Over) {
                return true;
            }

            return false;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }

    interface IGameModel
    {
        char? ChooseStartingPeg(Dictionary<char, bool> pegs);
        Jump? ChooseNextJump(Jump[] jumps);
        bool PlayAgain(Dictionary<char, bool> pegs);
        void PrintStats();
    }

    class InteractiveGameModel : IGameModel
    {
        public virtual char? ChooseStartingPeg(Dictionary<char, bool> pegs)
        {
            Func<char, bool> HasPeg = (char selectedPeg) => pegs[selectedPeg];

            Console.Write("Choose the peg to remove: ");

            var peg = ReadPeg(HasPeg);
            Console.WriteLine(peg);

            return peg;
        }

        public virtual Jump? ChooseNextJump(Jump[] jumps)
        {
            Console.Write("Choose where to jump from: ");

            Func<char, bool> CanJumpFrom = (char selectedPeg) => CanJump(jumps, selectedPeg);

            var from = ReadPeg(CanJumpFrom);
            Console.WriteLine(from);

            if (from == null) {
                return null;
            }

            Console.Write("Choose where to jump over: ");

            Func<char, bool> CanJumpTo = (char selectedPeg) => CanJump(jumps, from.Value, selectedPeg);

            var over = ReadPeg(CanJumpTo);
            Console.WriteLine(over);

            if (over != null) {
                foreach (var jump in jumps) {
                    if (jump.From == from && jump.Over == over) {
                        return jump;
                    }
                }
            }

            return ChooseNextJump(jumps);
        }

        public virtual bool PlayAgain(Dictionary<char, bool> pegs) {
            var pegsRemaining = Array.FindAll(GameInterface.PegChars, p => pegs[p] == true).Length;

            Console.WriteLine();
            Console.WriteLine($"Game Over. Pegs Remaining: {pegsRemaining}");

            Console.Write("Play Again? [y/n] ");

            while (true) {
                var answer = Console.ReadKey(true);

                if (answer.Key == ConsoleKey.Escape || Char.ToUpper(answer.KeyChar) == 'Y' || Char.ToUpper(answer.KeyChar) == 'N') {
                    if (Char.ToUpper(answer.KeyChar) == 'Y') {
                        Console.WriteLine('Y');
                        return true;
                    }

                    Console.WriteLine('N');

                    return false;
                }
            }
        }

        public virtual void PrintStats() {
        }

        static bool CanJump(Jump[] jumps, char from, char? over = (char?)null) {
            foreach (var jump in jumps) {
                if (jump.From == from && (over == null || jump.Over == over.Value)) {
                    return true;
                }
            }

            return false;
        }

        static char? ReadPeg(Func<char, bool> isAllowed) {
            while (true) {
                var key = Console.ReadKey(true);
                var keyChar = Char.ToUpper(key.KeyChar);

                if (Array.IndexOf(GameInterface.PegChars, keyChar) > -1 && isAllowed(keyChar) == true) {
                    return keyChar;
                } else if (key.Key == ConsoleKey.Escape) {
                    return null;
                }
            }
        }
    }

    class LastChoiceGameModel : InteractiveGameModel
    {
        protected Dictionary<char, List<History.GameRecord>> history;
        protected int nextStartingPeg = 0;
        protected char? startingPeg;
        protected History.JumpList currentPath;
        protected History.JumpList lastPath;
        protected Dictionary<char, List<History.GameRecord>> wins;
        private int? maxAttemptsPerPeg;

        public LastChoiceGameModel()
        {
            history = new Dictionary<char, List<History.GameRecord>>();
            wins = new Dictionary<char, List<History.GameRecord>>();
        }

        public LastChoiceGameModel(string[] args) : this()
        {
            var maxArg = Array.IndexOf(args, "-m");

            if (maxArg >= 0 && args.Length > maxArg + 1) {
                int maxAttempts = 0;
                int.TryParse(args[maxArg + 1], out maxAttempts);

                if (maxAttempts > 0) {
                    maxAttemptsPerPeg = maxAttempts;
                }
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

        public override Jump? ChooseNextJump(Jump[] jumps)
        {
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

            Console.WriteLine($"Jumping {thisJump.From} over {thisJump.Over}.");

            return thisJump.Jump;
        }

        public override bool PlayAgain(Dictionary<char, bool> pegs) {
            var pegsRemaining = Array.FindAll(GameInterface.PegChars, p => pegs[p] == true).Length;

            history[startingPeg.Value].Add(new History.GameRecord(startingPeg.Value, currentPath, pegsRemaining));

            if (pegsRemaining == 1) {
                wins[startingPeg.Value].Add(new History.GameRecord(startingPeg.Value, currentPath, pegsRemaining));
                Console.WriteLine("Game won!");
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

            return GameInterface.PegChars.Length > nextStartingPeg;
        }

        public override void PrintStats() {
            Console.WriteLine();
            Console.WriteLine("Last Path:".PadRight(35) + "This Path:");

            var pathLength = Math.Max(Math.Max(lastPath != null ? lastPath.Count : 0, currentPath.Count), history.Keys.Count);
            var pegStats = new List<string>();

            foreach (var peg in history.Keys) {
                decimal winPercent = history[peg].Count > 0 ? ((decimal)wins[peg].Count / (decimal)history[peg].Count) : (decimal)0.00;
                pegStats.Add($"Starting Peg: {peg}. Attempts: {history[peg].Count.ToString("N0")}. Wins: {wins[peg].Count.ToString("N0")} ({winPercent.ToString("P2")}).");
            }

            for (var i = 0; i < pathLength; i++) {
                var lastJump = lastPath != null && lastPath.Count > i ? $"Jumped {lastPath[i].From} over {lastPath[i].Over}. Jump index: {lastPath[i].JumpIndex}." : "";
                var currentJump = currentPath.Count > i ? $"Jumped {currentPath[i].From} over {currentPath[i].Over}. Jump index: {currentPath[i].JumpIndex}." : "";
                var pegStat = pegStats.Count > i ? pegStats[i] : "";

                Console.WriteLine(lastJump.PadRight(35) + currentJump.PadRight(35) + pegStat);
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

    class FindFirstWinGameModel : LastChoiceGameModel
    {
        public override bool PlayAgain(Dictionary<char, bool> pegs) {
            var pegsRemaining = Array.FindAll(GameInterface.PegChars, p => pegs[p] == true).Length;

            history[startingPeg.Value].Add(new History.GameRecord(startingPeg.Value, currentPath, pegsRemaining));

            if (pegsRemaining == 1) {
                wins[startingPeg.Value].Add(new History.GameRecord(startingPeg.Value, currentPath, pegsRemaining));
                Console.WriteLine("Game won!");
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
                    }
                } else {
                    Console.WriteLine("No wins");
                }

                Console.WriteLine();
            }

            return false;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            IGameModel model;

            if (Array.IndexOf(args, "-a") >= 0) {
                model = new LastChoiceGameModel(args);
            } else if (Array.IndexOf(args, "-1") >= 0) {
                model = new FindFirstWinGameModel();
            } else {
                model = new InteractiveGameModel();
            }

            Dictionary<char, bool> pegs;

            do {
                pegs = new Dictionary<char, bool>();

                InitializePegs(pegs);
                GameInterface.PrintPegs(pegs);

                var peg = model.ChooseStartingPeg(pegs);

                if (peg == null) {
                    return;
                }

                pegs[peg.Value] = false;

                GameInterface.PrintPegs(pegs);

                var jumps = GetPossibleJumps(pegs);

                do {
                    model.PrintStats();
                    GameInterface.PrintJumps(jumps);

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

                    var jump = model.ChooseNextJump(jumps);

                    if (jump == null) {
                        break;
                    }

                    pegs[jump.Value.From] = false;
                    pegs[jump.Value.Over] = false;
                    pegs[jump.Value.To] = true;

                    GameInterface.PrintPegs(pegs);

                    jumps = GetPossibleJumps(pegs);
                }
                while (jumps.Length > 0);
            }
            while (model.PlayAgain(pegs));
        }

        static void InitializePegs(Dictionary<char, bool> pegs) {
            for (var i = 0; i < GameInterface.PegChars.Length; i++) {
                pegs[GameInterface.PegChars[i]] = true;
            }
        }

        static Jump[] GetPossibleJumps(Dictionary<char, bool> pegs) {
            var jumps = new Jump[] {
                new Jump {From = '1', To = '4', Over = '2'},
                new Jump {From = '1', To = '6', Over = '3'},

                new Jump {From = '2', To = '7', Over = '4'},
                new Jump {From = '2', To = '9', Over = '5'},

                new Jump {From = '3', To = '8', Over = '5'},
                new Jump {From = '3', To = '0', Over = '6'},

                new Jump {From = '4', To = '1', Over = '2'},
                new Jump {From = '4', To = '6', Over = '5'},
                new Jump {From = '4', To = 'A', Over = '7'},
                new Jump {From = '4', To = 'C', Over = '8'},

                new Jump {From = '5', To = 'B', Over = '8'},
                new Jump {From = '5', To = 'D', Over = '9'},

                new Jump {From = '6', To = '1', Over = '3'},
                new Jump {From = '6', To = '4', Over = '5'},
                new Jump {From = '6', To = 'C', Over = '9'},
                new Jump {From = '6', To = 'E', Over = '0'},

                new Jump {From = '7', To = '2', Over = '4'},
                new Jump {From = '7', To = '9', Over = '8'},

                new Jump {From = '8', To = '3', Over = '5'},
                new Jump {From = '8', To = '0', Over = '9'},

                new Jump {From = '9', To = '2', Over = '5'},
                new Jump {From = '9', To = '7', Over = '8'},

                new Jump {From = '0', To = '3', Over = '6'},
                new Jump {From = '0', To = '8', Over = '9'},

                new Jump {From = 'A', To = '4', Over = '7'},
                new Jump {From = 'A', To = 'C', Over = 'B'},

                new Jump {From = 'B', To = '5', Over = '8'},
                new Jump {From = 'B', To = 'D', Over = 'C'},

                new Jump {From = 'C', To = '4', Over = '8'},
                new Jump {From = 'C', To = '6', Over = '9'},
                new Jump {From = 'C', To = 'A', Over = 'B'},
                new Jump {From = 'C', To = 'E', Over = 'D'},

                new Jump {From = 'D', To = '5', Over = '9'},
                new Jump {From = 'D', To = 'B', Over = 'C'},

                new Jump {From = 'E', To = '6', Over = '0'},
                new Jump {From = 'E', To = 'C', Over = 'D'}
            };

            return Array.FindAll(jumps, jump =>
                pegs[jump.From] == true &&
                pegs[jump.Over] == true &&
                pegs[jump.To] == false
            );
        }
    }
}
