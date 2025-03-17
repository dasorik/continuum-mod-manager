using Continuum.Core.Utilities;
using Continuum.Core.Enums;
using Continuum.Core.Models;
using Continuum.GUI.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Continuum.Core;
using Continuum.Common.Logging;
using System.Reflection;
using System;
using Continuum.Common;
using System.IO.Compression;

namespace Continuum.GUI.Services
{
    public enum ModSetUpStatus
    {
        Success,
        ModifiedInstallation,
        SetUpFailed
    }

    public class ModService
    {
#if DEBUG
        const string TOOL_PATH = "..\\..\\..\\..\\Tools";
#else
		const string TOOL_PATH = "..\\tools";
#endif

        public UserModData Settings = new UserModData();

        public GameIntegration[] AvailableIntegrations { get; private set; } = new GameIntegration[0];
        public ModConfiguration[] AvailableMods { get; private set; } = new ModConfiguration[0];

        public IEnumerable<LoadResult<GameIntegration>> IntegerationLoadResults { get; private set; } = new LoadResult<GameIntegration>[0];
        public IEnumerable<LoadResult<ModConfiguration>> ModLoadResults { get; private set; } = new LoadResult<ModConfiguration>[0];

        public bool modLoadWarningShown = false;

        string toolPath;
        string userSaveDataPath;
        string integrationBackupFolder;
        string backupFolder;
        string tempFolder;
        string modCacheFolder;
        string integrationCacheFolder;

        public ModService()
        {

#if DEBUG
            var executionPath = Path.GetDirectoryName(AppContext.BaseDirectory);
#else
			var executionPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
#endif
            this.toolPath = Path.Combine(Path.Combine(executionPath, TOOL_PATH));
            this.userSaveDataPath = Path.Combine(Global.APP_DATA_FOLDER, "UserSettings.json");
            this.integrationBackupFolder = Path.Combine(Global.APP_DATA_FOLDER, "IntegrationBackup");
            this.backupFolder = Path.Combine(Global.APP_DATA_FOLDER, "Backup");
            this.tempFolder = Path.Combine(Global.APP_DATA_FOLDER, "Temp");
            this.modCacheFolder = Path.Combine(Global.APP_DATA_FOLDER, "ModCache");
            this.integrationCacheFolder = Path.Combine(Global.APP_DATA_FOLDER, "IntegrationCache");

            CreateDirectories(); // Create the directories in case this is the first time we're loading

            Logger.Log($"Loading tools from path: '{this.toolPath}'", LogSeverity.Info);

            this.Settings = LoadSettings();

            ReloadIntegrations();
            SetIntegrationDefaults();

            ReloadMods();
            SetModOptionDefaults();

            SaveSettings();
        }

        void CreateDirectories()
        {
            if (!Directory.Exists(this.integrationBackupFolder))
            {
                Directory.CreateDirectory(this.integrationBackupFolder);
            }

            if (!Directory.Exists(this.backupFolder))
            {
                Directory.CreateDirectory(this.backupFolder);
            }

            if (!Directory.Exists(this.modCacheFolder))
            {
                Directory.CreateDirectory(this.modCacheFolder);
            }

            if (!Directory.Exists(this.integrationCacheFolder))
            {
                Directory.CreateDirectory(this.integrationCacheFolder);
            }
        }

        public UserModData LoadSettings()
        {
            Logger.Log($"Loading user settings from {userSaveDataPath}", LogSeverity.Info);

            var settings = SettingsUtility.LoadSettings<UserModData>(userSaveDataPath);
            return settings;
        }

