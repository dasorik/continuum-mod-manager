using Continuum.Core;
using Continuum.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Continuum.GUI.Models
{
	public class ModData : IEquatable<ModData>
	{
		public string ModID;
		public List<ModSettingData> Settings = new List<ModSettingData>();

		public bool Equals([AllowNull] ModData other)
		{
			return other?.ModID?.Equals(ModID) ?? false;
		}
	}

	public class ModSettingData : IEquatable<ModSettingData>
	{
		public string SettingID;
		public object Value;

		[JsonIgnore] public string StringValue => Value?.ToString() ?? string.Empty;

		public bool Equals([AllowNull] ModSettingData other)
		{
			return other?.SettingID?.Equals(SettingID) ?? false;
		}
	}

	public class IntegrationData : IEquatable<IntegrationData>
	{
		public string IntegrationID;
		public bool SetUpApplied = false;
		public List<ModSettingData> Settings = new List<ModSettingData>();
		public FileModificationCache ModFileModifications = new FileModificationCache();
		public FileModificationCache IntegrationFileModifications = new FileModificationCache();
		public List<string> InstalledMods = new List<string>();

		public ModSettingData GetSetting(string settingID)
		{
			return Settings.FirstOrDefault(s => s.SettingID == settingID);
		}

		public ModSettingData GetInstallPath()
		{
			return Settings.FirstOrDefault(s => s.SettingID == GameIntegration.INSTALL_PATH_SETTING_NAME);
		}

		public bool Equals([AllowNull] IntegrationData other)
		{
			return other?.IntegrationID?.Equals(IntegrationID) ?? false;
		}
	}

	public class UserModData
	{
		public SortOrder IntegrationSortOrder;
		public SortOrder ModSortOrder;
		public SortGrouping ModSortGrouping;
		public ApplicationSettings ApplicationSettings = new ApplicationSettings();
		public List<IntegrationData> IntegrationData = new List<IntegrationData>();
		public List<ModData> ModData = new List<ModData>();

		public IntegrationData GetIntegration(string integrationID)
		{
			return IntegrationData.FirstOrDefault(i => i.IntegrationID == integrationID);
		}

		public ModData GetModOptions(string modID)
		{
			return ModData.FirstOrDefault(o => o.ModID == modID);
		}

		public ModSettingData GetModOptions(string modID, string settingID)
		{
			return GetModOptions(modID)?.Settings.FirstOrDefault(s => s.SettingID == settingID);
		}

		public void SetModOption(string modID, string settingID, object value)
		{
			var option = GetModOptions(modID, settingID);
			option.Value = value;
		}
	}

	public class ApplicationSettings
	{
		public string Theme = "dark";
	}
}
