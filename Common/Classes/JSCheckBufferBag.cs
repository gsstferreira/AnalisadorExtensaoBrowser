using ICSharpCode.SharpZipLib.Tar;
using System.IO.Compression;
using System.Text;

namespace Common.Classes
{
    public class JSCheckBufferBag
    {
        public MemoryStream DownloadStream {  get; set; }
        public MemoryStream EntryStream { get; set; }
        public Dictionary<ulong, int> ContentProfile { get; set; }
        private StreamReader Reader { get; set; }
        private ExtendedTarStream TarStream { get; set; }
        private byte[] ByteBuffer { get; set; }
        public JSCheckBufferBag() 
        {
            DownloadStream = new MemoryStream();
            EntryStream = new MemoryStream();
            ByteBuffer = new byte[2];

            Reader = new StreamReader(EntryStream);

            TarStream = new ExtendedTarStream(DownloadStream);
            ContentProfile = [];
        }

        public TarInputStream SetTarReading()
        {
            DownloadStream.Seek(0, SeekOrigin.Begin);
            DownloadStream.Read(ByteBuffer, 0, 2);
            DownloadStream.Seek(0, SeekOrigin.Begin);

            if (ByteBuffer[0] == 0x1F && ByteBuffer[1] == 0x8B)
            {
                return new TarInputStream(new GZipStream(DownloadStream,CompressionMode.Decompress),Encoding.Default);
            }
            else
            {
                TarStream.Restart();
                return TarStream;
            }
        }
        public void Clear()
        {
            DownloadStream.SetLength(0);
            EntryStream.SetLength(0);
            ContentProfile.Clear();
        }

        public string GetEntry(TarInputStream stream)
        {
            EntryStream.SetLength(0);
            stream.CopyEntryContents(EntryStream);
            EntryStream.Seek(0, SeekOrigin.Begin);

            return Reader.ReadToEnd();
        }

        public void Close()
        {
            DownloadStream.Dispose();
            EntryStream.Dispose();
        }
    }
}
