namespace Community.PowerToys.Run.Plugin.Install.Models
{
    public class Plugin
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Website { get; set; }
        public Repository Repository { get; set; }
        public Release Release { get; set; }
    }
}
