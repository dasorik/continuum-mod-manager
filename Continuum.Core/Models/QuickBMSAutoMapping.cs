using Continuum.Common;
using Continuum.Core.InstallActions;
using System.Collections.Generic;

namespace Continuum.Core.Models
{
	public class AutoMapping
	{
		public string TargetPath;
		public string FileFilter;
		public ModInstallAction[] Actions;

		public bool ValidateSettings(List<string> loadErrors)
		{
			bool result = true;

			if (string.IsNullOrWhiteSpace(TargetPath))
			{
				loadErrors.Add($"QuickBMSAutoMapping - {nameof(TargetPath)}: Provided path must not be NULL or empty");
				result = false;
			}

			if (Actions == null || Actions.Length == 0)
			{
				loadErrors.Add($"QuickBMSAutoMapping - {nameof(Actions)}: At least one action must be defined for each automapping");
				return false;
			}

			foreach (var action in Actions)
			{
				var validationResult = action.ValidateSettings(Enums.InstallActionValidationContext.Automapping);
				if (validationResult.Type == Common.ValidationSeverity.Error)
				{
					loadErrors.Add(validationResult.Message);
					result = false;
				}

				bool hasRecursiveQuickBMSAction = action is QuickBMSExtractAction && ((QuickBMSExtractAction)action).UseAutoMapping;
				bool hasRecursiveUnzipFileAction = action is UnzipFileAction && ((UnzipFileAction)action).UseAutoMapping;
				bool hasRecursiveUnzipFilesAction = action is UnzipFilesAction && ((UnzipFilesAction)action).UseAutoMapping;

				if (hasRecursiveQuickBMSAction || hasRecursiveUnzipFileAction || hasRecursiveUnzipFilesAction)
				{
					loadErrors.Add("Recursive automapping is now allowed");
					result = false;
				}
			}

			return result;
		}
	}
}
