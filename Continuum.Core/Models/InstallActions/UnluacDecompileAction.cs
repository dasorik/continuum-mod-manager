
using Continuum.Common;
using Continuum.Core.Enums;
using Continuum.Core.Utilities;
using System.Linq;

namespace Continuum.Core.InstallActions
{
	public class UnluacDecompileAction : ModInstallAction
	{
		public override string ActionName => "UnluacDecompile";
		public override string ActionDescription => "Unluac Decompile";

		public string[] TargetFiles;

		public override ValidationResponse ValidateSettings(InstallActionValidationContext context)
		{
			if (TargetFiles == null || TargetFiles.Length == 0)
				return ValidationResponse.Error($"UnluacDecompile - {nameof(TargetFiles)}: No items provided in 'TargetFiles' list");

			// Can only modify files in the game path
			if (!TargetFiles.All(f => ModFilePathUtility.ValidGameFilePath(f)))
				return ValidationResponse.Error($"UnluacDecompile - {nameof(TargetFiles)}: All provided paths for Unluac decompile must be in the [GAME] folder");

			return ValidationResponse.Success();
		}
	}
}
