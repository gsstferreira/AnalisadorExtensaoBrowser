using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Common.Classes
{
    public class Constantes
    {
        public static string urlTeste = "https://chromewebstore.google.com/detail/browsergpt-seu-assistente/njggknpmkjapgklcfhaiigafiiebpchm?hl=pt-br";


        public static string ChromeExtensionViewUrl = "https://chromewebstore.google.com/detail/";

        public static string ChromeExtensionDownloadUrl = "https://clients2.google.com/service/update2/crx?response=redirect&prodversion=[PRODVER]&acceptformat=[FORMAT]&x=id%3D[ID]%26uc";

        public static string ChromeProdVersion = "31.0.1609.0";
        public static string AcceptedFormat = "crx2,crx3";

    }
}
