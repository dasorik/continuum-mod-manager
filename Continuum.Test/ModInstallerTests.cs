using Continuum.Core;
using Continuum.Core.Enums;
using Continuum.Core.InstallActions;
using Continuum.Core.Models;
using Continuum.Core.Test;
using Continuum.Core.Utilities;
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
			Assert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));
			Assert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "assets\\asset1.pak")));

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

			Assert.AreEqual(InstallationStatus.Success, result.status);
			Assert.IsTrue(!File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));
			Assert.IsTrue(!File.Exists(Path.Combine(this.configuration.TargetPath, "assets\\asset1.pak")));
		}

		[Test]
		public async Task DeleteFiles_SameFIle()
		{
			Assert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));

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

			Assert.AreEqual(InstallationStatus.Success, result.status);
			Assert.IsTrue(!File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));
		}

		[Test]
		public async Task DeleteFiles_NonExistent()
		{
			Assert.IsTrue(!File.Exists(Path.Combine(this.configuration.TargetPath, "resource12.pak")));

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

			Assert.AreEqual(InstallationStatus.Success, result.status);
		}

		[Test]
		public async Task DeleteFiles_SameFileDeletedTwice()
		{
			Assert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));

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

			Assert.AreEqual(InstallationStatus.Success, result.status);
		}

		// Move File

		[Test]
		public async Task MoveFile_GameToGame()
		{
			Assert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));
			Assert.IsTrue(!File.Exists(Path.Combine(this.configuration.TargetPath, "resource3.pak")));

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

			Assert.AreEqual(InstallationStatus.Success, result.status);
			Assert.IsTrue(!File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));
			Assert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource3.pak")));
		}

		[Test]
		public async Task MoveFile_ModToGame()
		{
			Assert.IsTrue(!File.Exists(Path.Combine(this.configuration.TargetPath, "assets\\mod_asset1.pak")));

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

			Assert.AreEqual(InstallationStatus.InvalidActions, result.status);
			Assert.IsTrue(!File.Exists(Path.Combine(this.configuration.TargetPath, "assets\\mod_asset3.pak")));
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

			Assert.AreEqual(InstallationStatus.InvalidActions, result.status);
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

			Assert.AreEqual(InstallationStatus.InvalidActions, result.status);
		}

		[Test]
		public async Task MoveFile_SameFileMovedTwice_SameLocation()
		{
			Assert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));
			Assert.IsTrue(!File.Exists(Path.Combine(this.configuration.TargetPath, "resource3.pak")));

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

			Assert.AreEqual(InstallationStatus.Success, result.status);
			Assert.IsTrue(!File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));
			Assert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource3.pak")));
		}

		[Test]
		public async Task MoveFile_SameFileMovedTwice_DifferentLocation()
		{
			Assert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));
			Assert.IsTrue(!File.Exists(Path.Combine(this.configuration.TargetPath, "resource3.pak")));

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

			Assert.AreEqual(InstallationStatus.UnresolvableConflict, result.status);
			Assert.IsTrue(result.conflicts.First().description.Contains("Attempting to move a file"));
		}

		[Test]
		public async Task MoveFile_DifferentFilesMoved_SameDestination()
		{
			Assert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));
			Assert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource2.pak")));
			Assert.IsTrue(!File.Exists(Path.Combine(this.configuration.TargetPath, "resource3.pak")));

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

			Assert.AreEqual(InstallationStatus.UnresolvableConflict, result.status);
			Assert.IsTrue(result.conflicts.First().description.Contains("Attempting to move a file that has been added by another mod (with different data)"), result.conflicts.First().description);
		}

		[Test]
		public async Task MoveFile_DifferentFilesMoved_DifferentDestination()
		{
			Assert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));
			Assert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource2.pak")));
			Assert.IsTrue(!File.Exists(Path.Combine(this.configuration.TargetPath, "resource3.pak")));

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

			Assert.AreEqual(InstallationStatus.Success, result.status);

			Assert.IsTrue(!File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));
			Assert.IsTrue(!File.Exists(Path.Combine(this.configuration.TargetPath, "resource2.pak")));
			Assert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource3.pak")));
			Assert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource4.pak")));
		}

		[Test]
		public async Task MoveFile_SameFileDeleted()
		{
			Assert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));

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

			Assert.AreEqual(InstallationStatus.Success, result.status);
		}

		[Test]
		public async Task MoveFile_SameFileDeleted_Invterted()
		{
			Assert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));

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

			Assert.AreEqual(InstallationStatus.Success, result.status);
		}


		// Replace File
		[Test]
		public async Task ReplaceFile_SameFileReplaced_SameContents()
		{
			Assert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));

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

			var loader = new ModLoader(modCacheFolder, new[] { integration });
			loader.Load(mod1FilePath, mod2FilePath);

			var modInstallInfo = GetModInstallationInfo(modConfig);
			var modInstallInfo2 = GetModInstallationInfo(modConfig2);
			var result = await installer.ApplyChanges(new[] { modInstallInfo, modInstallInfo2 });

			string originalFilePath = Path.Combine(modConfig.CacheFolder, "resources\\mod_resource1.pak");
			string replacedFilePath = Path.Combine(this.configuration.TargetPath, "resource1.pak");

			Assert.AreEqual(InstallationStatus.Success, result.status);
			Assert.AreEqual(MD5Utility.CalculateMD5Hash(originalFilePath), MD5Utility.CalculateMD5Hash(replacedFilePath));
		}

		[Test]
		public async Task ReplaceFile_SameFileReplaced_DifferentContents()
		{
			Assert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));

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

			var loader = new ModLoader(modCacheFolder, new[] { integration });
			loader.Load(mod1FilePath, mod2FilePath);

			var modInstallInfo = GetModInstallationInfo(modConfig);
			var modInstallInfo2 = GetModInstallationInfo(modConfig2);
			var result = await installer.ApplyChanges(new[] { modInstallInfo, modInstallInfo2 });

			Assert.AreEqual(InstallationStatus.UnresolvableConflict, result.status);
		}


		[Test]
		public async Task ZipFiles()
		{
			Assert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));

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

			Assert.AreEqual(InstallationStatus.Success, result.status);
			Assert.IsTrue(File.Exists(zipPath));
		}

		[Test]
		public async Task ZipDirectory()
		{
			Assert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));

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

			Assert.AreEqual(InstallationStatus.Success, result.status);
			Assert.IsTrue(File.Exists(zipPath));
		}

		[Test]
		public async Task UnzipFile_DontDeleteOnComplete()
		{
			Assert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));

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

			Assert.AreEqual(InstallationStatus.Success, result.status);
			Assert.IsTrue(File.Exists(originalZipPath));
			Assert.IsTrue(File.Exists(Path.Combine(newZipPath, "game_resource1.pak")));
		}

		[Test]
		public async Task UnzipFile_DeleteOnComplete()
		{
			Assert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));

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

			Assert.AreEqual(InstallationStatus.Success, result.status);
			Assert.IsTrue(!File.Exists(originalZipPath));
			Assert.IsTrue(File.Exists(Path.Combine(newZipPath, "game_resource1.pak")));
		}

		[Test]
		public async Task UnzipFiles_DontDeleteOnComplete()
		{
			Assert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));

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

			Assert.AreEqual(InstallationStatus.Success, result.status);
			Assert.IsTrue(File.Exists(originalZipPath1));
			Assert.IsTrue(File.Exists(originalZipPath2));
			Assert.IsTrue(File.Exists(Path.Combine(newZipPath1, "game_resource1.pak")));
			Assert.IsTrue(File.Exists(Path.Combine(newZipPath2, "game_resource3.pak")));
		}

		[Test]
		public async Task UnzipFiles_DeleteOnComplete()
		{
			Assert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));

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

			Assert.AreEqual(InstallationStatus.Success, result.status);
			Assert.IsTrue(!File.Exists(originalZipPath1));
			Assert.IsTrue(!File.Exists(originalZipPath2));
			Assert.IsTrue(File.Exists(Path.Combine(newZipPath1, "game_resource1.pak")));
			Assert.IsTrue(File.Exists(Path.Combine(newZipPath2, "game_resource3.pak")));
		}

		[Test]
		public async Task UnzipFiles_ExtractToSameDirectory()
		{
			Assert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));

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

			Assert.AreEqual(InstallationStatus.Success, result.status);
			Assert.IsTrue(!File.Exists(originalZipPath1));
			Assert.IsTrue(!File.Exists(originalZipPath2));
			Assert.IsTrue(!File.Exists(Path.Combine(newZipPath1, "game_resource1.pak")));
			Assert.IsTrue(!File.Exists(Path.Combine(newZipPath2, "game_resource3.pak")));
			Assert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "game_resource1.pak")));
			Assert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "game_resource3.pak")));
		}

		[Test]
		public async Task Integration_Setup()
		{
			Assert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));

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
			Utilities.ZipUtility.Unzip(path, newIntegration.CacheFolder);

			var result = await installer.ApplySetupActions(newIntegration);

			string integrationFile = Path.Combine(this.configuration.TargetPath, "test.pak");

			Assert.AreEqual(InstallationStatus.Success, result.status);
			Assert.IsTrue(File.Exists(integrationFile));
		}

		[Test]
		public async Task Integration_Setup_NoFileMoves()
		{
			Assert.IsTrue(File.Exists(Path.Combine(this.configuration.TargetPath, "resource1.pak")));

			var newIntegration = CreateTestIntegration("test-integration");
			var installer = new ModInstaller(configuration, newIntegration);

			newIntegration.SetupActions = new ModInstallAction[0];

			string path = CreateTempIntegrationFiles(newIntegration);
			Utilities.ZipUtility.Unzip(path, newIntegration.CacheFolder);

			var result = await installer.ApplySetupActions(newIntegration);

			string integrationFile = Path.Combine(this.configuration.TargetPath, "test.pak");

			Assert.AreEqual(InstallationStatus.Success, result.status);
			Assert.IsTrue(!File.Exists(integrationFile));
		}
	}
}
