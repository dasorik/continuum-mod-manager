using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Continuum.Core.Models
{
	[JsonConverter(typeof(ModSettingValidatorConverter))]
	public abstract class ModSettingValidator
	{
		public string ValidationType { get; set; }
		public string ErrorMessage { get; set; }
		public abstract int Order { get; }

		public abstract bool Validate(object value);
	}

	public class GenericModSettingValidator : ModSettingValidator
	{
		public override int Order => int.MaxValue;

		public override bool Validate(object value)
		{
			return true;
		}
	}

	public class MandatoryFieldValidator : ModSettingValidator
	{
		public override int Order => 0;

		public override bool Validate(object value)
		{
			if (value is string)
				return !string.IsNullOrEmpty((string)value);

			return value != null;
		}
	}

	public class PathExistsValidator : ModSettingValidator
	{
		public override int Order => 200;

		public string PathSuffix { get; set; }
		public string[] AllowedPathSuffixes { get; set; }

		public override bool Validate(object value)
		{
			string basePath = (string)value;
			string[] availableSuffixes = null;

			if (AllowedPathSuffixes != null && AllowedPathSuffixes.Length > 0)
			{
				availableSuffixes = AllowedPathSuffixes;
			}
			else if (!string.IsNullOrEmpty(PathSuffix))
			{
				availableSuffixes = new string[] { PathSuffix };
			}

			if (availableSuffixes != null)
			{
				foreach (string suffix in availableSuffixes)
				{
					string path = System.IO.Path.Combine(basePath, suffix);
					bool validPath = PathExists(path);

					if (validPath)
						return true;
				}
			}
			else
			{
				return PathExists(basePath);
			}

			return false;
		}

		private bool PathExists(string path)
		{
			try
			{
				var attributes = System.IO.File.GetAttributes(path);

				if ((attributes & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory)
					return System.IO.Directory.Exists(path);
				else
					return System.IO.File.Exists(path);
			}
			catch
			{
				return false;
			}
		}
	}

	public class RegexValidator : ModSettingValidator
	{
		public override int Order => 100;

		public string Regex { get; set; }

		public override bool Validate(object value)
		{
			if (!(value is string))
				return false;

			// If the value is null, ignore regex comparisons
			if (string.IsNullOrEmpty((string)value))
				return true;

			Regex regex = new Regex(Regex);
			Match match = regex.Match((string)value);

			return match.Success;
		}
	}
}
