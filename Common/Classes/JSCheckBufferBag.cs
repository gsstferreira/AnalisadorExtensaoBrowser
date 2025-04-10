using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System;
using System.Text;

namespace Common.Classes
{
    public class JSCheckBufferBag
    {
        public MemoryStream DownloadStream {  get; set; }
        public MemoryStream EntryStream { get; set; }
        public StreamReader EntryReader { get; set; }
        private TarInputStream TarStream { get; set; }
        private TarInputStream TarStreamGzip {  get; set; }
        private GZipInputStream GzipDownloadStream { get; set; }
        private byte[] ByteBuffer { get; set; }

        public JSCheckBufferBag() 
        {
            DownloadStream = new MemoryStream();
            EntryStream = new MemoryStream();
            EntryReader = new StreamReader(EntryStream);
            GzipDownloadStream = new GZipInputStream(DownloadStream);

            TarStream = new TarInputStream(DownloadStream, Encoding.Default);
            TarStreamGzip = new TarInputStream(GzipDownloadStream, Encoding.Default);
            ByteBuffer = new byte[2];
        }

        public TarInputStream GetTarStream()
        {
            DownloadStream.Seek(0, SeekOrigin.Begin);
            DownloadStream.Read(ByteBuffer, 0, 2);
            DownloadStream.Seek(0, SeekOrigin.Begin);

            if(ByteBuffer[0] == 0x1F && ByteBuffer[1] == 0x8B)
            {
                return TarStreamGzip;
            }
            else
            {
                return TarStream;
            }
        }
        public string GetEntryContents(TarInputStream tarStream)
        {
            EntryStream.SetLength(0);
            tarStream.CopyEntryContents(EntryStream);
            EntryStream.Seek(0, SeekOrigin.Begin);
            return EntryReader.ReadToEnd();
        }
        public void Clear()
        {
            DownloadStream.SetLength(0);
            EntryStream.SetLength(0);
        }

        public void Close()
        {
            DownloadStream.Close();
            EntryStream.Close();
            EntryReader.Close();
            GzipDownloadStream.Close();
            TarStream.Close();
            TarStreamGzip.Close();
        }
    }
}
