using System;
using System.Security.Cryptography;

namespace RecipeShelf.Common
{
    public static class Helper
    {
        /// <summary>
        /// Generate an 8 character id that will work as filename and aws s3 key
        /// </summary>
        /// <returns></returns>
        public static string GenerateNewId()
        {
            var length = 8;
            var numBytes = (int)(Math.Log(Math.Pow(64, length)) / Math.Log(2) / 8);
            return Convert.ToBase64String(GenerateRandomBytes(numBytes)).Replace('/', '_').Replace('+', '-').Replace("=", string.Empty).Substring(0, length);
        }

        private static byte[] GenerateRandomBytes(int length)
        {
            // Create a buffer
            byte[] randBytes;

            if (length >= 1)
                randBytes = new byte[length];
            else
                randBytes = new byte[1];

            // Create a new RNGCryptoServiceProvider.
            var rand = RandomNumberGenerator.Create();

            // Fill the buffer with random bytes.
            rand.GetBytes(randBytes);

            // return the bytes.
            return randBytes;
        }
    }
}