        public void SetIntegrationDefaults()
        {
            if (Settings.IntegrationData == null)
            {
                Settings.IntegrationData = new List<IntegrationData>();
            }

            foreach (var integration in AvailableIntegrations)
            {
                var integrationData = Settings.IntegrationData.FirstOrDefault(d => integration.IntegrationID.Equals(d.IntegrationID, StringComparison.InvariantCultureIgnoreCase));

                if (integrationData == null)
                {
                    integrationData = new IntegrationData() { IntegrationID = integration.IntegrationID, SetUpApplied = false };
                    Settings.IntegrationData.Add(integrationData);
                }

                foreach (var category in integration.Settings)
                {
                    foreach (var setting in category.Settings)
                    {
                        var settingData = integrationData.Settings.FirstOrDefault(s => setting.SettingID.Equals(s.SettingID, StringComparison.InvariantCultureIgnoreCase));

                        if (settingData == null)
                        {
                            settingData = new ModSettingData() { SettingID = setting.SettingID, Value = setting.DefaultValue };
                            integrationData.Settings.Add(settingData);
                        }

                        PopulateDefaultSettings(setting, settingData);
                    }
                }
            }

            // Trim any settings for mods that have been removed
            Settings.IntegrationData.RemoveAll(id => !AvailableIntegrations.Any(ai => ai.IntegrationID.Equals(id.IntegrationID, StringComparison.InvariantCultureIgnoreCase)));
        }

        public void SetModOptionDefaults()
        {
            if (Settings.ModData == null)
            {
                Settings.ModData = new List<ModData>();
            }

            foreach (var mod in AvailableMods)
            {
                var modData = Settings.ModData.FirstOrDefault(mo => mod.ModID.Equals(mo.ModID, StringComparison.InvariantCultureIgnoreCase));

                if (modData == null)
                {
                    modData = new ModData() { ModID = mod.ModID };
                    Settings.ModData.Add(modData);
                }

                if (mod.Settings == null)
                    continue;

                foreach (var category in mod.Settings)
                {
                    foreach (var setting in category.Settings)
                    {
                        var settingData = modData.Settings.FirstOrDefault(s => setting.SettingID.Equals(s.SettingID, StringComparison.InvariantCultureIgnoreCase));

                        if (settingData == null)
                        {
                            settingData = new ModSettingData() { SettingID = setting.SettingID, Value = setting.DefaultValue };
                            modData.Settings.Add(settingData);
                        }

                        PopulateDefaultSettings(setting, settingData);
                    }
                }
            }

            // Trim any settings for mods that have been removed
            Settings.ModData.RemoveAll(md => !AvailableMods.Any(am => am.ModID.Equals(md.ModID, StringComparison.InvariantCultureIgnoreCase)));
        }

        void PopulateDefaultSettings(ModSetting setting, ModSettingData settingData)
        {
            // Set the default option if no default option is selected (or no valid default is applied)
            switch (setting.Type)
            {
                case ModSettingType.Checkbox:
                    if (!(settingData.Value is bool) || settingData.Value == null)
                        settingData.Value = false;
                    break;

                case ModSettingType.DropDown:
                    if (!setting.Options.Any(s => s.Value == settingData.Value?.ToString()))
                        settingData.Value = setting.Options.FirstOrDefault();
                    break;
            }
        }

        public ValidationResponse ValidateSetting(ModSetting setting, object value)
        {
            if (setting.Validations == null)
                return ValidationResponse.Success();

            foreach (var validation in setting.Validations.OrderBy(v => v.Order))
            {
                if (!validation.Validate(value))
                    return new ValidationResponse(ValidationSeverity.Error, validation.ErrorMessage);
            }

            return ValidationResponse.Success();
        }

        public bool IntegrationSettingsAreValid(GameIntegration integration)
        {
            bool valid = true;

            if (integration != null)
            {
                var integrationSettings = Settings.GetIntegration(integration.IntegrationID);

                foreach (var setting in integration.Settings.SelectMany(c => c.Settings))
                {
                    valid &= !ValidateSetting(setting, integrationSettings.GetSetting(setting.SettingID).Value).IsError;
                }
            }

            return valid;
        }

