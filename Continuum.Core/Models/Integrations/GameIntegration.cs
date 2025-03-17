using Continuum.Core.InstallActions;
using Continuum.Core.Interfaces;
using Newtonsoft.Json;
using System.Linq;

namespace Continuum.Core.Models
{
    public class GameIntegration : BaseConfiguration
    {
        public const string INSTALL_PATH_SETTING_NAME = "install-path";

        public string IntegrationID;
        public string DisplayIcon;
        public string ModCompatibilityVersion;
        public string MinimumApplicationVersion;
        public string TargetApplicationVersion;
        public string QuickBMSScript;
        public QuickBMSExtractMode QuickBMSExtractMode = QuickBMSExtractMode.NamedFolder;
        public string QuickBMSExtractPath;
        public ModInstallAction[] SetupActions;
        public AutoMapping[] QuickBMSAutoMappings;
        public AutoMapping[] UnzipAutoMappings;
        public ModCategory[] Categories;
        public ModSettingCategory[] Settings;

        [JsonIgnore] public ModSetting InstallPath => Settings?.SelectMany(c => c.Settings).FirstOrDefault(s => INSTALL_PATH_SETTING_NAME.Equals(s.SettingName, System.StringComparison.InvariantCultureIgnoreCase));

        // Cached data
        [JsonIgnore] public override string CacheFolder { get; set; }
        [JsonIgnore] public override string ID => IntegrationID;
        [JsonIgnore] public override string Type => "Integration";

        public override string ToString()
        {
            return $"[Integration: {IntegrationID}]";
        }
    }

    public class LinkedCollections
    {
        public string Nexus;
    }

    public static class GameIntegrationExtensions
    {
        public static string GetRelativePath(this GameIntegration integration, string path)
        {
            return System.IO.Path.Combine(integration.CacheFolder, path);
        }
    }
}
