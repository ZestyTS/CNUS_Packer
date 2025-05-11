using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CNUSPACKER.Crypto;
using CNUSPACKER.FST;

namespace CNUSPACKER.Packaging
{
    /// <summary>
    /// Represents and manages a collection of content files during packaging.
    /// </summary>
    public class Contents
    {
        private readonly List<Content> _contents = new List<Content>();

        /// <summary>
        /// The special content used to store the FST data.
        /// </summary>
        public Content FstContent { get; }

        public Contents()
        {
            var details = new ContentDetails(false, 0, 0, 0);
            FstContent = CreateNewContent(details, isFstContent: true);
        }

        public Content CreateNewContent(ContentDetails details, bool isFstContent = false)
        {
            var content = new Content(
                id: _contents.Count,
                index: (short)_contents.Count,
                entriesFlags: details.EntriesFlag,
                groupId: details.GroupID,
                parentTitleId: details.ParentTitleID,
                isHashed: details.IsHashed,
                isFstContent: isFstContent);

            _contents.Add(content);
            return content;
        }

        public short GetContentCount() => (short)_contents.Count;

        public byte[] GetAsData() => _contents.SelectMany(c => c.GetAsData()).ToArray();

        public int GetDataSize() => GetContentCount() * Content.StaticDataSize;

        public byte[] GetFstContentHeaderAsData()
        {
            long contentOffset = 0;
            using var buffer = new MemoryStream(GetFstContentHeaderDataSize());
            foreach (var content in _contents)
            {
                var kvp = content.GetFSTContentHeaderAsData(contentOffset);
                contentOffset = kvp.Key;
                buffer.Write(kvp.Value, 0, kvp.Value.Length);
            }

            return buffer.ToArray();
        }

        public int GetFstContentHeaderDataSize() => GetContentCount() * Content.StaticFSTContentHeaderDataSize;

        public void ResetFileOffsets()
        {
            foreach (var content in _contents)
                content.ResetFileOffset();
        }

        public void Update(FSTEntries fileEntries)
        {
            foreach (var content in _contents)
            {
                var entries = fileEntries.GetFSTEntriesByContent(content);
                content.Update(entries);
            }
        }

        public async Task PackContentsAsync(string outputDir, Encryption encryption)
        {
            foreach (var content in _contents)
                if (!content.Equals(FstContent))
                    await content.PackContentToFileAsync(outputDir, encryption);
        }

        public void DeleteContent(Content content)
        {
            _contents.Remove(content);
        }
    }
}