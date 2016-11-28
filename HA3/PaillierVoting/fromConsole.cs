using System;
using System.Collections.Generic;
using System.Numerics;

namespace PaillierVoting
{
    class FromConsole : Input
    {
        public FromConsole()
        {
            Console.Write("What is the number p? :");
            p = Convert.ToInt32(Console.ReadLine());
           Console.Write("What is the number q? :");
            q = Convert.ToInt32(Console.ReadLine());
            Console.Write("What value is g?: ");
            g = new BigInteger(Convert.ToDouble(Console.ReadLine()));

            Console.WriteLine("Please enter or paste in the votes to analyze, one per line. Press enter twice when done: ");
            string line;
            votes = new List<BigInteger>();
            
            while (!string.IsNullOrWhiteSpace(line = Console.ReadLine()))
            {
                votes.Add(new BigInteger(Convert.ToDouble(line)));
            }
        }

        public BigInteger p { get; set; }
        public BigInteger q { get; set; }
        public BigInteger g { get; set; }
        public List<BigInteger> votes { get; set; }
    }
}
