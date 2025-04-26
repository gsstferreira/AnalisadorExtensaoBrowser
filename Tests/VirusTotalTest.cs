using Common.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing
{
    public abstract class VirusTotalTest
    {
        public static void EicarTest()
        {
            var memStream = new MemoryStream();

            using (var fileStream = File.Open("C:/XExtensionDL/adobe-virus.zip", FileMode.Open))
            {
                fileStream.CopyTo(memStream);
            }

            var bytes = memStream.ToArray();

            var analysisUrl = VirusTotalHandler.UploadFileToVTRestSharp(bytes, bytes.Length);

            Console.WriteLine("URL da análise: {0}",analysisUrl);
        }
    }
}
