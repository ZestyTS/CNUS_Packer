using System;
using System.IO;

namespace CNUSPACKER.Utils
{
    /// <summary>
    /// A memory stream that provides convenience methods for writing big-endian primitive values.
    /// </summary>
    public class BigEndianMemoryStream : MemoryStream
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BigEndianMemoryStream"/> class with a specified capacity.
        /// </summary>
        /// <param name="capacity">The initial size of the internal buffer.</param>
        public BigEndianMemoryStream(int capacity) : base(capacity) { }

        /// <summary>
        /// Writes a 16-bit signed integer in big-endian format.
        /// </summary>
        public void WriteBigEndian(short value)
        {
            WriteBigEndianInternal(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes a 32-bit signed integer in big-endian format.
        /// </summary>
        public void WriteBigEndian(int value)
        {
            WriteBigEndianInternal(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes a 32-bit unsigned integer in big-endian format.
        /// </summary>
        public void WriteBigEndian(uint value)
        {
            WriteBigEndianInternal(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes a 64-bit signed integer in big-endian format.
        /// </summary>
        public void WriteBigEndian(long value)
        {
            WriteBigEndianInternal(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Internal helper for reversing and writing byte arrays.
        /// </summary>
        private void WriteBigEndianInternal(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            Write(bytes, 0, bytes.Length);
        }
    }
}