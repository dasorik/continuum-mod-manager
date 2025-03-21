﻿using Continuum.Core.Enums;
using Continuum.Core.Models;
using Continuum.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Continuum.Core
{
    public class GameIntegrationLoader : BaseConfigurationLoader<GameIntegration>
    {
        protected override string FileExtension => "integration";
        protected override string DisplayName => "integration";

        string applicationVersion;
        string minimumSupportedVersion;

        public GameIntegrationLoader(string applicationVersion, string minimumSupportedVersion)
        {
            this.applicationVersion = applicationVersion;
            this.minimumSupportedVersion = minimumSupportedVersion;
        }

        protected override void PostValidationChecks(GameIntegration integration, List<string> loadErrors)
        {
            base.PostValidationChecks(integration, loadErrors);

            bool versionAreValid = true;

            if (string.IsNullOrEmpty(integration.DisplayIcon))
                loadErrors.Add("Integration display icon must be defined");

            if (integration.MinimumApplicationVersion != null && !Regex.IsMatch(integration.MinimumApplicationVersion, @"\d+\.(?:\d+|\*)"))
                loadErrors.Add("Integration's minimum application version is not in the correct format (Expected {major}.{minor*})");

            if (integration.TargetApplicationVersion == null || !Regex.IsMatch(integration.TargetApplicationVersion, @"\d+\.(?:\d+|\*)"))
            {
                versionAreValid = false;
                loadErrors.Add("Integration must define an application version (In format {major}.{minor*})");
            }

            if (integration.ModCompatibilityVersion != null && !Regex.IsMatch(integration.ModCompatibilityVersion, @"\d+\.(?:\d+|\*)"))
                loadErrors.Add("Integration's mod compatibility version is not in the correct format (Expected {major}.{minor*})");

            if (CheckSettingsAreValid(integration.Settings, loadErrors))
            {
                var allSettings = integration.Settings.SelectMany(c => c.Settings);
                CheckForInstallPathSetting(allSettings, loadErrors);
            }

            if (integration.QuickBMSExtractMode == QuickBMSExtractMode.StaticFolder && string.IsNullOrEmpty(integration.QuickBMSExtractPath))
                loadErrors.Add("A QuickBMS folder path must be supplied for QuickBMSFolderMode = 'StaticFolder'");

            if (integration.QuickBMSExtractMode != QuickBMSExtractMode.StaticFolder && !string.IsNullOrEmpty(integration.QuickBMSExtractPath))
                loadErrors.Add("A QuickBMS folder can only be supplied for QuickBMSFolderMode = 'StaticFolder'");

            CheckAutomappingsAreValid(integration.QuickBMSAutoMappings, loadErrors);
            CheckAutomappingsAreValid(integration.UnzipAutoMappings, loadErrors);
            CheckActionsAreValid(integration.SetupActions, InstallActionValidationContext.Integration, loadErrors);

            if (versionAreValid && !ValidForApplicationVersion(integration))
                loadErrors.Add($"Application version mismatch, integration could be found for this application version (integration target/min version: {integration.TargetApplicationVersion}/{integration.MinimumApplicationVersion} | application version: {applicationVersion})");
        }

        protected void CheckForInstallPathSetting(IEnumerable<ModSetting> settings, List<string> installErrors)
        {
            var installPathSetting = settings.FirstOrDefault(s => GameIntegration.INSTALL_PATH_SETTING_NAME.Equals(s.SettingID, StringComparison.InvariantCultureIgnoreCase));

            if (installPathSetting == null)
            {
                installErrors.Add($"No setting with ID '{GameIntegration.INSTALL_PATH_SETTING_NAME}' was defined, this setting must be defined");
            }
            else
            {
                if (installPathSetting.Type != ModSettingType.Text)
                {
                    installErrors.Add($"Integration setting '{GameIntegration.INSTALL_PATH_SETTING_NAME}' must be of type '{ModSettingType.Text}'");
                }
            }
        }

        protected bool CheckAutomappingsAreValid(AutoMapping[] autoMappings, List<string> loadErrors)
        {
            bool result = true;

            // If we don't have any actions, no need to validate
            if (autoMappings == null)
                return true;

            foreach (var autoMapping in autoMappings)
                result &= autoMapping.ValidateSettings(loadErrors);

            return result;
        }

        private bool ValidForApplicationVersion(GameIntegration integration)
        {
            return VersionUtility.CompatibleWithVersion(
                new VersionCompatibleObject(integration.TargetApplicationVersion, integration.MinimumApplicationVersion),
                new VersionedObject(applicationVersion, minimumSupportedVersion)
            );
        }
    }
}
