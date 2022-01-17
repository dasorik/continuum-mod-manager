
using Continuum.Common;
using Continuum.Core.Enums;
using Continuum.Core.Utilities;

namespace Continuum.Core.InstallActions
{
	public class ReplaceFilesAction : ModInstallAction
	{
		public override string ActionName => "ReplaceFiles";
		public override string ActionDescription => "Replace Files";

		public string ReplacementPath;
		public string FileFilter;
		public bool IncludeSubfolders;
		public string TargetPath;

		public override ValidationResponse ValidateSettings(InstallActionValidationContext context)
		{
			if (string.IsNullOrWhiteSpace(ReplacementPath))
				return ValidationResponse.Error($"ReplaceFiles - {nameof(ReplacementPath)}: Provided path must not be NULL or empty");

			if (string.IsNullOrWhiteSpace(TargetPath))
				return ValidationResponse.Error($"ReplaceFiles - {nameof(TargetPath)}: Provided path must not be NULL or empty");

			if (string.IsNullOrWhiteSpace(FileFilter))
				return ValidationResponse.Error($"ReplaceFiles - {nameof(FileFilter)}: Provided path must not be NULL or empty (This is a regex pattern expression, eg. \".*\")");

			if (context == InstallActionValidationContext.Mod && !ModFilePathUtility.ValidModFilePath(ReplacementPath))
				return ValidationResponse.Error($"ReplaceFiles - {nameof(ReplacementPath)}: Provided path must be in the [MOD] folder");

			if (context == InstallActionValidationContext.Integration && !ModFilePathUtility.ValidIntegrationFilePath(ReplacementPath))
				return ValidationResponse.Error($"ReplaceFiles - {nameof(ReplacementPath)}: Provided path must be in the [INTEGRATION] folder");

			if (context == InstallActionValidationContext.Automapping)
				return ValidationResponse.Error($"ReplaceFiles: Action cannot be defined for integration automappings");

			if (!ModFilePathUtility.ValidGameFilePath(TargetPath))
				return ValidationResponse.Error($"ReplaceFiles - {nameof(TargetPath)}: Provided path must be in the [GAME] folder");

			return ValidationResponse.Success();
		}
	}
}
