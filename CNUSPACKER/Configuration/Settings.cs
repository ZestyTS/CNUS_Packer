namespace CNUSPACKER.Configuration
{
    /// <summary>
    /// Application-wide constant settings.
    /// </summary>
    public class Settings
    {
        public static readonly Settings Default = new Settings();

        public short GroupIdCode { get; set; } = 0x0000;
        public short GroupIdMeta { get; set; } = 0x0400;

        public short FstFlagsCode { get; set; } = 0x0000;
        public short FstFlagsMeta { get; set; } = 0x0040;
        public short FstFlagsContent { get; set; } = 0x0400;

        public string EncryptWithFile { get; set; } = "encryptKeyWith";
        public string DefaultEncryptionKey { get; set; } = "13371337133713371337133713371337";
        public string DefaultEncryptWithKey { get; set; } = "00000000000000000000000000000000";
        public string PathToAppXml { get; set; } = @"\code\app.xml";
        public string TmpDir { get; set; } = "tmp";
    }
}
