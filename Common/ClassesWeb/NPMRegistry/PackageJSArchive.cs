namespace Common.ClassesWeb.NPMRegistry
{
    public class PackageJSArchive
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Content { get; set; }
        public double Similarity { get; set; }
        public PackageJSArchive(string fullName, Stream content) 
        { 
            Name = fullName.Split('/').Last().Trim();
            Path = fullName.Replace(Name, string.Empty).Trim();
            Similarity = 0;

            using (var reader = new StreamReader(content)) 
            {
                Content = reader.ReadToEnd().Trim();
            }
        }
    }
}
