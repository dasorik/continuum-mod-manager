
using Continuum.Common;
using Continuum.Core.Enums;
using Continuum.Core.Utilities;
using System.Linq;

namespace Continuum.Core.InstallActions
{
	public class UnzipFileAction : ModInstallAction
	{
		public override string ActionName => "UnzipFile";
		public override string ActionDescription => "Unzip File";

		public string TargetFile;
		public string DestinationPath;
		public bool UseAutoMapping;
		public bool DeleteWhenComplete;

		public override ValidationResponse ValidateSettings(InstallActionValidationContext context)
		{
			if (string.IsNullOrWhiteSpace(TargetFile))
				return ValidationResponse.Error($"UnzipFile - {nameof(TargetFile)}: Target file must be supplied");

			if (!ModFilePathUtility.ValidGameFilePath(TargetFile))
				return ValidationResponse.Error($"UnzipFile - {nameof(TargetFile)}: Target file must be in the [GAME] folder");

			if (string.IsNullOrWhiteSpace(DestinationPath))
				return ValidationResponse.Error($"UnzipFile - {nameof(DestinationPath)}: Destination path must be supplied");

			if (!ModFilePathUtility.ValidGameFilePath(DestinationPath))
				return ValidationResponse.Error($"UnzipFile - {nameof(DestinationPath)}: Destination path must be in the [GAME] folder");

			return ValidationResponse.Success();
		}
	}
}
