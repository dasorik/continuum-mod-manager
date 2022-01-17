using Continuum.Core.InstallActions;
using Continuum.Core.Interfaces;
using Continuum.Core.Utilities;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Continuum.Core.Models
{
	public class ModConfiguration : IVersionLoadableData
	{
		public string ModID;
		public string Version;
		public ModContributor Author;
		public ModContributor[] Contributors;
		public string DisplayName;
		public string DisplayImage;
		public string DisplayBackground;
		public string Description;
		public LinkedIntegration[] LinkedIntegrations;
		public ModInstallAction[] InstallActions;
		public ModSettingCategory[] Settings;

		// Cached data
		[JsonIgnore] public string CacheFolder { get; set; }
		[JsonIgnore] public string ID => ModID;

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
