using System.Collections.Generic;

namespace DeactivationCodes
{
    internal class ExampleInput : Input
    {
        public int k => 5;

        public int n => 8;
        public int[] privatePolynomial => new[] {13, 8, 11, 1, 5};

        public List<int> polynomialShares => new List<int>
        {
            75,
            75,
            54,
            52,
            77,
            54, 
            43
        };

        public List<int> sharedSums => new List<int> {2782, -1, 30822, 70960, -1, 256422, -1};
    }
}
