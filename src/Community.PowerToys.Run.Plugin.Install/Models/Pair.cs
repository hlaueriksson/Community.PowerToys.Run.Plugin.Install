namespace Community.PowerToys.Run.Plugin.Install.Models
{
    public class Pair
    {
        public Plugin Plugin { get; set; }

        public Metadata? Metadata { get; set; }

        public int Score { get; set; }

        public bool IsInstalled => Metadata != null;

        public bool IsNotInstalled => !IsInstalled;

        public bool IsInstallAvailable
        {
            get
            {
                if (Plugin?.Release?.Asset == null)
                {
                    return false;
                }

                return IsNotInstalled;
            }
        }

        public bool IsUpdateAvailable
        {
            get
            {
                if (Plugin?.Release?.Asset == null)
                {
                    return false;
                }

                if (Metadata == null)
                {
                    return false;
                }

                var latestVersion = GetVersion(Plugin.Release.TagName);
                var currentVersion = GetVersion(Metadata.Version);

                return latestVersion > currentVersion;

                Version GetVersion(string version)
                {
#pragma warning disable CA1867 // Use char overload
                    if (version.StartsWith("v", StringComparison.OrdinalIgnoreCase))
#pragma warning restore CA1867 // Use char overload
                    {
                        version = version.Substring(1);
                    }

                    if (Version.TryParse(version, out Version? result))
                    {
                        return result;
                    }

                    return new Version();
                }
            }
        }

        public bool HasRepository => !string.IsNullOrWhiteSpace(Plugin?.Repository?.Url);

        public bool HasReleaseNotes => !string.IsNullOrWhiteSpace(Plugin?.Release?.Url);

        public string Queryable => $"{Plugin?.Name} {Plugin?.Description} {Plugin?.Author} {(IsUpdateAvailable ? "outdated" : string.Empty)}";

        public string ToolTipTitle => $"{Plugin?.Name}";

        public string ToolTipText =>
$"""
{Metadata?.ToolTipText ?? "Installed: ❌"}

{Plugin?.Repository?.ToolTipText}

{Plugin?.Release?.ToolTipText}
""";
    }
}
