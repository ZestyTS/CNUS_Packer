using System.IO;
using System.Text;
using CNUSPACKER.contents;
using CNUSPACKER.fst;
using CNUSPACKER.utils;

namespace CNUSPACKER.packaging
{
    public class FST
    {
        private static readonly byte[] magicbytes = { 0x46, 0x53, 0x54, 0x00 };
        private const int unknown = 0x20;
        private int contentCount;

        public readonly Contents contents;
        public readonly FSTEntries fileEntries = new FSTEntries();

        private static readonly MemoryStream strings = new MemoryStream();

        public static int curEntryOffset { get; set; }

        public FST(Contents contents)
        {
            this.contents = contents;
        }

        public void Update()
        {
            contents.ResetFileOffsets();
            fileEntries.Update();
            contents.Update(fileEntries);
            fileEntries.GetRootEntry().SetEntryCount(fileEntries.GetFSTEntryCount());

            contentCount = contents.GetContentCount();
        }

        public static int GetStringPosition()
        {
            return (int)strings.Position;
        }

        public static void AddString(string filename)
        {
            strings.Write(Encoding.ASCII.GetBytes(filename), 0, filename.Length);
            strings.WriteByte(0x00);
        }

        public byte[] GetAsData()
        {
            BigEndianMemoryStream buffer = new BigEndianMemoryStream(GetDataSize());

            buffer.Write(magicbytes, 0, magicbytes.Length);
            buffer.WriteBigEndian(unknown);
            buffer.WriteBigEndian(contentCount);
            buffer.Seek(20, SeekOrigin.Current);

            byte[] contentHeaderData = contents.GetFSTContentHeaderAsData();
            buffer.Write(contentHeaderData, 0, contentHeaderData.Length);

            byte[] fileEntriesData = fileEntries.GetAsData();
            buffer.Write(fileEntriesData, 0, fileEntriesData.Length);

            byte[] stringsData = strings.ToArray();
            buffer.Write(stringsData, 0, stringsData.Length);

            return buffer.GetBuffer();
        }


        public int GetDataSize()
        {
            int size = 0;
            size += magicbytes.Length;
            size += 0x04; // unknown
            size += 0x04; // contentCount
            size += 20; // padding
            size += contents.GetFSTContentHeaderDataSize();
            size += fileEntries.GetDataSize();
            size += (int)strings.Position;
            return (int)Utils.Align(size, 0x8000);
        }
    }
}
