using System.Runtime.InteropServices;

namespace Community.PowerToys.Run.Plugin.Install.Models
{
    public class Release
    {
        public string Url { get; set; }
        public string TagName { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime PublishedAt { get; set; }
        public IEnumerable<Asset> Assets { get; set; }

        public Asset? Asset
        {
            get
            {
                if (Assets == null)
                {
                    return null;
                }

                if (Assets.Count() == 1 && Assets.First().Name?.Contains(OppositePlatform(), StringComparison.OrdinalIgnoreCase) == false)
                {
                    return Assets.First();
                }

                return Assets.SingleOrDefault(x => x.Name?.Contains(Platform(), StringComparison.OrdinalIgnoreCase) == true);

                string Platform() => RuntimeInformation.ProcessArchitecture.ToString();
                string OppositePlatform()
                {
                    return RuntimeInformation.ProcessArchitecture switch
                    {
                        Architecture.X64 => nameof(Architecture.Arm64),
                        Architecture.Arm64 => nameof(Architecture.X64),
                        _ => string.Empty,
                    };
                }
            }
        }

        public string ToolTipText =>
$"""
Release: {TagName}
{Asset?.ToolTipText}
""";
    }

    public class Asset
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public int Size { get; set; }
        public int DownloadCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string ToolTipText => $"{Name} | 📦 {Size.FormatSize()} | 💾 {DownloadCount} downloads";
    }
}
