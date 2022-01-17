
using Continuum.Common;
using Continuum.Core.Enums;
using Continuum.Core.Utilities;
using System.Linq;

namespace Continuum.Core.InstallActions
{
	public class QuickBMSExtractAction : ModInstallAction
	{
		public override string ActionName => "QuickBMSExtract";
		public override string ActionDescription => "QuickBMS Extract";

		public string[] TargetFiles;
		public bool UseAutoMapping;
		public bool DeleteWhenComplete;

		public override ValidationResponse ValidateSettings(InstallActionValidationContext context)
		{
			if (TargetFiles == null || TargetFiles.Length == 0)
				return ValidationResponse.Error($"QuickBMSExtract - {nameof(TargetFiles)}: No items provided in 'TargetFiles' list");

			// Can only modify files in the game path
			if (!TargetFiles.All(f => ModFilePathUtility.ValidGameFilePath(f)))
				return ValidationResponse.Error($"QuickBMSExtract - {nameof(TargetFiles)}: All provided paths for QuickBMS extraction must be in the [GAME] folder");

			return ValidationResponse.Success();
		}
	}
}
