using System;
using System.Numerics;
using System.Security.Cryptography;

namespace RSAKey2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the RSA recovery program");
            var n = BigInteger.Parse("139451183963871661852493509702550055888772127943475012196720868913821321186788862229937379878301796229442092322123475565777962005276614324512542879210090758119759298328704618790175627414149459990262563667327453343296812385352598281614810625090599579749746759760890557708919757915886273418864964285273247627959");
            var e = BigInteger.Parse("65537");
            var d =
                BigInteger.Parse("97035979111221694229356344139648102993473235782030168979539666727177265783733342530447515369511948144576833774688176003372429188585596095809976942441427392990151026528187482014094117335842544863877086096454548134790624549504097689049787678262608825586139911442930809194643080765014416542915984075625821257441");
            var p =
                BigInteger.Parse("12710704528284626658156805778821852382103996398260119866833530161861047758694507696668550948760328371763601085231764022077721271789239494299392005238563967");
            var q =
                BigInteger.Parse("10971495673542452647230717494161230638467424498902156978693581279774991083067154896487136489032000184434746556548127060645863633294998522190328312852934089");
            var dP =
                BigInteger.Parse("10449282510191049795711036894358681977976059538109167005895755138785535958240139023351870233095201362367937740040461879815697834203409619518262097231153559");
            var dq =
                BigInteger.Parse("5358433152551182146460179830524935079055576595829229909425578674688159582935036604158876127087542333390105860536680820866881352129427845312846615466022465");
            var qInv =
                BigInteger.Parse("293850525884800098939874471896584900656239343004543921433799219061333103951726554625988684331746104214985079136614128680288250787842032235537287760514941");


            Console.WriteLine("Calculating from p and q");
            Console.WriteLine("n: {0}", BigInteger.Multiply(p, q));
            Console.WriteLine("Same as given: {0}", n == BigInteger.Multiply(p, q));
          
            var phi = BigInteger.Multiply(BigInteger.Subtract(p, 1), BigInteger.Subtract(q, 1));
            Console.WriteLine("d: {0}", modInverse(e, phi));
            Console.WriteLine("Same as given: {0}", d == modInverse(e, phi));

            Console.WriteLine("dP: {0}", BigInteger.ModPow(d, 1, BigInteger.Subtract(p, 1)));
            Console.WriteLine("Same as given: {0}", dP == BigInteger.ModPow(d, 1, BigInteger.Subtract(p, 1)));

            Console.WriteLine("dQ: {0}", BigInteger.ModPow(d, 1, BigInteger.Subtract(q, 1)));
            Console.WriteLine("Same as given: {0}", dq == BigInteger.ModPow(d, 1, BigInteger.Subtract(q, 1)));

            Console.WriteLine("q_inv: {0}", modInverse(q, p));
            Console.WriteLine("Same as given: {0}", qInv == modInverse(q, p));

           
            Console.WriteLine("Calculating from n, d and e");
            try
            {
                var calcP = new BigInteger();
                var calcQ = new BigInteger();
                RecoverPQ(n, q, d, out calcP, out calcQ);

                Console.WriteLine("p: {0}", calcP);
                Console.WriteLine("Same as given: {0}", p == calcP);

                Console.WriteLine("q: {0}", calcQ);
                Console.WriteLine("Same as given: {0}", q == calcQ);

                Console.WriteLine("dP: {0}", BigInteger.ModPow(d, 1, BigInteger.Subtract(calcP, 1)));
                Console.WriteLine("Same as given: {0}", dP == BigInteger.ModPow(d, 1, BigInteger.Subtract(calcP, 1)));

                Console.WriteLine("dQ: {0}", BigInteger.ModPow(d, 1, BigInteger.Subtract(calcQ, 1)));
                Console.WriteLine("Same as given: {0}", dq == BigInteger.ModPow(d, 1, BigInteger.Subtract(calcQ, 1)));

                Console.WriteLine("q_inv: {0}", modInverse(calcQ, calcP));
                Console.WriteLine("Same as given: {0}", qInv == modInverse(calcQ, calcP));
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to calculate p and q from n, d, and e");
            }

            Console.WriteLine("Press any key to quit");
            Console.ReadKey();
        }

        public static void RecoverPQ(
       BigInteger n,
       BigInteger e,
       BigInteger d,
       out BigInteger p,
       out BigInteger q
       )
        {
            int nBitCount = (int)(BigInteger.Log(n, 2) + 1);

            // Step 1: Let k = de – 1. If k is odd, then go to Step 4
            BigInteger k = d * e - 1;
            if (k.IsEven)
            {
                // Step 2 (express k as (2^t)r, where r is the largest odd integer
                // dividing k and t >= 1)
                BigInteger r = k;
                BigInteger t = 0;

                do
                {
                    r = r / 2;
                    t = t + 1;
                } while (r.IsEven);

                // Step 3
                var rng = new RNGCryptoServiceProvider();
                bool success = false;
                BigInteger y = 0;

                for (int i = 1; i <= 100; i++)
                {

                    // 3a
                    BigInteger g;
                    do
                    {
                        byte[] randomBytes = new byte[nBitCount / 8 + 1]; // +1 to force a positive number
                        rng.GetBytes(randomBytes);
                        randomBytes[randomBytes.Length - 1] = 0;
                        g = new BigInteger(randomBytes);
                    } while (g >= n);

                    // 3b
                    y = BigInteger.ModPow(g, r, n);

                    // 3c
                    if (y == 1 || y == n - 1)
                    {
                        // 3g
                        continue;
                    }

                    // 3d
                    BigInteger x;
                    for (BigInteger j = 1; j < t; j = j + 1)
                    {
                        // 3d1
                        x = BigInteger.ModPow(y, 2, n);

                        // 3d2
                        if (x == 1)
                        {
                            success = true;
                            break;
                        }

                        // 3d3
                        if (x == n - 1)
                        {
                            // 3g
                            continue;
                        }

                        // 3d4
                        y = x;
                    }

                    // 3e
                    x = BigInteger.ModPow(y, 2, n);
                    if (x == 1)
                    {

                        success = true;
                        break;

                    }

                    // 3g
                    // (loop again)
                }

                if (success)
                {
                    // Step 5
                    p = BigInteger.GreatestCommonDivisor((y - 1), n);
                    q = n / p;
                    return;
                }
            }
            throw new Exception("Cannot compute P and Q");
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
