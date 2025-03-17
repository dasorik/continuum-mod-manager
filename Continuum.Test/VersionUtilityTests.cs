using NUnit.Framework.Legacy;
using NUnit.Framework;
using Continuum.Core.Models;
using Continuum.Core.Utilities;

namespace Continuum.Core.Test
{
	public class VersionUtiityTests
	{
		[Test]
		public void VersionCheck_Equals_BackwardsCompatible_Wildcard()
		{
			var versionInfo = new VersionCompatibleObject()
			{
				minimumVersion = new VersionInfo("3.*"),
				targetVersion = new VersionInfo("4.*")
			};

			var checkInfo = new VersionedObject()
			{
				currentVersion = new VersionInfo("5.1"),
				minimumSupportedVersion = new VersionInfo("4.*")
			};

			ClassicAssert.IsTrue(VersionUtility.CompatibleWithVersion(versionInfo, checkInfo));
		}

		[Test]
		public void VersionCheck_Equals_BackwardsCompatible_NonWildcard_Compatible_LessThan()
		{
			var versionInfo = new VersionCompatibleObject()
			{
				minimumVersion = new VersionInfo("3.*"),
				targetVersion = new VersionInfo("4.3")
			};

			var checkInfo = new VersionedObject()
			{
				currentVersion = new VersionInfo("5.1"),
				minimumSupportedVersion = new VersionInfo("4.1")
			};

            ClassicAssert.IsTrue(VersionUtility.CompatibleWithVersion(versionInfo, checkInfo));
		}

		[Test]
		public void VersionCheck_Equals_BackwardsCompatible_NonWildcard_Compatible_GreaterThan()
		{
			var versionInfo = new VersionCompatibleObject()
			{
				minimumVersion = new VersionInfo("3.*"),
				targetVersion = new VersionInfo("5.2")
			};

			var checkInfo = new VersionedObject()
			{
				currentVersion = new VersionInfo("5.1"),
				minimumSupportedVersion = new VersionInfo("4.1")
			};

            ClassicAssert.IsTrue(VersionUtility.CompatibleWithVersion(versionInfo, checkInfo));
		}

		[Test]
		public void VersionCheck_Equals_BackwardsCompatible_Wildcard_Compatible()
		{
			var versionInfo = new VersionCompatibleObject()
			{
				minimumVersion = new VersionInfo("3.*"),
				targetVersion = new VersionInfo("5.*")
			};

			var checkInfo = new VersionedObject()
			{
				currentVersion = new VersionInfo("5.1"),
				minimumSupportedVersion = new VersionInfo("4.1")
			};

            ClassicAssert.IsTrue(VersionUtility.CompatibleWithVersion(versionInfo, checkInfo));
		}

		[Test]
		public void VersionCheck_Equals_BackwardsCompatible_NonWildcard_Compatible_Equal()
		{
			var versionInfo = new VersionCompatibleObject()
			{
				minimumVersion = new VersionInfo("3.*"),
				targetVersion = new VersionInfo("4.3")
			};

			var checkInfo = new VersionedObject()
			{
				currentVersion = new VersionInfo("5.1"),
				minimumSupportedVersion = new VersionInfo("4.3")
			};

            ClassicAssert.IsTrue(VersionUtility.CompatibleWithVersion(versionInfo, checkInfo));
		}

		[Test]
		public void VersionCheck_NullMinVersion_NullMinimumSupported_Compatible_Wildcard()
		{
			var versionInfo = new VersionCompatibleObject()
			{
				targetVersion = new VersionInfo("6.*")
			};

			var checkInfo = new VersionedObject()
			{
				currentVersion = new VersionInfo("6.3"),
			};

            ClassicAssert.IsTrue(VersionUtility.CompatibleWithVersion(versionInfo, checkInfo));
		}

		[Test]
		public void VersionCheck_NullMinVersion_NullMinimumSupported_NotCompatible_LessThan()
		{
			var versionInfo = new VersionCompatibleObject()
			{
				targetVersion = new VersionInfo("6.1")
			};

			var checkInfo = new VersionedObject()
			{
				currentVersion = new VersionInfo("6.3"),
			};

            ClassicAssert.IsFalse(VersionUtility.CompatibleWithVersion(versionInfo, checkInfo));
		}

		[Test]
		public void VersionCheck_NullMinVersion_NullMinimumSupported_NotCompatible_GreaterThan()
		{
			var versionInfo = new VersionCompatibleObject()
			{
				targetVersion = new VersionInfo("6.4")
			};

			var checkInfo = new VersionedObject()
			{
				currentVersion = new VersionInfo("6.3"),
			};

            ClassicAssert.IsFalse(VersionUtility.CompatibleWithVersion(versionInfo, checkInfo));
		}

		[Test]
		public void VersionCheck_Equals_BackwardsCompatible_NonWildcard_CompatibleWithMinimum_Physical()
		{
			var versionInfo = new VersionCompatibleObject()
			{
				minimumVersion = new VersionInfo("3.*"),
				targetVersion = new VersionInfo("4.3")
			};

			var checkInfo = new VersionedObject()
			{
				currentVersion = new VersionInfo("5.1"),
				minimumSupportedVersion = new VersionInfo("3.5")
			};

            ClassicAssert.IsTrue(VersionUtility.CompatibleWithVersion(versionInfo, checkInfo));
		}

