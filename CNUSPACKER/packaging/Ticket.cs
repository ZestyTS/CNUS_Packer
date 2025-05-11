using System;
using System.IO;
using CNUSPACKER.crypto;
using CNUSPACKER.utils;

namespace CNUSPACKER.packaging
{
    public class Ticket
    {
        public readonly long titleID;
        public readonly Key decryptedKey;
        public readonly Key encryptWith;

        public Ticket(long titleID, Key decryptedKey, Key encryptWith)
        {
            this.titleID = titleID;
            this.decryptedKey = decryptedKey;
            this.encryptWith = encryptWith;
        }

        public byte[] GetAsData()
        {
            Random rdm = new Random();
            BigEndianMemoryStream buffer = new BigEndianMemoryStream(0x350);

            byte[] hexData = Utils.HexStringToByteArray("00010004");
            buffer.Write(hexData, 0, hexData.Length);

            byte[] randomData = new byte[0x100];
            rdm.NextBytes(randomData);
            buffer.Write(randomData, 0, randomData.Length);

            buffer.Seek(0x3C, SeekOrigin.Current);

            hexData = Utils.HexStringToByteArray("526F6F742D434130303030303030332D58533030303030303063000000000000");
            buffer.Write(hexData, 0, hexData.Length);

            buffer.Seek(0x5C, SeekOrigin.Current);

            hexData = Utils.HexStringToByteArray("010000");
            buffer.Write(hexData, 0, hexData.Length);

            byte[] encryptedKey = GetEncryptedKey().key;
            buffer.Write(encryptedKey, 0, encryptedKey.Length);

            hexData = Utils.HexStringToByteArray("000005");
            buffer.Write(hexData, 0, hexData.Length);

            randomData = new byte[0x06];
            rdm.NextBytes(randomData);
            buffer.Write(randomData, 0, randomData.Length);

            buffer.Seek(0x04, SeekOrigin.Current);

            buffer.WriteBigEndian(titleID);

            hexData = Utils.HexStringToByteArray("00000011000000000000000000000005");
            buffer.Write(hexData, 0, hexData.Length);

            buffer.Seek(0xB0, SeekOrigin.Current);

            hexData = Utils.HexStringToByteArray("00010014000000AC000000140001001400000000000000280000000100000084000000840003000000000000FFFFFF01");
            buffer.Write(hexData, 0, hexData.Length);

            buffer.Seek(0x7C, SeekOrigin.Current);

            return buffer.GetBuffer();
        }


        public Key GetEncryptedKey()
        {
            BigEndianMemoryStream ivStream = new BigEndianMemoryStream(0x10);
            ivStream.WriteBigEndian(titleID);
            Encryption encrypt = new Encryption(encryptWith, new IV(ivStream.GetBuffer()));

            return new Key(encrypt.Encrypt(decryptedKey.key));
        }
    }
}