        public bool ModSettingsAreValid(ModConfiguration[] mods)
        {
            bool valid = true;

            foreach (var mod in mods)
            {
                if (mod.Settings == null)
                    continue;

                foreach (var setting in mod.Settings.SelectMany(c => c.Settings))
                {
                    valid &= !setting.ValidateSetting(Settings.GetModOptions(mod.ModID, setting.SettingID).Value).IsError;
                }
            }

            return valid;
        }

        public void SaveSettings()
        {
            Logger.Log($"Saving user settings to {userSaveDataPath}", LogSeverity.Info);
            SettingsUtility.SaveSettings(Settings, userSaveDataPath);
        }

        public void SetModFilterOptions(SortOrder sortOrder, SortGrouping sortGrouping)
        {
            this.Settings.ModSortOrder = sortOrder;
            this.Settings.ModSortGrouping = sortGrouping;

            SaveSettings();
        }

        public void SetIntegrationFilterOptions(SortOrder sortOrder)
        {
            this.Settings.IntegrationSortOrder = sortOrder;

            SaveSettings();
        }

        private ModInstallerConfiguration GetConfiguration(GameIntegration integration, bool checkForCollisions, string backupFolder)
        {
            if (integration == null)
                throw new System.Exception("Game integration must be supplied and not null");

            var integrationSettings = Settings.GetIntegration(integration.IntegrationID);

            return new ModInstallerConfiguration()
            {
                TargetPath = integrationSettings?.GetInstallPath()?.StringValue,
                BackupFolder = backupFolder,
                TempFolder = tempFolder,
                ToolPath = toolPath,
                CheckForCollisions = checkForCollisions,
                MaxQuickBMSBatches = 4
            };
        }

        public bool RequiresSetUp(GameIntegration integration)
        {
            if (integration == null || !IntegrationSettingsAreValid(integration))
                return false;

            var integrationSettings = Settings.GetIntegration(integration.IntegrationID);

            return !string.IsNullOrEmpty(integrationSettings?.GetInstallPath()?.StringValue)
                && !integrationSettings.SetUpApplied
                && integration?.SetupActions != null
                && (integration?.SetupActions?.Length ?? 0) > 0;
        }

        public async Task<ModSetUpStatus> ApplySetUp(GameIntegration integration, ProgressTracker progressTracker)
        {
            if (await CheckForNonStandardInstallation())
                return ModSetUpStatus.ModifiedInstallation;

            try
            {
                var integrationSettings = Settings.GetIntegration(integration.IntegrationID);
                var configuration = GetConfiguration(integration, checkForCollisions: false, backupFolder: integrationBackupFolder);

                var modInstaller = new ModInstaller(configuration, integration, fileModifications: integrationSettings.IntegrationFileModifications, progressTracker: progressTracker);
                var result = await modInstaller.ApplySetupActions(integration);

                if (result.status == InstallationStatus.Success)
                {
                    integrationSettings.SetUpApplied = true;
                    integrationSettings.IntegrationFileModifications = result.fileModifications;
                    SaveSettings();

                    Logger.Log("Setup completed successfully", LogSeverity.Info);
                    return ModSetUpStatus.Success;
                }
                else
                {
                    Logger.Log("Setup encountered an issue", LogSeverity.Error);
                    return ModSetUpStatus.SetUpFailed;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), LogSeverity.Error);
                return ModSetUpStatus.SetUpFailed;
            }
        }

