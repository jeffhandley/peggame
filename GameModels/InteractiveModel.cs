using System;
using System.Collections.Generic;

namespace peggame
{
    class InteractiveModel : IGameModel
    {
        public virtual bool RemoveStartingPeg(Dictionary<char, bool> pegs)
        {
            Func<char, bool> HasPeg = (char selectedPeg) => Array.IndexOf(GameInterface.PegChars, selectedPeg) >= 0;

            Console.Write("Choose the peg to remove: ");

            var peg = ReadPeg(HasPeg);
            Console.WriteLine(peg);

            if (peg.HasValue) {
                GameInterface.RemovePeg(pegs, peg.Value);
            }

            return peg.HasValue;
        }

        public virtual bool PerformNextJump(Dictionary<char, bool> pegs)
        {
            var jumps = GameInterface.GetPossibleJumps(pegs);
            Console.Write("Choose the peg to jump with: ");

            var left = Console.CursorLeft;
            var top = Console.CursorTop;

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            GameInterface.PrintJumps(jumps);

            Console.SetCursorPosition(left, top);

            Func<char, bool> CanJumpFrom = (char selectedPeg) => CanJump(jumps, selectedPeg);

            var from = ReadPeg(CanJumpFrom);
            Console.WriteLine(from);

            if (from == null) {
                GameInterface.PrintPegs(pegs);

                return false;
            }

            Console.Write("Choose the peg to jump over: ");

            Func<char, bool> CanJumpTo = (char selectedPeg) => CanJump(jumps, from.Value, selectedPeg);

            var over = ReadPeg(CanJumpTo);
            Console.WriteLine(over);

            if (over != null) {
                foreach (var jump in jumps) {
                    if (jump.From == from && jump.Over == over) {
                        GameInterface.PerformJump(pegs, jump);

                        return true;
                    }
                }
            }

            GameInterface.PrintPegs(pegs);

            // Over selection was aborted, ask for the From selection again
            return PerformNextJump(pegs);
        }

        public virtual bool PlayAgain(Dictionary<char, bool> pegs) {
            var pegsRemaining = GameInterface.GetRemainingPegs(pegs).Length;

            if (pegsRemaining == 1) {
                Console.WriteLine("You won!");
            } else {
                Console.WriteLine($"Game Over. Pegs Remaining: {pegsRemaining}");
            }

            Console.WriteLine();
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

        static protected bool CanJump(Jump[] jumps, char from, char? over = (char?)null) {
            foreach (var jump in jumps) {
                if (jump.From == from && (over == null || jump.Over == over.Value)) {
                    return true;
                }
            }

            return false;
        }

        static protected char? ReadPeg(Func<char, bool> isAllowed) {
            while (true) {
                var key = Console.ReadKey(true);
                var keyChar = Char.ToUpper(key.KeyChar);

                if (isAllowed(keyChar) == true) {
                    return keyChar;
                } else if (key.Key == ConsoleKey.Escape) {
                    return null;
                }
            }
        }
    }
}
