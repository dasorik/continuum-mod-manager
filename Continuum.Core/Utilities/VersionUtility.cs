using System.Linq;
using System.Text.RegularExpressions;

namespace Continuum.Core.Utilities
{
	public struct VersionCompatibleObject
	{
		public VersionInfo targetVersion;
		public VersionInfo? minimumVersion;

		public VersionCompatibleObject(string targetVersion, string minimumVersion)
		{
			this.targetVersion = new VersionInfo(targetVersion);

			if (!string.IsNullOrWhiteSpace(minimumVersion))
				this.minimumVersion = new VersionInfo(minimumVersion);
			else
				this.minimumVersion = null;
		}
	}

	public struct VersionedObject
	{
		public VersionInfo currentVersion;
		public VersionInfo? minimumSupportedVersion;

		public VersionedObject(string currentVersion, string minimumSupportedVersion)
		{
			this.currentVersion = new VersionInfo(currentVersion);

			if (!string.IsNullOrWhiteSpace(minimumSupportedVersion))
				this.minimumSupportedVersion = new VersionInfo(minimumSupportedVersion);
			else
				this.minimumSupportedVersion = null;
		}
	}

	public struct VersionInfo
	{
		public int majorVersion;
		public int? minorVersion;

		public VersionInfo(string versionString)
		{
			if (Regex.IsMatch(versionString, @"\d+\.(\d+|\*)"))
			{
				var split = versionString.Split('.');

				majorVersion = int.Parse(split[0]);

				if (split[1] != "*")
					minorVersion = int.Parse(split[1]);
				else
					minorVersion = null;
			}
			else
			{
				throw new System.Exception($"Invalid version string passed to VersionInfo object ('{versionString}')");
			}
		}

		public bool Equals(VersionInfo versionInfo, bool includeWildcard)
		{
			bool majorVersionsMatch = majorVersion == versionInfo.majorVersion;
			bool minorVersionsMatch = true;

			if (includeWildcard)
			{
				if (!versionInfo.minorVersion.HasValue || !minorVersion.HasValue)
					minorVersionsMatch = true;
				else
					minorVersionsMatch = versionInfo.minorVersion.Value == minorVersion.Value; // Safe to do, as we null check above
			}
			else
			{
				if (!versionInfo.minorVersion.HasValue && !minorVersion.HasValue)
					minorVersionsMatch = true;
				else if ((!versionInfo.minorVersion.HasValue && minorVersion.HasValue) || (versionInfo.minorVersion.HasValue && !minorVersion.HasValue))
					minorVersionsMatch = false;
				else
					minorVersionsMatch = versionInfo.minorVersion.Value == minorVersion.Value;
			}

			return majorVersionsMatch && minorVersionsMatch;
		}

		public bool LessThan(VersionInfo versionInfo)
		{
			bool majorVersionLessThan = majorVersion < versionInfo.majorVersion;
			bool majorVersionGreaterThan = majorVersion > versionInfo.majorVersion;

			if (majorVersionLessThan)
				return true;

			if (majorVersionGreaterThan)
				return false;

			if (!versionInfo.minorVersion.HasValue || !minorVersion.HasValue)
				return false;
			else
				return minorVersion.Value < versionInfo.minorVersion.Value;
		}

		public bool GreaterThan(VersionInfo versionInfo)
		{
			bool majorVersionGreaterThan = majorVersion > versionInfo.majorVersion;
			bool majorVersionLessThan = majorVersion < versionInfo.majorVersion;

			if (majorVersionGreaterThan)
				return true;

			if (majorVersionLessThan)
				return false;

			if (!versionInfo.minorVersion.HasValue || !minorVersion.HasValue)
				return false;
			else
				return minorVersion.Value > versionInfo.minorVersion.Value;
		}

		public bool GreaterThanOrEqualTo(VersionInfo versionInfo, bool includeWildcard)
		{
			if (GreaterThan(versionInfo))
				return true;

			if (Equals(versionInfo, includeWildcard))
				return true;

			return false;
		}

		public bool LessThanOrEqualTo(VersionInfo versionInfo, bool includeWildcard)
		{
			if (LessThan(versionInfo))
				return true;

			if (Equals(versionInfo, includeWildcard))
				return true;

			return false;
		}
	}

	public class VersionUtility
	{
		public static bool CompatibleWithVersion(VersionCompatibleObject compatibleOject, VersionedObject parentObject)
		{
			if (compatibleOject.targetVersion.LessThanOrEqualTo(parentObject.currentVersion, true) && compatibleOject.targetVersion.GreaterThanOrEqualTo(parentObject.minimumSupportedVersion ?? parentObject.currentVersion, true))
				return true;

			if (compatibleOject.targetVersion.GreaterThan(parentObject.currentVersion) && compatibleOject.minimumVersion.HasValue && (compatibleOject.minimumVersion.Value).LessThanOrEqualTo(parentObject.currentVersion, true))
				return true;

			return false;
		}
	}
}
