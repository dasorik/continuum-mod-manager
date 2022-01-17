
using Continuum.Common;
using Continuum.Core.Enums;
using Continuum.Core.Utilities;
using System.Linq;

namespace Continuum.Core.InstallActions
{
	public class ZipDirectoryAction : ModInstallAction
	{
		public override string ActionName => "ZipDirectory";
		public override string ActionDescription => "Zip Directory";

		public string DirectoryPath;
		public string DestinationPath;
		public bool DeleteDirectoryWhenComplete;
		public bool ReplaceFileAtDestination;
		public bool IncludeBaseDirectory;

		public override ValidationResponse ValidateSettings(InstallActionValidationContext context)
		{
			if (string.IsNullOrWhiteSpace(DirectoryPath))
				return ValidationResponse.Error($"ZipDirectory - {nameof(DirectoryPath)}: Directory path must be supplied");

			if (!ModFilePathUtility.ValidGameFilePath(DirectoryPath))
				return ValidationResponse.Error($"ZipDirectory - {nameof(DirectoryPath)}: Directory path must be in the [GAME] folder");

			if (string.IsNullOrEmpty(DestinationPath))
				return ValidationResponse.Error($"ZipDirectory - {nameof(DestinationPath)}: Destination path must be supplied");

			if (!ModFilePathUtility.ValidGameFilePath(DestinationPath))
				return ValidationResponse.Error($"ZipDirectory - {nameof(DestinationPath)}: Destination path must be in the [GAME] folder");

			return ValidationResponse.Success();
		}
	}
}
