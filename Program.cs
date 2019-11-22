﻿using System;
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
    }

    struct Jump
    {
        public char From;
        public char To;
        public char Over;
    }

    interface IGameModel
    {
        char? ChooseStartingPeg(Dictionary<char, bool> pegs);
        Jump? ChooseNextJump(Jump[] jumps);
        bool PlayAgain(Dictionary<char, bool> pegs);
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

    class FirstChoiceGameModel : InteractiveGameModel
    {
        public override Jump? ChooseNextJump(Jump[] jumps)
        {
            var jump = jumps[0];

            Console.WriteLine($"Jumping {jump.From} over {jump.Over}");
            System.Threading.Thread.Sleep(1000);

            return jump;
        }
    }

    class LastChoiceGameModel : InteractiveGameModel
    {
        private Dictionary<char, List<Tuple<List<Tuple<Jump, int>>, int>>> history;
        private int nextStartingPeg = 0;
        private char? startingPeg;
        private List<Tuple<Jump, int>> currentPath;
        private List<Tuple<Jump, int>> lastPath;

        public LastChoiceGameModel()
        {
            history = new Dictionary<char, List<Tuple<List<Tuple<Jump, int>>, int>>>();
        }

        public override char? ChooseStartingPeg(Dictionary<char, bool> pegs)
        {
            startingPeg = nextStartingPeg < GameInterface.PegChars.Length ? GameInterface.PegChars[nextStartingPeg] : (char?)null;

            if (startingPeg.HasValue) {
                currentPath = new List<Tuple<Jump, int>>();

                if (history.ContainsKey(startingPeg.Value)) {
                    var paths = history[startingPeg.Value];
                    lastPath = paths.Count > 0 ? paths[paths.Count - 1].Item1 : (List<Tuple<Jump, int>>)null;
                }
            } else {
                currentPath = (List<Tuple<Jump, int>>)null;
                lastPath = (List<Tuple<Jump, int>>)null;
            }

            return startingPeg;
        }

        public override Jump? ChooseNextJump(Jump[] jumps)
        {
            Tuple<Jump, int> thisJump;

            var attempts = 1;

            if (history.ContainsKey(startingPeg.Value)) {
                attempts = history[startingPeg.Value].Count + 1;
            }

            Console.WriteLine($"Starting Peg: {startingPeg}. Attempt: {attempts}.");

            if (lastPath != null) {
                Console.WriteLine("Last Path:");

                foreach (var lastPathJump in lastPath) {
                    Console.WriteLine($"Jumped {lastPathJump.Item1.From} over {lastPathJump.Item1.Over}. Jump index: {lastPathJump.Item2}.");
                }
            }

            // If the last path got as far as our previous jump
            if (lastPath != null && lastPath.Count > currentPath.Count - 1) {
                Jump? lastPathPreviousJump = currentPath.Count > 0 ? lastPath[currentPath.Count - 1].Item1 : (Jump?)null;
                Jump? currentPathPreviousJump = currentPath.Count > 0 ? currentPath[currentPath.Count - 1].Item1 : (Jump?)null;

                // And if the last path's previous jump was the
                // same as our previous jump, then we need to decide
                // what to do on this jump based on the last path
                if (currentPathPreviousJump.HasValue && lastPathPreviousJump.Value.From == currentPathPreviousJump.Value.From && lastPathPreviousJump.Value.Over == currentPathPreviousJump.Value.Over) {
                    // The previous jumps were the same (or this is the first jump)
                    // Now we need to look ahead at the next jump to determine if
                    // this jump should be the same
                    var lastPathThisJumpIndex = lastPath[currentPath.Count].Item2;

                    if (lastPath.Count > currentPath.Count + 1) {
                        var lastPathNextJumpIndex = lastPath[currentPath.Count + 1].Item2;

                        if (lastPathNextJumpIndex > 0) {
                            // There are remaining jumps to decrement on the next
                            // jump, so we will keep this jump the same
                            thisJump = new Tuple<Jump, int>(jumps[lastPathThisJumpIndex], lastPathThisJumpIndex);
                        } else {
                            // There are no remaining jumps to decrement on the next
                            // jump, so we need to decrement this one if possible

                            if (lastPathThisJumpIndex > 0) {
                                thisJump = new Tuple<Jump, int>(jumps[lastPathThisJumpIndex - 1], lastPathThisJumpIndex - 1);
                            } else {
                                throw new Exception("Cannot decrement this jump");
                            }
                        }
                    } else {
                        // The previous jumps were the same, but there is
                        // no future jump in the last path, so this was
                        // the last jump in that path. We will try to
                        // decrement this jump.
                        if (lastPathThisJumpIndex > 0) {
                            thisJump = new Tuple<Jump, int>(jumps[lastPathThisJumpIndex - 1], lastPathThisJumpIndex - 1);
                        } else {
                            throw new Exception("Cannot decrement this jump; there is no future jump");
                        }
                    }
                } else {
                    // The previous jumps were different
                    // We will choose the last option for this jump
                    thisJump = new Tuple<Jump, int>(jumps[jumps.Length - 1], jumps.Length - 1);
                }
            } else {
                // The last path didn't get as far as we have gotten
                // We will choose the last option for this jump
                thisJump = new Tuple<Jump, int>(jumps[jumps.Length - 1], jumps.Length - 1);
            }

            Console.WriteLine("This Path:");

            foreach (var currentPathJump in currentPath) {
                Console.WriteLine($"Jumped {currentPathJump.Item1.From} over {currentPathJump.Item1.Over}");
            }

            currentPath.Add(thisJump);
            var jump = thisJump.Item1;

            Console.WriteLine($"Jumping {jump.From} over {jump.Over}.");

            return jump;
        }

        public override bool PlayAgain(Dictionary<char, bool> pegs) {
            var pegsRemaining = Array.FindAll(GameInterface.PegChars, p => pegs[p] == true).Length;

            history.TryAdd(startingPeg.Value, new List<Tuple<List<Tuple<Jump, int>>, int>>());
            history[startingPeg.Value].Add(new Tuple<List<Tuple<Jump, int>>, int>(currentPath, pegsRemaining));

            Console.WriteLine();
            Console.WriteLine($"Game Over. Pegs Remaining: {pegsRemaining}");

            Console.WriteLine();
            Console.WriteLine($"Starting Peg: {startingPeg}. Attempt: {history[startingPeg.Value].Count}.");

            bool hasMoreMoves = false;

            foreach (var jump in history[startingPeg.Value][history[startingPeg.Value].Count - 1].Item1) {
                Console.WriteLine($"Jumped {jump.Item1.From} over {jump.Item1.Over}. Jump index: {jump.Item2}.");

                if (jump.Item2 > 0) {
                    hasMoreMoves = true;
                }
            }

            if (!hasMoreMoves) {
                nextStartingPeg++;
            }

            return GameInterface.PegChars.Length > nextStartingPeg;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            IGameModel model = new LastChoiceGameModel();
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
                    GameInterface.PrintJumps(jumps);

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
