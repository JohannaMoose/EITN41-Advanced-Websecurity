using System;
using System.Collections.Generic;

namespace LuhnAlgorithm
{
    class Program
    {
        static readonly int[] Deltas = { 0, 1, 2, 3, 4, -4, -3, -2, -1, 0 };

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to a Luhn Algorithm to find the missing number");
            Console.WriteLine("Please enter or paste in the number/numbers to analyze: ");
            string line;
            var numbers = new List<string>();
            while (!string.IsNullOrWhiteSpace(line = Console.ReadLine()))
            {
                numbers.Add(line);
            }
          
            var replacementNumers = new List<int>();

            foreach (var number in numbers)
            {
                var replacementNumber = 0;

                while (!NumberIsValid(ConstructPosibility(number, replacementNumber)) && replacementNumber < 10)
                {
                    replacementNumber++;
                }

                if (replacementNumber >= 10)
                    Console.WriteLine(
                        "{0} is not a valid card number and can't be no matter what replacment is made for X", number);
                else
                {
                    Console.WriteLine("X should be replaced with {0}, valid numer is {1}", replacementNumber, ConstructPosibility(number, replacementNumber));
                    replacementNumers.Add(replacementNumber);
                }
            }

            Console.WriteLine("The following is the answer: ");
            foreach (var n in replacementNumers)
            {
                Console.Write(n);
            }

            Console.WriteLine("\nEnter anything for exit");
            var exit = Console.Read();
        }

        private static string ConstructPosibility(string str, int replacement)
        {
            if (str.StartsWith("X"))
                return replacement + str.Substring(1);
            if (str.EndsWith("X"))
                return str.Substring(0, str.Length - 1) + replacement;

            var parts = str.Split(new[] {"X"}, StringSplitOptions.RemoveEmptyEntries);
            return parts[0] + replacement + parts[1];
        }

        private static bool NumberIsValid(string number)
        {
            var checksum = 0;
            var nbr = number.ToCharArray();
            for (var i = nbr.Length - 1; i > -1; i--)
            {
                var j = nbr[i] - 48;
                checksum += j;
                if ((i - nbr.Length) % 2 == 0)
                    checksum += Deltas[j];
            }

            return checksum % 10 == 0;
        }
    }
}
