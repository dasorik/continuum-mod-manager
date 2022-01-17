
using Continuum.Common;
using Continuum.Core.Enums;
using Continuum.Core.Interfaces;
using Continuum.Core.Models;
using Continuum.Core.Utilities;

namespace Continuum.Core.InstallActions
{
	public class WriteToFileAction : ModInstallAction
	{
		public override string ActionName => "WriteToFile";
		public override string ActionDescription => "Modify File";

		public string TargetFile;
		public WriteContent[] Content;

		public override ValidationResponse ValidateSettings(InstallActionValidationContext context)
		{
			if (Content == null || Content.Length == 0)
				return ValidationResponse.Error($"WriteToFile - {nameof(Content)}: No items provided in 'Content' list");

			// Can only modify files in the game path
			if (string.IsNullOrWhiteSpace(TargetFile))
				return ValidationResponse.Error($"WriteToFile - {nameof(TargetFile)}: Provided path must not be NULL or empty");

			// Can only modify files in the game path
			if (!ModFilePathUtility.ValidGameFilePath(TargetFile))
				return ValidationResponse.Error($"WriteToFile - {nameof(TargetFile)}: Provided path must be in the [GAME] folder");

			foreach (var content in Content)
			{
				if (!string.IsNullOrWhiteSpace(content.DataFilePath) && !string.IsNullOrWhiteSpace(content.Text))
					return ValidationResponse.Error($"WriteToFile - {nameof(content.DataFilePath)}: File write action must provide either 'Text' or 'DataFilePath' properties, not both");

				if (string.IsNullOrWhiteSpace(content.DataFilePath) && string.IsNullOrWhiteSpace(content.Text))
					return ValidationResponse.Error($"WriteToFile - {nameof(content.DataFilePath)}: File write action must provide either 'Text' or 'DataFilePath' properties");

				if (!string.IsNullOrWhiteSpace(content.DataFilePath))
				{
					if (context == InstallActionValidationContext.Mod && !ModFilePathUtility.ValidModFilePath(content.DataFilePath))
						return ValidationResponse.Error($"WriteToFile - {nameof(content.DataFilePath)}: Provided path must be in the [MOD] folder");

					if (context == InstallActionValidationContext.Integration && !ModFilePathUtility.ValidIntegrationFilePath(content.DataFilePath))
						return ValidationResponse.Error($"WriteToFile - {nameof(content.DataFilePath)}: Provided path must be in the [INTEGRATION] folder");

					if (context == InstallActionValidationContext.Automapping)
						return ValidationResponse.Error($"WriteToFile - {nameof(content.DataFilePath)}: Action (with DataFilePath) cannot be defined for integration automappings");
				}
			}

			return ValidationResponse.Success();
		}
	}
}
