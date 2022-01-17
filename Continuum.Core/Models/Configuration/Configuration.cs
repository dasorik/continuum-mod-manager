using Continuum.Core.Models;
using System.Collections.Generic;

namespace Continuum.Core
{
	public struct ModInstallerConfiguration
	{
		public bool CheckForCollisions;
		public string TargetPath;
		public string TempFolder;
		public string BackupFolder;
		public string ToolPath;
		public int MaxQuickBMSBatches;
	}
}
