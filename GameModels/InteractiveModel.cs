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
            GameInterface.PrintJumps(jumps);
            Console.Write("Choose the peg to jump with: ");

            Func<char, bool> CanJumpFrom = (char selectedPeg) => CanJump(jumps, selectedPeg);

            var from = ReadPeg(CanJumpFrom);
            Console.WriteLine(from);

            if (from == null) {
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

            return PerformNextJump(pegs);
        }

        public virtual bool PlayAgain(Dictionary<char, bool> pegs) {
            var pegsRemaining = GameInterface.GetRemainingPegs(pegs).Length;

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
}
