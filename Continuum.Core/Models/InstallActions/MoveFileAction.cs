
using Continuum.Common;
using Continuum.Core.Enums;
using Continuum.Core.Utilities;

namespace Continuum.Core.InstallActions
{
	public class MoveFileAction : ModInstallAction
	{
		public override string ActionName => "MoveFile";
		public override string ActionDescription => "Move File";

		public string TargetFile;
		public string DestinationPath;

		public override ValidationResponse ValidateSettings(InstallActionValidationContext context)
		{
			if (string.IsNullOrWhiteSpace(TargetFile))
				return ValidationResponse.Error($"MoveFile - {nameof(TargetFile)}: Provided path must not be NULL or empty");

			if (string.IsNullOrWhiteSpace(DestinationPath))
				return ValidationResponse.Error($"MoveFile - {nameof(DestinationPath)}: Provided path must not be NULL or empty");

			// Files can only be moved into the game folder
			if (!ModFilePathUtility.ValidGameFilePath(DestinationPath))
				return ValidationResponse.Error($"MoveFile - {nameof(DestinationPath)}: Provided path must be in the [GAME] folder");

			// Files can only be moved from the game folder
			if (!ModFilePathUtility.ValidGameFilePath(TargetFile))
				return ValidationResponse.Error($"MoveFile - {nameof(TargetFile)}: Provided path must be in the [GAME] folder");

			return ValidationResponse.Success();
		}
	}
}
