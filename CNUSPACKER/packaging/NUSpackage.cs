using System.IO;
using System.Threading.Tasks;
using CNUSPACKER.Crypto;
using CNUSPACKER.Utils;
using Microsoft.Extensions.Logging;

namespace CNUSPACKER.Packaging
{
    /// <summary>
    /// Represents the core package builder for WUP content.
    /// </summary>
    public class NUSpackage
    {
        private readonly FST _fst;
        private readonly Ticket _ticket;
        private readonly TMD _tmd;
        private readonly ILogger<NUSpackage>? _logger;

        public NUSpackage(FST fst, Ticket ticket, TMD tmd, ILogger<NUSpackage>? logger = null)
        {
            _fst = fst;
            _ticket = ticket;
            _tmd = tmd;
            _logger = logger;
        }

        /// <summary>
        /// Packs all content files, encrypts them, and writes metadata files.
        /// </summary>
        public async Task PackContentsAsync(string outputDir)
        {
            _logger?.LogInformation("Packing contents to directory: {OutputDir}", outputDir);

            Encryption encryption = _tmd.GetEncryption();
            await _fst.Contents.PackContentsAsync(outputDir, encryption);

            _logger?.LogInformation("Packing FST as 00000000.app");
            string fstPath = Path.Combine(outputDir, "00000000.app");
            await encryption.EncryptFileWithPaddingAsync(_fst, fstPath, 0, Content.ContentFilePadding);

            Content fstContent = _fst.Contents.FstContent;
            fstContent.Sha1 = HashUtil.HashSHA1(_fst.GetAsData());
            fstContent.EncryptedFileSize = _fst.GetDataSize();

            _tmd.contentInfo.Sha2Hash = HashUtil.HashSHA2(_fst.Contents.GetAsData());
            _tmd.UpdateContentInfoHash();

            await WriteFileAsync(Path.Combine(outputDir, "title.tmd"), await _tmd.GetAsDataAsync(), "TMD");
            await WriteFileAsync(Path.Combine(outputDir, "title.cert"), Cert.GetCertAsData(), "Cert");
            await WriteFileAsync(Path.Combine(outputDir, "title.tik"), await _ticket.GetAsDataAsync(), "Ticket");
        }

        /// <summary>
        /// Logs summary of ticket encryption keys.
        /// </summary>
        public void PrintTicketInfos()
        {
            _logger?.LogInformation("Encrypted with key         : {Key}", _ticket.DecryptedKey);
            _logger?.LogInformation("Key encrypted with         : {With}", _ticket.EncryptWith);
            _logger?.LogInformation("Encrypted key              : {EncryptedKey}", _ticket.GetEncryptedKey());
        }

        private async Task WriteFileAsync(string path, byte[] data, string label)
        {
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            await fs.WriteAsync(data, 0, data.Length);
            _logger?.LogInformation("{Label} saved to {Path}", label, path);
        }

    }
}
