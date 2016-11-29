using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Interpolation;

namespace DeactivationCodes
{
    class Program
    {
        private static int[] privatePolynomial;
        private static int k;

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the program that will stop WW3");
            var inputs = new ConsoleInput();

            k = inputs.k; // Number that need to coperate to solve it
            privatePolynomial = inputs.privatePolynomial; // My private poly, starting with constant and ending in highest factor
            var polynomailShares = inputs.polynomialShares; //Other participants value of f_i(1)
            var sharedSums = inputs.sharedSums; // The shared points in the master plynomial

            polynomailShares.Insert(0, calcPol(1)); // Insert my value of f_1(1)
            var sharedX = new List<double> {1};
            var sharedY = new List<double>();
            for (int j = 2; j < sharedSums.Count+2; j++)
            {
                if (sharedSums[j-2] == -1) continue;
                sharedX.Add(j);
                sharedY.Add(sharedSums[j-2]);
            }

            var sumPoint = polynomailShares.Sum();
            sharedY.Insert(0,sumPoint);

            var key = new NevillePolynomialInterpolation(sharedX.ToArray(),
              sharedY.ToArray());

            Console.WriteLine("Got the deactivation code: {0}", key.Interpolate(0));
            Console.WriteLine("Press any key to quit");
            Console.ReadKey();
        }

        private static int calcPol(int x)
        {
            var sum = Convert.ToDouble(privatePolynomial[0]);
            for (int i = 1; i < privatePolynomial.Length; i++)
            {
                sum += privatePolynomial[i]*Math.Pow(x, i);
            }

            return Convert.ToInt32(sum);
        }
    }
}
