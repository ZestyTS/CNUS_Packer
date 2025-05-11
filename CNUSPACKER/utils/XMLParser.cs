using System.Xml;
using CNUSPACKER.Models;
using Microsoft.Extensions.Logging;

namespace CNUSPACKER.Utils
{
    /// <summary>
    /// Parses application metadata from an app.xml file.
    /// </summary>
    public class XMLParser
    {
        private readonly XmlDocument _document = new XmlDocument();
        private readonly ILogger<XMLParser>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="XMLParser"/> class.
        /// </summary>
        /// <param name="logger">Optional logger for diagnostics.</param>
        public XMLParser(ILogger<XMLParser>? logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Loads the XML document from the specified file path.
        /// </summary>
        /// <param name="path">Path to the app.xml file.</param>
        public void LoadDocument(string path)
        {
            _document.Load(path);
            _logger?.LogInformation("Loaded app.xml from path: {Path}", path);
        }

        /// <summary>
        /// Parses metadata from the XML document into an <see cref="AppXMLInfo"/> object.
        /// </summary>
        public AppXMLInfo GetAppXMLInfo()
        {
            return new AppXMLInfo
            {
                OSVersion = GetValueOfElementAsLongHex("app/os_version"),
                TitleID = GetValueOfElementAsLongHex("app/title_id"),
                TitleVersion = (short)GetValueOfElementAsLongHex("app/title_version"),
                SDKVersion = GetValueOfElementAsUnsignedInt("app/sdk_version"),
                AppType = (uint)GetValueOfElementAsLongHex("app/app_type"),
                GroupID = (short)GetValueOfElementAsLongHex("app/group_id"),
                OSMask = Utils.HexStringToByteArray(GetValueOfElement("app/os_mask") ?? string.Empty),
                CommonID = GetValueOfElementAsLongHex("app/common_id")
            };
        }

        private uint GetValueOfElementAsUnsignedInt(string element)
        {
            return uint.TryParse(GetValueOfElement(element), out var value) ? value : 0;
        }

        private long GetValueOfElementAsLongHex(string element)
        {
            string raw = GetValueOfElement(element);
            return long.TryParse(raw, System.Globalization.NumberStyles.HexNumber, null, out var value) ? value : 0;
        }

        private string GetValueOfElement(string xpath)
        {
            XmlNode node = _document.SelectSingleNode(xpath);
            if (node == null)
            {
                _logger?.LogWarning("Missing XML node for element: {XPath}. Default will be used.", xpath);
                return null;
            }

            return node.InnerText;
        }
    }
}
