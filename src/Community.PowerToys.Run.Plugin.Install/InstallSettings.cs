using Microsoft.PowerToys.Settings.UI.Library;

namespace Community.PowerToys.Run.Plugin.Install
{
    /// <summary>
    /// Plugin settings.
    /// </summary>
    public class InstallSettings
    {
        private const string DefaultPluginSource = "https://install.ptrun.net/awesome.json";

        /// <summary>
        /// Plugin source.
        /// </summary>
        public string PluginSource { get; set; } = DefaultPluginSource;

        internal IEnumerable<PluginAdditionalOption> GetAdditionalOptions()
        {
            return
            [
                new()
                {
                    Key = nameof(PluginSource),
                    DisplayLabel = "Plugin source",
                    DisplayDescription = "URL or path to the plugin source file",
                    PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Textbox,
                    TextValue = PluginSource,
                },
            ];
        }

        internal void SetAdditionalOptions(IEnumerable<PluginAdditionalOption> additionalOptions)
        {
            ArgumentNullException.ThrowIfNull(additionalOptions);

            var options = additionalOptions.ToList();
            PluginSource = options.Find(x => x.Key == nameof(PluginSource))?.TextValue ?? DefaultPluginSource;
        }
    }
}
