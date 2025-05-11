using System.Collections.Generic;
using CNUSPACKER.Configuration;

namespace CNUSPACKER.Packaging
{
    /// <summary>
    /// Represents a rule for assigning file entries to content definitions.
    /// </summary>
    public class ContentRule
    {
        /// <summary>
        /// Gets the regex pattern used to match file paths.
        /// </summary>
        public string Pattern { get; }

        /// <summary>
        /// Gets the content metadata to apply when the pattern matches.
        /// </summary>
        public ContentDetails Details { get; }

        /// <summary>
        /// Gets a value indicating whether to create a new content section for every match.
        /// </summary>
        public bool ContentPerMatch { get; }

        private ContentRule(string pattern, ContentDetails details, bool contentPerMatch = false)
        {
            Pattern = pattern;
            Details = details;
            ContentPerMatch = contentPerMatch;
        }

        /// <summary>
        /// Returns a list of common rules for Wii U packaging content layout.
        /// </summary>
        public static List<ContentRule> GetCommonRules(short contentGroup, long titleID)
        {
            var s = Settings.Default;

            var commonRules = new List<ContentRule>
            {
                new ContentRule(@"^/code/app\.xml$", new ContentDetails(false, s.GroupIdCode, 0x0L, s.FstFlagsCode)),
                new ContentRule(@"^/code/cos\.xml$", new ContentDetails(false, s.GroupIdCode, 0x0L, s.FstFlagsCode)),
                new ContentRule(@"^/meta/meta\.xml$", new ContentDetails(true, s.GroupIdMeta, 0x0L, s.FstFlagsMeta)),
                new ContentRule(@"^/meta/((?!\.xml).)*$", new ContentDetails(true, s.GroupIdMeta, 0x0L, s.FstFlagsMeta)),
                new ContentRule(@"^/meta/bootMovie\.h264$", new ContentDetails(true, s.GroupIdMeta, 0x0L, s.FstFlagsMeta)),
                new ContentRule(@"^/meta/bootLogoTex\.tga$", new ContentDetails(true, s.GroupIdMeta, 0x0L, s.FstFlagsMeta)),
                new ContentRule(@"^/meta/Manual\.bfma$", new ContentDetails(true, s.GroupIdMeta, 0x0L, s.FstFlagsMeta)),
                new ContentRule(@"^/meta/.*\.jpg$", new ContentDetails(true, s.GroupIdMeta, 0x0L, s.FstFlagsMeta)),
                new ContentRule(@"/code/.*(\.rpx|\.rpl)$", new ContentDetails(true, s.GroupIdCode, 0x0L, s.FstFlagsCode), true),
                new ContentRule(@"^/code/preload\.txt$", new ContentDetails(true, s.GroupIdCode, 0x0L, s.FstFlagsCode)),
                new ContentRule(@"^/code/fw\.img$", new ContentDetails(false, s.GroupIdCode, 0x0L, s.FstFlagsCode)),
                new ContentRule(@"^/code/fw\.tmd$", new ContentDetails(false, s.GroupIdCode, 0x0L, s.FstFlagsCode)),
                new ContentRule(@"^/code/htk\.bin$", new ContentDetails(false, s.GroupIdCode, 0x0L, s.FstFlagsCode)),
                new ContentRule(@"^/code/rvlt\.tik$", new ContentDetails(false, s.GroupIdCode, 0x0L, s.FstFlagsCode)),
                new ContentRule(@"^/code/rvlt\.tmd$", new ContentDetails(false, s.GroupIdCode, 0x0L, s.FstFlagsCode)),
                new ContentRule(@"^/content/.*$", new ContentDetails(true, contentGroup, titleID, s.FstFlagsContent))
            };

            return commonRules;
        }
    }
}
