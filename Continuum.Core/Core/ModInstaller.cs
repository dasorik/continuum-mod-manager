using Continuum.Common.Logging;
using Continuum.Core.Enums;
using Continuum.Core.InstallActions;
using Continuum.Core.Models;
using Continuum.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Continuum.Core
{
	public class ModInstaller
	{
		public class ReservedFileModificationException : Exception
		{
			public ReservedFileModificationException(string message) : base(message) { }
		}

		ProgressTracker progressTracker;

		ModInstallerConfiguration configuration;
		string integrationBackupPath;
		bool isIntegrationSetup = false;

		GameIntegration integration;

		FileWriter fileWriter;
		List<ModCollision> conflicts;
		FileModificationCache modifications;

		HashSet<string> reservedFiles = new HashSet<string>();
		HashSet<string> decompiledFiles = new HashSet<string>();
		HashSet<string> unzippedFiles = new HashSet<string>();
		HashSet<string> extractedFiles = new HashSet<string>();
		HashSet<string> deletedFiles = new HashSet<string>();

		public ModInstaller(ModInstallerConfiguration configuration, GameIntegration integration, FileModificationCache fileModifications = null, ProgressTracker progressTracker = null)
		{
			this.configuration = configuration;
			this.integration = integration;

			this.progressTracker = progressTracker ?? new ProgressTracker();

			if (fileModifications != null)
				this.modifications = new FileModificationCache(fileModifications);

			if (integration == null)
				throw new System.Exception("An integration must be selected before installing mods");

			if (string.IsNullOrWhiteSpace(configuration.TargetPath))
				throw new System.Exception("Configuration 'TargetPath' cannot be null (this may indicate that an integration is not correctly validating this option)");

			if (string.IsNullOrWhiteSpace(configuration.TempFolder))
				throw new System.Exception("Configuration 'TempFolder' cannot be null");

			if (string.IsNullOrWhiteSpace(configuration.ToolPath))
				throw new System.Exception("Configuration 'ToolPath' cannot be null");

			if (configuration.MaxQuickBMSBatches <= 0)
				throw new System.Exception("Configuration 'MaxQuickBMSBatches' cannot be less than 1");

			if (!string.IsNullOrWhiteSpace(configuration.BackupFolder))
				integrationBackupPath = Path.Combine(configuration.BackupFolder, StringUtility.GetSHAOfString(integration.IntegrationID));
		}

		private void ResetInternalState()
		{
			RemoveAllChanges();

			conflicts = new List<ModCollision>();
			modifications = new FileModificationCache();
			
			decompiledFiles = new HashSet<string>();
			unzippedFiles = new HashSet<string>();
			extractedFiles = new HashSet<string>();
			deletedFiles = new HashSet<string>();

			fileWriter = new FileWriter();

			// Our game state has been restored (if nothing has been externally modified), so track changes
			reservedFiles = new HashSet<string>(Directory.GetFiles(configuration.TargetPath, "*", SearchOption.AllDirectories));

			Directory.CreateDirectory(configuration.TempFolder);

			if (!string.IsNullOrEmpty(integrationBackupPath))
				Directory.CreateDirectory(integrationBackupPath);
		}

		int GetActionCount(ModActionCollection actions)
		{
			int actionCount = 0;

			actionCount += actions.extractActions.Sum(a => a.action.TargetFiles.Count() * 2);
			actionCount += actions.decompileActions.Sum(a => a.action.TargetFiles.Count());
			actionCount += actions.fileMoveActions.Count();
			actionCount += actions.bulkFileMoveActions.Count();
			actionCount += actions.fileDeleteActions.Sum(a => a.action.TargetFiles.Count());
			actionCount += actions.fileWriteActions.Count();
			actionCount += actions.fileReplaceActions.Count();
			actionCount += actions.bulkFileReplaceActions.Count();
			actionCount += actions.fileCopyActions.Count();
			actionCount += actions.bulkFileCopyActions.Count();
			actionCount += actions.zipDirectoryActions.Count();
			actionCount += actions.zipFilesActions.Count();
			actionCount += actions.unzipFileActions.Count();
			actionCount += actions.unzipFilesActions.Sum(a => a.action.TargetFiles.Count());

			return actionCount;
		}

		public async Task<InstallResult> ApplySetupActions(GameIntegration integration)
		{
			// To allow for rollback, create a fake mod config
			var blankModConfig = new ModConfiguration()
			{
				ModID = integration.IntegrationID,
				InstallActions = integration.SetupActions
			};

			isIntegrationSetup = true;
			return await Task.Run(() => ApplyChangesInternal(new[] { new ModInstallInfo(blankModConfig, integration) }, false));
		}

		public async Task<InstallResult> RemoveAllChanges(bool isIntegration = false)
		{
			isIntegrationSetup = isIntegration;
			return await Task.Run(() => ApplyChangesInternal(new ModInstallInfo[0], false));
		}

		public async Task<InstallResult> ApplyChanges(params ModInstallInfo[] mods)
		{
			isIntegrationSetup = false;
			return await Task.Run(() => ApplyChangesInternal(mods, false));
		}

		private InstallResult ApplyChangesInternal(ModInstallInfo[] mods, bool reverting)
		{
			progressTracker?.UpdateContext("Reverting mod changes");

			// Start with a blank canvas
			ResetInternalState();

			if (mods.Length == 0)
				return new InstallResult(InstallationStatus.Success, conflicts, modifications);

			try
			{
				var modActions = GetModActions(mods);

				progressTracker?.ResetState(GetActionCount(modActions));

				// Before performing any changes, validate that the mod install actions are correct (if we have not passed this in from a UI)
				foreach (var action in mods.SelectMany(m => m.Config.InstallActions))
				{
					var validationResult = action.ValidateSettings(isIntegrationSetup ? InstallActionValidationContext.Integration : InstallActionValidationContext.Mod);

					// Ignore warnings, since these are only applicable for UI validation/confirmation
					if (validationResult.Type == Common.ValidationSeverity.Error)
						return new InstallResult(InstallationStatus.InvalidActions, conflicts, modifications);
				}

				return PerformActions(modActions, trackProgress: true);
			}
			catch (Exception ex)
			{
				Logger.Log(ex.ToString(), LogSeverity.Error);

				if (Directory.Exists(configuration.TempFolder))
					Directory.Delete(configuration.TempFolder, true);

				if (reverting)
				{
					// If we error out during revert, delete everything (something has gone badly wrong)
					RemoveAllChanges();
					return new InstallResult(InstallationStatus.FatalError, conflicts, modifications);
				}
				else
				{
					// Revert back to the previous install state
					ApplyChangesInternal(mods.Take(mods.Count() - 1).ToArray(), true);
					return new InstallResult(InstallationStatus.RolledBackError, conflicts, modifications);
				}
			}
			finally
			{
				if (Directory.Exists(configuration.TempFolder))
					Directory.Delete(configuration.TempFolder, true);
			}
		}

		private bool IsValidStatus(InstallationStatus status)
		{
			return status == InstallationStatus.Success || status == InstallationStatus.ResolvableConflict;
		}

		private InstallResult GetInstallResult(InstallationStatus status)
		{
			if (status != InstallationStatus.Success)
			{
				if (status == InstallationStatus.ResolvableConflict)
					return new InstallResult(InstallationStatus.Success, conflicts, modifications);
				else
					return new InstallResult(status, conflicts, modifications);
			}

			return new InstallResult(InstallationStatus.Success, conflicts, modifications);
		}

		private InstallResult PerformActions(ModActionCollection modActions, bool trackProgress)
		{
			foreach (var action in modActions.extractActions)
			{
				var status = QuickBMSExtract(action, action.action.UseAutoMapping);

				if (!IsValidStatus(status))
					return GetInstallResult(status);
			}

			foreach (var action in modActions.decompileActions)
			{
				var status = UnluacDecompile(action);

				if (!IsValidStatus(status))
					return GetInstallResult(status);
			}

			foreach (var action in modActions.unzipFilesActions)
			{
				var status = Unzip(action, action.action.UseAutoMapping, trackProgress: trackProgress);

				if (!IsValidStatus(status))
					return GetInstallResult(status);
			}

			foreach (var action in modActions.unzipFileActions)
			{

				var status = Unzip(action, action.action.UseAutoMapping, trackProgress: trackProgress);

				if (!IsValidStatus(status))
					return GetInstallResult(status);
			}

			foreach (var action in modActions.fileCopyActions)
			{
				var status = CopyFile(action, trackProgress: trackProgress);

				if (!IsValidStatus(status))
					return GetInstallResult(status);
			}

			foreach (var action in modActions.bulkFileCopyActions)
			{
				var status = BulkCopyFiles(action, trackProgress: trackProgress);

				if (!IsValidStatus(status))
					return GetInstallResult(status);
			}

			foreach (var action in modActions.fileReplaceActions)
			{
				var status = ReplaceFile(action, trackProgress: trackProgress);

				if (!IsValidStatus(status))
					return GetInstallResult(status);
			}

			foreach (var action in modActions.bulkFileReplaceActions)
			{
				var status = BulkReplaceFiles(action, trackProgress: trackProgress);

				if (!IsValidStatus(status))
					return GetInstallResult(status);
			}

			foreach (var action in modActions.fileWriteActions)
			{
				var status = WriteToFile(action, trackProgress: trackProgress);

				if (!IsValidStatus(status))
					return GetInstallResult(status);
			}

			foreach (var action in modActions.fileMoveActions)
			{
				var status = MoveFile(action, trackProgress: trackProgress);

				if (!IsValidStatus(status))
					return GetInstallResult(status);
			}

			foreach (var action in modActions.bulkFileMoveActions)
			{
				var status = BulkMoveFiles(action, trackProgress: trackProgress);

				if (!IsValidStatus(status))
					return GetInstallResult(status);
			}

			foreach (var action in modActions.zipFilesActions)
			{
				var status = Zip(action, trackProgress: trackProgress);

				if (!IsValidStatus(status))
					return GetInstallResult(status);
			}

			foreach (var action in modActions.zipDirectoryActions)
			{
				var status = Zip(action, trackProgress: trackProgress);

				if (!IsValidStatus(status))
					return GetInstallResult(status);
			}

			foreach (var action in modActions.fileDeleteActions)
			{
				var status = DeleteFile(action, trackProgress: trackProgress);

				if (!IsValidStatus(status))
					return GetInstallResult(status);
			}

			return GetInstallResult(InstallationStatus.Success);
		}

		private ModActionCollection GetModActions(ModInstallInfo[] mods)
		{
			var collection = new ModActionCollection();

			// We need to collate these steps, so we can minimize collision issues
			foreach (var mod in mods)
				collection.AddActions(mod);

			return collection;
		}

		private InstallationStatus QuickBMSExtract(ModActionLink<QuickBMSExtractAction> modAction, bool autoUnpack)
		{
			var tasks = new List<Task>();
			var cacheFolders = new Dictionary<string, string>();
			var skipProcessing = new Dictionary<string, bool>();

			try
			{
				// Create all temp folders up front so that files are mapped to directories in order
				foreach (var file in modAction.action.TargetFiles)
				{
					var folderPath = DirectoryUtility.GenerateTempFolder(configuration.TempFolder);
					cacheFolders.Add(file, folderPath);
					skipProcessing.Add(file, false);
				}

				Parallel.ForEach(
					modAction.action.TargetFiles,
					new ParallelOptions() { MaxDegreeOfParallelism = configuration.MaxQuickBMSBatches },
					targetFile =>
					{
						string cachedPath = cacheFolders[targetFile];
						bool skipped = QuickBMSExtractToTempDirectory(targetFile, modAction, cachedPath);
						skipProcessing[targetFile] = skipped;
					}
				);

				foreach (var file in modAction.action.TargetFiles)
				{
					if (skipProcessing[file])
						continue;

					string cachedPath = cacheFolders[file];
					CopyQuickBMSExtractedFilesToDirectories(file, modAction, cachedPath, autoUnpack);
				}
			}
			catch
			{
				throw;
			}
			finally
			{
				// Clean up any folders that were created during this step
				foreach (var cacheFolder in cacheFolders)
				{
					Directory.Delete(cacheFolder.Value, true);
				}
			}

			return InstallationStatus.Success;
		}

		private bool QuickBMSExtractToTempDirectory(string file, ModActionLink<QuickBMSExtractAction> modAction, string folderPath)
		{
			progressTracker?.UpdateContext($"Extracting file '{file}' with QuickBMS");

			var physicalTargetPath = ResolvePath(file, modAction.mod);

			// Check to ensure that we aren't going to accidentally extract the same file twice
			lock (extractedFiles)
			{
				// Check if this has already been extracted
				if (extractedFiles.Contains(physicalTargetPath))
				{
					progressTracker?.CompleteAction();
					return true;
				}

				extractedFiles.Add(physicalTargetPath);
			}

			if (!File.Exists(physicalTargetPath))
				throw new Exception($"Unable to find target path: {physicalTargetPath}");

			if (string.IsNullOrWhiteSpace(integration.QuickBMSScript))
				throw new Exception($"No QuickBMS script defined on the current integration, cannot use QuickBMS");

			string quickBMSScriptPath = Path.Combine(integration.CacheFolder, integration.QuickBMSScript);

			if (!File.Exists(quickBMSScriptPath))
				throw new Exception($"Could not find QuickBMS script defined on the current integration, cannot use QuickBMS");

			QuickBMSUtility.ExtractFiles(physicalTargetPath, folderPath, configuration.ToolPath, quickBMSScriptPath);

			progressTracker?.CompleteAction();
			return false;
		}

		private void CopyQuickBMSExtractedFilesToDirectories(string file, ModActionLink<QuickBMSExtractAction> modAction, string tempFolder, bool autoUnpack)
		{
			// Move all extracted files into the root folder
			var physicalTargetPath = ResolvePath(file, modAction.mod);
			var fileInfo = new FileInfo(physicalTargetPath);
			var newFolder = Path.Combine(tempFolder, Path.GetFileNameWithoutExtension(fileInfo.Name));
			var newFiles = Directory.GetFiles(newFolder, "*", SearchOption.AllDirectories);

			foreach (var newFile in newFiles)
			{
				var targetPath = Path.Combine(fileInfo.Directory.FullName, newFile.Substring(newFolder.Length).TrimStart('\\').TrimStart('/'));
				MoveFile_Internal(newFile, targetPath, modAction.mod, false);
			}

			progressTracker?.UpdateContext($"Applying automapping settings to extracted files ({file})");

			if (autoUnpack && HasAutoMapping(physicalTargetPath, integration.QuickBMSAutoMappings, out var autoMapping))
			{
				var autoUnpackActions = new ModActionCollection();
				autoUnpackActions.AddActions(modAction.mod, autoMapping.Actions);

				PerformActions(autoUnpackActions, trackProgress: false);
			}

			progressTracker?.CompleteAction();

			// Allow these files to be automatically deleted when finished with
			if (modAction.action.DeleteWhenComplete)
				DeleteFile_Internal(physicalTargetPath, modAction.mod);
		}

		private InstallationStatus UnluacDecompile(ModActionLink<UnluacDecompileAction> modAction, bool trackProgress = true)
		{
			foreach (var file in modAction.action.TargetFiles)
			{
				progressTracker?.UpdateContext($"Decompiling file '{file}' with Unluac");

				var physicalTargetPath = ResolvePath(file, modAction.mod);
				var fileInfo = new FileInfo(physicalTargetPath);

				var targetPath = Path.Combine(configuration.TempFolder, $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}_decomp{fileInfo.Extension}");

				// Check if this has already been decompiled
				if (decompiledFiles.Contains(physicalTargetPath))
				{
					if (trackProgress)
						progressTracker?.CompleteAction();

					continue;
				}

				if (!File.Exists(physicalTargetPath))
					throw new Exception($"Unable to find target path: {physicalTargetPath}");

				// Decompile the file, and replace the file
				UnluacUtility.Decompile(physicalTargetPath, targetPath);
				MoveFile_Internal(targetPath, physicalTargetPath, modAction.mod, false);

				decompiledFiles.Add(physicalTargetPath);

				if (trackProgress)
					progressTracker?.CompleteAction();
			}

			return InstallationStatus.Success;
		}

		private InstallationStatus Unzip(ModActionLink<UnzipFilesAction> modAction, bool autoUnpack, bool trackProgress = true)
		{
			foreach (var file in modAction.action.TargetFiles)
			{
				progressTracker?.UpdateContext($"Unzipping file '{file}'");

				var physicalTargetPath = ResolvePath(file, modAction.mod);
				var fileInfo = new FileInfo(physicalTargetPath);

				string destinationPath;

				if (modAction.action.ExtractToSameDirectory)
					destinationPath = fileInfo.Directory.FullName;
				else
					destinationPath = Path.Combine(fileInfo.Directory.FullName, Path.GetFileNameWithoutExtension(fileInfo.Name));

				Unzip_Internal(modAction.mod, physicalTargetPath, destinationPath);
				ApplyAutomappingToUnzippedFiles(modAction.mod, autoUnpack, physicalTargetPath);

				if (modAction.action.DeleteWhenComplete)
					DeleteFile_Internal(physicalTargetPath, modAction.mod);

				if (trackProgress)
					progressTracker?.CompleteAction();
			}

			return InstallationStatus.Success;
		}

		private InstallationStatus Unzip(ModActionLink<UnzipFileAction> modAction, bool autoUnpack, bool trackProgress = true)
		{
			progressTracker?.UpdateContext($"Unzipping file '{modAction.action.TargetFile}'");

			var physicalTargetPath = ResolvePath(modAction.action.TargetFile, modAction.mod);
			var physicalDestinationPath = ResolvePath(modAction.action.DestinationPath, modAction.mod);

			Unzip_Internal(modAction.mod, physicalTargetPath, physicalDestinationPath);
			ApplyAutomappingToUnzippedFiles(modAction.mod, autoUnpack, physicalTargetPath);

			if (modAction.action.DeleteWhenComplete)
				DeleteFile_Internal(physicalTargetPath, modAction.mod);

			if (trackProgress)
				progressTracker?.CompleteAction();

			return InstallationStatus.Success;
		}

		private void Unzip_Internal(ModInstallInfo mod, string targetFile, string destinationPath)
		{
			// Check if this has already been unzipped
			if (unzippedFiles.Contains(targetFile))
				return;

			var tempFolderPath = DirectoryUtility.GenerateTempFolder(configuration.TempFolder);

			try
			{
				if (!File.Exists(targetFile))
					throw new Exception($"Unable to find target path: {targetFile}");

				ZipUtility.Unzip(targetFile, tempFolderPath);

				if (!Directory.Exists(destinationPath))
					Directory.CreateDirectory(destinationPath);

				var newFiles = Directory.GetFiles(tempFolderPath, "*", SearchOption.AllDirectories);

				// Move all unzipped files to the target directory (as if unzipping directly - this was we can track changes)
				foreach (var newFile in newFiles)
				{
					var targetPath = Path.Combine(destinationPath, newFile.Substring(tempFolderPath.Length).TrimStart('\\').TrimStart('/'));
					MoveFile_Internal(newFile, targetPath, mod, false);
				}

				unzippedFiles.Add(targetFile);
			}
			finally
			{
				// Clean up any folders that were created during this step
				Directory.Delete(tempFolderPath, true);
			}
		}

		private void ApplyAutomappingToUnzippedFiles(ModInstallInfo mod, bool autoUnpack, string targetPath)
		{
			if (autoUnpack && HasAutoMapping(targetPath, integration.UnzipAutoMappings, out var autoMapping))
			{
				var autoUnpackActions = new ModActionCollection();
				autoUnpackActions.AddActions(mod, autoMapping.Actions);

				PerformActions(autoUnpackActions, trackProgress: false);
			}
		}

		private InstallationStatus Zip(ModActionLink<ZipFilesAction> modAction, bool trackProgress = true)
		{
			progressTracker?.UpdateContext($"Zipping files to path '{modAction.action.DestinationPath}'");

			var physicalFilePaths = modAction.action.FilesToInclude.Select(f => ResolvePath(f, modAction.mod));
			var physicalDestinationPath = ResolvePath(modAction.action.DestinationPath, modAction.mod);

			if (configuration.CheckForCollisions && ModCollisionTracker.HasZipCollision(modAction.mod, physicalFilePaths, physicalDestinationPath, modifications, out var collision))
				return HandleCollision(collision);

			if (File.Exists(physicalDestinationPath))
			{
				if (modAction.action.ReplaceFileAtDestination)
					DeleteFile_Internal(physicalDestinationPath, modAction.mod);
				else
					throw new Exception($"Unable to find target path: {physicalDestinationPath}");
			}

			ZipUtility.ZipFiles(physicalFilePaths, physicalDestinationPath);

			if (modAction.action.DeleteFilesWhenComplete)
			{
				foreach (var file in modAction.action.FilesToInclude)
					DeleteFile_Internal(file, modAction.mod);
			}

			if (trackProgress)
				progressTracker?.CompleteAction();

			return InstallationStatus.Success;
		}

		private InstallationStatus Zip(ModActionLink<ZipDirectoryAction> modAction, bool trackProgress = true)
		{
			progressTracker?.UpdateContext($"Zipping directory to path '{modAction.action.DestinationPath}'");

			var physicalTargetPath = ResolvePath(modAction.action.DirectoryPath, modAction.mod);
			var physicalDestinationPath = ResolvePath(modAction.action.DestinationPath, modAction.mod);
			string[] allSubFiles = Directory.GetFiles(physicalTargetPath, "*", SearchOption.AllDirectories);

			if (configuration.CheckForCollisions && ModCollisionTracker.HasZipCollision(modAction.mod, allSubFiles, physicalDestinationPath, modifications, out var collision))
				return HandleCollision(collision);

			if (File.Exists(physicalDestinationPath))
			{
				if (modAction.action.ReplaceFileAtDestination)
					DeleteFile_Internal(physicalDestinationPath, modAction.mod);
				else
					throw new Exception($"Unable to find target path: {physicalDestinationPath}");
			}

			ZipUtility.ZipDirectory(physicalTargetPath, physicalDestinationPath, modAction.action.IncludeBaseDirectory);

			if (modAction.action.DeleteDirectoryWhenComplete)
			{
				foreach (var file in allSubFiles)
					DeleteFile_Internal(file, modAction.mod);

				Directory.Delete(physicalTargetPath, true);
			}

			if (trackProgress)
				progressTracker?.CompleteAction();

			return InstallationStatus.Success;
		}

		private InstallationStatus MoveFile(ModActionLink<MoveFileAction> modAction, bool trackProgress = true)
		{
			var physicalTargetPath = ResolvePath(modAction.action.TargetFile, modAction.mod);
			var physicalDestinationPath = ResolvePath(modAction.action.DestinationPath, modAction.mod);

			bool trackTargetChanges = ModFilePathUtility.ValidGameFilePath(modAction.action.TargetFile);

			var status = MoveFile(physicalTargetPath, physicalDestinationPath, modAction.mod, trackTargetChanges);

			if (trackProgress)
				progressTracker?.CompleteAction();

			return status;
		}

		private InstallationStatus BulkMoveFiles(ModActionLink<MoveFilesAction> modAction, bool trackProgress = true)
		{
			var physicalDirectoryPath = ResolvePath(modAction.action.TargetPath, modAction.mod);
			var physicalDestinationPath = ResolvePath(modAction.action.DestinationPath, modAction.mod);

			var files = GetFilteredFilesFromDirectory(physicalDirectoryPath, modAction.action.FileFilter, modAction.action.IncludeSubfolders);
			bool trackTargetChanges = ModFilePathUtility.ValidGameFilePath(modAction.action.TargetPath);

			foreach (var file in files)
			{
				var relativePath = file.Substring(physicalDirectoryPath.Length).TrimStart('\\').TrimStart('/');
				var status = MoveFile(file, Path.Combine(physicalDestinationPath, relativePath), modAction.mod, trackTargetChanges);

				if (status != InstallationStatus.Success)
					return status;
			}

			if (trackProgress)
				progressTracker?.CompleteAction();

			return InstallationStatus.Success;
		}

		private InstallationStatus MoveFile(string targetFile, string destinationPath, ModInstallInfo mod, bool trackTargetChanges)
		{
			progressTracker?.UpdateContext($"Moving file '{targetFile}'");

			if (FileMovedToSameDirectory(targetFile, destinationPath))
				return InstallationStatus.Success;

			if (configuration.CheckForCollisions && ModCollisionTracker.HasMoveCollision(mod, targetFile, destinationPath, modifications, out var collision))
				return HandleCollision(collision);

			if (!File.Exists(targetFile))
				throw new Exception($"Unable to find target path: {targetFile}");

			MoveFile_Internal(targetFile, destinationPath, mod, trackTargetChanges);
			return InstallationStatus.Success;
		}

		private bool FileMovedToSameDirectory(string file, string destination)
		{
			if (modifications.HasModification(file, FileModificationType.Moved, out var action))
				return action.DestinationPath == destination;

			return false;
		}

		private InstallationStatus ReplaceFile(ModActionLink<ReplaceFileAction> modAction, bool trackProgress = true)
		{
			var physicalTargetPath = ResolvePath(modAction.action.TargetFile, modAction.mod);
			var physicalReplacementPath = ResolvePath(modAction.action.ReplacementFile, modAction.mod);

			var status = ReplaceFile(physicalReplacementPath, physicalTargetPath, modAction.mod);

			if (trackProgress)
				progressTracker?.CompleteAction();

			return status;
		}

		private InstallationStatus BulkReplaceFiles(ModActionLink<ReplaceFilesAction> modAction, bool trackProgress = true)
		{
			var physicalDirectoryPath = ResolvePath(modAction.action.ReplacementPath, modAction.mod);
			var physicalDestinationPath = ResolvePath(modAction.action.TargetPath, modAction.mod);
			var files = GetFilteredFilesFromDirectory(physicalDirectoryPath, modAction.action.FileFilter, modAction.action.IncludeSubfolders);

			foreach (var file in files)
			{
				var relativePath = file.Substring(physicalDirectoryPath.Length).TrimStart('\\').TrimStart('/');
				var status = ReplaceFile(file, Path.Combine(physicalDestinationPath, relativePath), modAction.mod);

				if (status != InstallationStatus.Success)
					return status;
			}

			if (trackProgress)
				progressTracker?.CompleteAction();

			return InstallationStatus.Success;
		}

		private InstallationStatus ReplaceFile(string replacementFile, string targetFile, ModInstallInfo mod)
		{
			progressTracker?.UpdateContext($"Replacing file '{targetFile}'");

			if (configuration.CheckForCollisions && ModCollisionTracker.HasReplaceCollision(mod, targetFile, replacementFile, modifications, out var collision))
				return HandleCollision(collision);
			
			if (!File.Exists(targetFile))
				throw new Exception($"Unable to find target path: {targetFile}");

			CopyFile_Internal(replacementFile, targetFile, mod);
			return InstallationStatus.Success;
		}

		private InstallationStatus CopyFile(ModActionLink<CopyFileAction> modAction, bool trackProgress = true)
		{
			var physicalTargetPath = ResolvePath(modAction.action.TargetFile, modAction.mod);
			var physicalDestinationPath = ResolvePath(modAction.action.DestinationPath, modAction.mod);

			var status = CopyFile(physicalTargetPath, physicalDestinationPath, modAction.mod);

			if (trackProgress)
				progressTracker?.CompleteAction();

			return status;
		}

		private InstallationStatus BulkCopyFiles(ModActionLink<CopyFilesAction> modAction, bool trackProgress = true)
		{
			var physicalDirectoryPath = ResolvePath(modAction.action.TargetPath, modAction.mod);
			var physicalDestinationPath = ResolvePath(modAction.action.DestinationPath, modAction.mod);
			var files = GetFilteredFilesFromDirectory(physicalDirectoryPath, modAction.action.FileFilter, modAction.action.IncludeSubfolders);

			foreach (var file in files)
			{
				var relativePath = file.Substring(physicalDirectoryPath.Length).TrimStart('\\').TrimStart('/');
				var status = CopyFile(file, Path.Combine(physicalDestinationPath, relativePath), modAction.mod);

				if (status != InstallationStatus.Success)
					return status;
			}

			if (trackProgress)
				progressTracker?.CompleteAction();

			return InstallationStatus.Success;
		}

		private InstallationStatus CopyFile(string targetFile, string destinationPath, ModInstallInfo mod)
		{
			progressTracker?.UpdateContext($"Copying file '{targetFile}'");

			if (configuration.CheckForCollisions && ModCollisionTracker.HasCopyCollision(mod, targetFile, destinationPath, modifications, out var collision))
				return HandleCollision(collision);

			if (!File.Exists(targetFile))
				throw new Exception($"Unable to find target path: {targetFile}");

			// Copying files occurs near the start, and should not cause conflicts

			CopyFile_Internal(targetFile, destinationPath, mod);
			return InstallationStatus.Success;
		}

		private InstallationStatus DeleteFile(ModActionLink<DeleteFilesAction> modAction, bool trackProgress = true)
		{
			foreach (var file in modAction.action.TargetFiles)
			{
				var physicalTargetPath = ResolvePath(file, modAction.mod);

				progressTracker?.UpdateContext($"Deleting file '{physicalTargetPath}'");

				// Check if this has already been deleted
				if (deletedFiles.Contains(physicalTargetPath) || !File.Exists(physicalTargetPath))
				{
					if (trackProgress)
						progressTracker?.CompleteAction();

					continue;
				}

				DeleteFile_Internal(physicalTargetPath, modAction.mod);

				if (trackProgress)
					progressTracker?.CompleteAction();
			}

			// Delete actions do not currently cause collisions
			return InstallationStatus.Success;
		}

		private InstallationStatus WriteToFile(ModActionLink<WriteToFileAction> modAction, bool trackProgress = true)
		{
			var physicalTargetPath = ResolvePath(modAction.action.TargetFile, modAction.mod);

			progressTracker?.UpdateContext($"Writing data to file '{physicalTargetPath}'");

			foreach (var content in modAction.action.Content)
			{
				string filePath = null;

				if (!File.Exists(physicalTargetPath))
					throw new Exception($"Unable to find target path: {physicalTargetPath}");

				if (!string.IsNullOrEmpty(content.DataFilePath))
					filePath = ResolvePath(content.DataFilePath, modAction.mod);

				var dataToWrite = content.Text ?? File.ReadAllText(filePath);

				if (configuration.CheckForCollisions && ModCollisionTracker.HasEditCollision(modAction.mod, physicalTargetPath, content, fileWriter, modifications, out var collision))
					return HandleCollision(collision);

				byte[] originalBytes = File.ReadAllBytes(physicalTargetPath);

				if (content.EndOffset.HasValue)
					fileWriter.WriteToFileRange(physicalTargetPath, dataToWrite, content.StartOffset, content.EndOffset.Value, false);
				else
					fileWriter.WriteToFile(physicalTargetPath, dataToWrite, content.StartOffset, content.Replace, false);

				if (IsReservedFile(physicalTargetPath))
				{
					modifications.AddModification(physicalTargetPath, new FileModification(null, FileModificationType.Edited, modAction.mod?.Config.ModID, true));

					byte[] newBytes = File.ReadAllBytes(physicalTargetPath);
					GenerateDeltaPatch(physicalTargetPath, originalBytes, newBytes);
				}
				else
				{
					modifications.AddModification(physicalTargetPath, new FileModification(null, FileModificationType.Edited, modAction.mod?.Config.ModID, false));
				}
			}

			if (trackProgress)
				progressTracker?.CompleteAction();

			return InstallationStatus.Success;
		}

		private void MoveFile_Internal(string targetPath, string destinationPath, ModInstallInfo mod, bool trackTargetChanges)
		{
			if (trackTargetChanges)
			{
				if (IsReservedFile(targetPath))
				{
					modifications.AddModification(targetPath, new FileModification(destinationPath, FileModificationType.Moved, mod?.Config.ModID, true));
					BackupFile(targetPath);
				}
				else
				{
					modifications.AddModification(targetPath, new FileModification(destinationPath, FileModificationType.Moved, mod?.Config.ModID, false));
				}
			}

			if (IsReservedFile(destinationPath))
			{
				modifications.AddModification(destinationPath, new FileModification(null, FileModificationType.Replaced, mod?.Config.ModID, true));
				BackupFile(destinationPath);
			}
			else
			{
				modifications.AddModification(destinationPath, new FileModification(null, FileModificationType.Added, mod?.Config.ModID, false));
			}

			Directory.CreateDirectory(new FileInfo(destinationPath).DirectoryName);
			File.Move(targetPath, destinationPath, true);
		}

		private void CopyFile_Internal(string targetPath, string destinationPath, ModInstallInfo mod)
		{
			if (IsReservedFile(destinationPath))
			{
				modifications.AddModification(destinationPath, new FileModification(null, FileModificationType.Replaced, mod?.Config.ModID, true));
				BackupFile(destinationPath);
			}
			else
			{
				modifications.AddModification(destinationPath, new FileModification(null, FileModificationType.Added, mod?.Config.ModID, false));
			}

			Directory.CreateDirectory(new FileInfo(destinationPath).DirectoryName);
			File.Copy(targetPath, destinationPath, true);
		}

		private void DeleteFile_Internal(string targetPath, ModInstallInfo mod)
		{
			if (IsReservedFile(targetPath))
			{
				modifications.AddModification(targetPath, new FileModification(null, FileModificationType.Deleted, mod?.Config.ModID, true));
				BackupFile(targetPath);
			}
			else
			{
				modifications.AddModification(targetPath, new FileModification(null, FileModificationType.Deleted, mod?.Config.ModID, true));
			}

			File.Delete(targetPath);
			deletedFiles.Add(targetPath);
		}

		private IEnumerable<string> GetFilteredFilesFromDirectory(string folder, string pattern, bool includeSubFolders)
		{
			var files = Directory.GetFiles(folder, "*", includeSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
			return files.Where(f => Regex.IsMatch(new FileInfo(f).Name, pattern));
		}

		private string GetBackupPath(string path, string fileExtension = "", bool createFolder = true)
		{
			string relativePath = path.Substring(configuration.TargetPath.Length).Trim('\\').Trim('.');
			string sha = StringUtility.GetSHAOfString(relativePath);
			string backupPath = Path.Combine(integrationBackupPath, sha, $"{sha}.{fileExtension}");

			if (createFolder)
			{
				var fileInfo = new FileInfo(backupPath);

				if (!fileInfo.Directory.Exists)
					Directory.CreateDirectory(fileInfo.Directory.FullName);
			}

			return backupPath;
		}

		private void BackupFile(string path)
		{
			// If no backup path has been supplied, don't backup
			if (string.IsNullOrWhiteSpace(integrationBackupPath))
				return;

			string backupPath = GetBackupPath(path, "backup", true);

			// Prevent issues from occuring if we modify an already backed-up file
			if (File.Exists(backupPath))
				return;

			File.Copy(path, backupPath, true);
		}

		private void GenerateDeltaPatch(string path, byte[] file1Data, byte[] file2Data)
		{
			// If no backup path has been supplied, don't backup
			if (string.IsNullOrWhiteSpace(integrationBackupPath))
				return;

			string backupBasePath = GetBackupPath(path, "patch", true);

			string backupPath = null;
			int counter = 1;

			do
			{
				// Keep looping until we find an avialable patch slot
				backupPath = $"{backupBasePath}{counter++}";
			}
			while (File.Exists(backupPath));

			using (FileStream stream = File.OpenWrite(backupPath))
			{
				deltaq.BsDiff.BsDiff.Create(file2Data, file1Data, stream);
			}
		}

		private bool IsReservedFile(string path)
		{
			return reservedFiles.Contains(path);
		}

		private string ResolvePath(string path, ModInstallInfo mod)
		{
			if (string.IsNullOrEmpty(path))
				return null;

			if (ModFilePathUtility.ValidGameFilePath(path))
				return Path.Combine(configuration.TargetPath, path.Substring(ModFilePathUtility.GAME_PATH_PREFIX.Length, path.Length - ModFilePathUtility.GAME_PATH_PREFIX.Length).TrimStart('\\').TrimStart('/'));

			if (!isIntegrationSetup && ModFilePathUtility.ValidModFilePath(path))
			{
				if (mod?.Config == null)
					throw new System.Exception("Cannot resolve [Mod] path when no mod is supplied");

				return Path.Combine(mod.Config.CacheFolder, "resources", path.Substring(ModFilePathUtility.MOD_PATH_PREFIX.Length, path.Length - ModFilePathUtility.MOD_PATH_PREFIX.Length).TrimStart('\\').TrimStart('/'));
			}

			if (isIntegrationSetup && ModFilePathUtility.ValidIntegrationFilePath(path))
			{
				if (mod?.Integration == null)
					throw new System.Exception("Cannot resolve [Integration] path when no integration is supplied");

				return Path.Combine(mod.Integration.CacheFolder, "resources", path.Substring(ModFilePathUtility.INTEGRATION_PATH_PREFIX.Length, path.Length - ModFilePathUtility.INTEGRATION_PATH_PREFIX.Length).TrimStart('\\').TrimStart('/'));
			}

			throw new Exception("Supplied path must begin with the following: [GAME], [MOD]");
		}

		private void RemoveAllChanges()
		{
			if (modifications != null)
			{
				// Remove all files that were added as a part of the install process
				foreach (var modification in modifications.GetModifications(type: FileModificationType.Added | FileModificationType.Moved, reserved: false))
				{
					progressTracker?.UpdateContext($"Deleting modified game file '{modification.file}'");
					
					var filePath = Path.Combine(configuration.TargetPath, modification.file);

					if (File.Exists(filePath))
						File.Delete(filePath);
				}
			}

			// Don't reapply backup folder if not supplied or doesn't exist
			if (string.IsNullOrWhiteSpace(integrationBackupPath) || !Directory.Exists(integrationBackupPath))
				return;

			HashSet<string> editedFiles = new HashSet<string>();

			if (modifications != null)
			{
				// Remove all files that were added as a part of the install process
				foreach (var modification in modifications.GetModifications(type: FileModificationType.Deleted | FileModificationType.Edited | FileModificationType.Moved | FileModificationType.Replaced, reserved: true))
				{
					if (editedFiles.Contains(modification.file))
						continue;

					string relativePath = modification.file.Substring(configuration.TargetPath.Length).Trim('\\').Trim('.');
					string sha = StringUtility.GetSHAOfString(relativePath);

					var backupDirectory = Path.Combine(integrationBackupPath, sha);
					var backupFiles = Directory.GetFiles(backupDirectory, "*", SearchOption.TopDirectoryOnly);

					string baseFile = null;
					List<string> patches = new List<string>();

					// Replace all files with those stored in the backup location
					foreach (var file in backupFiles)
					{
						if (Regex.IsMatch(file, @".+\.patch\d+$"))
							patches.Add(file);
						else
							baseFile = file;
					}

					progressTracker?.UpdateContext($"Restoring data from backup for file '{modification.file}'");

					var gamePath = Path.Combine(configuration.TargetPath, modification.file);

					// Restore the backup file first (as this may need the patches applied)
					if (!string.IsNullOrWhiteSpace(baseFile))
						File.Copy(baseFile, gamePath, true);

					// Load the current file (or restored file, if restored in the previous step)
					byte[] patchedFileBytes = File.ReadAllBytes(gamePath);
					bool patchApplied = false;

					// Re-apply patches in reverse order
					foreach (var patch in patches.OrderByDescending(p => int.Parse(Regex.Match(p, @"(?<=patch)\d+$").Value)))
					{
						patchApplied = true;

						using (var outputStream = new MemoryStream())
						{
							byte[] patchData = File.ReadAllBytes(patch);
							deltaq.BsDiff.BsPatch.Apply(patchedFileBytes, patchData, outputStream);

							// Update the patchedFile bytes to include the last patch
							patchedFileBytes = outputStream.ToArray();
						}
					}

					if (patchApplied)
						File.WriteAllBytes(gamePath, patchedFileBytes);

					editedFiles.Add(modification.file);
				}
			}

			// If everthing was restored, delete the backup folder and all of its content
			foreach (var directory in Directory.GetDirectories(integrationBackupPath, "*", SearchOption.AllDirectories))
				Directory.Delete(directory, true);
		}

		private InstallationStatus HandleCollision(ModCollision collision)
		{
			conflicts.Add(collision);
			return collision.severity == ModCollisionSeverity.Clash ? InstallationStatus.UnresolvableConflict : InstallationStatus.ResolvableConflict;
		}

		private bool HasAutoMapping(string resourceFile, IEnumerable<AutoMapping> automappings, out AutoMapping autoMapping)
		{
			foreach (var map in automappings)
			{
				var files = Directory.GetFiles(ResolvePath(map.TargetPath, null));
				var filteredFiles = files.Where(f => map.FileFilter == null || Regex.IsMatch(f, map.FileFilter));

				if (filteredFiles.Any(f => f.Equals(resourceFile, StringComparison.InvariantCultureIgnoreCase)))
				{
					autoMapping = map;
					return true;
				}
			}

			autoMapping = null;
			return false;
		}
	}
}