        public async Task<ModSetUpStatus> RemoveSetUp(GameIntegration integration, ProgressTracker progressTracker)
        {
            try
            {
                var integrationSettings = Settings.GetIntegration(integration.IntegrationID);
                var configuration = GetConfiguration(integration, checkForCollisions: false, backupFolder: integrationBackupFolder);

                if (!integrationSettings.SetUpApplied)
                {
                    Logger.Log("Setup has not applied for this integration, no changes have been made", LogSeverity.Info);
                    return ModSetUpStatus.Success;
                }

                var modInstaller = new ModInstaller(configuration, integration, fileModifications: integrationSettings.IntegrationFileModifications, progressTracker: progressTracker);
                var result = await modInstaller.RemoveAllChanges(isIntegration: true);

                if (result.status == InstallationStatus.Success)
                {
                    integrationSettings.SetUpApplied = false;
                    integrationSettings.IntegrationFileModifications = result.fileModifications;
                    SaveSettings();

                    Logger.Log("Remove setup files successfully", LogSeverity.Info);
                    return ModSetUpStatus.Success;
                }
                else
                {
                    Logger.Log("Encountered an issue when removing set-up files", LogSeverity.Error);
                    return ModSetUpStatus.SetUpFailed;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), LogSeverity.Error);
                return ModSetUpStatus.SetUpFailed;
            }
        }

        public async Task<bool> ClearIntegrationCache(GameIntegration integration, ProgressTracker progressTracker)
        {
            try
            {
                var integrationSettings = Settings.GetIntegration(integration.IntegrationID);

                var modBackupPath = Path.Combine(backupFolder, StringUtility.GetSHAOfString(integration.IntegrationID));
                var integrationBackupPath = Path.Combine(integrationBackupFolder, StringUtility.GetSHAOfString(integration.IntegrationID));

                if (Directory.Exists(modBackupPath))
                    Directory.Delete(modBackupPath, true);

                if (Directory.Exists(integrationBackupPath))
                    Directory.Delete(integrationBackupPath, true);

                integrationSettings.SetUpApplied = false;
                integrationSettings.ModFileModifications = new FileModificationCache();
                integrationSettings.IntegrationFileModifications = new FileModificationCache();
                integrationSettings.InstalledMods = new List<string>();

                SaveSettings();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), LogSeverity.Error);
                return false;
            }

            return true;
        }

        async Task<bool> CheckForNonStandardInstallation()
        {
            return false; // TODO - Implement this
        }

        public void ReloadIntegrations()
        {
            Logger.Log($"Reloading Integrations", LogSeverity.Info);

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var versionString = $"{version.Major}.{version.Minor}";

            var loader = new GameIntegrationLoader(versionString, "1.0"); // Hardcoded for the time being, to ensure that integrations are supported after major version release
            var integrationPaths = Directory.GetDirectories(integrationCacheFolder, "*", SearchOption.TopDirectoryOnly);

            Logger.Log($"{integrationPaths.Length} integrations found", LogSeverity.Info);

            this.IntegerationLoadResults = loader.Load(integrationPaths);
            this.AvailableIntegrations = IntegerationLoadResults.Where(lr => lr.status == LoadStatus.Success).Select(lr => lr.data).ToArray();

            foreach (var loadResult in this.IntegerationLoadResults)
            {
                Logger.Log($"Load status: {loadResult.data} - {loadResult.status}", LogSeverity.Info);

                if (loadResult.loadErrors.Count() == 0)
                    continue;

                foreach (var error in loadResult.loadErrors)
                    Logger.Log($"{loadResult.data} - {error}", LogSeverity.Error);
            }

            if (IntegerationLoadResults.All(m => m.status == LoadStatus.Success))
                Logger.Log("Loaded all integrations successfully!", LogSeverity.Info);
        }

        public void ReloadMods()
        {
            Logger.Log("Reloading Mods", LogSeverity.Info);

            var loader = new ModLoader(AvailableIntegrations);
            var modPaths = Directory.GetDirectories(modCacheFolder, "*", SearchOption.TopDirectoryOnly);

            this.ModLoadResults = loader.Load(modPaths.ToArray());
            this.AvailableMods = ModLoadResults.Where(lr => lr.status == LoadStatus.Success).Select(lr => lr.data).ToArray();

            foreach (var loadResult in this.ModLoadResults)
            {
                if (loadResult.loadErrors.Count() == 0)
                    continue;

                foreach (var error in loadResult.loadErrors)
                    Logger.Log($"{loadResult.data} - {error}", LogSeverity.Error);
            }

            if (ModLoadResults.All(m => m.status == LoadStatus.Success))
                Logger.Log("Loaded all mods successfully!", LogSeverity.Info);
        }

        public async Task<LoadResult<GameIntegration>> TryAddIntegration(string fileName, MemoryStream memoryStream)
        {
            try
            {
                string filePath;
                if (memoryStream != null)
                {
                    if (!Directory.Exists(tempFolder))
                    {
                        Directory.CreateDirectory(tempFolder);
                    }

                    filePath = Path.Combine(tempFolder, fileName);

                    using var fileStream = File.Create(filePath);
                    await memoryStream.CopyToAsync(fileStream);
                }
                else
                {
                    filePath = fileName;
                }

                var fileInfo = new FileInfo(filePath);
                var extractFolder = Path.Combine(integrationCacheFolder, fileInfo.Name);

                if (Directory.Exists(extractFolder))
                    extractFolder = FileWriter.GetUniqueFilePath(extractFolder);

                if (fileInfo.Extension == ".integration" && !Directory.Exists(extractFolder))
                {
                    ZipFile.ExtractToDirectory(filePath, extractFolder);
                }
                else
                {
                    return new LoadResult<GameIntegration>(fileInfo.Name, null, LoadStatus.ExtensionInvalid, $"The selected integration is not an .integration file");
                }

                ReloadIntegrations();

                var result = IntegerationLoadResults.First(r => r.fileName == fileInfo.Name);
                var success = result.status == LoadStatus.Success;

                if (!success)
                {
                    // If this mod failed to load, 
                    File.Delete(extractFolder);
                    ReloadIntegrations();
                }
                else
                {
                    SetIntegrationDefaults();
                    SaveSettings();
                }

                return result;
            }
            finally
            {
                if (Directory.Exists(tempFolder))
                {
                    Directory.Delete(tempFolder, true);
                }
            }
        }

        public async Task<LoadResult<ModConfiguration>> TryAddMod(string fileName, MemoryStream memoryStream)
        {
            try
            {
                string filePath;
                if (memoryStream != null)
                {
                    if (!Directory.Exists(tempFolder))
                    {
                        Directory.CreateDirectory(tempFolder);
                    }

                    filePath = Path.Combine(tempFolder, fileName);

                    using var fileStream = File.Create(filePath);
                    await memoryStream.CopyToAsync(fileStream);
                }
                else
                {
                    filePath = fileName;
                }

                var fileInfo = new FileInfo(filePath);
                var extractFolder = Path.Combine(modCacheFolder, fileInfo.Name);

                if (Directory.Exists(extractFolder))
                    extractFolder = FileWriter.GetUniqueFilePath(extractFolder);

                if ((fileInfo.Extension == ".mod" || fileInfo.Extension == ".zip") && !Directory.Exists(extractFolder))
                {
                    ZipFile.ExtractToDirectory(filePath, extractFolder);
                }
                else
                {
                    return new LoadResult<ModConfiguration>(fileInfo.Name, null, LoadStatus.ExtensionInvalid, $"The selected mod is not an .mod or .zip file");
                }

                ReloadMods();

                var result = ModLoadResults.First(r => r.fileName == fileInfo.Name);
                var success = result.status == LoadStatus.Success;

                if (success && !HasAnyIntegrations(result.data))
                {
                    result = new LoadResult<ModConfiguration>(result.fileName, result.data, LoadStatus.NoMatchingIntegration, "Unable to load this mod, no installed game integrations found that support this");
                    success = false;
                }

                if (!success)
                {
                    // If this mod failed to load, 
                    Directory.Delete(extractFolder);
                    ReloadMods();
                }
                else
                {
                    SetModOptionDefaults();
                    SaveSettings();
                }

                return result;
            }
            finally
            {
                if (Directory.Exists(tempFolder))
                {
                    Directory.Delete(tempFolder, true);
                }
            }
        }

        private bool HasAnyIntegrations(ModConfiguration mod)
        {
            var successfullyLoadedIntegrations = IntegerationLoadResults.Where(i => i.status == LoadStatus.Success);
            return AvailableIntegrations.Any(i => mod.CompatibleWith(i));
        }

        public bool IsModInstalled(GameIntegration integration, ModConfiguration mod)
        {
            IntegrationData integrationSettings = Settings.GetIntegration(integration.IntegrationID);
            return integrationSettings.InstalledMods.Any(m => m == mod.ModID);
        }

        public string GetModIcon(GameIntegration integration, ModConfiguration mod)
        {
            var modCategory = mod.GetCategoryForIntegration(integration);

            if (modCategory == null)
                return null;

            var category = integration?.Categories?.FirstOrDefault(c => c.Category.Equals(modCategory, StringComparison.InvariantCultureIgnoreCase));

            string iconPath = category?.IconPath;

            if (iconPath != null)
                return integration.GetRelativePath(iconPath);

            return null;
        }

        public void GetModInfoForIntegration(GameIntegration integration, out int available, out int installed)
        {
            available = 0;
            installed = 0;

            if (integration == null)
                return;

            var modsForIntegration = AvailableMods.Where(m => m.CompatibleWith(integration));
            available = modsForIntegration.Count();
            installed = modsForIntegration.Where(m => IsModInstalled(integration, m)).Count();
        }

        public IEnumerable<ModConfiguration> GetModsForCategory(GameIntegration integration, string category, bool installed)
        {
            var modsForIntegration = AvailableMods.Where(m => m.CompatibleWith(integration));

            if (category == null)
                return modsForIntegration.Where(m => IsModInstalled(integration, m) == installed);
            else
                return modsForIntegration.Where(m => IsModInstalled(integration, m) == installed && (m.GetCategoryForIntegration(integration) ?? string.Empty).Equals(category, StringComparison.InvariantCultureIgnoreCase));
        }

        public ModConfiguration GetMod(string modID)
        {
            return AvailableMods.FirstOrDefault(m => m.ModID == modID);
        }

        public T GetMod<T>(string modID)
            where T : ModConfiguration
        {
            return AvailableMods.FirstOrDefault(m => m.ModID == modID && m is T) as T;
        }

        public IEnumerable<ModConfiguration> GetMods(params string[] modIDs)
        {
            if (modIDs == null)
                return null;

            return AvailableMods.Where(m => modIDs.Contains(m.ModID));
        }

        private ModInstallInfo GetModInstallInfo(GameIntegration integration, string modID)
        {
            var modParameters = new Dictionary<string, string>();
            var integrationParameters = new Dictionary<string, string>();

            var modSettings = Settings.GetModOptions(modID).Settings;
            var integrationSettings = Settings.GetIntegration(integration.IntegrationID).Settings;

            foreach (var setting in modSettings)
                modParameters.Add(setting.SettingID, setting.Value?.ToString());

            foreach (var setting in integrationSettings)
                integrationParameters.Add(setting.SettingID, setting.Value?.ToString());

            return new ModInstallInfo(config: GetMod(modID), integration: integration, modParameters: modParameters, integrationParameters: integrationParameters);
        }

        public async Task<InstallResult> InstallMods(GameIntegration integration, params ModConfiguration[] mods)
        {
            return await InstallMods(integration, mods, null);
        }

        public async Task<InstallResult> InstallMods(GameIntegration integration, IEnumerable<ModConfiguration> mods, ProgressTracker progressTracker)
        {
            var modInstalInfo = mods.Select(m => GetModInstallInfo(integration, m.ModID));
            return await UpdateModConfiguration(integration, modInstalInfo, true, progressTracker);
        }

        public async Task<InstallResult> UninstallMods(GameIntegration integration, params ModConfiguration[] mods)
        {
            return await UninstallMods(integration, mods, null);
        }

        public async Task<InstallResult> UninstallMods(GameIntegration integration, IEnumerable<ModConfiguration> mods, ProgressTracker progressTracker)
        {
            var modInstalInfo = mods.Select(m => GetModInstallInfo(integration, m.ModID));
            return await UpdateModConfiguration(integration, modInstalInfo, false, progressTracker);
        }

        private async Task<InstallResult> UpdateModConfiguration(GameIntegration integration, IEnumerable<ModInstallInfo> mods, bool install, ProgressTracker progressTracker = null)
        {
            if (!IntegrationSettingsAreValid(integration))
                throw new System.Exception("Cannot apply mods if integration settings have not been configured");

            if (!ModSettingsAreValid(mods.Select(m => m.Config).ToArray()))
                throw new System.Exception("Cannot apply mods if the settings of one or more have not been configured");

            var integrationSettings = Settings.GetIntegration(integration.IntegrationID);
            var currentModList = integrationSettings.InstalledMods.Select(id => GetModInstallInfo(integration, id)).ToList();
            var newModList = new List<ModInstallInfo>(currentModList);

            if (install)
            {
                newModList.AddRange(mods);
            }
            else
            {
                // Remove the selected mods from the install list
                foreach (var mod in mods)
                    newModList.Remove(mod);
            }

            try
            {
                var configuration = GetConfiguration(integration, checkForCollisions: true, backupFolder: backupFolder);
                var modInstaller = new ModInstaller(configuration, integration, fileModifications: integrationSettings.ModFileModifications, progressTracker: progressTracker, installedModList: currentModList);
                var result = await modInstaller.ApplyChanges(newModList.ToArray());

                switch (result.status)
                {
                    case InstallationStatus.Success:
                    case InstallationStatus.ResolvableConflict:
                        integrationSettings.InstalledMods = newModList.Select(m => m.Config.ModID).ToList();
                        break;
                    case InstallationStatus.FatalError:
                        // We remove all installed mods in this instance (something really bad has happened)
                        integrationSettings.InstalledMods.Clear();
                        break;
                }

                integrationSettings.ModFileModifications = result.fileModifications;
                SaveSettings();

                return result;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), LogSeverity.Error);
                return new InstallResult(InstallationStatus.FatalError, null, null);
            }
        }

        public void DeleteMod(ModConfiguration mod)
        {
            var loadedMod = ModLoadResults.First(m => m.data?.ModID == mod.ModID);

            Directory.Delete(Path.Combine(modCacheFolder, loadedMod.fileName), true);

            ReloadMods();
            SaveSettings();
        }

        public void DeleteIntegration(GameIntegration integration)
        {
            var integrationData = IntegerationLoadResults.First(m => m.data?.IntegrationID == integration.IntegrationID);

            Directory.Delete(Path.Combine(integrationCacheFolder, integrationData.fileName), true);

            ReloadIntegrations();
            SaveSettings();
        }

        public bool CanDeleteIntegration(GameIntegration integration)
        {
            // Find all mods in the system that link to the specified intrgration to see if any of the mods are only linked to one available integration
            foreach (var mod in AvailableMods.Where(m => m.CompatibleWith(integration)))
            {
                // Cannot uninstall integration if it is in use with the active integration
                if (IsModInstalled(integration, mod))
                    return false;

                // Check other integration links to see if this mod is linked to any other valid integrations
                var otherIntegrations = AvailableIntegrations.Where(i => !integration.IntegrationID.Equals(i.IntegrationID, StringComparison.InvariantCultureIgnoreCase));

                // Don't allow delete of integration if the mod is only linked to one available integration
                if (!otherIntegrations.Any(i => mod.CompatibleWith(i)))
                    return false;
            }

            return true;
        }
    }
}
