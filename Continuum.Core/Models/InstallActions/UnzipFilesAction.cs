
using Continuum.Common;
using Continuum.Core.Enums;
using Continuum.Core.Utilities;
using System.Linq;

namespace Continuum.Core.InstallActions
{
	public class UnzipFilesAction : ModInstallAction
	{
		public override string ActionName => "UnzipFiles";
		public override string ActionDescription => "Unzip Files";

		public string[] TargetFiles;
		public bool ExtractToSameDirectory; // By default this will extract to a folder with the same name
		public bool UseAutoMapping;
		public bool DeleteWhenComplete;

		public override ValidationResponse ValidateSettings(InstallActionValidationContext context)
		{
			if (TargetFiles == null || TargetFiles.Length == 0)
				return ValidationResponse.Error($"UnzipFiles - {nameof(TargetFiles)}: No items provided in 'TargetFiles' list");

			// Can only modify files in the game path
			if (!TargetFiles.All(f => ModFilePathUtility.ValidGameFilePath(f)))
				return ValidationResponse.Error($"UnzipFiles - {nameof(TargetFiles)}: All provided paths for unzip must be in the [GAME] folder");

			return ValidationResponse.Success();
		}
	}
}
