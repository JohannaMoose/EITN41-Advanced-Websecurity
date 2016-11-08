using System;

namespace LuhnAlgorithm
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to a Luhn Algorithm to find the missing number");
            Console.WriteLine("Please enter or paste in the number/numbers to analyze: ");
            var input = Console.ReadLine();
            var numbers = input.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
