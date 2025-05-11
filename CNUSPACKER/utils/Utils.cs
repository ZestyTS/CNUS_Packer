using System;
using System.IO;
using System.Text.RegularExpressions;

namespace CNUSPACKER.Utils
{
    /// <summary>
    /// General utility methods for alignment, file operations, and conversions.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Deletes a directory and all its contents recursively.
        /// </summary>
        public static void DeleteDir(string dir)
        {
            if (!Directory.Exists(dir))
                return;

            foreach (var file in Directory.EnumerateFiles(dir))
            {
                File.Delete(file);
            }

            foreach (var subDir in Directory.EnumerateDirectories(dir))
            {
                DeleteDir(subDir);
            }

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
        /// Converts a hexadecimal string to a byte array.
        /// </summary>
        public static byte[] HexStringToByteArray(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                throw new ArgumentException("Hex string cannot be null or empty.", nameof(hex));

            if (hex.Length % 2 != 0)
                throw new ArgumentException("Hex string length must be even.", nameof(hex));

            if (!Regex.IsMatch(hex, "^[0-9a-fA-F]*$"))
                throw new ArgumentException("Hex string contains invalid characters.", nameof(hex));

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            return bytes;
        }

        /// <summary>
        /// Converts a byte array into a hexadecimal string representation.
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
        /// Copies a range of bytes from the source array.
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