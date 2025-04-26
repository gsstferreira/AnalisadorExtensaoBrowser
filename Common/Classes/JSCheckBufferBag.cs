using SharpZipLib.Tar;
using System.Buffers;
using System.IO.Compression;
using System.Text;

namespace Common.Classes
{
    public class JSCheckBufferBag
    {
        public MemoryStream DownloadStream {  get; set; }
        public Dictionary<ulong, int> ContentProfile { get; set; }
        private TarInputStream TarStream { get; set; }
        private TarInputStream GzipTarStream { get; set; }
        private byte[] ByteBuffer { get; set; }

        public JSCheckBufferBag(int dictionaryStartCap) 
        {
            DownloadStream = new MemoryStream();
            ByteBuffer = ArrayPool<byte>.Shared.Rent(2);

            TarStream = new (DownloadStream,Encoding.Default);
            GzipTarStream = new (DownloadStream, Encoding.Default);
            ContentProfile = new(dictionaryStartCap);
        }
        public TarInputStream SetTarReading()
        {
            DownloadStream.Seek(0, SeekOrigin.Begin);
            DownloadStream.Read(ByteBuffer, 0, 2);
            DownloadStream.Seek(0, SeekOrigin.Begin);

            if (ByteBuffer[0] == 0x1F && ByteBuffer[1] == 0x8B)
            {
                GzipTarStream.ReturnArrays();

                GzipTarStream = new TarInputStream(new GZipStream(DownloadStream, CompressionMode.Decompress), Encoding.Default);

                return GzipTarStream;
            }
            else
            {
                TarStream.Reset();
                return TarStream;
            }
        }
        public void Clear()
        {
            DownloadStream.SetLength(0);
            ContentProfile.Clear();
        }

        public void Close()
        {
            DownloadStream.Dispose();
            TarStream.ReturnArrays();
            ArrayPool<byte>.Shared.Return(ByteBuffer);
        }
    }
}
