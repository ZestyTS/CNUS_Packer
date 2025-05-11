using System.IO;
using System.Security.Cryptography;

namespace CNUSPACKER.Utils
{
    /// <summary>
    /// Provides utility methods for hashing data.
    /// </summary>
    public static class HashUtil
    {
        /// <summary>
        /// Computes the SHA-256 hash of the input byte array.
        /// </summary>
        public static byte[] HashSHA2(byte[] data)
        {
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(data);
            }
        }

        /// <summary>
        /// Computes the SHA-1 hash of the input byte array.
        /// </summary>
        public static byte[] HashSHA1(byte[] data)
        {
            using (var sha1 = SHA1.Create())
            {
                return sha1.ComputeHash(data);
            }
        }

        /// <summary>
        /// Computes the SHA-1 hash of the aligned content of a file.
        /// </summary>
        /// <param name="file">Path to the file to hash.</param>
        /// <param name="alignment">Block alignment size (e.g., 32768).</param>
        public static byte[] HashSHA1(string file, int alignment)
        {
            using (var sha1 = SHA1.Create())
            using (var input = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                long targetSize = Utils.Align(input.Length, alignment);
                byte[] buffer = new byte[targetSize];

                int totalRead = 0;
                while (totalRead < targetSize)
                {
                    int read = input.Read(buffer, totalRead, (int)(targetSize - totalRead));
                    if (read == 0)
                    {
                        break; // Reached EOF
                    }
                    totalRead += read;
                }

                return sha1.ComputeHash(buffer);
            }
        }
    }
}