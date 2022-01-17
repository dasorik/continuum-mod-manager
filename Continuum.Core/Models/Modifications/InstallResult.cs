using Continuum.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Continuum.Core.Models
{
	public class InstallResult
	{
		public readonly InstallationStatus status;
		public readonly IEnumerable<ModCollision> conflicts;
		public readonly FileModificationCache fileModifications;

		public InstallResult(InstallationStatus status, IEnumerable<ModCollision> conflicts, FileModificationCache fileModifications)
		{
			this.status = status;
			this.conflicts = conflicts;
			this.fileModifications = fileModifications;
		}
	}
}
