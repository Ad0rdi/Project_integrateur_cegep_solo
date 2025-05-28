/* Original author name: Adam Turcotte
 * Creation date: 2025/04/25
 * Goal: Créée une clée RSA
 * Modification listing:
 * 2025/04/25:
 *      Author Name: Adam Turcotte
 *      Goal: Create an RSA key with n bit length.
 */
// This code is contributed by phasing17.

using System;
using System.Collections.Generic;
using BigIntRandNum;
using BigInteger = System.Numerics.BigInteger;


namespace Reseau
{
    public class RsaKeys
    {
        private RsaKeyData privateKey;
        private RsaKeyData publicKey;

        public RsaKeys(RsaKeyData privateKey, RsaKeyData publicKey)
        {
            this.privateKey = privateKey;
            this.publicKey = publicKey;
        }

        public static RsaKeys Create()
        {
            var rsa = new RsaKey();
            var privateKey = rsa.GetPrivateKey();
            var publicKey = rsa.GetPubKey();
            return new RsaKeys(privateKey, publicKey);
        }

        public RsaKeyData getPrivateKey()
        {
            return privateKey;
        }

        public RsaKeyData getPublicKey()
        {
            return publicKey;
        }
    }

    public class RsaKey
    {
        private const int BitLength = 1024;
        private const int Certainty = 20;
        private readonly BigInteger _n;
        private const int _e = 65537;
        private readonly BigInteger _d;

        public RsaKey()
        {
            BigInteger p = FindPrime(0);
            BigInteger q = FindPrime(p);
            _n = p * q;
            BigInteger phi_n = (p - 1) * (q - 1);
            _d = MathFunctions.InverseMod(_e, phi_n);

            if (_d == -1) throw new Exception("Fait des niaiseri en créant la clée");
        }


        private static BigInteger FindPrime(BigInteger isNotThisPrime)
        {
            List<int> firstPrime = SieveOfEratosthenes(10000);
            BigInteger prime = 0;
            var primeTest = new BigIntegerPrimeTest();

            var isMillerRabin = false;
            while (!isMillerRabin || prime == isNotThisPrime)
            {
                //Retourne un nb qui n'est pas divisible par les nb premiers de bas niveau
                prime = GetLowLevelPrime(BitLength, firstPrime);
                isMillerRabin = primeTest.IsProbablePrime(prime, Certainty);
            }

            return prime;
        }

        private static BigInteger NBitRandom(int n)
        {
            var rand = new RandomBigIntegerGenerator();
            // Returns a random number 
            // between 2**(n-1)+1 and 2**n-1''' 
            BigInteger max = BigInteger.Pow(2, n) - 1;
            BigInteger min = (BigInteger.Pow(2, n - 1) + 1);
            return rand.RandomBigInteger(min, max + 1);
            // This code is contributed by phasing17.
        }

        private static List<int> SieveOfEratosthenes(int n)
        {
            // Crée un vecteur booléen
            // "prime[0..n]" et initialise toutes les valeurs à true.
            // La valeur de prime[i] devient fausse si elle n'est pas un nb premier

            var prime = new bool[n + 1];

            for (var i = 0; i <= n; i++)
                prime[i] = true;

            for (var p = 2; p * p <= n; p++)
            {
                // Si prime[p] n'est pas changé, c'est un nb premier
                if (prime[p] != true) continue;

                // Metre a jour tous les multiples de p
                for (var i = p * p; i <= n; i += p)
                    prime[i] = false;
            }

            // Print all prime numbers
            var primes = new List<int>();
            for (var i = 2; i <= n; i++)
            {
                if (prime[i] != true)
                    continue;

                // Console.Write(i + " ");
                primes.Add(i);
            }

            return primes;
        }

        private static BigInteger GetLowLevelPrime(int nbit, List<int> firstPrimeList)
        {
            // Crée un nombre qui est co-premier avec les premiers nombres premiers

            //Tant qu'un nombre n'est pas trouvé, recommencer
            while (true)
            {
                var primeNumberCandidate = NBitRandom(nbit);

                foreach (var div in firstPrimeList)
                {
                    if (primeNumberCandidate % div == 0 && div * div <= primeNumberCandidate)
                        break;

                    return primeNumberCandidate;
                }
            }
        }

        public RsaKeyData GetPubKey()
        {
            return new RsaKeyData( _n.ToByteArray(), new BigInteger(_e).ToByteArray() );
        }

        public RsaKeyData GetPrivateKey()
        {
            return new RsaKeyData(_n.ToByteArray(), _d.ToByteArray());
        }
        
    }
}