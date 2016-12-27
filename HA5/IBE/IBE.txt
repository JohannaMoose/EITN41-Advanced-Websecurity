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
            var publicId = Console.ReadLine(); 
            Console.Write("What is the prime p?: ");
            p = BigInteger.Parse("0" + Console.ReadLine(), NumberStyles.AllowHexSpecifier); // Obs, måste lägga till 0
            Console.Write("What is the prime q?: ");
            q = BigInteger.Parse("0" + Console.ReadLine(), NumberStyles.AllowHexSpecifier);  // Obs måste lägga till 0
            M = BigInteger.Multiply(p, q);

            var pk = PKG(Encoding.UTF8.GetBytes(publicId));
            Console.WriteLine("Found private key: {0}", pk.Substring(1));

            Console.Write("What are the encrypted bits (one line each)?: ");
            var encryptedBits = new List<string>();
            string line;
            while (!string.IsNullOrWhiteSpace(line = Console.ReadLine()))
            {
                encryptedBits.Add(line);
            }
            
            Console.WriteLine("Decoded messages is: {0}", decodeDecryption(encryptedBits.Select(Decrypt).ToList()));

            Console.WriteLine("Press any key to quit");
            Console.ReadKey();
        }

        /// <returns>
        /// A hexadecimal representation of the key
        /// </returns>
        private static string PKG(byte[] publicId)
        {
            var a = calculateA(publicId.ToArray());

            a = BigInteger.Parse("025a4d152bf555e0f61fb94ac4ee60962decbbe99", NumberStyles.HexNumber);

            var mAdd5 = BigInteger.Add(M, 5);
            var pAddQ = BigInteger.Add(p, q);
            var exponent = BigInteger.Divide(BigInteger.Subtract(mAdd5, pAddQ), new BigInteger(8));

            r = BigInteger.ModPow(a, exponent, M);

            return r.ToString("x");
        }

        private static BigInteger calculateA(byte[] publicId)
        {
            var hashResult = sha1.ComputeHash(publicId);
            var hashOfResult = BitConverter.ToString(hashResult).Replace("-", "");

            while (jacobi(BigInteger.Parse("0" + hashOfResult, NumberStyles.HexNumber), M) != 1)
            {
                hashResult = sha1.ComputeHash(hashResult);
                hashOfResult = BitConverter.ToString(hashResult).Replace("-", "");
            }

            return BigInteger.Parse(hashOfResult, NumberStyles.HexNumber);
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
            var c = BigInteger.Parse("0" + encyptedMsg, NumberStyles.HexNumber);

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
