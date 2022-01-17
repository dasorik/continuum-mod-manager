using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Continuum.Core.Models
{
	public enum FileModificationType
	{
		None = 0,
		Moved = 1 << 0,
		Deleted = 1 << 1,
		Edited = 1 << 2,
		Replaced = 1 << 3,
		Added = 1 << 4
	}

	public struct FileModification
	{
		public string DestinationPath;
		public FileModificationType Type;
		public string ModID;
		public bool ReservedFile;

		public FileModification(string destinationPath, FileModificationType type, string modID, bool reservedFile)
		{
			DestinationPath = destinationPath;
			Type = type;
			ModID = modID;
			ReservedFile = reservedFile;
		}
	}
}
