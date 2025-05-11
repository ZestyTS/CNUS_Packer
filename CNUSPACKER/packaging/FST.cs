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
        private readonly FSTEntries _fileEntries;
        private readonly MemoryStream _strings = new MemoryStream();

        public int CurEntryOffset { get; set; }

        public Contents Contents => _contents;
        public FSTEntries FileEntries => _fileEntries;

        public FST(Contents contents)
        {
            _contents = contents;
            _fileEntries = new FSTEntries(this); // Pass reference to this instance
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

        public int GetStringPosition() => (int)_strings.Position;

        public void AddString(string filename)
        {
            _strings.Write(Encoding.ASCII.GetBytes(filename), 0, filename.Length);
            _strings.WriteByte(0x00);
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
            buffer.Write(_strings.ToArray(), 0, (int)_strings.Length);

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
            size += (int)_strings.Position;

            return (int)Utils.Utils.Align(size, 0x8000);
        }
    }
}