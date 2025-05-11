using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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

        public byte[] TMDHash { get; private set; }
        private int _blockCount;

        private ContentHashes(ILogger<ContentHashes> logger = null)
        {
            _logger = logger;
        }

        public static async Task<ContentHashes> CreateAsync(string file, bool hashed, ILogger<ContentHashes> logger = null)
        {
            var instance = new ContentHashes(logger);

            if (hashed)
            {
                logger?.LogInformation("Calculating hierarchical SHA1 hashes for {File}.", file);
                await instance.CalculateH0HashesAsync(file);
                instance.CalculateOtherHashes(1, instance._h0Hashes, instance._h1Hashes);
                instance.CalculateOtherHashes(2, instance._h1Hashes, instance._h2Hashes);
                instance.CalculateOtherHashes(3, instance._h2Hashes, instance._h3Hashes);
                instance.TMDHash = HashUtil.HashSHA1(instance.GetH3Hashes());
                logger?.LogInformation("TMD hash calculation complete.");
            }
            else
            {
                logger?.LogInformation("Calculating SHA1 hash directly for {File}.", file);
                instance.TMDHash = HashUtil.HashSHA1(file, Content.ContentFilePadding);
            }

            return instance;
        }

        private async Task CalculateH0HashesAsync(string file)
        {
            using (var input = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
            {
                const int bufferSize = 0xFC00;
                byte[] buffer = new byte[bufferSize];
                int totalBlocks = (int)(input.Length / bufferSize) + 1;

                for (int block = 0; block < totalBlocks; block++)
                {
                    int read = await input.ReadAsync(buffer, 0, bufferSize);
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

        public async Task SaveH3ToFileAsync(string path)
        {
            if (_h3Hashes.Count == 0)
                return;

            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                var h3 = GetH3Hashes();
                await fs.WriteAsync(h3, 0, h3.Length);
                _logger?.LogInformation("H3 hash file saved to {Path}.", path);
            }
        }
    }
}
