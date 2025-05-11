namespace CNUSPACKER.Crypto
{
    /// <summary>
    /// Represents a 16-byte initialization vector for AES encryption.
    /// </summary>
    public class IV
    {
        private const int Length = 0x10;

        /// <summary>
        /// Gets the raw IV byte array.
        /// </summary>
        public byte[] iv { get; } = new byte[Length];

        /// <summary>
        /// Initializes a new instance of the <see cref="IV"/> class using the provided array.
        /// </summary>
        /// <param name="iv">The byte array representing the IV.</param>
        public IV(byte[] iv)
        {
            if (iv != null && iv.Length == Length)
                this.iv = iv;
        }
    }
}
