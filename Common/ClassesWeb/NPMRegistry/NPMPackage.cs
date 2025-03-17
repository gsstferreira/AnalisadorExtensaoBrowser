namespace Common.ClassesWeb.NPMRegistry
{
    public class NPMPackage
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string TarballUrl { get; set; }
        public double BestSimilarity { get; set; }
        public int NumJsFiles { get; set; }
        public NPMPackage() {}
    }
}
