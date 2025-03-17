using System.Collections.Generic;
using Continuum.Core.Models;
using System.Text.RegularExpressions;
using System.Linq;
using Continuum.Core.Utilities;

namespace Continuum.Core
{
    public class ModLoader : BaseConfigurationLoader<ModConfiguration>
    {
        protected override string FileExtension => "mod";
        protected override string DisplayName => "mod";

        IEnumerable<GameIntegration> availableIntegrations;

        public ModLoader(IEnumerable<GameIntegration> availableIntegrations)
        {
            this.availableIntegrations = availableIntegrations ?? new GameIntegration[0];
        }

        protected override void PostValidationChecks(ModConfiguration mod, List<string> installErrors)
        {
            base.PostValidationChecks(mod, installErrors);

            if (mod.LinkedIntegrations != null && mod.LinkedIntegrations.Length > 0)
            {
                foreach (var integration in mod.LinkedIntegrations)
                {
                    if (integration == null)
                    {
                        installErrors.Add("Integration link cannot be null");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(integration.IntegrationID))
                        installErrors.Add("Integration link cannot have a null 'IntegrationID'");

                    if (string.IsNullOrWhiteSpace(integration.TargetVersion))
                        installErrors.Add("Integration link cannot have a null 'TargetVersion'");

                    if (integration.MinimumVersion != null && !Regex.IsMatch(integration.MinimumVersion, @"\d+\.(?:\d+|\*)"))
                        installErrors.Add("An integration link must define a minimum integration version (In format {major}.{minor*})");

                    if (integration.TargetVersion == null || !Regex.IsMatch(integration.TargetVersion, @"\d+\.(?:\d+|\*)"))
                        installErrors.Add("An integration link must define a target integration version (In format {major}.{minor*})");
                }
            }
            else
            {
                installErrors.Add("Mod must be linked to at least one integration");
            }

            CheckActionsAreValid(mod.InstallActions, Enums.InstallActionValidationContext.Mod, installErrors);
            CheckSettingsAreValid(mod.Settings, installErrors);
        }

        protected override void FinalValidationChecks(ModConfiguration data, ref LoadResult<ModConfiguration> result)
        {
            base.FinalValidationChecks(data, ref result);

            if (result.status == Enums.LoadStatus.Success)
            {
                if (!data.LinkedIntegrations.Any(li => availableIntegrations.Select(ai => ai.IntegrationID).Contains(li.IntegrationID)))
                    result = new LoadResult<ModConfiguration>(result.fileName, result.data, Enums.LoadStatus.NoMatchingIntegration, new[] { "No supported integrations could be found for this mod" });
            }
        }
    }
}
