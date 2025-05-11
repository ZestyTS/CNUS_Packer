namespace CNUSPACKER.Models
{
    /// <summary>
    /// Holds options used when executing the CNUS packager process.
    /// </summary>
    public class CnusPackagerOptions
    {
        /// <summary>The root input folder containing code, content, and meta.</summary>
        public string InputPath { get; set; } = "input";

        /// <summary>The output folder where the WUP package will be created.</summary>
        public string OutputPath { get; set; } = "output";

        /// <summary>The 32-character key used to encrypt the package.</summary>
        public string EncryptionKey { get; set; } = string.Empty;

        /// <summary>The 32-character key used to encrypt the encryption key.</summary>
        public string EncryptKeyWith { get; set; } = string.Empty;

        /// <summary>The full 16-character Title ID for the WUP package.</summary>
        public long TitleID { get; set; } = 0;

        /// <summary>The OS version this package is targeting.</summary>
        public long OSVersion { get; set; } = 0x000500101000400AL;

        /// <summary>The app type for the WUP package (e.g., game, update, DLC).</summary>
        public uint AppType { get; set; } = 0x80000000;

        /// <summary>The version of the title being packaged.</summary>
        public short TitleVersion { get; set; } = 0;

        /// <summary>Whether to skip app.xml parsing and rely entirely on CLI-supplied options.</summary>
        public bool SkipXmlParsing { get; set; } = false;
    }
}
