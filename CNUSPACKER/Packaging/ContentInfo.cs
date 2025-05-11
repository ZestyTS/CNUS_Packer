using CNUSPACKER.Utils;
using Microsoft.Extensions.Logging;

namespace CNUSPACKER.Packaging
{
    /// <summary>
    /// Represents metadata for a set of content chunks.
    /// </summary>
    public class ContentInfo
    {
        public const int StaticDataSize = 0x24;

        /// <summary>
        /// The number of content entries.
        /// </summary>
        public short ContentCount { get; }

        /// <summary>
        /// A SHA-256 hash over all associated contents.
        /// </summary>
        public byte[] Sha2Hash { get; set; } = new byte[32];

        private readonly short _indexOffset;
        private readonly ILogger<ContentInfo>? _logger;

        /// <summary>
        /// Creates a new ContentInfo instance.
        /// </summary>
        /// <param name="contentCount">The number of contents described.</param>
        /// <param name="logger">Optional logger for diagnostics.</param>
        public ContentInfo(short contentCount, ILogger<ContentInfo>? logger = null) : this(0, contentCount, logger) { }

        private ContentInfo(short indexOffset, short contentCount, ILogger<ContentInfo>? logger)
        {
            _indexOffset = indexOffset;
            ContentCount = contentCount;
            _logger = logger;
            _logger?.LogDebug("Initialized ContentInfo with ContentCount={Count} and IndexOffset={Offset}.", contentCount, indexOffset);
        }

        /// <summary>
        /// Converts the content info to its byte representation.
        /// </summary>
        /// <returns>A byte array of the serialized content info.</returns>
        public byte[] GetAsData()
        {
            var buffer = new BigEndianMemoryStream(StaticDataSize);
            buffer.WriteBigEndian(_indexOffset);
            buffer.WriteBigEndian(ContentCount);
            buffer.Write(Sha2Hash, 0, Sha2Hash.Length);
            _logger?.LogDebug("Serialized ContentInfo to {ByteCount} bytes.", StaticDataSize);
            return buffer.GetBuffer();
        }
    }
}