/* Original author name: Adam Turcotte
 * Creation date: 2025/04/25
 * Goal: Complex mathematic functions
 *  Functions:
 * - PgcdEtendu
 * - InverseMod
 * - PowMod (Puissance modulaire de BigInt)
 * - CryptRsaData
 * - DeCryptRsaData
 * - CryptAesData
 * - DeCryptAesData
 */

using System;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;

namespace Reseau
{
    public static class MathFunctions
    {
        private static BigInteger _x, _y;

        static BigInteger PgcdEtendu(BigInteger a, BigInteger b)
        {
            if (a == 0) //Cas par default
            {
                _x = 0;
                _y = 1;
                return b;
            }

            //Pour storer les résultats de la récursion
            BigInteger pgcd = PgcdEtendu(b % a, a);
            BigInteger x1 = _x;
            BigInteger y1 = _y;

            //Mettre a jour x et y avec les résultats des récursions
            _x = y1 - (b / a) * x1;
            _y = x1;

            return pgcd;
        }

        public static BigInteger InverseMod(int A, BigInteger M)
        {
            BigInteger g = PgcdEtendu(A, M);
            if (g != 1)
            {
                Console.Write("Pas d'inverse");
                return -1;
            }
            else
            {
                //On ajoute M pour gérer les x négatifs
                BigInteger res = (_x % M + M) % M;
                return res;
            }
        }

        public static BigInteger BigIntegerPowMod(BigInteger a, BigInteger exp, BigInteger mod)
        {
            BigInteger result = 1;
            while (exp > 0)
            {
                if ((exp & 1) > 0) result = (result * a) % mod;
                exp >>= 1;
                a = (a * a) % mod;
            }

            return result;
        }

        public static byte[] CryptRsaData(byte[] data, RsaKeyData key)
        {
            BigInteger bigIntdata = new BigInteger(data);
            return BigIntegerPowMod(bigIntdata, new BigInteger(key.getOther()), new BigInteger(key.getN())).ToByteArray();
        }

        public static byte[] DecryptRsaData(byte[] data, RsaKeyData key)
        {
            BigInteger bigIntData = new BigInteger(data);
            return BigIntegerPowMod(bigIntData, new BigInteger(key.getOther()), new BigInteger(key.getN())).ToByteArray();
        }

        public static byte[] CryptAesData(byte[] data, byte[] key, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (MemoryStream ms = new MemoryStream())
                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                    cs.FlushFinalBlock();
                    return ms.ToArray();
                }
            }
        }

        public static byte[] DecryptAesData(byte[] data, byte[] key, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;


                using (MemoryStream ms = new MemoryStream(data))
                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                    cs.FlushFinalBlock();
                    return ms.ToArray();
                }
            }
        }
    }
}