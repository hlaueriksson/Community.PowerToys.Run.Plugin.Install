namespace Community.PowerToys.Run.Plugin.Install.Models
{
#pragma warning disable CA1724 // Type names should not match namespaces
    public class Plugin
#pragma warning restore CA1724 // Type names should not match namespaces
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
