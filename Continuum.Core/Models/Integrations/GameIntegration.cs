using Continuum.Core.InstallActions;
using Continuum.Core.Interfaces;
using Newtonsoft.Json;
using System.Linq;

namespace Continuum.Core.Models
{
	public class GameIntegration : IVersionLoadableData
	{
		public const string INSTALL_PATH_SETTING_NAME = "install-path";

		public string IntegrationID;
		public string DisplayName;
		public string DisplayIcon;
		public string DisplayImage;
		public ModContributor Author;
		public ModContributor[] Contributors;
		public string Version;
		public string ModCompatibilityVersion;
		public string MinimumApplicationVersion;
		public string TargetApplicationVersion;
		public string QuickBMSScript;
		public ModInstallAction[] SetupActions;
		public AutoMapping[] QuickBMSAutoMappings;
		public AutoMapping[] UnzipAutoMappings;
		public ModCategory[] Categories;
		public ModSettingCategory[] Settings;

		[JsonIgnore] public ModSetting InstallPath => Settings?.SelectMany(c => c.Settings).FirstOrDefault(s => INSTALL_PATH_SETTING_NAME.Equals(s.SettingName, System.StringComparison.InvariantCultureIgnoreCase));

		// Cached data
		[JsonIgnore] public string CacheFolder { get; set; }
		[JsonIgnore] public string ID => IntegrationID;

		public override string ToString()
		{
			return $"[Integration: {IntegrationID}]";
		}
	}

	public static class GameIntegrationExtensions
	{
		public static string GetRelativePath(this GameIntegration integration, string path)
		{
			return System.IO.Path.Combine(integration.CacheFolder, path);
		}
	}
}
