
using System;
using Newtonsoft.Json;
using Continuum.Common;
using Continuum.Core.Enums;

namespace Continuum.Core.InstallActions
{
	[JsonConverter(typeof(ModInstallActionConverter))]
	public abstract class ModInstallAction
	{
		[JsonProperty("Action")] public abstract string ActionName { get; }
		[JsonIgnore] public abstract string ActionDescription { get; }

		public string Disabled;

		public abstract ValidationResponse ValidateSettings(InstallActionValidationContext context);
	}

	public class GenericInstallAction : ModInstallAction
	{
		[JsonProperty("ActionName")] public override string ActionName => "GenericAction";
		public override string ActionDescription => "Generic Action";
		public string Action;

		public override ValidationResponse ValidateSettings(InstallActionValidationContext context)
		{
			if (string.IsNullOrWhiteSpace(Action))
			{
				return ValidationResponse.Error($"No 'Action' property defined");
			}
			else
			{
				return ValidationResponse.Error($"The supplied install action '{Action}' does not match any known action types");
			}
		}
	}
}
