using System;
using System.Collections.Generic;
using System.IO;
using CNUSPACKER.Crypto;
using CNUSPACKER.FST;
using CNUSPACKER.Utils;
using Microsoft.Extensions.Logging;

namespace CNUSPACKER.Packaging
{
    /// <summary>
    /// Represents a single content chunk in the WUP packaging process.
    /// </summary>
    public class Content : IEquatable<Content>
    {
        public const int StaticFSTContentHeaderDataSize = 32;
        public const int StaticDataSize = 48;
        public const int ContentFilePadding = 32768;

        private const int AlignmentInContentFile = 32;
        private const short TypeContent = 8192;
        private const short TypeEncrypted = 1;
        private const short TypeHashed = 2;

        private short _type = TypeContent | TypeEncrypted;
        private long _curFileOffset;
        private List<FSTEntry> _entries = new List<FSTEntry>();

        private readonly short _index;
        private readonly int _groupId;
        private readonly long _parentTitleId;
        private readonly bool _isFstContent;
        private readonly ILogger<Content>? _logger;

        public int Id { get; }
        public long EncryptedFileSize { get; set; }
        public byte[] Sha1 { get; set; } = new byte[20];
        public short EntriesFlags { get; }

        private bool IsHashed => (_type & TypeHashed) == TypeHashed;

        public Content(int id, short index, short entriesFlags, int groupId, long parentTitleId, bool isHashed, bool isFstContent, ILogger<Content>? logger = null)
        {
            Id = id;
            _index = index;
            EntriesFlags = entriesFlags;
            _groupId = groupId;
            _parentTitleId = parentTitleId;
            _isFstContent = isFstContent;
            _logger = logger;

            if (isHashed)
                _type |= TypeHashed;
        }

        public KeyValuePair<long, byte[]> GetFSTContentHeaderAsData(long oldContentOffset)
        {
            var buffer = new BigEndianMemoryStream(StaticFSTContentHeaderDataSize);

            byte unknown;
            long contentOffset = oldContentOffset;
            long fstContentSize = EncryptedFileSize / ContentFilePadding;
            long fstContentSizeWritten = fstContentSize;

            if (IsHashed)
            {
                unknown = 2;
                fstContentSizeWritten -= ((fstContentSize / 64) + 1) * 2;
                if (fstContentSizeWritten < 0)
                    fstContentSizeWritten = 0;
            }
            else
            {
                unknown = 1;
            }

            if (_isFstContent)
            {
                unknown = 0;
                if (fstContentSize == 1)
                    fstContentSize = 0;

                contentOffset += fstContentSize + 2;
            }
            else
            {
                contentOffset += fstContentSize;
            }

            buffer.WriteBigEndian((int)oldContentOffset);
            buffer.WriteBigEndian((int)fstContentSizeWritten);
            buffer.WriteBigEndian(_parentTitleId);
            buffer.WriteBigEndian(_groupId);
            buffer.WriteByte(unknown);

            return new KeyValuePair<long, byte[]>(contentOffset, buffer.GetBuffer());
        }

        public long GetOffsetForFileAndIncrease(FSTEntry fstEntry)
        {
            long oldOffset = _curFileOffset;
            _curFileOffset += Utils.Utils.Align(fstEntry.FileSize, AlignmentInContentFile);
            return oldOffset;
        }

        public void ResetFileOffset() => _curFileOffset = 0;

        public byte[] GetAsData()
        {
            var buffer = new BigEndianMemoryStream(StaticDataSize);
            buffer.WriteBigEndian(Id);
            buffer.WriteBigEndian(_index);
            buffer.WriteBigEndian(_type);
            buffer.WriteBigEndian(EncryptedFileSize);
            buffer.Write(Sha1, 0, Sha1.Length);
            return buffer.GetBuffer();
        }

        public void PackContentToFile(string outputDir, Encryption encryption)
        {
            _logger?.LogInformation("Packing content {Id:X8}", Id);

            string decryptedFile = PackDecrypted();
            _logger?.LogInformation("Generating hashes");
            var contentHashes = new ContentHashes(decryptedFile, IsHashed);
            string h3Path = Path.Combine(outputDir, $"{Id:X8}.h3");
            contentHashes.SaveH3ToFile(h3Path);
            Sha1 = contentHashes.TMDHash;

            _logger?.LogInformation("Encrypting content {Id:X8}", Id);
            string outputFilePath = Path.Combine(outputDir, $"{Id:X8}.app");
            EncryptedFileSize = PackEncrypted(decryptedFile, outputFilePath, contentHashes, encryption);

            _logger?.LogInformation("Content {Id:X8} packed to file \"{FileName}\"", Id, $"{Id:X8}.app");
        }

        private long PackEncrypted(string decryptedFile, string outputFilePath, ContentHashes hashes, Encryption encryption)
        {
            using var input = new FileStream(decryptedFile, FileMode.Open);
            using var output = new FileStream(outputFilePath, FileMode.Create);

            if (IsHashed)
            {
                encryption.EncryptFileHashed(input, Id, output, hashes);
            }
            else
            {
                encryption.EncryptFileWithPadding(input, Id, output, ContentFilePadding);
            }

            return output.Length;
        }

        private string PackDecrypted()
        {
            string tmpPath = Path.Combine(Settings.tmpDir, $"{Id:X8}.dec");
            using var fos = new FileStream(tmpPath, FileMode.Create);
            int totalCount = _entries.Count;
            int fileIndex = 1;
            long currentOffset = 0;

            foreach (var entry in _entries)
            {
                if (entry.IsFile)
                {
                    if (currentOffset != entry.FileOffset)
                    {
                        _logger?.LogWarning("Expected file offset {Expected} but found {Actual}", currentOffset, entry.FileOffset);
                    }

                    _logger?.LogInformation("[{FileIndex}/{Total}] Writing file: {File}", fileIndex, totalCount, entry.Filename);

                    using var input = new FileStream(entry.Filepath, FileMode.Open);
                    input.CopyTo(fos);

                    long alignedSize = Utils.Utils.Align(entry.FileSize, AlignmentInContentFile);
                    currentOffset += alignedSize;
                    long padding = alignedSize - entry.FileSize;
                    fos.Write(new byte[padding], 0, (int)padding);
                }
                else
                {
                    _logger?.LogInformation("[{FileIndex}/{Total}] Folder: {Folder}", fileIndex, totalCount, entry.Filename);
                }
                fileIndex++;
            }

            return tmpPath;
        }

        public void Update(List<FSTEntry> entries)
        {
            if (entries != null)
            {
                _entries = entries;
            }
        }

        public bool Equals(Content? other)
        {
            return other != null && Id == other.Id;
        }
    }
}