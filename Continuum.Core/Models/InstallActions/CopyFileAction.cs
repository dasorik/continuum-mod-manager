
using Continuum.Common;
using Continuum.Core.Enums;
using Continuum.Core.Utilities;

namespace Continuum.Core.InstallActions
{
	public class CopyFileAction : ModInstallAction
	{
		public override string ActionName => "CopyFile";
		public override string ActionDescription => "Copy File";

		public string TargetFile;
		public string DestinationPath;

		public override ValidationResponse ValidateSettings(InstallActionValidationContext context)
		{
			if (string.IsNullOrWhiteSpace(TargetFile))
				return ValidationResponse.Error($"CopyFile - {nameof(TargetFile)}: Provided path must not be NULL or empty");

			if (string.IsNullOrWhiteSpace(DestinationPath))
				return ValidationResponse.Error($"CopyFile - {nameof(DestinationPath)}: Provided path must not be NULL or empty");

			// Files can only be copied into the game folder
			if (!ModFilePathUtility.ValidGameFilePath(DestinationPath))
				return ValidationResponse.Error($"CopyFile - {nameof(DestinationPath)}: Provided path must be in the [GAME] folder");

			// Target file can be either in game or mod paths
			if (context == InstallActionValidationContext.Mod && !(ModFilePathUtility.ValidGameFilePath(TargetFile) || ModFilePathUtility.ValidModFilePath(TargetFile)))
				return ValidationResponse.Error($"CopyFile - {nameof(TargetFile)}: Provided path must be in the [GAME] or [MOD] folder");

			// Target file can be either in game or integration paths
			if (context == InstallActionValidationContext.Integration && !(ModFilePathUtility.ValidGameFilePath(TargetFile) || ModFilePathUtility.ValidIntegrationFilePath(TargetFile)))
				return ValidationResponse.Error($"CopyFile - {nameof(TargetFile)}: Provided path must be in the [GAME] or [INTEGRATION] folder");

			if (context == InstallActionValidationContext.Automapping && !ModFilePathUtility.ValidGameFilePath(TargetFile))
				return ValidationResponse.Error($"CopyFile - {nameof(TargetFile)}: Provided path must be in the [GAME] folder");

			return ValidationResponse.Success();
		}
	}
}
