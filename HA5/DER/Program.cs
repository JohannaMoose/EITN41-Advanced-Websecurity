using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;

namespace DER
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.SetIn(new StreamReader(Console.OpenStandardInput(8192)));
            Console.WriteLine("Welcome to the large integer DER encoder");
            Console.Write("What is the integer to encode?: ");
            var intVal = Console.ReadLine();
            var toEncode = BigInteger.Parse(intVal);

            var encoded = derEncode(toEncode);

            Console.WriteLine("The resulting DER encoding for the string is: {0}", encoded);
            Console.WriteLine("Press any key to quit");
            Console.ReadKey(); 
        }

        private static string derEncode(BigInteger val)
        {
            var tag = "02";
            var valHex = val.ToString("x");

            if (valHex.Length%2 != 0)
                valHex = "0" + valHex; 

            var lengthHex = lengthValue(valHex.Length/2);

             return tag + lengthHex + valHex; 
        }

        private static string lengthValue(BigInteger nbrOfBytesOfData)
        {
            var b = nbrOfBytesOfData.ToByteArray().ToList();
            b.Reverse();
            if (b.Last() == 0x00 && nbrOfBytesOfData != 256)
                b.RemoveAt(b.Count - 1);
            else if (b.First() == 0x00)
                b.RemoveAt(0);

            if (nbrOfBytesOfData >= 128)
            {
                var lengthBit = new BitArray(8) { [0] = true };
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
    }
}
