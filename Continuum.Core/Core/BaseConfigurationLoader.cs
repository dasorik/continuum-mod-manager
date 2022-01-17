using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using Continuum.Core.InstallActions;
using Continuum.Core.Enums;
using Continuum.Core.Models;
using Continuum.Common.Logging;
using Continuum.Core.Utilities;
using Continuum.Core.Interfaces;
using Newtonsoft.Json;

namespace Continuum.Core
{
	public abstract class BaseConfigurationLoader<T> where T : IVersionLoadableData
	{
		string cacheFolder;

		protected abstract string FileExtension { get; }
		protected abstract string DisplayName { get; }

		public BaseConfigurationLoader(string cacheFolder)
		{
			this.cacheFolder = cacheFolder;
		}

		public IEnumerable<LoadResult<T>> Load(params string[] paths)
		{
			DirectoryUtility.DeleteAndRecreateFolder(cacheFolder);

			var results = new List<LoadResult<T>>();

			foreach (var path in paths)
			{
				Logger.Log($"Loading data from: {path}", LogSeverity.Info);

				var fileInfo = new FileInfo(path);
				var loadResult = TryLoad(fileInfo);

				// Check for duplicate IDs
				if (loadResult.status == LoadStatus.Success && results.Any(r => loadResult.data.ID == r.data?.ID))
					loadResult = new LoadResult<T>(loadResult.fileName, loadResult.data, LoadStatus.DuplicateID, $"A {DisplayName} with the same ID has already been added");

				results.Add(loadResult);
			}

			return results;
		}

		protected virtual LoadResult<T> TryLoad(FileInfo fileInfo)
		{
			T data = default(T);
			string zipFile = null;

			try
			{
				// Rename to .zip
				zipFile = Path.ChangeExtension(fileInfo.FullName, ".zip");
				File.Copy(fileInfo.FullName, zipFile);

				if (fileInfo.Extension == $".{FileExtension}")
				{
					var extractFolder = Path.Combine(cacheFolder, fileInfo.Name);

					System.IO.Compression.ZipFile.ExtractToDirectory(zipFile, extractFolder);

					var configPath = Path.Combine(extractFolder, "config.json");

					if (!File.Exists(configPath))
						return new LoadResult<T>(fileInfo.Name, data, LoadStatus.NoConfig, $"No config file could be located for this {DisplayName}");

					var fileData = File.ReadAllText(configPath);
					data = JsonConvert.DeserializeObject<T>(fileData, new ModInstallActionConverter(), new ModSettingValidatorConverter());

					if (string.IsNullOrEmpty(data.ID))
						return new LoadResult<T>(fileInfo.Name, data, LoadStatus.ConfigInvalid, $"Unable to load {DisplayName} due to an invalid config file");

					data.CacheFolder = extractFolder;

					var loadErrors = new List<string>();
					PostValidationChecks(data, loadErrors);

					var result = new LoadResult<T>(fileInfo.Name, data, loadErrors.Any() ? LoadStatus.ConfigInvalid : LoadStatus.Success, loadErrors.ToArray());
					FinalValidationChecks(data, ref result);

					return result;
				}
				else
				{
					return new LoadResult<T>(fileInfo.Name, default(T), LoadStatus.ExtensionInvalid, $"The selected {DisplayName} is not a .{FileExtension} file");
				}
			}
			catch (JsonReaderException ex)
			{
				Logger.Log(ex.ToString(), LogSeverity.Error);
				return new LoadResult<T>(fileInfo.Name, data, LoadStatus.ConfigInvalid, $"The {DisplayName} trying being loaded has an invalid/malformed config file");
			}
			catch (Exception ex)
			{
				Logger.Log(ex.ToString(), LogSeverity.Error);
				return new LoadResult<T>(fileInfo.Name, data, LoadStatus.UnspecifiedFailure, $"An error occurred trying to load the {DisplayName} - {ex?.Message}");
			}
			finally
			{
				if (!string.IsNullOrWhiteSpace(zipFile))
					File.Delete(zipFile);
			}
		}

		protected virtual void PostValidationChecks(T data, List<string> installErrors)
		{
		}

		protected virtual void FinalValidationChecks(T data, ref LoadResult<T> result)
		{

		}

		protected bool CheckActionsAreValid(ModInstallAction[] installActions, InstallActionValidationContext context, List<string> loadErrors)
		{
			bool result = true;

			// If we don't have any actions, no need to validate
			if (installActions == null)
				return true;

			foreach (var action in installActions)
			{
				var validationResult = action.ValidateSettings(context);
				if (validationResult.Type == Common.ValidationSeverity.Error)
				{
					loadErrors.Add(validationResult.Message);
					result = false;
				}
			}

			return result;
		}

		protected bool CheckSettingsAreValid(ModSettingCategory[] settings, List<string> loadErrors)
		{
			bool result = true;

			// If we don't have any setting categories, no need to validate
			if (settings == null)
				return true;

			foreach (var settingCategory in settings)
			{
				var validationResult = settingCategory.Validate();
				if (validationResult.Type == Common.ValidationSeverity.Error)
				{
					loadErrors.Add(validationResult.Message);
					result = false;
				}
				else
				{
					CheckSettingsAreValid(settingCategory.Settings, loadErrors);
				}
			}

			return result;
		}

		protected bool CheckSettingsAreValid(ModSetting[] settings, List<string> loadErrors)
		{
			bool result = true;

			foreach (var setting in settings)
			{
				var validationResult = setting.Validate();
				if (validationResult.Type == Common.ValidationSeverity.Error)
				{
					loadErrors.Add(validationResult.Message);
					result = false;
				}
			}

			return result;
		}

	}
}
