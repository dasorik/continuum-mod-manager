using Continuum.Core.Enums;
using Continuum.Common;
using System;
using System.Text.RegularExpressions;
using System.Linq;

namespace Continuum.Core.Models
{
	public class ModSetting
	{
		public string SettingID { get; set; }
		public string SettingName { get; set; }
		public string Description { get; set; }
		public ModSettingType Type { get; set; }
		public object DefaultValue { get; set; }
		public ModSettingValidator[] Validations { get; set; }
		public ModSettingOption[] Options { get; set; }

		public ValidationResponse Validate()
		{
			if (string.IsNullOrWhiteSpace(SettingID))
				return ValidationResponse.Error("SettingID must have a value");

			if (!Regex.IsMatch(SettingID, @"[a-zA-Z0-9_\-\.]"))
				return ValidationResponse.Error("SettingID can only contain alpha - numeric characters(a - z, 0 - 9), hypens(-), underscores(_) and dots(.)");
			
			if (string.IsNullOrWhiteSpace(SettingName))
				return ValidationResponse.Error("SettingName must have a value");

			if (!Enum.IsDefined(typeof(ModSettingType), Type))
				return ValidationResponse.Error($"The supplied install option type '{Type}' does not match any known option types");

			if (Type != ModSettingType.DropDown && Options != null)
				return ValidationResponse.Error("Only install options using 'DropDown' cannot define options");

			if (Type == ModSettingType.DropDown && (Options == null || Options.Length == 0))
				return ValidationResponse.Error("Install options using 'DropDown' must define at least 1 option " + Options + " " + (Options?.Length ?? 0));

			if (Validations != null)
			{
				foreach (var validation in Validations)
				{
					if (validation.ValidationType == "Regex" && Type != ModSettingType.Text)
						return ValidationResponse.Error("'Regex' validator can only be applied to settings of type 'Text'");

					if (validation.ValidationType == "PathExists" && Type != ModSettingType.Text)
						return ValidationResponse.Error("'PathExists' validator can only be applied to settings of type 'Text'");
				}
			}

			return ValidationResponse.Success();
		}

		public ValidationResponse ValidateSetting(object value)
		{
			if (Validations == null)
				return ValidationResponse.Success();

			foreach (var validation in Validations.OrderBy(v => v.Order))
			{
				if (!validation.Validate(value))
					return new ValidationResponse(ValidationSeverity.Error, validation.ErrorMessage);
			}

			return ValidationResponse.Success();
		}
	}
}
