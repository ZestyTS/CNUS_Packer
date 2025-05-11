using System.IO;
using System.Security.Cryptography;

namespace CNUSPACKER.utils
{
    public static class HashUtil
    {
        public static byte[] HashSHA2(byte[] data)
        {
            using SHA256 sha256 = SHA256.Create();
            return sha256.ComputeHash(data);
        }

        public static byte[] HashSHA1(byte[] data)
        {
            using SHA1 sha1 = SHA1.Create();
            return sha1.ComputeHash(data);
        }

        public static byte[] HashSHA1(string file, int alignment)
        {
            using SHA1 sha1 = SHA1.Create();
            using FileStream input = new FileStream(file, FileMode.Open);

            long targetSize = Utils.Align(input.Length, alignment);
            byte[] alignedFileContents = new byte[targetSize];

            int bytesRead = input.Read(alignedFileContents, 0, alignedFileContents.Length);

            while (bytesRead < targetSize)
                bytesRead += input.Read(alignedFileContents, bytesRead, alignedFileContents.Length - bytesRead);

            return sha1.ComputeHash(alignedFileContents);
        }
    }
}
