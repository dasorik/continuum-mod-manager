
using Continuum.Common;
using Continuum.Core.Enums;
using Continuum.Core.Utilities;
using System.Linq;

namespace Continuum.Core.InstallActions
{
	public class DeleteFilesAction : ModInstallAction
	{
		public override string ActionName => "DeleteFiles";
		public override string ActionDescription => "Delete Files";

		public string[] TargetFiles;

		public override ValidationResponse ValidateSettings(InstallActionValidationContext context)
		{
			if (TargetFiles == null || TargetFiles.Length == 0)
				return ValidationResponse.Error($"DeleteFiles - {nameof(TargetFiles)}: No items provided in 'TargetFiles' list");

			if (TargetFiles.Any(s => string.IsNullOrWhiteSpace(s)))
				return ValidationResponse.Error($"DeleteFiles - {nameof(TargetFiles)}: One or more target files were NULL or Empty");

			// Can only delete files in the game path
			if (!TargetFiles.All(f => ModFilePathUtility.ValidGameFilePath(f)))
				return ValidationResponse.Error($"DeleteFiles - {nameof(TargetFiles)}: Provided path must be in the [GAME] folder");

			return ValidationResponse.Success();
		}
	}
}
