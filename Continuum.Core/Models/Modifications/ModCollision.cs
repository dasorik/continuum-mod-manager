using Continuum.Core.Enums;

namespace Continuum.Core.Models
{
	public class ModCollision
	{
		public readonly string modID;
		public readonly ModCollisionSeverity severity;
		public readonly string description;

		public ModCollision(string modID, ModCollisionSeverity severity, string description)
		{
			this.modID = modID;
			this.severity = severity;
			this.description = description;
		}
	}
}
