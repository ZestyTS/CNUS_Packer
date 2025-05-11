using System;
using System.IO;
using CNUSPACKER.Crypto;
using CNUSPACKER.Utils;

namespace CNUSPACKER.Packaging
{
    /// <summary>
    /// Generates the ticket (title.tik) containing the encrypted title key and other metadata.
    /// </summary>
    public class Ticket
    {
        public long TitleID { get; }
        public Key DecryptedKey { get; }
        public Key EncryptWith { get; }

        public Ticket(long titleID, Key decryptedKey, Key encryptWith)
        {
            TitleID = titleID;
            DecryptedKey = decryptedKey;
            EncryptWith = encryptWith;
        }

        public byte[] GetAsData()
        {
            var random = new Random();
            var buffer = new BigEndianMemoryStream(0x350);

            buffer.Write(Utils.Utils.HexStringToByteArray("00010004"), 0, 4);

            byte[] randomData = new byte[0x100];
            random.NextBytes(randomData);
            buffer.Write(randomData, 0, randomData.Length);

            buffer.Seek(0x3C, SeekOrigin.Current);

            var issuer = Utils.Utils.HexStringToByteArray("526F6F742D434130303030303030332D58533030303030303063000000000000");
            buffer.Write(issuer, 0, issuer.Length);

            buffer.Seek(0x5C, SeekOrigin.Current);
            buffer.Write(Utils.Utils.HexStringToByteArray("010000"), 0, 3);

            var encryptedKey = GetEncryptedKey().key;
            buffer.Write(encryptedKey, 0, encryptedKey.Length);

            buffer.Write(Utils.Utils.HexStringToByteArray("000005"), 0, 3);
            randomData = new byte[0x06];
            random.NextBytes(randomData);
            buffer.Write(randomData, 0, randomData.Length);

            buffer.Seek(0x04, SeekOrigin.Current);
            buffer.WriteBigEndian(TitleID);

            buffer.Write(Utils.Utils.HexStringToByteArray("00000011000000000000000000000005"), 0, 17);

            buffer.Seek(0xB0, SeekOrigin.Current);
            buffer.Write(Utils.Utils.HexStringToByteArray("00010014000000AC000000140001001400000000000000280000000100000084000000840003000000000000FFFFFF01"), 0, 64);

            buffer.Seek(0x7C, SeekOrigin.Current);

            return buffer.GetBuffer();
        }

        public Key GetEncryptedKey()
        {
            var ivStream = new BigEndianMemoryStream(0x10);
            ivStream.WriteBigEndian(TitleID);
            var encryptor = new Encryption(EncryptWith, new IV(ivStream.GetBuffer()));

            return new Key(encryptor.Encrypt(DecryptedKey.key));
        }
    }
}