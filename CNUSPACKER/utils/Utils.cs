using System;
using System.IO;
using System.Text.RegularExpressions;

namespace CNUSPACKER.utils
{
    public static class Utils
    {
        /// <summary>
        /// Deletes a directory and all its contents recursively.
        /// </summary>
        public static void DeleteDir(string dir)
        {
            if (!Directory.Exists(dir))
                return;

            foreach (string filepath in Directory.EnumerateFiles(dir))
                File.Delete(filepath);

            foreach (string subDir in Directory.EnumerateDirectories(dir))
                DeleteDir(subDir);

            Directory.Delete(dir);
        }

        /// <summary>
        /// Aligns the input value to the specified alignment boundary.
        /// </summary>
        public static long Align(long input, int alignment)
        {
            if (alignment <= 0)
                throw new ArgumentOutOfRangeException(nameof(alignment), "Alignment must be greater than zero.");

            long newSize = input / alignment;
            if (newSize * alignment != input)
                newSize++;

            return newSize * alignment;
        }

        /// <summary>
        /// Converts a hex string (e.g. "A1B2C3") to a byte array.
        /// </summary>
        public static byte[] HexStringToByteArray(string s)
        {
            if (string.IsNullOrEmpty(s))
                throw new ArgumentException("Input hex string cannot be null or empty.", nameof(s));

            if (s.Length % 2 != 0)
                throw new ArgumentException("Hex string must have an even length.", nameof(s));

            if (!Regex.IsMatch(s, @"\A\b[0-9a-fA-F]*\b\Z"))
                throw new ArgumentException("Hex string contains invalid characters.", nameof(s));

            int outputLength = s.Length / 2;
            byte[] output = new byte[outputLength];
            for (int i = 0; i < outputLength; i++)
            {
                output[i] = Convert.ToByte(s.Substring(i * 2, 2), 16);
            }

            return output;
        }

        /// <summary>
        /// Converts a byte array into a hexadecimal string representation (e.g. "A1B2C3").
        /// </summary>
        public static string ByteArrayToHexString(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            char[] c = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                int b = bytes[i] >> 4;
                c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
                b = bytes[i] & 0xF;
                c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
            }

            return new string(c);
        }

        /// <summary>
        /// Creates a copy of a sub-range from a byte array.
        /// </summary>
        public static byte[] CopyOfRange(byte[] src, int start, int end)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            if (start < 0 || end > src.Length || start > end)
                throw new ArgumentOutOfRangeException("Invalid range specified.");

            int len = end - start;
            byte[] dest = new byte[len];
            Array.Copy(src, start, dest, 0, len);

            return dest;
        }
    }
}
