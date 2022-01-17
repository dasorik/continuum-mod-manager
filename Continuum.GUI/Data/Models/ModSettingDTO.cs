using Continuum.Core.Enums;
using Continuum.Core.Models;

namespace Continuum.GUI.Models
{
	public class ModSettingCategoryDTO
	{
		public string Category;
		public ModSettingDTO[] Settings;

		public ModSettingCategoryDTO(string category, ModSettingDTO[] settings)
		{
			this.Category = category;
			this.Settings = settings;
		}
	}

	public class ModSettingDTO
	{
		public string SettingID => Setting?.SettingID;

		public ModSetting Setting;
		public object Value;
		public bool IsValid = true;
		public bool IsLocked = false;

		public string StringValue
		{
			get { return (string)Value; }
			set { Value = value; }
		}

		public ModSettingDTO(ModSetting setting, object value, bool isLocked = false)
		{
			this.Setting = setting;
			this.Value = value;
			this.IsLocked = isLocked;
		}
	}
}
