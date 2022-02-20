using Continuum.Core.Enums;
using Continuum.Common;
using System;
using System.Text.RegularExpressions;
using System.Linq;

namespace Continuum.Core.Models
{
	public class ModSettingCategory
	{
		public string Category { get; set; }
		public ModSetting[] Settings { get; set; }

		public ValidationResponse Validate()
		{
			if (string.IsNullOrWhiteSpace(Category))
				return ValidationResponse.Error("Category must have a value");

			if (Settings == null || Settings.Length == 0)
				return ValidationResponse.Error("Settings must contain at least one item");

			return ValidationResponse.Success();
		}
	}
}
