using ICSharpCode.SharpZipLib.Tar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Classes
{
    public class ExtendedTarStream : TarInputStream
    {
        private Stream stream;
        public ExtendedTarStream(Stream inputStream) : base(inputStream,TarBuffer.DefaultBlockFactor, Encoding.Default) 
        { 
            this.stream = inputStream;
        }

        public void Restart()
        {
            tarBuffer = TarBuffer.CreateInputTarBuffer(stream, TarBuffer.DefaultBlockFactor);
        }

        public void Restart(Stream newStream)
        {
            stream = newStream;
            tarBuffer = TarBuffer.CreateInputTarBuffer(newStream, TarBuffer.DefaultBlockFactor);
        }
    }
}
