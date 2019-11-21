﻿using System;
using System.Collections.Generic;

namespace peggame
{
    struct Jump
    {
        public char From;
        public char To;
        public char Over;
    }

    class Program
    {
        static char[] PegChars = {'1', '2', '3', '4', '5', '6', '7', '8', '9', '0', 'A', 'B', 'C', 'D', 'E'};

        static void Main(string[] args)
        {
            var pegs = StartNewGame();

            while (true) {
                var jumps = GetPossibleJumps(pegs);

                if (jumps.Length == 0) {
                    var pegsRemaining = Array.FindAll(PegChars, p => pegs[p] == true).Length;

                    Console.WriteLine($"Game Over. Pegs Remaining: {pegsRemaining}");

                    if (!PlayAgain()) {
                        return;
                    }

                    pegs = StartNewGame();
                    jumps = GetPossibleJumps(pegs);
                }

                PrintJumps(jumps);

                var jump = GetNextJump(jumps);

                if (jump == null) {
                    return;
                }

                pegs[jump.Value.From] = false;
                pegs[jump.Value.Over] = false;
                pegs[jump.Value.To] = true;

                PrintPegs(pegs);
            }
        }

        static Dictionary<char, bool> StartNewGame() {
            var pegs = new Dictionary<char, bool>();

            InitializePegs(pegs);
            PrintPegs(pegs);
            RemoveStartingPeg(pegs);
            PrintPegs(pegs);

            return pegs;
        }

        static void InitializePegs(Dictionary<char, bool> pegs) {
            for (var i = 0; i < PegChars.Length; i++) {
                pegs[PegChars[i]] = true;
            }
        }

        static void RemoveStartingPeg(Dictionary<char, bool> pegs) {
            Func<char, bool> HasPeg = (char selectedPeg) => pegs[selectedPeg];

            Console.Write("Choose the peg to remove: ");

            var peg = ReadPeg(HasPeg);
            Console.WriteLine(peg);

            if (peg == null) {
                return;
            }

            pegs[peg.Value] = false;
        }

        static Jump? GetNextJump(Jump[] jumps) {
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

            return null;
        }

        static void PrintPegs(Dictionary<char, bool> pegs) {
            Console.Clear();
            Console.WriteLine("             {0}    ", ShowPeg(pegs, 0));
            Console.WriteLine("            {0} {1}   ", ShowPeg(pegs, 1), ShowPeg(pegs, 2));
            Console.WriteLine("           {0} {1} {2}  ", ShowPeg(pegs, 3), ShowPeg(pegs, 4), ShowPeg(pegs, 5));
            Console.WriteLine("          {0} {1} {2} {3} ", ShowPeg(pegs, 6), ShowPeg(pegs, 7), ShowPeg(pegs, 8), ShowPeg(pegs, 9));
            Console.WriteLine("         {0} {1} {2} {3} {4}", ShowPeg(pegs, 10), ShowPeg(pegs, 11), ShowPeg(pegs, 12), ShowPeg(pegs, 13), ShowPeg(pegs, 14));
            Console.WriteLine();
        }

        static void PrintJumps(Jump[] jumps) {
            Console.WriteLine("Possible Jumps:");

            for (var j = 0; j < jumps.Length; j++) {
                var jump = jumps[j];
                Console.WriteLine($"  {j + 1}. From: {jump.From}; Over: {jump.Over}; To: {jump.To}");
            }

            Console.WriteLine();
        }

        static char ShowPeg(Dictionary<char, bool> pegs, int index) {
            char pegChar = PegChars[index];
            bool hasPeg = pegs[pegChar];

            if (hasPeg) {
                return pegChar;
            }

            return '∘';
        }

        static char? ReadPeg(Func<char, bool> isAllowed) {
            while (true) {
                var key = Console.ReadKey(true);
                var keyChar = Char.ToUpper(key.KeyChar);

                if (Array.IndexOf(PegChars, keyChar) > -1 && isAllowed(keyChar) == true) {
                    return keyChar;
                } else if (key.Key == ConsoleKey.Escape) {
                    return null;
                }
            }
        }

        static bool PlayAgain() {
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
