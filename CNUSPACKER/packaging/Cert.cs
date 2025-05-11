using System.IO;
using CNUSPACKER.utils;

namespace CNUSPACKER.packaging
{
    public static class Cert
    {
        public static byte[] GetCertAsData()
        {
            MemoryStream buffer = new MemoryStream(0xA00);

            byte[] hexData = Utils.HexStringToByteArray("00010003");
            buffer.Write(hexData, 0, hexData.Length);

            buffer.Seek(0x400, SeekOrigin.Begin);
            hexData = Utils.HexStringToByteArray("00010004");
            buffer.Write(hexData, 0, hexData.Length);

            buffer.Seek(0x700, SeekOrigin.Begin);
            hexData = Utils.HexStringToByteArray("00010004");
            buffer.Write(hexData, 0, hexData.Length);

            buffer.Seek(0x240, SeekOrigin.Begin);
            hexData = Utils.HexStringToByteArray("526F6F74000000000000000000000000");
            buffer.Write(hexData, 0, hexData.Length);

            buffer.Seek(0x280, SeekOrigin.Begin);
            hexData = Utils.HexStringToByteArray("00000001434130303030303030330000");
            buffer.Write(hexData, 0, hexData.Length);

            buffer.Seek(0x540, SeekOrigin.Begin);
            hexData = Utils.HexStringToByteArray("526F6F742D4341303030303030303300");
            buffer.Write(hexData, 0, hexData.Length);

            buffer.Seek(0x580, SeekOrigin.Begin);
            hexData = Utils.HexStringToByteArray("00000001435030303030303030620000");
            buffer.Write(hexData, 0, hexData.Length);

            buffer.Seek(0x840, SeekOrigin.Begin);
            hexData = Utils.HexStringToByteArray("526F6F742D4341303030303030303300");
            buffer.Write(hexData, 0, hexData.Length);

            buffer.Seek(0x880, SeekOrigin.Begin);
            hexData = Utils.HexStringToByteArray("00000001585330303030303030630000");
            buffer.Write(hexData, 0, hexData.Length);

            return buffer.GetBuffer();
        }
    }
}
