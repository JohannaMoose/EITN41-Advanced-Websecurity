using System.Collections.Generic;
using System.Numerics;

namespace PaillierVoting
{
    class ExampleInput : Input
    {
        public ExampleInput()
        {
            p = 5;
            q = 7;
            g = p*q + 1;
            votes = new List<BigInteger>
            {
                new BigInteger(929),
                new BigInteger(296),
                new BigInteger(428)
            };
        }

        public BigInteger p { get; set; }
        public BigInteger q { get; set; }
        public BigInteger g { get; set; }
        public List<BigInteger> votes { get; set; }
    }
}
