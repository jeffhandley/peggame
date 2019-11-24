using System;
using System.Collections.Generic;

namespace peggame
{
    class GameInterface
    {
        public static char[] PegChars = {'1', '2', '3', '4', '5', '6', '7', '8', '9', '0', 'A', 'B', 'C', 'D', 'E'};
        public static char[][] Others = new char[][]
        {
            new char[] {'A', 'B', '7', 'C', '8', '4', 'D', '9', '5', '2', 'E', '0', '6', '3', '1'},
            new char[] {'E', '0', 'D', '6', '9', 'C', '3', '5', '8', 'B', '1', '2', '4', '7', 'A'},
            new char[] {'1', '3', '2', '6', '5', '4', '0', '9', '8', '7', 'E', 'D', 'C', 'B', 'A'},
            new char[] {'A', '7', 'B', '4', '8', 'C', '2', '5', '9', 'D', '1', '3', '6', '0', 'E'},
            new char[] {'E', 'D', '0', 'C', '9', '6', 'B', '8', '5', '3', 'A', '7', '4', '2', '1'}
        };

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
            var options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            };

            foreach (var peg in wins.Keys)
            {
                string winsJson = System.Text.Json.JsonSerializer.Serialize(wins[peg], options);
                System.IO.File.WriteAllText($"wins-{peg}.json", winsJson);
            }
        }
    }
}
