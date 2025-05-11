using System.Collections.Generic;

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
            var commonRules = new List<ContentRule>
            {
                new ContentRule(@"^/code/app\.xml$", new ContentDetails(false, Settings.groupid_code, 0x0L, Settings.fstflags_code)),
                new ContentRule(@"^/code/cos\.xml$", new ContentDetails(false, Settings.groupid_code, 0x0L, Settings.fstflags_code)),
                new ContentRule(@"^/meta/meta\.xml$", new ContentDetails(true, Settings.groupid_meta, 0x0L, Settings.fstflags_meta)),
                new ContentRule(@"^/meta/((?!\.xml).)*$", new ContentDetails(true, Settings.groupid_meta, 0x0L, Settings.fstflags_meta)),
                new ContentRule(@"^/meta/bootMovie\.h264$", new ContentDetails(true, Settings.groupid_meta, 0x0L, Settings.fstflags_meta)),
                new ContentRule(@"^/meta/bootLogoTex\.tga$", new ContentDetails(true, Settings.groupid_meta, 0x0L, Settings.fstflags_meta)),
                new ContentRule(@"^/meta/Manual\.bfma$", new ContentDetails(true, Settings.groupid_meta, 0x0L, Settings.fstflags_meta)),
                new ContentRule(@"^/meta/.*\.jpg$", new ContentDetails(true, Settings.groupid_meta, 0x0L, Settings.fstflags_meta)),
                new ContentRule(@"/code/.*(\.rpx|\.rpl)$", new ContentDetails(true, Settings.groupid_code, 0x0L, Settings.fstflags_code), true),
                new ContentRule(@"^/code/preload\.txt$", new ContentDetails(true, Settings.groupid_code, 0x0L, Settings.fstflags_code)),
                new ContentRule(@"^/code/fw\.img$", new ContentDetails(false, Settings.groupid_code, 0x0L, Settings.fstflags_code)),
                new ContentRule(@"^/code/fw\.tmd$", new ContentDetails(false, Settings.groupid_code, 0x0L, Settings.fstflags_code)),
                new ContentRule(@"^/code/htk\.bin$", new ContentDetails(false, Settings.groupid_code, 0x0L, Settings.fstflags_code)),
                new ContentRule(@"^/code/rvlt\.tik$", new ContentDetails(false, Settings.groupid_code, 0x0L, Settings.fstflags_code)),
                new ContentRule(@"^/code/rvlt\.tmd$", new ContentDetails(false, Settings.groupid_code, 0x0L, Settings.fstflags_code)),
                new ContentRule(@"^/content/.*$", new ContentDetails(true, contentGroup, titleID, Settings.fstflags_content))
            };

            return commonRules;
        }
    }
}
