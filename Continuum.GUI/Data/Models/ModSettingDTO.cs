using Continuum.Core.Enums;
using Continuum.Core.Models;

namespace Continuum.GUI.Models
{
    public class ModSettingDTO
    {
        public string SettingID => Setting?.SettingID;

        public string Category;
        public ModSetting Setting;
        public object Value;
        public bool IsValid = true;
        public bool IsLocked = false;

        public string StringValue
        {
            get { return (string)Value; }
            set { Value = value; }
        }

        public ModSettingDTO(ModSetting setting, string category, object value, bool isLocked = false)
        {
            this.Category = category;
            this.Setting = setting;
            this.Value = value;
            this.IsLocked = isLocked;
        }
    }
}
