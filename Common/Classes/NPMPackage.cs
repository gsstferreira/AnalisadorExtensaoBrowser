namespace Common.Classes
{
    public class NPMPackage
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string TarballUrl { get; set; }
        public int FilesChecked { get; set; }
        public int FilesOpened { get; set; }
        public double BestSimilarity { get; set; }
        public NPMPackage() 
        {
            Name = string.Empty;
            Version = string.Empty;
            ReleaseDate = DateTime.MinValue;
            TarballUrl = string.Empty;
            FilesChecked = 0;
            BestSimilarity = -1.0;
        }

        public void UpdateSimilarity(double similarity)
        {
            if (similarity > BestSimilarity && similarity > 0.9) 
            {
                BestSimilarity = similarity;
            }
        }
    }
}
