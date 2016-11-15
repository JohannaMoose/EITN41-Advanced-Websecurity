using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;

namespace MicroMintCoins
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("What value is u?: ");
            var u = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("What value is k?: ");
            var k = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("What value is c?: ");
            var c = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("What is the requird confidence interval width?: ");
            var recConfidence = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("Starts simulation");
            var startTime = DateTime.Now;
            var results = new List<int>();

            do
            {
                var bins = new Dictionary<string, List<Guid>>();
                var nbrOfCoins = 0;
                MD5 hashFunction = new MD5CryptoServiceProvider();
                var index = 0;

                while (nbrOfCoins < c)
                {
                    index++;
                    var val = Guid.NewGuid();
                    var hash = hashFunction.ComputeHash(val.ToByteArray());

                    var bin = createBin(hash, u);

                    if (bins.ContainsKey(bin))
                    {
                        List<Guid> l;
                        bins.TryGetValue(bin, out l);
                        l.Add(val);
                        if (l.Count == k)
                        {
                            nbrOfCoins++;
                        }
                    }
                    else
                    {
                        bins.Add(bin, new List<Guid> { val });
                    }
                }
                results.Add(index);
            } while (CILength(results) > recConfidence && (DateTime.Now - startTime).Minutes < 30);

            if ((DateTime.Now - startTime).Minutes >= 30)
                Console.WriteLine("Simulation stoped due to time limit reached");

            var mean = results.Sum() / results.Count;

            Console.WriteLine("The average number of rounds needed was {0}", mean);

            Console.WriteLine("Exit the app by pressing any key");
            Console.ReadLine();
        }

        private static double CILength(IReadOnlyCollection<int> counts)
        {
            if (counts.Count < 3)
                return 9999999999;

            var lambda = 3.66;

            var x = counts.Sum()/counts.Count;
            double sumOfSquaresOfDifferences = counts.Select(val => (val - x) * (val - x)).Sum();
            var s = Math.Sqrt(sumOfSquaresOfDifferences / counts.Count);

            var upperBound = x + lambda*(s/Math.Sqrt(counts.Count));
            var lowerBound = x - lambda * (s / Math.Sqrt(counts.Count));

            var interval = upperBound - lowerBound;
            Console.WriteLine("CI Interval at the n={0} is {1}", counts.Count, interval);
            return interval;
        }

        private static string createBin(byte[] hash, int u)
        {
            var array = new BitArray(hash) { Length = u };
            var bin = new StringBuilder();

            foreach (bool b in array)
            {
                bin.Append(b ? 1 : 0);
            }

            return bin.ToString();
        }
    }
}
