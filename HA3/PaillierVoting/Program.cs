﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace PaillierVoting
{
    class Program
    {
        private static BigInteger n;

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Paillier Voting");
           var input = new FromConsole();

            var p = input.p;
            var q = input.q;
            var g = input.g;
            var votes = input.votes;

            n = BigInteger.Multiply(p, q);
            var n2 = BigInteger.Multiply(n, n);
            var lambda = lcm(BigInteger.Subtract(p, new BigInteger(1)), BigInteger.Subtract(q, new BigInteger(1)));
            var mu = calcMu(g, lambda, n2);
           

            var c = getEncryptedMessage(votes, n2);

            // Decrypt sum
            var L = Program.L(BigInteger.ModPow(c, lambda, n2));
            var LMu = BigInteger.Multiply(L, mu);
            var decrypted = BigInteger.ModPow(LMu, 1, n);


            var result = decrypted - n;
            Console.WriteLine("Number of votes: {0}", result);
            Console.WriteLine("Press any key to quit");
            Console.ReadKey();
        }

        private static BigInteger getEncryptedMessage(List<BigInteger> votes, BigInteger n2)
        {
            var encryptedVoteProduct = new BigInteger(1);
            encryptedVoteProduct = votes.Aggregate(encryptedVoteProduct, (current, vote) => current*vote);

            var c = BigInteger.ModPow(encryptedVoteProduct, 1, n2);
            return c;
        }

        private static BigInteger calcMu(BigInteger g, int lambda, BigInteger n2)
        {
            var m2 = modInverse(L(BigInteger.ModPow(g, lambda, n2)), n);
            return m2;
        }

        private static int lcm(BigInteger a, BigInteger b)
        {
            int num1, num2; 
            if (a > b)
            {
                num1 = Convert.ToInt32(a.ToString()); num2 = Convert.ToInt32(b.ToString());
            }
            else
            {
                num1 = Convert.ToInt32(b.ToString()); num2 = Convert.ToInt32(a.ToString());
            }

            for (int i = 1; i <= num2; i++)
            {
                if ((num1 * i) % num2 == 0)
                {
                    return i * num1;
                }
            }
            return num2;
        }

        private static BigInteger L(BigInteger x)
        {
            return (x - 1) / n;
        }

        private static BigInteger modInverse(BigInteger a, BigInteger b)
        {
            a %= b;
            for (var x = 1; x < b; x++)
            {
                if (a*x%b == 1)
                {
                    return x;
                }
            }
            return BigInteger.MinusOne;
        }
    }
}
