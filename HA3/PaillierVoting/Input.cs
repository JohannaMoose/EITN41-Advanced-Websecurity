using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace PaillierVoting
{
    interface Input
    {
        BigInteger p { get; set; }

        BigInteger q { get; set; }

        BigInteger g { get; set; }

        List<BigInteger> votes { get; set; }
    }
}
