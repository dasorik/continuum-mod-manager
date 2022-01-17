
using Continuum.Common;
using Continuum.Core.Enums;
using Continuum.Core.Utilities;

namespace Continuum.Core.InstallActions
{
	public class MoveFilesAction : ModInstallAction
	{
		public override string ActionName => "MoveFiles";
		public override string ActionDescription => "Move Files";

		public string TargetPath;
		public string FileFilter;
		public bool IncludeSubfolders;
		public string DestinationPath;

		public override ValidationResponse ValidateSettings(InstallActionValidationContext context)
		{
			if (string.IsNullOrWhiteSpace(TargetPath))
				return ValidationResponse.Error($"MoveFiles - {nameof(TargetPath)}: Provided path must not be NULL or empty");

			if (string.IsNullOrWhiteSpace(DestinationPath))
				return ValidationResponse.Error($"MoveFiles - {nameof(DestinationPath)}: Provided path must not be NULL or empty");

			if (string.IsNullOrWhiteSpace(FileFilter))
				return ValidationResponse.Error($"MoveFiles - {nameof(FileFilter)}: Provided path must not be NULL or empty (This is a regex pattern expression, eg. \".*\")");

			// Files can only be moved into the game folder
			if (!ModFilePathUtility.ValidGameFilePath(DestinationPath))
				return ValidationResponse.Error($"MoveFiles - {nameof(DestinationPath)}: Provided path must be in the [GAME] folder");

			// Files can only be moved from the game folder
			if (!ModFilePathUtility.ValidGameFilePath(TargetPath))
				return ValidationResponse.Error($"MoveFiles - {nameof(TargetPath)}: Provided path must be in the [GAME] folder");

			return ValidationResponse.Success();
		}
	}
}
