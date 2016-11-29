using System.Collections.Generic;

namespace DeactivationCodes
{
    interface Input
    {
        int k { get; }

        int n { get; }

        int[] privatePolynomial { get; }

        List<int> polynomialShares { get; }

        List<int> sharedSums { get;  }
    }
}