		[Test]
		public void VersionCheck_Equals_BackwardsCompatible_NonWildcard_CompatibleWithMinimum()
		{
			var versionInfo = new VersionCompatibleObject()
			{
				minimumVersion = new VersionInfo("3.*"),
				targetVersion = new VersionInfo("4.3")
			};

			var checkInfo = new VersionedObject()
			{
				currentVersion = new VersionInfo("5.1"),
				minimumSupportedVersion = new VersionInfo("3.*")
			};

            ClassicAssert.IsTrue(VersionUtility.CompatibleWithVersion(versionInfo, checkInfo));
		}

		[Test]
		public void VersionCheck_Equals_VersionsMatch_Wildcard()
		{
			var versionInfo = new VersionCompatibleObject()
			{
				minimumVersion = new VersionInfo("3.*"),
				targetVersion = new VersionInfo("4.*")
			};

			var checkInfo = new VersionedObject()
			{
				currentVersion = new VersionInfo("4.0")
			};

            ClassicAssert.IsTrue(VersionUtility.CompatibleWithVersion(versionInfo, checkInfo));
		}

		[Test]
		public void VersionCheck_Equals_VersionsMatch_MinimumVersion()
		{
			var versionInfo = new VersionCompatibleObject()
			{
				minimumVersion = new VersionInfo("3.*"),
				targetVersion = new VersionInfo("4.1")
			};

			var checkInfo = new VersionedObject()
			{
				currentVersion = new VersionInfo("4.0")
			};

            ClassicAssert.IsTrue(VersionUtility.CompatibleWithVersion(versionInfo, checkInfo));
		}

		[Test]
		public void VersionCheck_Equals_VersionsMatch_MinimumVersion_Invalid()
		{
			var versionInfo = new VersionCompatibleObject()
			{
				minimumVersion = new VersionInfo("4.0"),
				targetVersion = new VersionInfo("4.1")
			};

			var checkInfo = new VersionedObject()
			{
				currentVersion = new VersionInfo("3.9")
			};

            ClassicAssert.IsFalse(VersionUtility.CompatibleWithVersion(versionInfo, checkInfo));
		}

		[Test]
		public void VersionCheck_Equals_VersionsMatch_MinimumVersion_Valid()
		{
			var versionInfo = new VersionCompatibleObject()
			{
				minimumVersion = new VersionInfo("3.0"),
				targetVersion = new VersionInfo("4.1")
			};

			var checkInfo = new VersionedObject()
			{
				currentVersion = new VersionInfo("3.9")
			};

            ClassicAssert.IsTrue(VersionUtility.CompatibleWithVersion(versionInfo, checkInfo));
		}

		[Test]
		public void VersionCheck_Equals_VersionsMatch_TooNew_NotCompatible()
		{
			var versionInfo = new VersionCompatibleObject()
			{
				minimumVersion = new VersionInfo("3.0"),
				targetVersion = new VersionInfo("4.1")
			};

			var checkInfo = new VersionedObject()
			{
				currentVersion = new VersionInfo("5.1")
			};

            ClassicAssert.IsFalse(VersionUtility.CompatibleWithVersion(versionInfo, checkInfo));
		}

		[Test]
		public void VersionCheck_Equals_VersionsMatch_TooNew_Compatible_Overlap()
		{
			var versionInfo = new VersionCompatibleObject()
			{
				minimumVersion = new VersionInfo("3.0"),
				targetVersion = new VersionInfo("4.1")
			};

			var checkInfo = new VersionedObject()
			{
				minimumSupportedVersion = new VersionInfo("4.0"),
				currentVersion = new VersionInfo("5.1")
			};

            ClassicAssert.IsTrue(VersionUtility.CompatibleWithVersion(versionInfo, checkInfo));
		}

		[Test]
		public void VersionCheck_Equals_VersionsMatch_Old_Compatible()
		{
			var versionInfo = new VersionCompatibleObject()
			{
				targetVersion = new VersionInfo("4.1")
			};

			var checkInfo = new VersionedObject()
			{
				minimumSupportedVersion = new VersionInfo("4.0"),
				currentVersion = new VersionInfo("5.1")
			};

            ClassicAssert.IsTrue(VersionUtility.CompatibleWithVersion(versionInfo, checkInfo));
		}

		[Test]
		public void VersionCheck_TargetAndMinimum_Greater_NotCompatible()
		{
			var versionInfo = new VersionCompatibleObject()
			{
				minimumVersion = new VersionInfo("6.0"),
				targetVersion = new VersionInfo("7.1")
			};

			var checkInfo = new VersionedObject()
			{
				currentVersion = new VersionInfo("5.1")
			};

            ClassicAssert.IsFalse(VersionUtility.CompatibleWithVersion(versionInfo, checkInfo));
		}

		[Test]
		public void VersionCheck_Application()
		{
			var versionInfo = new VersionCompatibleObject()
			{
				minimumVersion = new VersionInfo("1.0"),
				targetVersion = new VersionInfo("2.0")
			};

			var checkInfo = new VersionedObject()
			{
				currentVersion = new VersionInfo("1.0")
			};

            ClassicAssert.IsTrue(VersionUtility.CompatibleWithVersion(versionInfo, checkInfo));
		}
	}
}