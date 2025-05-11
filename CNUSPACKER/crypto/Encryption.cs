using System;
using System.IO;
using System.Security.Cryptography;
using CNUSPACKER.packaging;
using CNUSPACKER.utils;

namespace CNUSPACKER.crypto
{
    public class Encryption
    {
        private readonly Aes aes = Aes.Create();

        public Encryption(Key key, IV iv)
        {
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.None;
            aes.Key = key.key;
            aes.IV = iv.iv;
        }

        public void EncryptFileWithPadding(FST fst, string outputFilename, short contentID, int blockSize)
        {
            using FileStream output = new FileStream(outputFilename, FileMode.Create);

            MemoryStream input = new MemoryStream(fst.GetAsData());
            BigEndianMemoryStream ivStream = new BigEndianMemoryStream(0x10);
            ivStream.WriteBigEndian(contentID);
            IV iv = new IV(ivStream.GetBuffer());

            EncryptSingleFile(input, output, fst.GetDataSize(), iv, blockSize);
        }

        public void EncryptFileWithPadding(FileStream input, int contentID, FileStream output, int blockSize)
        {
            BigEndianMemoryStream ivStream = new BigEndianMemoryStream(0x10);
            ivStream.WriteBigEndian((short)contentID);
            IV iv = new IV(ivStream.GetBuffer());

            EncryptSingleFile(input, output, input.Length, iv, blockSize);
        }

        private void EncryptSingleFile(Stream input, Stream output, long inputLength, IV iv, int blockSize)
        {
            aes.IV = iv.iv;
            long targetSize = Utils.Align(inputLength, blockSize);

            int cur_position = 0;
            do
            {
                byte[] blockBuffer = new byte[blockSize];

                // Read up to `blockSize` bytes from the input
                int bytesRead = input.Read(blockBuffer, 0, blockSize);

                if (bytesRead == 0) // End of stream
                    break;

                // Encrypt the block of data
                byte[] encryptedBlock = Encrypt(blockBuffer);

                // Update the IV for the next block
                aes.IV = Utils.CopyOfRange(encryptedBlock, blockSize - 16, blockSize);

                // Write only the data that was read (not necessarily the whole block size)
                output.Write(encryptedBlock, 0, bytesRead);

                cur_position += bytesRead; // Update position with actual bytes processed
            } while (cur_position < targetSize);
        }


        public void EncryptFileHashed(FileStream input, int contentID, FileStream output, ContentHashes hashes)
        {
            EncryptFileHashed(input, output, input.Length, contentID, hashes);
        }

        private void EncryptFileHashed(Stream input, Stream output, long length, int contentID, ContentHashes hashes)
        {
            const int hashBlockSize = 0xFC00;

            byte[] buffer = new byte[hashBlockSize];
            int read;
            int block = 0;
            do
            {
                read = input.Read(buffer, 0, hashBlockSize);

                byte[] encryptedData = EncryptChunkHashed(buffer, block, hashes, contentID);
                output.Write(encryptedData, 0, encryptedData.Length);

                block++;
                if (block % 100 == 0)
                {
                    Console.Write($"\rEncryption: {(int)(100.0 * block * hashBlockSize / length)}%");
                }
            } while (read == hashBlockSize);
            Console.WriteLine("\rEncryption: 100%");
        }

        private byte[] EncryptChunkHashed(byte[] buffer, int block, ContentHashes hashes, int contentID)
        {
            BigEndianMemoryStream ivStream = new BigEndianMemoryStream(16);
            ivStream.WriteBigEndian((short) contentID);
            aes.IV = ivStream.GetBuffer();
            byte[] decryptedHashes = hashes.GetHashForBlock(block);
            decryptedHashes[1] ^= (byte)contentID;

            byte[] encryptedhashes = Encrypt(decryptedHashes);
            decryptedHashes[1] ^= (byte)contentID;
            int iv_start = (block % 16) * 20;

            aes.IV = Utils.CopyOfRange(decryptedHashes, iv_start, iv_start + 16);

            byte[] encryptedContent = Encrypt(buffer);
            MemoryStream outputStream = new MemoryStream(0x10000);
            outputStream.Write(encryptedhashes, 0, encryptedhashes.Length);
            outputStream.Write(encryptedContent, 0, encryptedContent.Length);

            return outputStream.GetBuffer();
        }

        public byte[] Encrypt(byte[] input)
        {
            return aes.CreateEncryptor().TransformFinalBlock(input, 0, input.Length);
        }
    }
}
