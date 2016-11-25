using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace DiningCryptographers
{
    class ParallellRun
    {
        public void Run()
        {
            Console.WriteLine("Welcome to the dining cryptographers problem!");
            Console.Write("What is the secret with Alice? (SA): ");
            var sa = toBits("0C73");
            var saCopy = new BitArray(sa);

            Console.Write("What is the shared secret with Bob? (SB): ");
            var sb = toBits("80C1");

            Console.Write("What is the boradcasted data sent by Alice? (DA): ");
            var da = toBits("A2A9");

            Console.Write("What is the boradcasted data from Bob? (DB): ");
            var db = toBits("92F5");

            Console.Write("What is the message (M) : ");
            var msg = toBits("9B57");

            Console.Write("What is the bit b? ");
            var b = "0";

            var broadcast = sa.Xor(sb).Xor(msg);

            if (b == "1")
            {
                // Your 16-bit broadcasted in hexadecimal format
                Console.WriteLine("Output: {0}", toHex(broadcast));
            }
            else
            {
                var check = sa.Xor(sb).Xor(broadcast);
                // Your broadcasted data in hex format, 
                //immediately followed by the anonymous message sent by some other party (0000 if no anonymous message was sent)
                Console.WriteLine("Output: {0}", toHex(check));
            }
        }

        private static BitArray toBits(string hexValue)
        {
            var sb = Convert.ToString(Convert.ToInt32(hexValue, 16), 2).PadLeft(16, '0');
            var bitAry = new BitArray(sb.Length);

            for (var i = 0; i < sb.Length; i++)
            {
                bitAry[i] = sb[i] == '1';
            }

            return bitAry;
        }

        private static string toHex(BitArray bits)
        {
            StringBuilder sb = new StringBuilder(bits.Length / 4);

            for (int i = 0; i < bits.Length; i += 4)
            {
                int v = (bits[i] ? 8 : 0) |
                        (bits[i + 1] ? 4 : 0) |
                        (bits[i + 2] ? 2 : 0) |
                        (bits[i + 3] ? 1 : 0);

                sb.Append(v.ToString("x1")); // Or "X1"
            }

            return sb.ToString();
        }

        private static string toBinaryString(BitArray bits)
        {
            var sb = new StringBuilder();
            foreach (bool bit in bits)
            {
                sb.Append(bit ? "1" : "0");
            }
            return sb.ToString();
        }

        public static BitArray concatArrays(BitArray current, BitArray after)
        {
            var bools = new bool[current.Count + after.Count];
            current.CopyTo(bools, 0);
            after.CopyTo(bools, current.Count);
            return new BitArray(bools);
        }
    }
}
