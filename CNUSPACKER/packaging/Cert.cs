using System.IO;

namespace CNUSPACKER.Packaging
{
    /// <summary>
    /// Provides a static method to generate a certificate block used in packaging.
    /// </summary>
    public static class Cert
    {
        public static byte[] GetCertAsData()
        {
            var certData = new (int Offset, string Hex)[]
            {
                (0x000, "00010003"),
                (0x400, "00010004"),
                (0x700, "00010004"),
                (0x240, "526F6F74000000000000000000000000"),
                (0x280, "00000001434130303030303030330000"),
                (0x540, "526F6F742D4341303030303030303300"),
                (0x580, "00000001435030303030303030620000"),
                (0x840, "526F6F742D4341303030303030303300"),
                (0x880, "00000001585330303030303030630000")
            };

            using var buffer = new MemoryStream(0xA00);
            foreach (var (Offset, Hex) in certData)
            {
                buffer.Seek(Offset, SeekOrigin.Begin);
                byte[] data = Utils.Utils.HexStringToByteArray(Hex);
                buffer.Write(data, 0, data.Length);
            }

            return buffer.ToArray();
        }
    }
}
