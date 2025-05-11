using System;
using System.Collections.Generic;
using System.IO;
using CNUSPACKER.Crypto;
using CNUSPACKER.Models;
using CNUSPACKER.Packaging;
using CNUSPACKER.Utils;
using Microsoft.Extensions.Logging;

namespace CNUSPACKER
{
    /// <summary>
    /// Responsible for creating a WUP package based on user-defined options.
    /// </summary>
    public class CnusPackagerRunner
    {
        private readonly ILogger<CnusPackagerRunner> _logger;

        /// <summary>
        /// Creates a new instance of the packager runner.
        /// </summary>
        /// <param name="logger">Logger used for diagnostic and status output.</param>
        public CnusPackagerRunner(ILogger<CnusPackagerRunner> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes the packager using the given options.
        /// </summary>
        /// <param name="options">Settings and input values to use.</param>
        public void Run(CnusPackagerOptions options)
        {
            ValidateInputFolders(options.InputPath);

            var appInfo = new AppXMLInfo
            {
                TitleID = options.TitleID,
                GroupID = (short)(options.TitleID >> 8),
                AppType = options.AppType,
                OSVersion = options.OSVersion,
                TitleVersion = options.TitleVersion
            };

            string encryptionKey = ValidateOrFallbackKey(options.EncryptionKey, Settings.defaultEncryptionKey, "encryptionKey");
            string encryptKeyWith = ValidateOrFallbackKey(options.EncryptKeyWith, LoadEncryptWithKey(), "encryptKeyWith");

            if (string.IsNullOrWhiteSpace(encryptKeyWith) || encryptKeyWith.Length != 32)
            {
                _logger.LogWarning("Empty or invalid encryptWith key provided. Falling back to default.");
                encryptKeyWith = Settings.defaultEncryptWithKey;
            }

            if (!options.SkipXmlParsing)
            {
                string appXmlPath = Path.Combine(options.InputPath, Settings.pathToAppXml);
                try
                {
                    var parser = new XMLParser();
                    parser.LoadDocument(appXmlPath);
                    appInfo = parser.GetAppXMLInfo();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Error parsing app.xml: {Settings.pathToAppXml}");
                }
            }
            else
            {
                _logger.LogInformation("XML parsing was skipped by request.");
            }

            long parentID = appInfo.TitleID & ~0x0000000F00000000L;
            short contentGroup = appInfo.GroupID;
            var rules = ContentRule.GetCommonRules(contentGroup, parentID);

            Directory.CreateDirectory(Settings.tmpDir);

            var config = new NusPackageConfiguration(options.InputPath, appInfo, new Key(encryptionKey), new Key(encryptKeyWith), rules);
            var nuspackage = NUSPackageFactory.CreateNewPackage(config);
            nuspackage.PackContents(options.OutputPath);
            nuspackage.PrintTicketInfos();

            Utils.Utils.DeleteDir(Settings.tmpDir);
        }

        private string ValidateOrFallbackKey(string key, string fallback, string name)
        {
            if (string.IsNullOrWhiteSpace(key) || key.Length != 32)
            {
                _logger.LogWarning($"{name} is empty or invalid. Using fallback value.");
                return fallback;
            }
            return key;
        }

        private static void ValidateInputFolders(string inputPath)
        {
            if (!Directory.Exists(Path.Combine(inputPath, "code")) ||
                !Directory.Exists(Path.Combine(inputPath, "content")) ||
                !Directory.Exists(Path.Combine(inputPath, "meta")))
            {
                throw new DirectoryNotFoundException($"Input directory '{Path.GetFullPath(inputPath)}' is missing required subfolders: code, content, meta.");
            }
        }

        private string LoadEncryptWithKey()
        {
            if (!File.Exists(Settings.encryptWithFile))
                return "";

            try
            {
                using var reader = new StreamReader(Settings.encryptWithFile);
                return reader.ReadLine() ?? "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to read encryption key file: {Settings.encryptWithFile}");
                return "";
            }
        }
    }
}
