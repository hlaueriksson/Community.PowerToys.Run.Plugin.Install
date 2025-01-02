namespace Community.PowerToys.Run.Plugin.Install.Models
{
    public class Metadata
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string PluginDirectory { get; set; }
        public string IcoPathDark { get; set; }
        public string IcoPathLight { get; set; }

        public string ToolTipText =>
$"""
Installed: {Name} {Version}
📁 {PluginDirectory}
""";
    }
}
