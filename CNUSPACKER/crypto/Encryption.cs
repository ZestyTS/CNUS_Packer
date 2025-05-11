using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using CNUSPACKER.Utils;
using Microsoft.Extensions.Logging;

namespace CNUSPACKER.Crypto
{
    /// <summary>
    /// Handles AES-CBC encryption for WUP packaging.
    /// </summary>
    public class Encryption
    {
        private readonly Aes _aes;
        private readonly ILogger<Encryption>? _logger;

        /// <summary>
        /// Initializes a new encryption engine with the specified key and IV.
        /// </summary>
        public Encryption(Key key, IV iv, ILogger<Encryption>? logger = null)
        {
            _aes = Aes.Create();
            _aes.Mode = CipherMode.CBC;
            _aes.Padding = PaddingMode.None;
            _aes.Key = key.key;
            _aes.IV = iv.iv;
            _logger = logger;
        }

        public async Task EncryptFileWithPaddingAsync(Packaging.FST fst, string outputFilename, short contentId, int blockSize)
        {
            using (FileStream output = new FileStream(outputFilename, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                var input = new MemoryStream(fst.GetAsData());
                var ivStream = new BigEndianMemoryStream(0x10);
                ivStream.WriteBigEndian(contentId);
                var iv = new IV(ivStream.GetBuffer());

                _logger?.LogInformation("Encrypting FST to {Output}", outputFilename);
                await EncryptSingleFileAsync(input, output, fst.GetDataSize(), iv, blockSize);
            }
        }

        public async Task EncryptFileWithPaddingAsync(FileStream input, int contentId, FileStream output, int blockSize)
        {
            var ivStream = new BigEndianMemoryStream(0x10);
            ivStream.WriteBigEndian((short)contentId);
            var iv = new IV(ivStream.GetBuffer());

            _logger?.LogInformation("Encrypting file stream with padding. ContentID={ContentId}", contentId);
            await EncryptSingleFileAsync(input, output, input.Length, iv, blockSize);
        }

        private async Task EncryptSingleFileAsync(Stream input, Stream output, long inputLength, IV iv, int blockSize)
        {
            _aes.IV = iv.iv;
            long targetSize = Utils.Utils.Align(inputLength, blockSize);

            int curPosition = 0;
            do
            {
                byte[] blockBuffer = new byte[blockSize];
                int bytesRead = await input.ReadAsync(blockBuffer, 0, blockSize);
                if (bytesRead == 0) break;

                byte[] encryptedBlock = Encrypt(blockBuffer);
                _aes.IV = Utils.Utils.CopyOfRange(encryptedBlock, blockSize - 16, blockSize);
                await output.WriteAsync(encryptedBlock, 0, bytesRead);
                curPosition += bytesRead;

                if (curPosition % (blockSize * 100) == 0)
                {
                    _logger?.LogDebug("Encrypted {Bytes}/{Total} bytes", curPosition, targetSize);
                }
            }
            while (curPosition < targetSize);

            _logger?.LogInformation("File encryption completed: {Bytes} bytes", curPosition);
        }

        public async Task EncryptFileHashedAsync(FileStream input, int contentId, FileStream output, ContentHashes hashes)
        {
            await EncryptFileHashedAsync(input, output, input.Length, contentId, hashes);
        }

        private async Task EncryptFileHashedAsync(Stream input, Stream output, long length, int contentId, ContentHashes hashes)
        {
            const int hashBlockSize = 0xFC00;
            byte[] buffer = new byte[hashBlockSize];
            int block = 0;
            int read;

            do
            {
                read = await input.ReadAsync(buffer, 0, hashBlockSize);
                byte[] encryptedData = EncryptChunkHashed(buffer, block, hashes, contentId);
                await output.WriteAsync(encryptedData, 0, encryptedData.Length);

                block++;
                if (block % 100 == 0)
                {
                    _logger?.LogDebug("Encryption progress: {Percent}%", (int)(100.0 * block * hashBlockSize / length));
                }
            } while (read == hashBlockSize);

            _logger?.LogInformation("Hashed encryption completed: {Blocks} blocks", block);
        }

        private byte[] EncryptChunkHashed(byte[] buffer, int block, ContentHashes hashes, int contentId)
        {
            var ivStream = new BigEndianMemoryStream(16);
            ivStream.WriteBigEndian((short)contentId);
            _aes.IV = ivStream.GetBuffer();

            byte[] decryptedHashes = hashes.GetHashForBlock(block);
            decryptedHashes[1] ^= (byte)contentId;

            byte[] encryptedHashes = Encrypt(decryptedHashes);
            decryptedHashes[1] ^= (byte)contentId;
            int ivStart = (block % 16) * 20;

            _aes.IV = Utils.Utils.CopyOfRange(decryptedHashes, ivStart, ivStart + 16);

            byte[] encryptedContent = Encrypt(buffer);
            var outputStream = new MemoryStream(0x10000);
            outputStream.Write(encryptedHashes, 0, encryptedHashes.Length);
            outputStream.Write(encryptedContent, 0, encryptedContent.Length);

            return outputStream.GetBuffer();
        }

        public byte[] Encrypt(byte[] input)
        {
            return _aes.CreateEncryptor().TransformFinalBlock(input, 0, input.Length);
        }
    }
}
