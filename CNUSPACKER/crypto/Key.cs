namespace CNUSPACKER.Crypto
{
    /// <summary>
    /// Represents a 16-byte AES key used for content encryption.
    /// </summary>
    public class Key
    {
        private const int Length = 0x10;

        /// <summary>
        /// Gets the raw key byte array.
        /// </summary>
        public byte[] key { get; } = new byte[Length];

        /// <summary>
        /// Initializes a new instance of the <see cref="Key"/> class using a byte array.
        /// </summary>
        /// <param name="key">The byte array representing the key.</param>
        public Key(byte[] key)
        {
            if (key != null && key.Length == Length)
            {
                this.key = key;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Key"/> class using a hexadecimal string.
        /// </summary>
        /// <param name="hex">Hex string representation of the key.</param>
        public Key(string hex) : this(Utils.Utils.HexStringToByteArray(hex)) { }

        /// <summary>
        /// Returns the hexadecimal string representation of the key.
        /// </summary>
        public override string ToString()
        {
            return Utils.Utils.ByteArrayToHexString(key);
        }
    }
}
