using System;
using System.Collections.Generic;

namespace peggame
{
    class GameInterface
    {
        public static char[] PegChars = {'1', '2', '3', '4', '5', '6', '7', '8', '9', '0', 'A', 'B', 'C', 'D', 'E'};

        public static Dictionary<char, bool> InitializePegs() {
            var pegs = new Dictionary<char, bool>();

            for (var i = 0; i < GameInterface.PegChars.Length; i++) {
                pegs[GameInterface.PegChars[i]] = true;
            }

            return pegs;
        }

        public static void PrintPegs(Dictionary<char, bool> pegs) {
            var output = new System.Text.StringBuilder();

            Console.Clear();
            output.AppendFormat("             {0}    \n", ShowPeg(pegs, 0));
            output.AppendFormat("            {0} {1}   \n", ShowPeg(pegs, 1), ShowPeg(pegs, 2));
            output.AppendFormat("           {0} {1} {2}  \n", ShowPeg(pegs, 3), ShowPeg(pegs, 4), ShowPeg(pegs, 5));
            output.AppendFormat("          {0} {1} {2} {3} \n", ShowPeg(pegs, 6), ShowPeg(pegs, 7), ShowPeg(pegs, 8), ShowPeg(pegs, 9));
            output.AppendFormat("         {0} {1} {2} {3} {4}\n", ShowPeg(pegs, 10), ShowPeg(pegs, 11), ShowPeg(pegs, 12), ShowPeg(pegs, 13), ShowPeg(pegs, 14));
            Console.WriteLine(output);
        }

        public static void PrintJumps(Jump[] jumps) {
            var output = new System.Text.StringBuilder();

            output.Append("Possible Jumps:\n");

            for (var j = 0; j < jumps.Length; j++) {
                var jump = jumps[j];
                output.Append($"  - Jump {jump.From} over {jump.Over}\n");
            }

            Console.WriteLine(output);
        }

        static char ShowPeg(Dictionary<char, bool> pegs, int index) {
            char pegChar = GameInterface.PegChars[index];
            bool hasPeg = pegs[pegChar];

            if (hasPeg) {
                return pegChar;
            }

            return '∘';
        }

        public static void RemovePeg(Dictionary<char, bool> pegs, char peg) {
            pegs[peg] = false;
        }

        public static void PlacePeg(Dictionary<char, bool> pegs, char peg) {
            pegs[peg] = true;
        }

        public static void PerformJump(Dictionary<char, bool> pegs, Jump jump) {
            RemovePeg(pegs, jump.From);
            PlacePeg(pegs, jump.To);
            RemovePeg(pegs, jump.Over);
        }

        public static char[] GetRemainingPegs(Dictionary<char, bool> pegs) {
            return Array.FindAll(PegChars, peg => pegs[peg] == true);
        }

        public static Jump[] GetPossibleJumps(Dictionary<char, bool> pegs) {
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
