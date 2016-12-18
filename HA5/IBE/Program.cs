using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace IBE
{
    class Program
    {
        private static BigInteger p;
        private static BigInteger q;
        private static BigInteger M;
        private static BigInteger r; 
        private static SHA1 sha1;

        static void Main(string[] args)
        {
            sha1 = SHA1.Create();
            Console.WriteLine("Welcome to the IBE program");
            Console.Write("What is the public id? : ");
            var publicId = "walterwhite@crypto.sec"; //Console.ReadLine(); 
            Console.Write("What is the prime p?: ");
            p = BigInteger.Parse("09240633d434a8b71a013b5b00513323f", NumberStyles.AllowHexSpecifier); // Console.ReadLine(); // Obs, måste lägga till 0
            Console.Write("What is the prime q?: ");
            q = BigInteger.Parse("0f870cfcd47e6d5a0598fc1eb7e999d1b", NumberStyles.AllowHexSpecifier); // Console.ReadLine(); // Obs måste lägga till 0
            M = BigInteger.Multiply(p, q);

            var pk = PKG(Encoding.UTF8.GetBytes(publicId));
            Console.WriteLine("Found private key: {0}", pk);

            Console.Write("What are the encrypted bits?: ");
            var encryptedBits = new List<string>
            {
                "83c297bfb0028bd3901ac5aaa88e9f449af50f12c2f43a5f61d9765e7beb2469",
                "519fac1f8ac05fd12f0cbd7aa46793210988a470d27385f6ae10518a0c6f2dd6",
                "2bda0d9c8c78cb5ec2f8c038671ddffc1a96b5d42004104c551e8390fbf4c42e"
            };// Console.ReadLine();

            Console.WriteLine("Decoded messages is: {0}", decodeDecryption(encryptedBits.Select(Decrypt).ToList()));

            Console.WriteLine("Press any key to quit");
            Console.ReadKey();
        }

        /// <returns>
        /// A hexadecimal representation of the key
        /// </returns>
        private static string PKG(byte[] publicId)
        {
            var a = calculateA(pb.ToArray());
            Console.WriteLine("\nExpected a found: {0}", "25a4d152bf555e0f61fb94ac4ee60962decbbe99" == a.ToString("x"));
            Console.WriteLine("Expected {0}, but was {1}", "25a4d152bf555e0f61fb94ac4ee60962decbbe99", a.ToString("x"));

            //a = BigInteger.Parse("025a4d152bf555e0f61fb94ac4ee60962decbbe99", NumberStyles.HexNumber);

            var mAdd5 = BigInteger.Add(M, 5);
            var pAddQ = BigInteger.Add(p, q);
            var exponent = BigInteger.Divide(BigInteger.Subtract(mAdd5, pAddQ), new BigInteger(8));

            r = BigInteger.ModPow(a, exponent, M);

            return r.ToString("x");
        }

        private static BigInteger calculateA(byte[] publicId)
        {
            var hashResult = sha1.ComputeHash(publicId);

            while (jacobi(new BigInteger(hashResult), M) != 1)
            {
                hashResult = sha1.ComputeHash(hashResult);
            }

            return new BigInteger(hashResult);
        }

        private static int jacobi(BigInteger a, BigInteger m)
        {
            var j = 1;
            a = BigInteger.ModPow(a, 1, m);
            while (a != 0)
            {
                var t = 0;
                while ((a & 1) == 0)
                {
                    a = a >> 1;
                    t = t + 1;
                }

                var mod8 = BigInteger.ModPow(m, 1, 8);
                if ((t & 1)==1 && (mod8 == 3 || mod8 == 5))
                    j = -j;

                if (BigInteger.ModPow(a, 1, 4) == 3 && BigInteger.ModPow(m, 1, 4) == 3)
                    j = -j;

                var temp = a;
                a = BigInteger.ModPow(m, 1, a);
                m = temp; 
            }

            return m == 1 ? j : 0; 
        }

        private static int Decrypt(string encyptedMsg)
        {
            var c = BigInteger.Parse(encyptedMsg, NumberStyles.HexNumber);

            var alpha = c + BigInteger.Multiply(2, r);
            var m = jacobi(alpha, M);

            return m; 
        }

        private static int decodeDecryption(IEnumerable<int> decryptedValues)
        {
            var binaryStr = new StringBuilder();
            foreach (var decryptedValue in decryptedValues)
            {
                binaryStr.Append(decryptedValue == -1 ? 0 : 1);
            }

            return Convert.ToInt32(binaryStr.ToString(), 2);
        }
    }
}
