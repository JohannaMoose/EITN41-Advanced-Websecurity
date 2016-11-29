using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeactivationCodes
{
    class ConsoleInput : Input
    {
        public ConsoleInput()
        {
            Console.Write("What is the value of k?: ");
            k = Convert.ToInt32(Console.ReadLine());

            Console.Write("What is the value of n?: ");
            n = Convert.ToInt32(Console.ReadLine());

            privatePolynomial = new int[k];
            for (int i = 0; i < k; i++)
            {
                if (i == 0)
                {
                    Console.Write("What is the constant in the private polynom?: ");
                    privatePolynomial[0] = Convert.ToInt32(Console.ReadLine());
                }
                else
                {
                    Console.Write("What is the term with x^{0}?: ", i);
                    privatePolynomial[i] = Convert.ToInt32(Console.ReadLine());
                }
            }

            polynomialShares = new List<int>();
            
            for (int i = 2; i <= n; i++)
            {
                Console.Write("What value was shared from participant {0}:", i);
                polynomialShares.Add(Convert.ToInt32(Console.ReadLine()));
            }
            
            sharedSums = new List<int>();

            for (int i = 2; i <= n; i++)
            {
                Console.Write("What sum was shared from participant {0} (enter -1 for no value):", i);
                sharedSums.Add(Convert.ToInt32(Console.ReadLine()));
            }
      
        }

        public int k { get; }
        public int n { get; }
        public int[] privatePolynomial { get; }
        public List<int> polynomialShares { get; }
        public List<int> sharedSums { get; }
    }
}
