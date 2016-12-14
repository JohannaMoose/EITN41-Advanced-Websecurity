using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace OAEP1
{
    class Program
    {
        private static HashAlgorithm hashFunction;
        private static int hLen;

        static void Main(string[] args)
        {
            hashFunction = SHA1.Create();
            hLen = hashFunction.HashSize / 8;

            Console.WriteLine("Welcome to the first implementation of OAEP");
            Console.Write("What value is mgfSeed: ");
            var mgfSeed = Console.ReadLine();
            Console.Write("What value is maskLen: ");
            var maskLen = Convert.ToInt32(Console.ReadLine());

            var seed2 = hexToBytes(mgfSeed);

            var mask = MGF1(seed2, maskLen);
            Console.WriteLine("\nThe generated mask is: \t {0}", BitConverter.ToString(mask.Take(maskLen).ToArray()).Replace("-", ""));
            Console.WriteLine("Press any key to quit");
            Console.ReadKey();
        }

        private static byte[] hexToBytes(string mgfSeed)
        {
            return Enumerable.Range(0, mgfSeed.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(mgfSeed.Substring(x, 2), 16))
                .ToArray();
        }

        private static byte[] MGF1(byte[] mgfSeed, int maskLen)
        {
            if (maskLen > Math.Pow(2, 32))
                throw new ArgumentException("Mask too long.");

            var result = new List<byte>();
            for (var i = 0; i <= maskLen / hLen; i++)
            {
                var data = new List<byte>();
                data.AddRange(mgfSeed);
                data.AddRange(I2OSP(i, 4));
                result.AddRange(hashFunction.ComputeHash(data.ToArray()));
            }

            if (maskLen <= result.Count)
                return result.GetRange(0, maskLen).ToArray();

            throw new ArgumentException("Invalid Mask Length.");
        }

        private static byte[] I2OSP(int x, int xLen)
        {
            var result = new byte[xLen];
            var index = 0;
            while ((x > 0) && (index < result.Length))
            {
                result[index++] = (byte)(x % 256);
                x /= 256;
            }
            Array.Reverse(result);
            return result;
        }
    }
}
