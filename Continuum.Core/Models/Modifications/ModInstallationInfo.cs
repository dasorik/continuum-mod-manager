using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Continuum.Core.Extension;

namespace Continuum.Core.Models
{
	public enum PredicateMatchType
	{
		MatchesPredicate,
		DoesNotMatchPredicate,
		InvalidPredicate
	}

    public class ModInstallInfo : IEquatable<ModInstallInfo>
    {
        public ModConfiguration Config;
		public GameIntegration Integration;
        public Dictionary<string, string> ModParameters;
        public Dictionary<string, string> IntegrationParameters;

		public ModInstallInfo(ModConfiguration config, GameIntegration integration, Dictionary<string, string> modParameters, Dictionary<string, string> integrationParameters)
		{
			this.Integration = integration;
			this.Config = config;
			this.ModParameters = modParameters;
			this.IntegrationParameters = integrationParameters;
		}

		public ModInstallInfo(ModConfiguration config, GameIntegration integration)
		{
			this.Integration = integration;
			this.Config = config;
			this.ModParameters = new Dictionary<string, string>();
			this.IntegrationParameters = new Dictionary<string, string>();
		}

		public bool Equals([AllowNull] ModInstallInfo other)
		{
			if (other == null)
				return false;

			return other?.Config?.ModID == Config?.ModID && other?.Integration?.IntegrationID == Integration?.IntegrationID;
		}

		public T GetConfig<T>()
            where T : ModConfiguration
        {
            return Config as T;
        }

		public PredicateMatchType PredicateMatch(string predicateCondition)
		{
			// If nothing is specified, always enable (by default)
			if (predicateCondition == null)
				return PredicateMatchType.InvalidPredicate;

			var pattern = @"^((?:\$MOD|\$INTEGRATION)\.[a-zA-Z0-9_\-\.]+|\$INTEGRATION_ID)\s*(<=|>=|=|!=|<|>)\s*(((?:\$MOD|\$INTEGRATION)\.[a-zA-Z0-9_\-\.]+)|[\""'].*[\""']|true|false|\d+(.\d+)?)$";
			var result = Regex.Match(predicateCondition, pattern);

			if (result.Success)
			{
				var parameter1 = result.Groups[1].Value;
				var operatorSymbol = result.Groups[2].Value;
				var parameter2 = result.Groups[3].Value;

				string parameter1Value = GetSettingValue(parameter1);
				string parameter2Value = GetSettingValue(parameter2);

				if (parameter1Value == null)
					return PredicateMatchType.InvalidPredicate;

				if (parameter2Value == null)
					return PredicateMatchType.InvalidPredicate;

				switch (operatorSymbol)
				{
					case "=":
						return parameter1Value.Equals(parameter2Value, StringComparison.InvariantCultureIgnoreCase) ? PredicateMatchType.MatchesPredicate : PredicateMatchType.DoesNotMatchPredicate;
					case "<=":
						if (!(parameter1Value.IsNumber() && parameter2Value.IsNumber()))
							return PredicateMatchType.InvalidPredicate;

						return parameter1Value.ToDecimal() <= parameter2Value.ToDecimal() ? PredicateMatchType.MatchesPredicate : PredicateMatchType.DoesNotMatchPredicate;
					case ">=":
						if (!(parameter1Value.IsNumber() && parameter2Value.IsNumber()))
							return PredicateMatchType.InvalidPredicate;

						return parameter1Value.ToDecimal() >= parameter2Value.ToDecimal() ? PredicateMatchType.MatchesPredicate : PredicateMatchType.DoesNotMatchPredicate;
					case "<":
						if (!(parameter1Value.IsNumber() && parameter2Value.IsNumber()))
							return PredicateMatchType.InvalidPredicate;

						return parameter1Value.ToDecimal() < parameter2Value.ToDecimal() ? PredicateMatchType.MatchesPredicate : PredicateMatchType.DoesNotMatchPredicate;
					case ">":
						if (!(parameter1Value.IsNumber() && parameter2Value.IsNumber()))
							return PredicateMatchType.InvalidPredicate;

						return parameter1Value.ToDecimal() > parameter2Value.ToDecimal() ? PredicateMatchType.MatchesPredicate : PredicateMatchType.DoesNotMatchPredicate;
					case "!=":
						return !parameter1Value.Equals(parameter2Value, StringComparison.InvariantCultureIgnoreCase) ? PredicateMatchType.MatchesPredicate : PredicateMatchType.DoesNotMatchPredicate;
				}
			}

			return PredicateMatchType.InvalidPredicate;
		}

		private string GetSettingValue(string parameter)
		{
			if (parameter.StartsWith("$MOD."))
			{
				if (ModParameters.TryGetValue(parameter.Substring("$MOD.".Length), out string value))
					return value;
			}
			else if (parameter.StartsWith("$INTEGRATION."))
			{
				if (IntegrationParameters.TryGetValue(parameter.Substring("$INTEGRATION.".Length), out string value))
					return value;
			}
			else if (parameter.StartsWith("$INTEGRATION_ID"))
			{
				return Integration?.IntegrationID;
			}
			else if (parameter.StartsWith("\""))
			{
				return parameter.Trim('\"');
			}
			else
			{
				return parameter;
			}

			return null;
		}
	}
}
