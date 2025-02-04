namespace Generator.Models
{
    public class Repository
    {
        public string Url { get; set; }
        public string Owner { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
        public string[] Topics { get; set; }
        public string? License { get; set; }
        public int StargazersCount { get; set; }
        public int WatchersCount { get; set; }
        public int ForksCount { get; set; }
        public int Size { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public IEnumerable<string> Validate()
        {
            if (Topics.Length == 0)
            {
                yield return "Topics missing";
            }
        }
    }
}
