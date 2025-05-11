using System.IO;
using System.Text;
using CNUSPACKER.FST;
using CNUSPACKER.Utils;

namespace CNUSPACKER.Packaging
{
    /// <summary>
    /// Represents the File System Table (FST) structure used in WUP packaging.
    /// </summary>
    public class FST
    {
        private static readonly byte[] MagicBytes = { 0x46, 0x53, 0x54, 0x00 }; // "FST\0"
        private const int HeaderPadding = 0x20;

        private readonly Contents _contents;
        private readonly FSTEntries _fileEntries = new FSTEntries();

        private static readonly MemoryStream Strings = new MemoryStream();
        public static int CurEntryOffset { get; set; }

        public Contents Contents => _contents;
        public FSTEntries FileEntries => _fileEntries;

        public FST(Contents contents)
        {
            _contents = contents;
        }

        /// <summary>
        /// Updates file entry offsets, hashes, and structure before writing.
        /// </summary>
        public void Update()
        {
            _contents.ResetFileOffsets();
            _fileEntries.Update();
            _contents.Update(_fileEntries);
            _fileEntries.GetRootEntry().SetEntryCount(_fileEntries.GetFSTEntryCount());
        }

        public static int GetStringPosition() => (int)Strings.Position;

        public static void AddString(string filename)
        {
            Strings.Write(Encoding.ASCII.GetBytes(filename), 0, filename.Length);
            Strings.WriteByte(0x00);
        }

        /// <summary>
        /// Serializes the entire FST (header, file entries, and strings) to a byte array.
        /// </summary>
        public byte[] GetAsData()
        {
            var buffer = new BigEndianMemoryStream(GetDataSize());

            buffer.Write(MagicBytes, 0, MagicBytes.Length);
            buffer.WriteBigEndian(HeaderPadding);
            buffer.WriteBigEndian(_contents.GetContentCount());
            buffer.Seek(20, SeekOrigin.Current); // Reserved

            buffer.Write(_contents.GetFstContentHeaderAsData(), 0, _contents.GetFstContentHeaderAsData().Length);
            buffer.Write(_fileEntries.GetAsData(), 0, _fileEntries.GetAsData().Length);
            buffer.Write(Strings.ToArray(), 0, (int)Strings.Length);

            return buffer.GetBuffer();
        }

        /// <summary>
        /// Calculates the total size of the FST structure.
        /// </summary>
        public int GetDataSize()
        {
            int size = 0;
            size += MagicBytes.Length;         // Header
            size += 4 + 4 + 20;                // Padding fields
            size += _contents.GetFstContentHeaderDataSize();
            size += _fileEntries.GetDataSize();
            size += (int)Strings.Position;

            return (int)Utils.Utils.Align(size, 0x8000);
        }
    }
}
