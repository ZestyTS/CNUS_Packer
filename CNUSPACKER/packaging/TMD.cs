// Refactored TMD.cs for clarity, structure, and documentation
using System.IO;
using CNUSPACKER.Contents;
using CNUSPACKER.Crypto;
using CNUSPACKER.Models;
using CNUSPACKER.Utils;

namespace CNUSPACKER.Packaging
{
    /// <summary>
    /// Represents the Title Metadata (TMD) structure for a WUP package.
    /// </summary>
    public class TMD
    {
        private const int SignatureType = 0x00010004;
        private readonly byte[] _signature = new byte[0x100];
        private static readonly byte[] Issuer = Utils.HexStringToByteArray("526F6F742D434130303030303030332D435030303030303030620000000000000000000000000000000000000000000000000000000000000000000000000000");

        private const byte Version = 0x01;
        private const byte CACRLVersion = 0x00;
        private const byte SignerCRLVersion = 0x00;
        private const int TitleType = 0x000100;
        private const int AccessRights = 0x0000;
        private const short BootIndex = 0x00;

        private readonly long _systemVersion;
        private readonly short _groupID;
        private readonly uint _appType;
        private readonly short _titleVersion;
        private readonly short _contentCount;
        private byte[] _sha2;

        public readonly ContentInfo contentInfo;
        private readonly Contents _contents;
        private readonly Ticket _ticket;

        public TMD(AppXMLInfo appInfo, FST.FST fst, Ticket ticket)
        {
            _groupID = appInfo.GroupID;
            _systemVersion = appInfo.OSVersion;
            _appType = appInfo.AppType;
            _titleVersion = appInfo.TitleVersion;
            _ticket = ticket;
            _contents = fst.Contents;
            _contentCount = _contents.GetContentCount();
            contentInfo = new ContentInfo(_contentCount)
            {
                SHA2Hash = HashUtil.HashSHA2(_contents.GetAsData())
            };
        }

        public void UpdateContentInfoHash()
        {
            _sha2 = HashUtil.HashSHA2(contentInfo.GetAsData());
        }

        public byte[] GetAsData()
        {
            var buffer = new BigEndianMemoryStream(GetDataSize());

            buffer.WriteBigEndian(SignatureType);
            buffer.Write(_signature, 0, _signature.Length);
            buffer.Seek(60, SeekOrigin.Current);
            buffer.Write(Issuer, 0, Issuer.Length);

            buffer.WriteByte(Version);
            buffer.WriteByte(CACRLVersion);
            buffer.WriteByte(SignerCRLVersion);
            buffer.Seek(1, SeekOrigin.Current);

            buffer.WriteBigEndian(_systemVersion);
            buffer.WriteBigEndian(_ticket.TitleID);
            buffer.WriteBigEndian(TitleType);
            buffer.WriteBigEndian(_groupID);
            buffer.WriteBigEndian(_appType);
            buffer.Seek(58, SeekOrigin.Current);
            buffer.WriteBigEndian(AccessRights);
            buffer.WriteBigEndian(_titleVersion);
            buffer.WriteBigEndian(_contentCount);
            buffer.WriteBigEndian(BootIndex);
            buffer.Seek(2, SeekOrigin.Current);

            buffer.Write(_sha2, 0, _sha2.Length);
            buffer.Write(contentInfo.GetAsData(), 0, ContentInfo.staticDataSize * 0x40);
            buffer.Write(_contents.GetAsData(), 0, _contents.GetDataSize());

            return buffer.GetBuffer();
        }

        private int GetDataSize()
        {
            const int StaticSize = 0x204;
            const int ContentInfoSize = 0x40 * ContentInfo.staticDataSize;
            int contentsSize = _contents.GetDataSize();

            return StaticSize + ContentInfoSize + contentsSize;
        }

        public Encryption GetEncryption()
        {
            var ivStream = new BigEndianMemoryStream(0x10);
            ivStream.WriteBigEndian(_ticket.TitleID);
            return new Encryption(_ticket.DecryptedKey, new IV(ivStream.GetBuffer()));
        }
    }
}