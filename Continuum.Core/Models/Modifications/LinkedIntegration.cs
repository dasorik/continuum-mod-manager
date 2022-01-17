using Continuum.Core.InstallActions;
using Continuum.Core.Interfaces;
using Newtonsoft.Json;

namespace Continuum.Core.Models
{
	public class LinkedIntegration
	{
		public string IntegrationID;
		public string TargetVersion;
		public string MinimumVersion;
		public string ModCategory;
	}
}
