using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace OAEP2
{
    class Program
    {
        private static HashAlgorithm hashFunction;
        private static int hLen;
        private static int k;

        static void Main(string[] args)
        {
            Console.SetIn(new StreamReader(Console.OpenStandardInput(8192))); // Remove the standard 256 limit of input

            hashFunction = SHA1.Create();
            hLen = hashFunction.HashSize/8;
            k = 1024/8;
            var L = hexToBytes("");
            
            Console.WriteLine("Welcome to the second, full, implementation of OAEP");
            Console.Write("What value is M: ");
            var M = Console.ReadLine();
            Console.Write("What value is seed: ");
            var seed = Console.ReadLine();

            var encrypted = RSAesOaepEncrypt(hexToBytes(M), L, hexToBytes(seed));

            Console.WriteLine("\nEM: {0}", bytesToHex(encrypted));
            
            Console.Write("What value EM to decrypt: ");
            var EM = Console.ReadLine();
            var decrypted = RsaesEmeOaepDecode(hexToBytes(EM), L);
            Console.WriteLine("\n M: {0}", bytesToHex(decrypted));

            Console.WriteLine("Press any key to quit");
            Console.ReadKey();
        }

        private static byte[] RSAesOaepEncrypt(byte[] M, byte[] L, byte[] seed)
        {
            //                           +----------+---------+-------+
            //                      DB = |  lHash   |    PS   |   M   |
            //                           +----------+---------+-------+
            //                                          |
            //                +----------+              V
            //                |   seed   |--> MGF ---> XOR
            //                +----------+              |
            //                      |                   |
            //             +--+     V                   |
            //             |00|    XOR <----- MGF <-----|
            //             +--+     |                   |
            //               |      |                   |
            //               V      V                   V
            //             +--+----------+----------------------------+
            //       EM =  |00|maskedSeed|          maskedDB          |
            //             +--+----------+----------------------------+

            // 1.  Length checking:
            var mLen = M.Length;
            if (mLen > k - 2*hLen - 2)
                //  If the length of L is greater than the input limitation for the hash function(2 ^ 61 - 1 octets for SHA - 1) or If mLen > k - 2hLen - 2
                throw new ArgumentException("C too long."); // output "label too long" and stop.

            //  2.  EME-OAEP encoding 
            var lHash = hashFunction.ComputeHash(L); //  Let lHash = Hash(L), an octet string of length hLen
            var PS = new byte[k - mLen - 2*hLen - 2];
            // Generate a padding string PS consisting of k - mLen -2hLen - 2 zero octets.  The length of PS may be zero.

            /*  Concatenate lHash, PS, a single octet with hexadecimal
              value 0x01, and the message M to form a data block DB of
              length k - hLen - 1 octets as

                 DB = lHash || PS || 0x01 || M.*/
            var _DB = new List<byte>();
            _DB.AddRange(lHash);
            _DB.AddRange(PS);
            _DB.Add(0x01);
            _DB.AddRange(M);
            var DB = _DB.ToArray();

            // Let dbMask = MGF(seed, k - hLen - 1).
            var dbMask = MGF1(seed, k - hLen - 1);

            // Let maskedDB = DB \xor dbMask.
            var maskedDB = Xor(DB, dbMask);
          
            // Let seedMask = MGF(maskedDB, hLen).
            var seedMask = MGF1(maskedDB, hLen);

            // Let maskedSeed = seed \xor seedMask.
            var maskedSeed = Xor(seed, seedMask);
           
            /*  Concatenate a single octet with hexadecimal value 0x00,
              maskedSeed, and maskedDB to form an encoded message EM of
              length k octets as

                 EM = 0x00 || maskedSeed || maskedDB.*/
            var result = new List<byte> {0x00};
            result.AddRange(maskedSeed);
            result.AddRange(maskedDB);

            return result.ToArray();
        }

        private static byte[] hexToBytes(string mgfSeed)
        {
            return Enumerable.Range(0, mgfSeed.Length)
                .Where(x => x%2 == 0)
                .Select(x =>
                        Convert.ToByte(mgfSeed.Substring(x, 2), 16))
                .ToArray();
        }

        private static string bytesToHex(byte[] octets)
        {
            return BitConverter.ToString(octets.ToArray()).Replace("-", "");
        }

        private static byte[] MGF1(byte[] mgfSeed, int maskLen)
        {
            if (maskLen > Math.Pow(2, 32))
                throw new ArgumentException("Mask too long.");

            var result = new List<byte>();
            for (var i = 0; i <= maskLen/hLen; i++)
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

        private static byte[] I2OSP(BigInteger x, int xLen)
        {
            var result = new byte[xLen];
            var index = 0;
            while ((x > 0) && (index < result.Length))
            {
                result[index++] = (byte) (x%256);
                x /= 256;
            }
            Array.Reverse(result);
            return result;
        }

        public static byte[] Xor(byte[] A, byte[] B)
        {
            var R = new byte[A.Length];

            for (var i = 0; i < A.Length; i++)
            {
                R[i] = (byte) (A[i] ^ B[i]);
            }
            return R;
        }

        private static byte[] RsaesEmeOaepDecode(byte[] C, byte[] L)
        {
            var EM = C.ToList();

            /*
             * b.  Separate the encoded message EM into a single octet Y, an
              octet string maskedSeed of length hLen, and an octet
              string maskedDB of length k - hLen - 1 as

                 EM = Y || maskedSeed || maskedDB. 
             */

            var maskedSeed = EM.ToList().GetRange(1, hLen).ToArray();
            var maskedDB = EM.ToList().GetRange(hLen +1, EM.Count - hLen-1).ToArray();

            var seedMask = MGF1(maskedDB, hLen); // Let seedMask = MGF(maskedDB, hLen).

            var seed = Xor(maskedSeed, seedMask); // Let seed = maskedSeed \xor seedMask.

            var dbMask = MGF1(seed, k - hLen - 1); // Let dbMask = MGF(seed, k - hLen - 1).

            var DB = Xor(maskedDB, dbMask).ToList(); // Let DB = maskedDB \xor dbMask.

            /* Separate DB into an octet string lHash' of length hLen, a
              (possibly empty) padding string PS consisting of octets
              with hexadecimal value 0x00, and a message M as

                 DB = lHash' || PS || 0x01 || M.*/

            var lHashD = DB.GetRange(0, hLen);
            var indexOfEndPs = DB.IndexOf(0x01);
            var PS = DB.GetRange(hLen, indexOfEndPs-hLen);
            var M = DB.GetRange(lHashD.Count + PS.Count + 1, DB.Count - lHashD.Count - PS.Count -1);

            // 4.  Output the message M.
            return M.ToArray();
        }
    }
}

