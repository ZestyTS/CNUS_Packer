using System;
using System.Collections.Generic;
using System.IO;
using CNUSPACKER.Packaging;
using CNUSPACKER.Utils;
using Microsoft.Extensions.Logging;

namespace CNUSPACKER.Crypto
{
    public class ContentHashes
    {
        private readonly Dictionary<int, byte[]> _h0Hashes = new Dictionary<int, byte[]>();
        private readonly Dictionary<int, byte[]> _h1Hashes = new Dictionary<int, byte[]>();
        private readonly Dictionary<int, byte[]> _h2Hashes = new Dictionary<int, byte[]>();
        private readonly Dictionary<int, byte[]> _h3Hashes = new Dictionary<int, byte[]>();

        private readonly ILogger<ContentHashes> _logger;

        public byte[] TMDHash { get; }
        private int _blockCount;

        public ContentHashes(string file, bool hashed, ILogger<ContentHashes> logger = null)
        {
            _logger = logger;

            if (hashed)
            {
                _logger?.LogInformation("Calculating hierarchical SHA1 hashes for {File}.", file);
                CalculateH0Hashes(file);
                CalculateOtherHashes(1, _h0Hashes, _h1Hashes);
                CalculateOtherHashes(2, _h1Hashes, _h2Hashes);
                CalculateOtherHashes(3, _h2Hashes, _h3Hashes);
                TMDHash = HashUtil.HashSHA1(GetH3Hashes());
                _logger?.LogInformation("TMD hash calculation complete.");
            }
            else
            {
                _logger?.LogInformation("Calculating SHA1 hash directly for {File}.", file);
                TMDHash = HashUtil.HashSHA1(file, Content.ContentFilePadding);
            }
        }

        private void CalculateH0Hashes(string file)
        {
            using (var input = new FileStream(file, FileMode.Open))
            {
                const int bufferSize = 0xFC00;
                byte[] buffer = new byte[bufferSize];
                int totalBlocks = (int)(input.Length / bufferSize) + 1;

                for (int block = 0; block < totalBlocks; block++)
                {
                    input.Read(buffer, 0, bufferSize);
                    _h0Hashes[block] = HashUtil.HashSHA1(buffer);

                    if (block % 100 == 0)
                    {
                        _logger?.LogDebug("H0 progress: {Percent}%", (100 * block / totalBlocks));
                    }
                }
                _blockCount = totalBlocks;
                _logger?.LogInformation("H0 hashing complete.");
            }
        }

        private void CalculateOtherHashes(int level, Dictionary<int, byte[]> inputHashes, Dictionary<int, byte[]> outputHashes)
        {
            int power = 1 << (4 * level);
            int totalBlocks = (_blockCount / power) + 1;

            for (int block = 0; block < totalBlocks; block++)
            {
                byte[] combined = new byte[16 * 20];

                for (int i = 0; i < 16; i++)
                {
                    int index = block * 16 + i;
                    if (inputHashes.TryGetValue(index, out var hash))
                    {
                        Array.Copy(hash, 0, combined, i * 20, 20);
                    }
                }
                outputHashes[block] = HashUtil.HashSHA1(combined);

                if (block % 100 == 0)
                {
                    _logger?.LogDebug("H{Level} progress: {Percent}%", level, (100 * block / totalBlocks));
                }
            }
            _logger?.LogInformation("H{Level} hashing complete.", level);
        }

        public byte[] GetHashForBlock(int block)
        {
            if (block > _blockCount)
                throw new ArgumentOutOfRangeException(nameof(block), "Block exceeds range.");

            var output = new MemoryStream(0x400);
            AppendHashes(output, _h0Hashes, block / 16 * 16);
            AppendHashes(output, _h1Hashes, block / 256 * 16);
            AppendHashes(output, _h2Hashes, block / 4096 * 16);

            return output.GetBuffer();
        }

        private void AppendHashes(Stream stream, Dictionary<int, byte[]> level, int start)
        {
            for (int i = 0; i < 16; i++)
            {
                if (level.TryGetValue(start + i, out var hash))
                    stream.Write(hash, 0, hash.Length);
                else
                    stream.Seek(20, SeekOrigin.Current);
            }
        }

        private byte[] GetH3Hashes()
        {
            var output = new MemoryStream(_h3Hashes.Count * 20);
            for (int i = 0; i < _h3Hashes.Count; i++)
            {
                output.Write(_h3Hashes[i], 0, _h3Hashes[i].Length);
            }
            return output.GetBuffer();
        }

        public void SaveH3ToFile(string path)
        {
            if (_h3Hashes.Count == 0) 
                return;

            using (var fs = new FileStream(path, FileMode.Create))
            {
                var h3 = GetH3Hashes();
                fs.Write(h3, 0, h3.Length);
                _logger?.LogInformation("H3 hash file saved to {Path}.", path);
            }
        }
    }
}
