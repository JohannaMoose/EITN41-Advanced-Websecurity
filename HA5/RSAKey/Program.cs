using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace RSAKey
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.SetIn(new StreamReader(Console.OpenStandardInput(8192)));
            var e = BigInteger.Parse("65537");
            Console.WriteLine("Welcome to the RSA Key");
            Console.Write("What is the value p?: ");
            var p = BigInteger.Parse("139721121696950524826588106850589277149201407609721772094240512732263435522747938311240453050931930261483801083660740974606647762343797901776568952627044034430252415109426271529273025919247232149498325412099418785867055970264559033471714066901728022294156913563009971882292507967574638004022912842160046962763"); //Console.ReadLine());
            Console.Write("What is the value q?: ");
            var q =
                BigInteger.Parse(
                    "141482624370070397331659016840167171669762175617573550670131965177212458081250216130985545188965601581445995499595853199665045326236858265192627970970480636850683227427420000655754305398076045013588894161738893242561531526805416653594689480170103763171879023351810966896841177322118521251310975456956247827719");//Console.ReadLine());

            var sequence = createSequence(p, q, e);

            Console.WriteLine("Found DER: {0}", BitConverter.ToString(sequence.ToArray()).Replace("-", "").ToLower());
            Console.WriteLine("\nEncoded string: {0}", Convert.ToBase64String(sequence.ToArray()));

            Console.WriteLine("Press any key to quit");
            Console.ReadKey();
        }

        private static List<byte> createSequence(BigInteger p, BigInteger q, BigInteger e)
        {
            var n = BigInteger.Multiply(p, q);
            var phi = BigInteger.Multiply(BigInteger.Subtract(p, 1), BigInteger.Subtract(q, 1));
            var d = modInverse(e, phi);
            var dP = BigInteger.ModPow(d, 1, BigInteger.Subtract(p, 1));
            var dQ = BigInteger.ModPow(d, 1, BigInteger.Subtract(q, 1));
            var q_inv = modInverse(q, p);

            var sequenceData = new List<byte>();
            sequenceData.AddRange(derEncode(0));
            sequenceData.AddRange(derEncode(n));
            sequenceData.AddRange(derEncode(e));
            sequenceData.AddRange(derEncode(d));
            sequenceData.AddRange(derEncode(p));
            sequenceData.AddRange(derEncode(q));
            sequenceData.AddRange(derEncode(dP));
            sequenceData.AddRange(derEncode(dQ));
            sequenceData.AddRange(derEncode(q_inv));

            var length = stringToByteArray(lengthValue(sequenceData.Count));
            Console.WriteLine("Debug: Sequence data length: {0}, encoded as {1}", sequenceData.Count, BitConverter.ToString(length).Replace("-", ""));
            var sequence = new List<byte> {0x30};
            sequence.AddRange(length);
            sequence.AddRange(sequenceData);

            Console.WriteLine("Debug: Sequence hex {0}", BitConverter.ToString(sequence.ToArray()).Replace("-", ""));

            return sequence;
        }

        private static byte[] derEncode(BigInteger val)
        {
            var tag = "02";
            var valHex = val.ToString("x");

            if (valHex.Length % 2 != 0)
                valHex = "0" + valHex;

            var lengthHex = lengthValue(valHex.Length / 2);
            Console.WriteLine("Lenght: {0}", lengthHex);

            
            if (lengthHex.Length > 4 && lengthHex.Substring(2, 2) == "00")
            {
                lengthHex = lengthHex.Replace("00", "");
            }

            return stringToByteArray(tag + lengthHex + valHex);
        }

        private static string lengthValue(BigInteger nbrOfBytesOfData)
        {
            var b = nbrOfBytesOfData.ToByteArray().ToList();
            b.Reverse();
            if(b.Last() == 0x00 && nbrOfBytesOfData != 256)
                b.RemoveAt(b.Count-1);
            else if(b.First() == 0x00)
                b.RemoveAt(0);

            if (nbrOfBytesOfData >= 128)
            {
                var lengthBit = new BitArray(8) {[0] = true};
                var bitRepOfNbrOfBytes = Convert.ToString(b.Count, 2);
                for (int i = 0; i < bitRepOfNbrOfBytes.Length; i++)
                {
                    if (bitRepOfNbrOfBytes[i] == '1')
                        lengthBit[i + 8 - bitRepOfNbrOfBytes.Length] = true;
                }
                b.Insert(0, convertToBytes(lengthBit)[0]);
            }

            return BitConverter.ToString(b.ToArray()).Replace("-", ""); 
        }

        static byte[] convertToBytes(BitArray bits)
        {
            var numBytes = bits.Count / 8;
            if (bits.Count % 8 != 0) numBytes++;

            var bytes = new byte[numBytes];
            int byteIndex = 0, bitIndex = 0;

            for (var i = 0; i < bits.Count; i++)
            {
                if (bits[i])
                    bytes[byteIndex] |= (byte)(1 << (7 - bitIndex));

                bitIndex++;
                if (bitIndex == 8)
                {
                    bitIndex = 0;
                    byteIndex++;
                }
            }

            return bytes;
        }

        private static byte[] stringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private static BigInteger modInverse(BigInteger a, BigInteger n)
        {
            BigInteger i = n, v = 0, d = 1;
            while (a > 0)
            {
                BigInteger t = i / a, x = a;
                a = i % x;
                i = x;
                x = d;
                d = v - t * x;
                v = x;
            }
            v %= n;
            if (v < 0) v = (v + n) % n;
            return v;
        }
    }
}
