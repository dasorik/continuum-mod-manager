
using Continuum.Common;
using Continuum.Core.Enums;
using Continuum.Core.Utilities;

namespace Continuum.Core.InstallActions
{
	public class CopyFilesAction : ModInstallAction
	{
		public override string ActionName => "CopyFiles";
		public override string ActionDescription => "Copy Files";

		public string TargetPath;
		public string FileFilter;
		public bool IncludeSubfolders;
		public string DestinationPath;

		public override ValidationResponse ValidateSettings(InstallActionValidationContext context)
		{
			if (string.IsNullOrWhiteSpace(TargetPath))
				return ValidationResponse.Error($"CopyFiles - {nameof(TargetPath)}: Provided path must not be NULL or empty");

			if (string.IsNullOrWhiteSpace(DestinationPath))
				return ValidationResponse.Error($"CopyFiles - {nameof(DestinationPath)}: Provided path must not be NULL or empty");

			if (string.IsNullOrWhiteSpace(FileFilter))
				return ValidationResponse.Error($"CopyFiles - {nameof(FileFilter)}: Provided path must not be NULL or empty (This is a regex pattern expression, eg. \".*\")");

			// Files can only be copied into the game folder
			if (!ModFilePathUtility.ValidGameFilePath(DestinationPath))
				return ValidationResponse.Error($"CopyFiles - {nameof(DestinationPath)}: Provided path must be in the [GAME] folder");

			// Target file can be either in game or mod paths
			if (context == InstallActionValidationContext.Mod && !(ModFilePathUtility.ValidGameFilePath(TargetPath) || ModFilePathUtility.ValidModFilePath(TargetPath)))
				return ValidationResponse.Error($"CopyFiles - {nameof(TargetPath)}: Provided path must be in the [GAME] or [MOD] folder");

			// Target file can be either in game or integration paths
			if (context == InstallActionValidationContext.Integration && !(ModFilePathUtility.ValidGameFilePath(TargetPath) || ModFilePathUtility.ValidIntegrationFilePath(TargetPath)))
				return ValidationResponse.Error($"CopyFiles - {nameof(TargetPath)}: Provided path must be in the [GAME] or [INTEGRATION] folder");

			if (context == InstallActionValidationContext.Automapping && !ModFilePathUtility.ValidGameFilePath(TargetPath))
				return ValidationResponse.Error($"CopyFiles - {nameof(TargetPath)}: Provided path must be in the [GAME] folder");

			return ValidationResponse.Success();
		}
	}
}