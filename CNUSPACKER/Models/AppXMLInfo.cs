namespace CNUSPACKER.Models
{
    /// <summary>
    /// Represents application metadata parsed from app.xml.
    /// </summary>
    public class AppXMLInfo
    {
        /// <summary>
        /// Gets or sets the OS version.
        /// </summary>
        public long OSVersion { get; set; }

        /// <summary>
        /// Gets or sets the title ID.
        /// </summary>
        public long TitleID { get; set; }

        /// <summary>
        /// Gets or sets the title version.
        /// </summary>
        public short TitleVersion { get; set; }

        /// <summary>
        /// Gets or sets the SDK version.
        /// </summary>
        public uint SDKVersion { get; set; }

        /// <summary>
        /// Gets or sets the application type.
        /// </summary>
        public uint AppType { get; set; }

        /// <summary>
        /// Gets or sets the group ID.
        /// </summary>
        public short GroupID { get; set; }

        /// <summary>
        /// Gets or sets the OS mask.
        /// </summary>
        public byte[] OSMask { get; set; } = new byte[32];

        /// <summary>
        /// Gets or sets the common ID.
        /// </summary>
        public long CommonID { get; set; }

        /// <summary>
        /// Returns a string representation of the metadata.
        /// </summary>
        public override string ToString()
        {
            return $"AppXMLInfo [OSVersion={OSVersion}, TitleID={TitleID}, TitleVersion={TitleVersion}, " +
                   $"SDKVersion={SDKVersion}, AppType={AppType}, GroupID={GroupID}, OSMask={Utils.Utils.ByteArrayToHexString(OSMask)}, CommonID={CommonID}]";
        }
    }
}