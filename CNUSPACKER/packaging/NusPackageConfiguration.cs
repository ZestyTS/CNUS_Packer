using System.Collections.Generic;
using CNUSPACKER.Crypto;
using CNUSPACKER.Models;

namespace CNUSPACKER.Packaging
{
    /// <summary>
    /// Represents configuration settings for building a NUSpackage.
    /// </summary>
    public class NusPackageConfiguration
    {
        /// <summary>
        /// Gets the input directory.
        /// </summary>
        public string Dir { get; }

        /// <summary>
        /// Gets the parsed app.xml info.
        /// </summary>
        public AppXMLInfo AppInfo { get; }

        /// <summary>
        /// Gets the content encryption key.
        /// </summary>
        public Key EncryptionKey { get; }

        /// <summary>
        /// Gets the key used to encrypt the encryption key.
        /// </summary>
        public Key EncryptKeyWith { get; }

        /// <summary>
        /// Gets the set of content rules to apply.
        /// </summary>
        public List<ContentRule> Rules { get; }

        public NusPackageConfiguration(string dir, AppXMLInfo appInfo, Key encryptionKey, Key encryptKeyWith, List<ContentRule> rules)
        {
            Dir = dir;
            AppInfo = appInfo;
            EncryptionKey = encryptionKey;
            EncryptKeyWith = encryptKeyWith;
            Rules = rules;
        }
    }
}
