using System;
using System.Collections.Generic;

namespace peggame
{
    class Program
    {
        static char[] PegChars = {'1', '2', '3', '4', '5', '6', '7', '8', '9', '0', 'A', 'B', 'C', 'D', 'E'};

        static void Main(string[] args)
        {
            var pegs = new Dictionary<char, bool>();

            for (var i = 0; i < PegChars.Length; i++) {
                pegs[PegChars[i]] = true;
            }

            while (true) {
                PrintPegs(pegs);

                Console.WriteLine();
                Console.Write("Choose the peg to remove: ");

                var peg = ReadPeg(pegs);
                Console.WriteLine(peg);

                if (peg == null) {
                    return;
                }

                pegs[peg.Value] = false;
            }
        }

        static void PrintPegs(Dictionary<char, bool> pegs) {
            Console.Clear();
            Console.WriteLine("             {0}    ", ShowPeg(pegs, 0));
            Console.WriteLine("            {0} {1}   ", ShowPeg(pegs, 1), ShowPeg(pegs, 2));
            Console.WriteLine("           {0} {1} {2}  ", ShowPeg(pegs, 3), ShowPeg(pegs, 4), ShowPeg(pegs, 5));
            Console.WriteLine("          {0} {1} {2} {3} ", ShowPeg(pegs, 6), ShowPeg(pegs, 7), ShowPeg(pegs, 8), ShowPeg(pegs, 9));
            Console.WriteLine("         {0} {1} {2} {3} {4}", ShowPeg(pegs, 10), ShowPeg(pegs, 11), ShowPeg(pegs, 12), ShowPeg(pegs, 13), ShowPeg(pegs, 14));
        }

        static char ShowPeg(Dictionary<char, bool> pegs, int index) {
            char pegChar = PegChars[index];
            bool hasPeg = pegs[pegChar];

            if (hasPeg) {
                return pegChar;
            }

            return '∘';
        }

        static char? ReadPeg(Dictionary<char, bool> pegs) {
            while (true) {
                var key = Console.ReadKey(true);
                var keyChar = Char.ToUpper(key.KeyChar);

                if (Array.IndexOf(PegChars, keyChar) > -1 && pegs[keyChar] == true) {
                    return keyChar;
                } else if (key.Key == ConsoleKey.Escape) {
                    return null;
                }
            }
        }
    }
}
