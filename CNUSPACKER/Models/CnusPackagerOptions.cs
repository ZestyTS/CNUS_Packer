namespace CNUSPACKER.Models
{
    public class CnusPackagerOptions
    {
        public string InputPath { get; set; } = "input";
        public string OutputPath { get; set; } = "output";
        public string EncryptionKey { get; set; } = "";
        public string EncryptKeyWith { get; set; } = "";
        public long TitleID { get; set; } = 0;
        public long OSVersion { get; set; } = 0x000500101000400AL;
        public uint AppType { get; set; } = 0x80000000;
        public short TitleVersion { get; set; } = 0;
        public bool SkipXmlParsing { get; set; } = false;
    }
}