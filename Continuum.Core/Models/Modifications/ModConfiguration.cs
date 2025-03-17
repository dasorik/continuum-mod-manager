using Continuum.Core.InstallActions;
using Continuum.Core.Interfaces;
using Continuum.Core.Utilities;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Continuum.Core.Models
{
    public class ModConfiguration : BaseConfiguration
    {
        public string ModID;
        public LinkedIntegration[] LinkedIntegrations;
        public ModInstallAction[] InstallActions;
        public ModSettingCategory[] Settings;
        public string[] ModDependencies;

        // Cached data
        [JsonIgnore] public override string CacheFolder { get; set; }
        [JsonIgnore] public override string ID => ModID;
        [JsonIgnore] public override string Type => "Mod";


        public override string ToString()
        {
            return $"[Mod: {ModID}]";
        }
    }

    public class ModContributor
    {
        public string Name;
        public string Role;
    }

    public class UpdateKeys
    {
        public string Nexus;
        public string GitHub; // Unused, for now
        public string GameBanana;
    }

    public static class ModConfigurationExtensions
    {
        public static string GetRelativePath(this ModConfiguration mod, string path)
        {
            return System.IO.Path.Combine(mod.CacheFolder, path);
        }

        public static bool CompatibleWith(this ModConfiguration mod, GameIntegration integration)
        {
            foreach (var link in mod.LinkedIntegrations)
            {
                if (link.IntegrationID.Equals(integration.IntegrationID, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    return VersionUtility.CompatibleWithVersion(new VersionCompatibleObject(link.TargetVersion, link.MinimumVersion), new VersionedObject(integration.Version, integration.MinimumApplicationVersion));
                }
            }

            return false;
        }

        public static string GetCategoryForIntegration(this ModConfiguration mod, GameIntegration integration)
        {
            return mod.LinkedIntegrations.FirstOrDefault(li => li.IntegrationID.Equals(integration.IntegrationID, StringComparison.InvariantCultureIgnoreCase))?.ModCategory;
        }
    }
}
