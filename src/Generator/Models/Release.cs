namespace Generator.Models
{
    public class Release
    {
        public string Url { get; set; }
        public string TagName { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime PublishedAt { get; set; }
        public IEnumerable<Asset> Assets { get; set; }

        public IEnumerable<string> Validate()
        {
            if (!Assets.Any())
            {
                yield return "Assets missing";
            }
        }
    }

    public class Asset
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public int Size { get; set; }
        public int DownloadCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
