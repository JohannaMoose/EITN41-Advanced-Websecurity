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

            var encodedBytes = stringToByteArray(encoded);
            var hexReturn = BitConverter.ToString(encodedBytes).Replace("-", "").ToLower();

            Console.WriteLine("The resulting DER encoding for the string is: {0}", encoded);
            Console.WriteLine("Press any key to quit");
            Console.ReadKey(); 
        }

        private static byte[] stringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private static string derEncode(BigInteger val)
        {
            var tag = "02";
            var valByte = val.ToByteArray();
            var valHex = val.ToString("x");

            if (valHex.Length%2 != 0)
                valHex = "0" + valHex; 

            var lengthHex = createLength(valHex.Length/2);

             return tag + lengthHex + valHex; 
        }

        private static string createLength(int nbrOfBytes)
        {
            var byteLength = calculateLengthInBytes(nbrOfBytes);

            if (nbrOfBytes < 128)
                return byteLength;

            var lenghByte = getRegularLengthValue(nbrOfBytes);
            return lenghByte + byteLength;
        }

        private static string calculateLengthInBytes(int nbrOfBytes)
        {
            var lengthAry = BitConverter.GetBytes(nbrOfBytes).ToList();
            var length = new List<byte>();
            while (lengthAry.Any(x => x != 0x00))
            {
                length.Add(lengthAry.First());
                lengthAry.RemoveAt(0);
            }

            return BitConverter.ToString(length.ToArray()).Replace("-", "");
        }

        private static string getRegularLengthValue(int nbrOfBytes)
        {
            var bitRep = Convert.ToString(nbrOfBytes, 2);
            while (bitRep.Length%8 != 0)
            {
                bitRep = "0" + bitRep;
            }

            var nbr = bitRep.Length/8;

            string s = Convert.ToString(nbr, 2); //Convert to binary in a string

            int[] bits = s.PadLeft(8, '0') // Add 0's from left
                .Select(c => int.Parse(c.ToString())) // convert each char to int
                .ToArray(); // Convert IEnumerable from select to Array
            bits[0] = 1;

            var bitAr = new BitArray(8);
            for (var i = 0; i < 8; i++)
            {
                if (bits[i] == 1)
                    bitAr[i] = true;
            }

            var bytes = ConvertToByte(bitAr);


            return bytes.ToString("x");
        }

        static byte ConvertToByte(BitArray bits)
        {
            if (bits.Count != 8)
            {
                throw new ArgumentException("bits");
            }
            byte[] bytes = new byte[1];
            bits.CopyTo(bytes, 0);
            return bytes[0];
        }
    }
}
