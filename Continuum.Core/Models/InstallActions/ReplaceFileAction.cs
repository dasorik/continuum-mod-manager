
using Continuum.Common;
using Continuum.Core.Enums;
using Continuum.Core.Utilities;

namespace Continuum.Core.InstallActions
{
	public class ReplaceFileAction : ModInstallAction
	{
		public override string ActionName => "ReplaceFile";
		public override string ActionDescription => "Replace File";

		public string ReplacementFile;
		public string TargetFile;

		public override ValidationResponse ValidateSettings(InstallActionValidationContext context)
		{
			if (string.IsNullOrWhiteSpace(TargetFile))
				return ValidationResponse.Error($"ReplaceFile - {nameof(TargetFile)}: Provided path must not be NULL or empty");

			if (string.IsNullOrWhiteSpace(ReplacementFile))
				return ValidationResponse.Error($"ReplaceFile - {nameof(ReplacementFile)}: Provided path must not be NULL or empty");

			if (context == InstallActionValidationContext.Mod && !ModFilePathUtility.ValidModFilePath(ReplacementFile))
				return ValidationResponse.Error($"ReplaceFile - {nameof(ReplacementFile)}: Provided path must be in the [MOD] folder");

			if (context == InstallActionValidationContext.Integration && !ModFilePathUtility.ValidIntegrationFilePath(ReplacementFile))
				return ValidationResponse.Error($"ReplaceFile - {nameof(ReplacementFile)}: Provided path must be in the [INTEGRATION] folder");

			if (context == InstallActionValidationContext.Automapping)
				return ValidationResponse.Error($"ReplaceFile: Action cannot be defined for integration automappings");

			if (!ModFilePathUtility.ValidGameFilePath(TargetFile))
				return ValidationResponse.Error($"ReplaceFile - {nameof(TargetFile)}: Provided path must be in the [GAME] folder");

			return ValidationResponse.Success();
		}
	}
}
