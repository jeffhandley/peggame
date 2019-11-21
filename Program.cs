using System;

namespace peggame
{
    class Program
    {
        static char[] PegChars = {'1', '2', '3', '4', '5', '6', '7', '8', '9', '0', 'A', 'B', 'C', 'D', 'E'};

        static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("             1    ");
            Console.WriteLine("            2 3   ");
            Console.WriteLine("           4 5 6  ");
            Console.WriteLine("          7 8 9 0 ");
            Console.WriteLine("         A B C D E");

            Console.WriteLine();
            Console.Write("Choose the peg to remove: ");

            var peg = ReadPeg();
            Console.WriteLine(peg);

            if (peg == null) {
                return;
            }
        }

        static char? ReadPeg() {
            while (true) {
                var key = Console.ReadKey(true);
                var keyChar = Char.ToUpper(key.KeyChar);

                if (Array.IndexOf(PegChars, keyChar) > -1) {
                    return keyChar;
                } else if (key.Key == ConsoleKey.Escape) {
                    return null;
                }
            }
        }
    }
}
