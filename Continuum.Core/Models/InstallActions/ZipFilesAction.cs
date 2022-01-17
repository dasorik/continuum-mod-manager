
using Continuum.Common;
using Continuum.Core.Enums;
using Continuum.Core.Utilities;
using System.Linq;

namespace Continuum.Core.InstallActions
{
	public class ZipFilesAction : ModInstallAction
	{
		public override string ActionName => "ZipFiles";
		public override string ActionDescription => "Zip Files";

		public string[] FilesToInclude;
		public string DestinationPath;
		public bool DeleteFilesWhenComplete;
		public bool ReplaceFileAtDestination;

		public override ValidationResponse ValidateSettings(InstallActionValidationContext context)
		{
			if (FilesToInclude == null || FilesToInclude.Length == 0)
				return ValidationResponse.Error($"ZipFiles - {nameof(FilesToInclude)}: No items provided in 'FilesToInclude' list");

			if (!FilesToInclude.All(f => ModFilePathUtility.ValidGameFilePath(f)))
				return ValidationResponse.Error($"ZipFiles - {nameof(FilesToInclude)}: All provided paths for inclusion in the zip archive must be in the [GAME] folder");

			if (string.IsNullOrWhiteSpace(DestinationPath))
				return ValidationResponse.Error($"ZipFiles - {nameof(DestinationPath)}: Destination path must be supplied");

			if (!ModFilePathUtility.ValidGameFilePath(DestinationPath))
				return ValidationResponse.Error($"ZipFiles - {nameof(DestinationPath)}: Destination path must be in the [GAME] folder");

			return ValidationResponse.Success();
		}
	}
}
