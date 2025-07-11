using System.IO;
using System.Threading.Tasks;
using CNUSPACKER.FST;
using Microsoft.Extensions.Logging;

namespace CNUSPACKER.Packaging
{
    /// <summary>
    /// Responsible for building a fully populated NUSpackage from configuration.
    /// </summary>
    public static class NUSPackageFactory
    {
        /// <summary>
        /// Constructs a new NUSpackage using the given configuration.
        /// </summary>
        public static async Task<NUSpackage> CreateNewPackageAsync(NusPackageConfiguration config, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<NUSpackage>();
            var contents = new Contents();
            var fst = new FST(contents);

            FSTEntry root = fst.FileEntries.GetRootEntry();
            root.SetContent(contents.FstContent);

            await PopulateFSTEntriesAsync(config.Dir, root, fst);
            logger.LogInformation("Finished reading input files. Applying content rules...");

            var ruleServiceLogger = loggerFactory.CreateLogger<ContentRulesService>();
            var ruleService = new ContentRulesService(ruleServiceLogger);
            ruleService.ApplyRules(root, contents, config.Rules);

            logger.LogInformation("Generating the FST...");
            fst.Update();

            logger.LogInformation("Generating the Ticket...");
            var ticket = new Ticket(config.AppInfo.TitleID, config.EncryptionKey, config.EncryptKeyWith);

            logger.LogInformation("Generating the TMD...");
            var tmd = new TMD(config.AppInfo, fst, ticket);

            return new NUSpackage(fst, ticket, tmd, logger);
        }

        private static async Task PopulateFSTEntriesAsync(string directory, FSTEntry parent, FST fst)
        {
            foreach (var file in Directory.EnumerateFiles(directory))
            {
                var entry = new FSTEntry(file, fst);
                parent.AddChild(entry);
                await Task.Yield();
            }

            foreach (var dir in Directory.EnumerateDirectories(directory))
            {
                var entry = new FSTEntry(dir, fst);
                parent.AddChild(entry);
                await PopulateFSTEntriesAsync(dir, entry, fst);
            }
        }

    }
}
