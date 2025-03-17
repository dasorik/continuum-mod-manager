using Continuum.Core;
using Continuum.Core.Enums;
using Continuum.Core.InstallActions;
using Continuum.Core.Models;
using Continuum.Core.Test;
using Continuum.Core.Utilities;
using NUnit.Framework.Legacy;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Continuum.Core.Test
{
	[TestFixture]
	class ModInstallerTests : BaseModTests
	{
		[SetUp]
		public void SetUp()
		{
			base.SetUpTestData();
		}

		[TearDown]
		public void TearDown()
		{
			base.TryDeleteTempFolders(configuration);
		}

		// Delete File

		[Test]
		public async Task DeleteFiles()
		{
			ClassicAssert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));
			ClassicAssert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "assets\\asset1.pak")));

			var installer = new ModInstaller(configuration, integration);
			var modConfig = CreateTestModConfig("test-mod");

			modConfig.InstallActions = new[]
			{
				new DeleteFilesAction()
				{
					TargetFiles = new[]
					{
						"[Game]\\resource1.pak",
						"[Game]\\assets\\asset1.pak",
					}
				}
			};

			var modInstallInfo = GetModInstallationInfo(modConfig);
			var result = await installer.ApplyChanges(modInstallInfo);

			ClassicAssert.AreEqual(InstallationStatus.Success, result.status);
			ClassicAssert.IsTrue(!File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));
			ClassicAssert.IsTrue(!File.Exists(Path.Combine(this.configuration.TargetPath, "assets\\asset1.pak")));
		}

		[Test]
		public async Task DeleteFiles_SameFIle()
		{
			ClassicAssert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));

			var installer = new ModInstaller(configuration, integration);
			var modConfig = CreateTestModConfig("test-mod");

			modConfig.InstallActions = new[]
			{
				new DeleteFilesAction()
				{
					TargetFiles = new[]
					{
						"[Game]\\resource1.pak",
						"[Game]\\resource1.pak",
					}
				}
			};

			var modInstallInfo = GetModInstallationInfo(modConfig);
			var result = await installer.ApplyChanges(modInstallInfo);

			ClassicAssert.AreEqual(InstallationStatus.Success, result.status);
			ClassicAssert.IsTrue(!File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));
		}

		[Test]
		public async Task DeleteFiles_NonExistent()
		{
			ClassicAssert.IsTrue(!File.Exists(Path.Combine(this.configuration.TargetPath, "resource12.pak")));

			var installer = new ModInstaller(configuration, integration);
			var modConfig = CreateTestModConfig("test-mod");

			modConfig.InstallActions = new[]
			{
				new DeleteFilesAction()
				{
					TargetFiles = new[]
					{
						"[Game]\\resource12.pak"
					}
				}
			};

			var modInstallInfo = GetModInstallationInfo(modConfig);
			var result = await installer.ApplyChanges(modInstallInfo);

			ClassicAssert.AreEqual(InstallationStatus.Success, result.status);
		}

		[Test]
		public async Task DeleteFiles_SameFileDeletedTwice()
		{
			ClassicAssert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));

			var installer = new ModInstaller(configuration, integration);
			var modConfig = CreateTestModConfig("test-mod");
			var modConfig2 = CreateTestModConfig("test-mod2");

			modConfig.InstallActions = new[]
			{
				new DeleteFilesAction()
				{
					TargetFiles = new[]
					{
						"[Game]\\resource1.pak",
						"[Game]\\assets\\asset1.pak"
					}
				}
			};

			modConfig2.InstallActions = new[]
			{
				new DeleteFilesAction()
				{
					TargetFiles = new[]
					{
						"[Game]\\resource1.pak"
					}
				}
			};

			var modInstallInfo = GetModInstallationInfo(modConfig);
			var modInstallInfo2 = GetModInstallationInfo(modConfig2);
			var result = await installer.ApplyChanges(new[] { modInstallInfo, modInstallInfo2 });

			ClassicAssert.AreEqual(InstallationStatus.Success, result.status);
		}

		// Move File

		[Test]
		public async Task MoveFile_GameToGame()
		{
			ClassicAssert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));
			ClassicAssert.IsTrue(!File.Exists(Path.Combine(this.configuration.TargetPath, "resource3.pak")));

			var installer = new ModInstaller(configuration, integration);
			var modConfig = CreateTestModConfig("test-mod");

			modConfig.InstallActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Game]\\resource1.pak",
					DestinationPath = "[Game]\\resource3.pak"
				}
			};

			var modInstallInfo = GetModInstallationInfo(modConfig);
			var result = await installer.ApplyChanges(modInstallInfo);

			ClassicAssert.AreEqual(InstallationStatus.Success, result.status);
			ClassicAssert.IsTrue(!File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));
			ClassicAssert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource3.pak")));
		}

		[Test]
		public async Task MoveFile_ModToGame()
		{
			ClassicAssert.IsTrue(!File.Exists(Path.Combine(this.configuration.TargetPath, "assets\\mod_asset1.pak")));

			var installer = new ModInstaller(configuration, integration);
			var modConfig = CreateTestModConfig("test-mod");

			modConfig.InstallActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Mod]\\assets\\mod_asset1.pak",
					DestinationPath = "[Game]\\assets\\mod_asset3.pak"
				}
			};

			var modInstallInfo = GetModInstallationInfo(modConfig);
			var result = await installer.ApplyChanges(modInstallInfo);

			ClassicAssert.AreEqual(InstallationStatus.InvalidActions, result.status);
			ClassicAssert.IsTrue(!File.Exists(Path.Combine(this.configuration.TargetPath, "assets\\mod_asset3.pak")));
		}

		[Test]
		public async Task MoveFile_GameToMod()
		{
			var installer = new ModInstaller(configuration, integration);
			var modConfig = CreateTestModConfig("test-mod");

			modConfig.InstallActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Game]\\assets\\mod_asset1.pak",
					DestinationPath = "[Mod]\\assets\\mod_asset1.pak"
				}
			};

			var modInstallInfo = GetModInstallationInfo(modConfig);
			var result = await installer.ApplyChanges(modInstallInfo);

			ClassicAssert.AreEqual(InstallationStatus.InvalidActions, result.status);
		}

		[Test]
		public async Task MoveFile_ModToMod()
		{
			var installer = new ModInstaller(configuration, integration);
			var modConfig = CreateTestModConfig("test-mod");

			modConfig.InstallActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Mod]\\assets\\mod_asset1.pak",
					DestinationPath = "[Mod]\\assets\\mod_asset1.pak"
				}
			};

			var modInstallInfo = GetModInstallationInfo(modConfig);
			var result = await installer.ApplyChanges(modInstallInfo);

			ClassicAssert.AreEqual(InstallationStatus.InvalidActions, result.status);
		}

		[Test]
		public async Task MoveFile_SameFileMovedTwice_SameLocation()
		{
			ClassicAssert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));
			ClassicAssert.IsTrue(!File.Exists(Path.Combine(this.configuration.TargetPath, "resource3.pak")));

			var installer = new ModInstaller(configuration, integration);
			var modConfig = CreateTestModConfig("test-mod");
			var modConfig2 = CreateTestModConfig("test-mod2");

			modConfig.InstallActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Game]\\resource1.pak",
					DestinationPath = "[Game]\\resource3.pak"
				}
			};

			modConfig2.InstallActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Game]\\resource1.pak",
					DestinationPath = "[Game]\\resource3.pak"
				}
			};

			var modInstallInfo = GetModInstallationInfo(modConfig);
			var modInstallInfo2 = GetModInstallationInfo(modConfig2);
			var result = await installer.ApplyChanges(new[] { modInstallInfo, modInstallInfo2 });

			ClassicAssert.AreEqual(InstallationStatus.Success, result.status);
			ClassicAssert.IsTrue(!File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));
			ClassicAssert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource3.pak")));
		}

		[Test]
		public async Task MoveFile_SameFileMovedTwice_DifferentLocation()
		{
			ClassicAssert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));
			ClassicAssert.IsTrue(!File.Exists(Path.Combine(this.configuration.TargetPath, "resource3.pak")));

			var installer = new ModInstaller(configuration, integration);
			var modConfig = CreateTestModConfig("test-mod");
			var modConfig2 = CreateTestModConfig("test-mod2");

			modConfig.InstallActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Game]\\resource1.pak",
					DestinationPath = "[Game]\\resource3.pak"
				}
			};

			modConfig2.InstallActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Game]\\resource1.pak",
					DestinationPath = "[Game]\\resource4.pak"
				}
			};

			var modInstallInfo = GetModInstallationInfo(modConfig);
			var modInstallInfo2 = GetModInstallationInfo(modConfig2);
			var result = await installer.ApplyChanges(new[] { modInstallInfo, modInstallInfo2 });

			ClassicAssert.AreEqual(InstallationStatus.UnresolvableConflict, result.status);
			ClassicAssert.IsTrue(result.conflicts.First().description.Contains("Attempting to move a file"));
		}

		[Test]
		public async Task MoveFile_DifferentFilesMoved_SameDestination()
		{
			ClassicAssert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));
			ClassicAssert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource2.pak")));
			ClassicAssert.IsTrue(!File.Exists(Path.Combine(this.configuration.TargetPath, "resource3.pak")));

			var installer = new ModInstaller(configuration, integration);
			var modConfig = CreateTestModConfig("test-mod");
			var modConfig2 = CreateTestModConfig("test-mod2");

			modConfig.InstallActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Game]\\resource1.pak",
					DestinationPath = "[Game]\\resource3.pak"
				}
			};

			modConfig2.InstallActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Game]\\resource2.pak",
					DestinationPath = "[Game]\\resource3.pak"
				}
			};

			var modInstallInfo = GetModInstallationInfo(modConfig);
			var modInstallInfo2 = GetModInstallationInfo(modConfig2);
			var result = await installer.ApplyChanges(new[] { modInstallInfo, modInstallInfo2 });

			ClassicAssert.AreEqual(InstallationStatus.UnresolvableConflict, result.status);
			ClassicAssert.IsTrue(result.conflicts.First().description.Contains("Attempting to move a file that has been added by another mod (with different data)"), result.conflicts.First().description);
		}

		[Test]
		public async Task MoveFile_DifferentFilesMoved_DifferentDestination()
		{
			ClassicAssert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));
			ClassicAssert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource2.pak")));
			ClassicAssert.IsTrue(!File.Exists(Path.Combine(this.configuration.TargetPath, "resource3.pak")));

			var installer = new ModInstaller(configuration, integration);
			var modConfig = CreateTestModConfig("test-mod");
			var modConfig2 = CreateTestModConfig("test-mod2");

			modConfig.InstallActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Game]\\resource1.pak",
					DestinationPath = "[Game]\\resource3.pak"
				}
			};

			modConfig2.InstallActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Game]\\resource2.pak",
					DestinationPath = "[Game]\\resource4.pak"
				}
			};

			var modInstallInfo = GetModInstallationInfo(modConfig);
			var modInstallInfo2 = GetModInstallationInfo(modConfig2);
			var result = await installer.ApplyChanges(new[] { modInstallInfo, modInstallInfo2 });

			ClassicAssert.AreEqual(InstallationStatus.Success, result.status);

			ClassicAssert.IsTrue(!File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));
			ClassicAssert.IsTrue(!File.Exists(Path.Combine(this.configuration.TargetPath, "resource2.pak")));
			ClassicAssert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource3.pak")));
			ClassicAssert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource4.pak")));
		}

		[Test]
		public async Task MoveFile_SameFileDeleted()
		{
			ClassicAssert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));

			var installer = new ModInstaller(configuration, integration);
			var modConfig = CreateTestModConfig("test-mod");
			var modConfig2 = CreateTestModConfig("test-mod2");

			modConfig.InstallActions = new[]
			{
				new DeleteFilesAction()
				{
					TargetFiles = new[]
					{
						"[Game]\\resource1.pak"
					}
				}
			};

			modConfig2.InstallActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Game]\\resource1.pak",
					DestinationPath = "[Game]\\resource3.pak"
				}
			};

			var modInstallInfo = GetModInstallationInfo(modConfig);
			var modInstallInfo2 = GetModInstallationInfo(modConfig2);
			var result = await installer.ApplyChanges(new[] { modInstallInfo, modInstallInfo2 });

			ClassicAssert.AreEqual(InstallationStatus.Success, result.status);
		}

		[Test]
		public async Task MoveFile_SameFileDeleted_Invterted()
		{
			ClassicAssert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));

			var installer = new ModInstaller(configuration, integration);
			var modConfig = CreateTestModConfig("test-mod");
			var modConfig2 = CreateTestModConfig("test-mod2");

			modConfig.InstallActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Game]\\resource1.pak",
					DestinationPath = "[Game]\\resource3.pak"
				}
			};

			modConfig2.InstallActions = new[]
			{
				new DeleteFilesAction()
				{
					TargetFiles = new[]
					{
						"[Game]\\resource1.pak"
					}
				}
			};

			var modInstallInfo = GetModInstallationInfo(modConfig);
			var modInstallInfo2 = GetModInstallationInfo(modConfig2);
			var result = await installer.ApplyChanges(new[] { modInstallInfo, modInstallInfo2 });

			ClassicAssert.AreEqual(InstallationStatus.Success, result.status);
		}


		// Replace File
		[Test]
		public async Task ReplaceFile_SameFileReplaced_SameContents()
		{
			ClassicAssert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));

			var installer = new ModInstaller(configuration, integration);
			var modConfig = CreateTestModConfig("test-mod");
			var modConfig2 = CreateTestModConfig("test-mod2");

			modConfig.InstallActions = new[]
			{
				new ReplaceFileAction()
				{
					ReplacementFile = "[Mod]\\mod_resource1.pak",
					TargetFile = "[Game]\\resource1.pak"
				}
			};

			modConfig2.InstallActions = new[]
			{
				new ReplaceFileAction()
				{
					ReplacementFile = "[Mod]\\mod_resource1.pak",
					TargetFile = "[Game]\\resource1.pak"
				}
			};

			var mod1FilePath = CreateTempModFiles(modConfig);
			var mod2FilePath = CreateTempModFiles(modConfig2);

			var loader = new ModLoader(new[] { integration });
			loader.Load(mod1FilePath, mod2FilePath);

			var modInstallInfo = GetModInstallationInfo(modConfig);
			var modInstallInfo2 = GetModInstallationInfo(modConfig2);
			var result = await installer.ApplyChanges(new[] { modInstallInfo, modInstallInfo2 });

			string originalFilePath = Path.Combine(modConfig.CacheFolder, "resources\\mod_resource1.pak");
			string replacedFilePath = Path.Combine(this.configuration.TargetPath, "resource1.pak");

			ClassicAssert.AreEqual(InstallationStatus.Success, result.status);
			ClassicAssert.AreEqual(MD5Utility.CalculateMD5Hash(originalFilePath), MD5Utility.CalculateMD5Hash(replacedFilePath));
		}

		[Test]
		public async Task ReplaceFile_SameFileReplaced_DifferentContents()
		{
			ClassicAssert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));

			var installer = new ModInstaller(configuration, integration);
			var modConfig = CreateTestModConfig("test-mod");
			var modConfig2 = CreateTestModConfig("test-mod2");

			modConfig.InstallActions = new[]
			{
				new ReplaceFileAction()
				{
					ReplacementFile = "[Mod]\\mod_resource1.pak",
					TargetFile = "[Game]\\resource1.pak"
				}
			};

			modConfig2.InstallActions = new[]
			{
				new ReplaceFileAction()
				{
					ReplacementFile = "[Mod]\\mod_resource2.pak",
					TargetFile = "[Game]\\resource1.pak"
				}
			};

			var mod1FilePath = CreateTempModFiles(modConfig);
			var mod2FilePath = CreateTempModFiles(modConfig2);

			var loader = new ModLoader(new[] { integration });
			loader.Load(mod1FilePath, mod2FilePath);

			var modInstallInfo = GetModInstallationInfo(modConfig);
			var modInstallInfo2 = GetModInstallationInfo(modConfig2);
			var result = await installer.ApplyChanges(new[] { modInstallInfo, modInstallInfo2 });

			ClassicAssert.AreEqual(InstallationStatus.UnresolvableConflict, result.status);
		}


		[Test]
		public async Task ZipFiles()
		{
			ClassicAssert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));

			var installer = new ModInstaller(configuration, integration);
			var modConfig = CreateTestModConfig("test-mod");

			modConfig.InstallActions = new[]
			{
				new ZipFilesAction()
				{
					FilesToInclude = new[] { "[Game]\\resource1.pak", "[Game]\\resource2.pak" },
					DestinationPath = "[Game]\\resource_zip.zip"
				}
			};

			var modInstallInfo = GetModInstallationInfo(modConfig);
			var result = await installer.ApplyChanges(new[] { modInstallInfo });

			string zipPath = Path.Combine(this.configuration.TargetPath, "resource_zip.zip");

			ClassicAssert.AreEqual(InstallationStatus.Success, result.status);
			ClassicAssert.IsTrue(File.Exists(zipPath));
		}

		[Test]
		public async Task ZipDirectory()
		{
			ClassicAssert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));

			var installer = new ModInstaller(configuration, integration);
			var modConfig = CreateTestModConfig("test-mod");

			modConfig.InstallActions = new[]
			{
				new ZipDirectoryAction()
				{
					DirectoryPath = "[Game]\\assets",
					DestinationPath = "[Game]\\resource_zip.zip"
				}
			};

			var modInstallInfo = GetModInstallationInfo(modConfig);
			var result = await installer.ApplyChanges(new[] { modInstallInfo });

			string zipPath = Path.Combine(this.configuration.TargetPath, "resource_zip.zip");

			ClassicAssert.AreEqual(InstallationStatus.Success, result.status);
			ClassicAssert.IsTrue(File.Exists(zipPath));
		}

		[Test]
		public async Task UnzipFile_DontDeleteOnComplete()
		{
			ClassicAssert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));

			var installer = new ModInstaller(configuration, integration);
			var modConfig = CreateTestModConfig("test-mod");

			modConfig.InstallActions = new[]
			{
				new UnzipFileAction()
				{
					TargetFile = "[Game]\\zip1.zip",
					DestinationPath = "[Game]\\unzipped_assets",
					DeleteWhenComplete = false
				}
			};

			var modInstallInfo = GetModInstallationInfo(modConfig);
			var result = await installer.ApplyChanges(new[] { modInstallInfo });

			string originalZipPath = Path.Combine(this.configuration.TargetPath, "zip1.zip");
			string newZipPath = Path.Combine(this.configuration.TargetPath, "unzipped_assets");

			ClassicAssert.AreEqual(InstallationStatus.Success, result.status);
			ClassicAssert.IsTrue(File.Exists(originalZipPath));
			ClassicAssert.IsTrue(File.Exists(Path.Combine(newZipPath, "game_resource1.pak")));
		}

		[Test]
		public async Task UnzipFile_DeleteOnComplete()
		{
			ClassicAssert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));

			var installer = new ModInstaller(configuration, integration);
			var modConfig = CreateTestModConfig("test-mod");

			modConfig.InstallActions = new[]
			{
				new UnzipFileAction()
				{
					TargetFile = "[Game]\\zip1.zip",
					DestinationPath = "[Game]\\unzipped_assets",
					DeleteWhenComplete = true
				}
			};

			var modInstallInfo = GetModInstallationInfo(modConfig);
			var result = await installer.ApplyChanges(new[] { modInstallInfo });

			string originalZipPath = Path.Combine(this.configuration.TargetPath, "zip1.zip");
			string newZipPath = Path.Combine(this.configuration.TargetPath, "unzipped_assets");

			ClassicAssert.AreEqual(InstallationStatus.Success, result.status);
			ClassicAssert.IsTrue(!File.Exists(originalZipPath));
			ClassicAssert.IsTrue(File.Exists(Path.Combine(newZipPath, "game_resource1.pak")));
		}

		[Test]
		public async Task UnzipFiles_DontDeleteOnComplete()
		{
			ClassicAssert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));

			var installer = new ModInstaller(configuration, integration);
			var modConfig = CreateTestModConfig("test-mod");

			modConfig.InstallActions = new[]
			{
				new UnzipFilesAction()
				{
					TargetFiles = new[] { "[Game]\\zip1.zip", "[Game]\\zip2.zip" },
					DeleteWhenComplete = false
				}
			};

			var modInstallInfo = GetModInstallationInfo(modConfig);
			var result = await installer.ApplyChanges(new[] { modInstallInfo });

			string originalZipPath1 = Path.Combine(this.configuration.TargetPath, "zip1.zip");
			string originalZipPath2 = Path.Combine(this.configuration.TargetPath, "zip2.zip");
			string newZipPath1 = Path.Combine(this.configuration.TargetPath, "zip1");
			string newZipPath2 = Path.Combine(this.configuration.TargetPath, "zip2");

			ClassicAssert.AreEqual(InstallationStatus.Success, result.status);
			ClassicAssert.IsTrue(File.Exists(originalZipPath1));
			ClassicAssert.IsTrue(File.Exists(originalZipPath2));
			ClassicAssert.IsTrue(File.Exists(Path.Combine(newZipPath1, "game_resource1.pak")));
			ClassicAssert.IsTrue(File.Exists(Path.Combine(newZipPath2, "game_resource3.pak")));
		}

		[Test]
		public async Task UnzipFiles_DeleteOnComplete()
		{
			ClassicAssert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));

			var installer = new ModInstaller(configuration, integration);
			var modConfig = CreateTestModConfig("test-mod");

			modConfig.InstallActions = new[]
			{
				new UnzipFilesAction()
				{
					TargetFiles = new[] { "[Game]\\zip1.zip", "[Game]\\zip2.zip" },
					DeleteWhenComplete = true
				}
			};

			var modInstallInfo = GetModInstallationInfo(modConfig);
			var result = await installer.ApplyChanges(new[] { modInstallInfo });

			string originalZipPath1 = Path.Combine(this.configuration.TargetPath, "zip1.zip");
			string originalZipPath2 = Path.Combine(this.configuration.TargetPath, "zip2.zip");
			string newZipPath1 = Path.Combine(this.configuration.TargetPath, "zip1");
			string newZipPath2 = Path.Combine(this.configuration.TargetPath, "zip2");

			ClassicAssert.AreEqual(InstallationStatus.Success, result.status);
			ClassicAssert.IsTrue(!File.Exists(originalZipPath1));
			ClassicAssert.IsTrue(!File.Exists(originalZipPath2));
			ClassicAssert.IsTrue(File.Exists(Path.Combine(newZipPath1, "game_resource1.pak")));
			ClassicAssert.IsTrue(File.Exists(Path.Combine(newZipPath2, "game_resource3.pak")));
		}

		[Test]
		public async Task UnzipFiles_ExtractToSameDirectory()
		{
			ClassicAssert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));

			var installer = new ModInstaller(configuration, integration);
			var modConfig = CreateTestModConfig("test-mod");

			modConfig.InstallActions = new[]
			{
				new UnzipFilesAction()
				{
					TargetFiles = new[] { "[Game]\\zip1.zip", "[Game]\\zip2.zip" },
					ExtractToSameDirectory = true,
					DeleteWhenComplete = true
				}
			};

			var modInstallInfo = GetModInstallationInfo(modConfig);
			var result = await installer.ApplyChanges(new[] { modInstallInfo });

			string originalZipPath1 = Path.Combine(this.configuration.TargetPath, "zip1.zip");
			string originalZipPath2 = Path.Combine(this.configuration.TargetPath, "zip2.zip");
			string newZipPath1 = Path.Combine(this.configuration.TargetPath, "zip1");
			string newZipPath2 = Path.Combine(this.configuration.TargetPath, "zip2");

			ClassicAssert.AreEqual(InstallationStatus.Success, result.status);
			ClassicAssert.IsTrue(!File.Exists(originalZipPath1));
			ClassicAssert.IsTrue(!File.Exists(originalZipPath2));
			ClassicAssert.IsTrue(!File.Exists(Path.Combine(newZipPath1, "game_resource1.pak")));
			ClassicAssert.IsTrue(!File.Exists(Path.Combine(newZipPath2, "game_resource3.pak")));
			ClassicAssert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "game_resource1.pak")));
			ClassicAssert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "game_resource3.pak")));
		}

		[Test]
		public async Task Integration_Setup()
		{
			ClassicAssert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));

			var newIntegration = CreateTestIntegration("test-integration");
			var installer = new ModInstaller(configuration, newIntegration);

			newIntegration.SetupActions = new[]
			{
				new CopyFileAction()
				{
					TargetFile = "[Integration]\\mod_resource1.pak",
					DestinationPath = "[Game]\\test.pak"
				}
			};

			string path = CreateTempIntegrationFiles(newIntegration);

			var result = await installer.ApplySetupActions(newIntegration);

			string integrationFile = Path.Combine(this.configuration.TargetPath, "test.pak");

			ClassicAssert.AreEqual(InstallationStatus.Success, result.status);
			ClassicAssert.IsTrue(File.Exists(integrationFile));
		}

		[Test]
		public async Task Integration_Setup_NoFileMoves()
		{
			ClassicAssert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));

			var newIntegration = CreateTestIntegration("test-integration");
			var installer = new ModInstaller(configuration, newIntegration);

			newIntegration.SetupActions = new ModInstallAction[0];

			string path = CreateTempIntegrationFiles(newIntegration);

			var result = await installer.ApplySetupActions(newIntegration);

			string integrationFile = Path.Combine(this.configuration.TargetPath, "test.pak");

			ClassicAssert.AreEqual(InstallationStatus.Success, result.status);
			ClassicAssert.IsTrue(!File.Exists(integrationFile));
		}

		[Test]
		public async Task Mod_Install_SelfConflict_MoveAction()
		{
			var installer = new ModInstaller(configuration, integration);
			var modConfig = CreateTestModConfig("test-mod");
			
			modConfig.InstallActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Game]\\assets\\asset1.pak",
					DestinationPath = "[Game]\\assets\\game_asset_renamed1.pak"
				},
				new MoveFileAction()
				{
					TargetFile = "[Game]\\assets\\asset1.pak",
					DestinationPath = "[Game]\\assets\\game_asset_renamed2.pak"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var loader = new ModLoader(new[] { integration });
			loader.Load(modPath);

			var modInstallInfo = GetModInstallationInfo(modConfig);
			var result = await installer.ApplyChanges(modInstallInfo);

			ClassicAssert.AreEqual(InstallationStatus.RolledBackError, result.status); // Mod doesn't conflict with itself, but does still fail
		}

		[Test]
		public async Task Mod_Install_SelfConflict_ReplaceAction()
		{
			var installer = new ModInstaller(configuration, integration);
			var modConfig = CreateTestModConfig("test-mod");

			modConfig.InstallActions = new[]
			{
				new ReplaceFileAction()
				{
					TargetFile = "[Game]\\assets\\asset1.pak",
					ReplacementFile = "[Mod]\\mod_resource1.pak"
				},
				new ReplaceFileAction()
				{
					TargetFile = "[Game]\\assets\\asset1.pak",
					ReplacementFile = "[Mod]\\mod_resource2.pak"
				},
			};

			var modPath = CreateTempModFiles(modConfig);
			var loader = new ModLoader(new[] { integration });
			loader.Load(modPath);

			var modInstallInfo = GetModInstallationInfo(modConfig);
			var result = await installer.ApplyChanges(modInstallInfo);

			ClassicAssert.AreEqual(InstallationStatus.Success, result.status);
		}

		[Test]
		public async Task Mod_Install_Conflict_CheckResults()
		{
			var installer = new ModInstaller(configuration, integration);
			var modConfig1 = CreateTestModConfig("test-mod");
			var modConfig2 = CreateTestModConfig("test-mod2");

			modConfig1.InstallActions = new[]
			{
				new ReplaceFileAction()
				{
					TargetFile = "[Game]\\assets\\asset1.pak",
					ReplacementFile = "[Mod]\\mod_resource1.pak"
				}
			};

			modConfig2.InstallActions = new[]
			{
				new ReplaceFileAction()
				{
					TargetFile = "[Game]\\assets\\asset1.pak",
					ReplacementFile = "[Mod]\\mod_resource2.pak"
				}
			};

			var modPath1 = CreateTempModFiles(modConfig1);
			var modPath2 = CreateTempModFiles(modConfig2);

			var loader = new ModLoader(new[] { integration });
			loader.Load(modPath1, modPath2);

			var modInstallInfo1 = GetModInstallationInfo(modConfig1);
			var modInstallInfo2 = GetModInstallationInfo(modConfig2);
			var result = await installer.ApplyChanges(modInstallInfo1, modInstallInfo2);

			ClassicAssert.AreEqual(InstallationStatus.UnresolvableConflict, result.status);
			ClassicAssert.AreEqual("test-mod", result.conflicts.FirstOrDefault().modID);
		}

		[Test]
		public async Task CheckUninstallRollback_TwoInstalled_OneRolledBack()
		{
			var installer = new ModInstaller(configuration, integration);
			var modConfig1 = CreateTestModConfig("test-mod");
			var modConfig2 = CreateTestModConfig("test-mod2");
			var modConfig3 = CreateTestModConfig("test-mod3");

			modConfig1.InstallActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Game]\\assets\\asset1.pak",
					DestinationPath = "[Game]\\assets\\asset1_renamed.pak"
				}
			};

			modConfig2.InstallActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Game]\\assets\\asset2.pak",
					DestinationPath = "[Game]\\assets\\asset2_renamed.pak"
				}
			};

			modConfig3.InstallActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Game]\\assets\\asset2.pak",
					DestinationPath = "[Game]\\assets\\asset2_conflict.pak"
				}
			};

			var modPath1 = CreateTempModFiles(modConfig1);
			var modPath2 = CreateTempModFiles(modConfig2);
			var modPath3 = CreateTempModFiles(modConfig3);

			var loader = new ModLoader(new[] { integration });
			loader.Load(modPath1, modPath2, modPath3);

			var modInstallInfo1 = GetModInstallationInfo(modConfig1);
			var modInstallInfo2 = GetModInstallationInfo(modConfig2);
			var modInstallInfo3 = GetModInstallationInfo(modConfig3);

			// Install the first two mods, and then install the third after
			var firstResult = await installer.ApplyChanges(modInstallInfo1, modInstallInfo2);
			var secondResult = await installer.ApplyChanges(modInstallInfo1, modInstallInfo2, modInstallInfo3);

			ClassicAssert.AreEqual(InstallationStatus.Success, firstResult.status);
			ClassicAssert.AreEqual(InstallationStatus.UnresolvableConflict, secondResult.status);

			string modFile1 = Path.Combine(this.configuration.TargetPath, "assets\\asset1_renamed.pak");
			string modFile2 = Path.Combine(this.configuration.TargetPath, "assets\\asset2_renamed.pak");
			string modFile3 = Path.Combine(this.configuration.TargetPath, "assets\\asset2_conflict.pak");

			ClassicAssert.IsTrue(File.Exists(modFile1));
			ClassicAssert.IsTrue(File.Exists(modFile2));
			ClassicAssert.IsTrue(!File.Exists(modFile3));
		}

		[Test]
		public async Task CheckUninstallRollback_OneInstalled_TwoRolledBack()
		{
			var installer = new ModInstaller(configuration, integration);
			var modConfig1 = CreateTestModConfig("test-mod");
			var modConfig2 = CreateTestModConfig("test-mod2");
			var modConfig3 = CreateTestModConfig("test-mod3");

			modConfig1.InstallActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Game]\\assets\\asset1.pak",
					DestinationPath = "[Game]\\assets\\asset1_renamed.pak"
				}
			};

			modConfig2.InstallActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Game]\\assets\\asset2.pak",
					DestinationPath = "[Game]\\assets\\asset2_renamed.pak"
				}
			};

			modConfig3.InstallActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Game]\\assets\\asset1.pak",
					DestinationPath = "[Game]\\assets\\asset1_conflict.pak"
				}
			};

			var modPath1 = CreateTempModFiles(modConfig1);
			var modPath2 = CreateTempModFiles(modConfig2);
			var modPath3 = CreateTempModFiles(modConfig3);

			var loader = new ModLoader(new[] { integration });
			loader.Load(modPath1, modPath2, modPath3);

			var modInstallInfo1 = GetModInstallationInfo(modConfig1);
			var modInstallInfo2 = GetModInstallationInfo(modConfig2);
			var modInstallInfo3 = GetModInstallationInfo(modConfig3);

			// Install the first two mods, and then install the third after
			var firstResult = await installer.ApplyChanges(modInstallInfo1);
			var secondResult = await installer.ApplyChanges(modInstallInfo1, modInstallInfo2, modInstallInfo3);

			ClassicAssert.AreEqual(InstallationStatus.Success, firstResult.status);
			ClassicAssert.AreEqual(InstallationStatus.UnresolvableConflict, secondResult.status);

			string modFile1 = Path.Combine(this.configuration.TargetPath, "assets\\asset1_renamed.pak");
			string modFile2 = Path.Combine(this.configuration.TargetPath, "assets\\asset2_renamed.pak");
			string modFile3 = Path.Combine(this.configuration.TargetPath, "assets\\asset1_conflict.pak");

			ClassicAssert.IsTrue(File.Exists(modFile1));
			ClassicAssert.IsTrue(!File.Exists(modFile2));
			ClassicAssert.IsTrue(!File.Exists(modFile3));
		}
	}
}
