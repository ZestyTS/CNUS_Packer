using System;
using System.Collections.Generic;
using System.IO;
using CNUSPACKER.crypto;
using CNUSPACKER.packaging;
using CNUSPACKER.utils;

namespace CNUSPACKER
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

    public class CnusPackagerRunner
    {
        public void Run(CnusPackagerOptions options)
        {
            ValidateInputFolders(options.InputPath);

            var appInfo = new AppXMLInfo
            {
                titleID = options.TitleID,
                groupID = (short)(options.TitleID >> 8),
                appType = options.AppType,
                osVersion = options.OSVersion,
                titleVersion = options.TitleVersion
            };

            string encryptionKey = string.IsNullOrWhiteSpace(options.EncryptionKey) || options.EncryptionKey.Length != 32
                ? Settings.defaultEncryptionKey
                : options.EncryptionKey;

            string encryptKeyWith = string.IsNullOrWhiteSpace(options.EncryptKeyWith) || options.EncryptKeyWith.Length != 32
                ? LoadEncryptWithKey()
                : options.EncryptKeyWith;

            if (string.IsNullOrWhiteSpace(encryptKeyWith) || encryptKeyWith.Length != 32)
                encryptKeyWith = Settings.defaultEncryptWithKey;

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
                    throw new Exception($"Error parsing app.xml: {e.Message}", e);
                }
            }

            long parentID = appInfo.titleID & ~0x0000000F00000000L;
            short contentGroup = appInfo.groupID;
            List<ContentRule> rules = ContentRule.GetCommonRules(contentGroup, parentID);

            Directory.CreateDirectory(Settings.tmpDir);

            var config = new NusPackageConfiguration(options.InputPath, appInfo, new Key(encryptionKey), new Key(encryptKeyWith), rules);
            var nuspackage = NUSPackageFactory.CreateNewPackage(config);
            nuspackage.PackContents(options.OutputPath);
            nuspackage.PrintTicketInfos();

            Utils.DeleteDir(Settings.tmpDir);
        }

        private static void ValidateInputFolders(string inputPath)
        {
            if (!Directory.Exists(Path.Combine(inputPath, "code")) ||
                !Directory.Exists(Path.Combine(inputPath, "content")) ||
                !Directory.Exists(Path.Combine(inputPath, "meta")))
            {
                throw new DirectoryNotFoundException($"Invalid input directory: \"{Path.GetFullPath(inputPath)}\". Missing 'code', 'content', or 'meta'.");
            }
        }

        private static string LoadEncryptWithKey()
        {
            if (!File.Exists(Settings.encryptWithFile))
                return "";

            try
            {
                using var reader = new StreamReader(Settings.encryptWithFile);
                return reader.ReadLine() ?? "";
            }
            catch
            {
                return "";
            }
        }
    }
}
