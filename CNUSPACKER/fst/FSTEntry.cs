using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CNUSPACKER.Packaging;
using CNUSPACKER.Utils;

namespace CNUSPACKER.FST
{
    public enum Types
    {
        DIR = 0x01,
        WiiVC = 0x02
    }

    public class FSTEntry
    {
        private readonly List<FSTEntry> _children = new List<FSTEntry>();
        private FSTEntry _parent;
        private int _nameOffset;
        private int _entryOffset;
        private short _flags;
        private readonly bool _isRoot;
        private int _rootEntryCount;
        private Content _content;
        private readonly Packaging.FST _fst;

        public string Filepath { get; }
        public string Filename { get; } = string.Empty;
        public bool IsDirectory { get; }
        public bool IsFile => !IsDirectory;
        public int ParentOffset { get; set; }
        public int NextOffset { get; set; }
        public long FileSize { get; }
        public long FileOffset { get; private set; }

        public IReadOnlyList<FSTEntry> Children => _children;

        public FSTEntry(Packaging.FST fst)
        {
            _fst = fst;
            _isRoot = true;
            IsDirectory = true;
        }

        public FSTEntry(string filepath, Packaging.FST fst)
        {
            _fst = fst;
            Filepath = Path.GetFullPath(filepath);
            IsDirectory = Directory.Exists(filepath);
            if (!IsDirectory)
            {
                FileSize = new FileInfo(filepath).Length;
            }
            Filename = Path.GetFileName(filepath);
        }

        public void AddChild(FSTEntry entry)
        {
            _children.Add(entry);
            entry._parent = this;
        }

        public void SetContent(Content content)
        {
            _flags = content.EntriesFlags;
            _content = content;
        }

        public byte[] GetAsData()
        {
            var buffer = new BigEndianMemoryStream(GetDataSize());

            if (_isRoot)
            {
                buffer.WriteByte(1);
                buffer.Seek(7, SeekOrigin.Current);
                buffer.WriteBigEndian(_rootEntryCount);
                buffer.Seek(4, SeekOrigin.Current);
            }
            else
            {
                buffer.WriteByte(GetTypeAsByte());
                buffer.WriteByte((byte)(_nameOffset >> 16));
                buffer.WriteByte((byte)(_nameOffset >> 8));
                buffer.WriteByte((byte)_nameOffset);

                if (IsDirectory)
                {
                    buffer.WriteBigEndian(ParentOffset);
                    buffer.WriteBigEndian(NextOffset);
                }
                else
                {
                    buffer.WriteBigEndian((int)(FileOffset >> 5));
                    buffer.WriteBigEndian((int)FileSize);
                }

                buffer.WriteBigEndian(_flags);
                buffer.WriteBigEndian((short)_content.Id);
            }

            foreach (var entry in _children)
            {
                var childData = entry.GetAsData();
                buffer.Write(childData, 0, childData.Length);
            }

            return buffer.ToArray();
        }

        private byte GetTypeAsByte()
        {
            byte type = 0;
            if (IsDirectory) type |= (byte)Types.DIR;
            if (Filename.EndsWith("nfs", StringComparison.OrdinalIgnoreCase))
                type |= (byte)Types.WiiVC;
            return type;
        }

        private int GetDataSize() => 0x10 + _children.Sum(c => c.GetDataSize());

        public void Update()
        {
            SetNameOffset(_fst.GetStringPosition());
            _fst.AddString(Filename);
            _entryOffset = _fst.CurEntryOffset;
            _fst.CurEntryOffset++;

            if (IsDirectory && !_isRoot)
                ParentOffset = _parent._entryOffset;

            if (_content != null && !IsDirectory)
                FileOffset = _content.GetOffsetForFileAndIncrease(this);

            foreach (var entry in _children)
            {
                entry.Update();
            }
        }

        private void SetNameOffset(int offset)
        {
            if (offset > 0xFFFFFF)
            {
                throw new InvalidOperationException($"Filename offset too large: {offset} > {0xFFFFFF}");
            }
            _nameOffset = offset;
        }

        public FSTEntry UpdateDirRefs()
        {
            if (!IsDirectory) return null;
            if (_parent != null) ParentOffset = _parent._entryOffset;

            FSTEntry result = null;
            var dirChildren = GetDirectoryChildren().ToList();

            for (int i = 0; i < dirChildren.Count; i++)
            {
                var current = dirChildren[i];
                if (i + 1 < dirChildren.Count)
                    current.NextOffset = dirChildren[i + 1]._entryOffset;

                var subResult = current.UpdateDirRefs();

                if (subResult != null)
                {
                    var nextParent = subResult._parent;
                    while (nextParent != null && nextParent.NextOffset == 0)
                    {
                        nextParent = nextParent._parent;
                    }
                    if (nextParent != null)
                        subResult.NextOffset = nextParent.NextOffset;
                }

                result = current;
            }

            return result;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (IsDirectory)
            {
                sb.AppendLine("DIR:");
                sb.AppendLine($"Filename: {Filename}");
                sb.AppendLine($"       ID: {_entryOffset}");
                sb.AppendLine($" ParentID: {ParentOffset}");
                sb.AppendLine($"   NextID: {NextOffset}");
            }
            foreach (var child in _children)
            {
                sb.Append(child);
            }
            return sb.ToString();
        }

        public IEnumerable<FSTEntry> GetFSTEntriesByContent(Content content)
        {
            var matches = new List<FSTEntry>();

            if (_content == null)
            {
                throw new InvalidOperationException($"Missing content assignment for '{Filename}'");
            }
            if (_content.Equals(content))
            {
                matches.Add(this);
            }

            matches.AddRange(_children.SelectMany(child => child.GetFSTEntriesByContent(content)));
            return matches;
        }

        public int GetEntryCount() => 1 + _children.Sum(c => c.GetEntryCount());

        public void SetEntryCount(int count)
        {
            _rootEntryCount = count;
        }

        private IEnumerable<FSTEntry> GetDirectoryChildren() => _children.Where(c => c.IsDirectory);
        private IEnumerable<FSTEntry> GetFileChildren() => _children.Where(c => c.IsFile);
    }
}
