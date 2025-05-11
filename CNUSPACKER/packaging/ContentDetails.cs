// Refactored ContentDetails.cs with XML documentation

namespace CNUSPACKER.Packaging
{
    /// <summary>
    /// Represents metadata for an individual content block in the WUP package.
    /// </summary>
    public class ContentDetails
    {
        private const bool IsContent = true;
        private const bool IsEncrypted = true;

        /// <summary>
        /// Indicates whether this content is hashed (uses H0-H3).
        /// </summary>
        public bool IsHashed { get; }

        /// <summary>
        /// The group ID used in the TMD for this content.
        /// </summary>
        public short GroupID { get; }

        /// <summary>
        /// The parent title ID associated with this content.
        /// </summary>
        public long ParentTitleID { get; }

        /// <summary>
        /// The entry flag configuration for this content's FST records.
        /// </summary>
        public short EntriesFlag { get; }

        public ContentDetails(bool isHashed, short groupID, long parentTitleID, short entriesFlag)
        {
            IsHashed = isHashed;
            GroupID = groupID;
            ParentTitleID = parentTitleID;
            EntriesFlag = entriesFlag;
        }

        public override string ToString()
        {
            return $"ContentDetails [isContent={IsContent}, isEncrypted={IsEncrypted}, isHashed={IsHashed}, groupID={GroupID}, parentTitleID={ParentTitleID}, entriesFlag={EntriesFlag}]";
        }
    }
}