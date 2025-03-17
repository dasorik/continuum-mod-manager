using Continuum.Core;
using Continuum.Core.Enums;
using Continuum.Core.InstallActions;
using Continuum.Core.Models;
using Continuum.Core.Test;
using NUnit.Framework.Legacy;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Continuum.Core.Test
{
	[TestFixture]
	class ModLoaderTests : BaseModTests
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

		// DELETE

		[Test]
		public void DeleteFiles_ValidConfig()
		{
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

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void DeleteFiles_InvalidConfig_NullTargetFiles()
		{
			var modConfig = CreateTestModConfig("test-mod");

			modConfig.InstallActions = new[]
			{
				new DeleteFilesAction()
				{
					TargetFiles = null
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("DeleteFiles - TargetFiles: No items provided in 'TargetFiles' list", result.First().loadErrors.First());
		}

		[Test]
		public void DeleteFiles_InvalidConfig_InvalidPath()
		{
			var modConfig = CreateTestModConfig("test-mod");

			modConfig.InstallActions = new[]
			{
				new DeleteFilesAction()
				{
					TargetFiles = new[]
					{
						"[Mod]\\resource1.pak",
					}
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("DeleteFiles - TargetFiles: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void DeleteFiles_InvalidConfig_NullPath()
		{
			var modConfig = CreateTestModConfig("test-mod");

			modConfig.InstallActions = new[]
			{
				new DeleteFilesAction()
				{
					TargetFiles = new string[]
					{
						null
					}
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("DeleteFiles - TargetFiles: One or more target files were NULL or Empty", result.First().loadErrors.First());
		}

		[Test]
		public void DeleteFiles_InvalidConfig_EmptyPath()
		{
			var modConfig = CreateTestModConfig("test-mod");

			modConfig.InstallActions = new[]
			{
				new DeleteFilesAction()
				{
					TargetFiles = new[]
					{
						string.Empty
					}
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("DeleteFiles - TargetFiles: One or more target files were NULL or Empty", result.First().loadErrors.First());
		}

		[Test]
		public void DeleteFiles_InvalidConfig_EmptyContent()
		{
			var modConfig = CreateTestModConfig("test-mod");

			modConfig.InstallActions = new[]
			{
				new DeleteFilesAction()
				{
					TargetFiles = new string[0]
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("DeleteFiles - TargetFiles: No items provided in 'TargetFiles' list", result.First().loadErrors.First());
		}


		// Move File

		[Test]
		public void MoveFile_GameToGame_ValidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Game]\\resource1.pak",
					DestinationPath = "[Game]\\resource3.pak"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void MoveFile_ModToGame_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Mod]\\assets\\mod_asset1.pak",
					DestinationPath = "[Game]\\assets\\mod_asset1.pak"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("MoveFile - TargetFile: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void MoveFile_GameToMod_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Game]\\assets\\mod_asset1.pak",
					DestinationPath = "[Mod]\\assets\\mod_asset1.pak"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("MoveFile - DestinationPath: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void MoveFile_ModToMod_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Mod]\\assets\\mod_asset1.pak",
					DestinationPath = "[Mod]\\assets\\mod_asset1.pak"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("MoveFile - DestinationPath: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void MoveFile_NullTargetFile_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = null,
					DestinationPath = "[Mod]\\assets\\mod_asset1.pak"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("MoveFile - TargetFile: Provided path must not be NULL or empty", result.First().loadErrors.First());
		}

		[Test]
		public void MoveFile_NullDestinationPath_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Mod]\\assets\\mod_asset1.pak",
					DestinationPath = null
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("MoveFile - DestinationPath: Provided path must not be NULL or empty", result.First().loadErrors.First());
		}


		// Move Files

		[Test]
		public void MoveFiles_GameToGame_ValidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new MoveFilesAction()
				{
					TargetPath = "[Game]\\resource1.pak",
					DestinationPath = "[Game]\\resource3.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void MoveFiles_ModToGame_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new MoveFilesAction()
				{
					TargetPath = "[Mod]\\assets\\mod_asset1.pak",
					DestinationPath = "[Game]\\assets\\mod_asset1.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("MoveFiles - TargetPath: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void MoveFiles_GameToMod_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new MoveFilesAction()
				{
					TargetPath = "[Game]\\assets\\mod_asset1.pak",
					DestinationPath = "[Mod]\\assets\\mod_asset1.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("MoveFiles - DestinationPath: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void MoveFiles_ModToMod_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new MoveFilesAction()
				{
					TargetPath = "[Mod]\\assets\\mod_asset1.pak",
					DestinationPath = "[Mod]\\assets\\mod_asset1.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("MoveFiles - DestinationPath: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void MoveFiles_NullTargetFile_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new MoveFilesAction()
				{
					TargetPath = null,
					DestinationPath = "[Mod]\\assets\\mod_asset1.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("MoveFiles - TargetPath: Provided path must not be NULL or empty", result.First().loadErrors.First());
		}

		[Test]
		public void MoveFiles_NullDestinationPath_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new MoveFilesAction()
				{
					TargetPath = "[Mod]\\assets\\mod_asset1.pak",
					DestinationPath = null,
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("MoveFiles - DestinationPath: Provided path must not be NULL or empty", result.First().loadErrors.First());
		}

		[Test]
		public void MoveFiles_NullFileFilter_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new MoveFilesAction()
				{
					TargetPath = "[Game]\\resource1.pak",
					DestinationPath = "[Game]\\resource3.pak",
					FileFilter = null
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("MoveFiles - FileFilter: Provided path must not be NULL or empty (This is a regex pattern expression, eg. \".*\")", result.First().loadErrors.First());
		}

		[Test]
		public void MoveFiles_EmptyFileFilter_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new MoveFilesAction()
				{
					TargetPath = "[Game]\\resource1.pak",
					DestinationPath = "[Game]\\resource3.pak",
					FileFilter = ""
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("MoveFiles - FileFilter: Provided path must not be NULL or empty (This is a regex pattern expression, eg. \".*\")", result.First().loadErrors.First());
		}


		// Copy File

		[Test]
		public void CopyFile_GameToGame_ValidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new CopyFileAction()
				{
					TargetFile = "[Game]\\resource1.pak",
					DestinationPath = "[Game]\\resource3.pak"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void CopyFile_ModToGame_ValidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new CopyFileAction()
				{
					TargetFile = "[Mod]\\assets\\mod_asset1.pak",
					DestinationPath = "[Game]\\assets\\mod_asset1.pak"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void CopyFile_GameToMod_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new CopyFileAction()
				{
					TargetFile = "[Game]\\assets\\mod_asset1.pak",
					DestinationPath = "[Mod]\\assets\\mod_asset1.pak"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("CopyFile - DestinationPath: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void CopyFile_ModToMod_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new CopyFileAction()
				{
					TargetFile = "[Mod]\\assets\\mod_asset1.pak",
					DestinationPath = "[Mod]\\assets\\mod_asset1.pak"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("CopyFile - DestinationPath: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void CopyFile_NullTargetFile_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new CopyFileAction()
				{
					TargetFile = null,
					DestinationPath = "[Mod]\\assets\\mod_asset1.pak"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("CopyFile - TargetFile: Provided path must not be NULL or empty", result.First().loadErrors.First());
		}

		[Test]
		public void CopyFile_NullDestinationPath_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new CopyFileAction()
				{
					TargetFile = "[Mod]\\assets\\mod_asset1.pak",
					DestinationPath = null
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("CopyFile - DestinationPath: Provided path must not be NULL or empty", result.First().loadErrors.First());
		}


		// Copy Files

		[Test]
		public void CopyFiles_GameToGame_ValidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new CopyFilesAction()
				{
					TargetPath = "[Game]\\resource1.pak",
					DestinationPath = "[Game]\\resource3.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void CopyFiles_ModToGame_ValidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new CopyFilesAction()
				{
					TargetPath = "[Mod]\\assets\\mod_asset1.pak",
					DestinationPath = "[Game]\\assets\\mod_asset1.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void CopyFiles_GameToMod_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new CopyFilesAction()
				{
					TargetPath = "[Game]\\assets\\mod_asset1.pak",
					DestinationPath = "[Mod]\\assets\\mod_asset1.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("CopyFiles - DestinationPath: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void CopyFiles_ModToMod_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new CopyFilesAction()
				{
					TargetPath = "[Mod]\\assets\\mod_asset1.pak",
					DestinationPath = "[Mod]\\assets\\mod_asset1.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("CopyFiles - DestinationPath: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void CopyFiles_NullTargetFile_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new CopyFilesAction()
				{
					TargetPath = null,
					DestinationPath = "[Mod]\\assets\\mod_asset1.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("CopyFiles - TargetPath: Provided path must not be NULL or empty", result.First().loadErrors.First());
		}

		[Test]
		public void CopyFiles_NullDestinationPath_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new CopyFilesAction()
				{
					TargetPath = "[Mod]\\assets\\mod_asset1.pak",
					DestinationPath = null,
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("CopyFiles - DestinationPath: Provided path must not be NULL or empty", result.First().loadErrors.First());
		}

		[Test]
		public void CopyFiles_NullFileFilter_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new CopyFilesAction()
				{
					TargetPath = "[Game]\\resource1.pak",
					DestinationPath = "[Game]\\resource3.pak",
					FileFilter = null
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("CopyFiles - FileFilter: Provided path must not be NULL or empty (This is a regex pattern expression, eg. \".*\")", result.First().loadErrors.First());
		}

		[Test]
		public void CopyFiles_EmptyFileFilter_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new CopyFilesAction()
				{
					TargetPath = "[Game]\\resource1.pak",
					DestinationPath = "[Game]\\resource3.pak",
					FileFilter = ""
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("CopyFiles - FileFilter: Provided path must not be NULL or empty (This is a regex pattern expression, eg. \".*\")", result.First().loadErrors.First());
		}


		// Replace File

		[Test]
		public void ReplaceFile_GameToGame_ValidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new ReplaceFileAction()
				{
					TargetFile = "[Game]\\resource1.pak",
					ReplacementFile = "[Game]\\resource3.pak"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("ReplaceFile - ReplacementFile: Provided path must be in the [MOD] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ReplaceFile_ModToGame_ValidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new ReplaceFileAction()
				{
					TargetFile = "[Game]\\assets\\mod_asset1.pak",
					ReplacementFile = "[Mod]\\assets\\mod_asset1.pak"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void ReplaceFile_GameToMod_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new ReplaceFileAction()
				{
					TargetFile = "[Mod]\\assets\\mod_asset1.pak",
					ReplacementFile = "[Game]\\assets\\mod_asset1.pak"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("ReplaceFile - ReplacementFile: Provided path must be in the [MOD] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ReplaceFile_ModToMod_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new ReplaceFileAction()
				{
					TargetFile = "[Mod]\\assets\\mod_asset1.pak",
					ReplacementFile = "[Mod]\\assets\\mod_asset1.pak"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("ReplaceFile - TargetFile: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ReplaceFile_NullTargetFile_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new ReplaceFileAction()
				{
					TargetFile = null,
					ReplacementFile = "[Mod]\\assets\\mod_asset1.pak"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("ReplaceFile - TargetFile: Provided path must not be NULL or empty", result.First().loadErrors.First());
		}

		[Test]
		public void ReplaceFile_NullDestinationPath_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new ReplaceFileAction()
				{
					TargetFile = "[Mod]\\assets\\mod_asset1.pak",
					ReplacementFile = null
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("ReplaceFile - ReplacementFile: Provided path must not be NULL or empty", result.First().loadErrors.First());
		}


		// Replace Files

		[Test]
		public void ReplaceFiles_GameToGame_ValidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new ReplaceFilesAction()
				{
					ReplacementPath = "[Mod]\\resource1.pak",
					TargetPath = "[Game]\\resource3.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void ReplaceFiles_ModToGame_ValidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new ReplaceFilesAction()
				{
					ReplacementPath = "[Mod]\\assets\\mod_asset1.pak",
					TargetPath = "[Game]\\assets\\mod_asset1.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void ReplaceFiles_GameToMod_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new ReplaceFilesAction()
				{
					ReplacementPath = "[Game]\\assets\\mod_asset1.pak",
					TargetPath = "[Mod]\\assets\\mod_asset1.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("ReplaceFiles - ReplacementPath: Provided path must be in the [MOD] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ReplaceFiles_ModToMod_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new ReplaceFilesAction()
				{
					ReplacementPath = "[Mod]\\assets\\mod_asset1.pak",
					TargetPath = "[Mod]\\assets\\mod_asset1.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("ReplaceFiles - TargetPath: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ReplaceFiles_NullTargetFile_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new ReplaceFilesAction()
				{
					ReplacementPath = null,
					TargetPath = "[Mod]\\assets\\mod_asset1.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("ReplaceFiles - ReplacementPath: Provided path must not be NULL or empty", result.First().loadErrors.First());
		}

		[Test]
		public void ReplaceFiles_NullDestinationPath_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new ReplaceFilesAction()
				{
					ReplacementPath = "[Mod]\\assets\\mod_asset1.pak",
					TargetPath = null,
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("ReplaceFiles - TargetPath: Provided path must not be NULL or empty", result.First().loadErrors.First());
		}

		[Test]
		public void ReplaceFiles_NullFileFilter_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new ReplaceFilesAction()
				{
					ReplacementPath = "[Game]\\resource1.pak",
					TargetPath = "[Game]\\resource3.pak",
					FileFilter = null
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("ReplaceFiles - FileFilter: Provided path must not be NULL or empty (This is a regex pattern expression, eg. \".*\")", result.First().loadErrors.First());
		}

		[Test]
		public void ReplaceFiles_EmptyFileFilter_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new ReplaceFilesAction()
				{
					ReplacementPath = "[Game]\\resource1.pak",
					TargetPath = "[Game]\\resource3.pak",
					FileFilter = ""
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("ReplaceFiles - FileFilter: Provided path must not be NULL or empty (This is a regex pattern expression, eg. \".*\")", result.First().loadErrors.First());
		}


		// File Write

		[Test]
		public void WriteToFile_TextContent_ValidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new WriteToFileAction()
				{
					TargetFile = "[Game]\\resource1.pak",
					Content = new[] {
						new WriteContent()
						{
							StartOffset = 1,
							EndOffset = 2,
							Text = "Test"
						}
					}
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void WriteToFile_TextContentNullEndOffset_ValidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new WriteToFileAction()
				{
					TargetFile = "[Game]\\resource1.pak",
					Content = new[] {
						new WriteContent()
						{
							StartOffset = 1,
							Text = "Test",
							Replace = true
						}
					}
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void WriteToFile_FileContent_ValidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new WriteToFileAction()
				{
					TargetFile = "[Game]\\resource1.pak",
					Content = new[] {
						new WriteContent()
						{
							StartOffset = 1,
							EndOffset = 2,
							DataFilePath = "[Mod]\\assets\\asset1.pak"
						}
					}
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void WriteToFile_FileContent_InvalidConfig_Integration()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new WriteToFileAction()
				{
					TargetFile = "[Game]\\resource1.pak",
					Content = new[] {
						new WriteContent()
						{
							StartOffset = 1,
							EndOffset = 2,
							DataFilePath = "[Integration]\\assets\\asset1.pak"
						}
					}
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("WriteToFile - DataFilePath: Provided path must be in the [MOD] folder", result.First().loadErrors.First());
		}

		[Test]
		public void WriteToFile_FileContent_InvalidConfig_Game()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new WriteToFileAction()
				{
					TargetFile = "[Game]\\resource1.pak",
					Content = new[] {
						new WriteContent()
						{
							StartOffset = 1,
							EndOffset = 2,
							DataFilePath = "[Game]\\assets\\asset1.pak"
						}
					}
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("WriteToFile - DataFilePath: Provided path must be in the [MOD] folder", result.First().loadErrors.First());
		}

		[Test]
		public void WriteToFile_FileAndTextContent_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new WriteToFileAction()
				{
					TargetFile = "[Game]\\resource1.pak",
					Content = new[] {
						new WriteContent()
						{
							StartOffset = 1,
							EndOffset = 2,
							DataFilePath = "[Mod]\\assets\\asset1.pak",
							Text = "Test"
						}
					}
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("WriteToFile - DataFilePath: File write action must provide either 'Text' or 'DataFilePath' properties, not both", result.First().loadErrors.First());
		}

		[Test]
		public void WriteToFile_FileAndTextContentNull_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new WriteToFileAction()
				{
					TargetFile = "[Game]\\resource1.pak",
					Content = new[] {
						new WriteContent()
						{
							StartOffset = 1,
							EndOffset = 2
						}
					}
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("WriteToFile - DataFilePath: File write action must provide either 'Text' or 'DataFilePath' properties", result.First().loadErrors.First());
		}

		[Test]
		public void WriteToFile_NullWriteContent_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new WriteToFileAction()
				{
					TargetFile = "[Game]\\resource1.pak",
					Content = null
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("WriteToFile - Content: No items provided in 'Content' list", result.First().loadErrors.First());
		}

		[Test]
		public void WriteToFile_EmptyWriteContent_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new WriteToFileAction()
				{
					TargetFile = "[Game]\\resource1.pak",
					Content = new WriteContent[] {}
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("WriteToFile - Content: No items provided in 'Content' list", result.First().loadErrors.First());
		}

		[Test]
		public void WriteToFile_EmptyTargetFile_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new WriteToFileAction()
				{
					TargetFile = "",
					Content = new[] {
						new WriteContent()
						{
							StartOffset = 1,
							EndOffset = 2
						}
					}
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("WriteToFile - TargetFile: Provided path must not be NULL or empty", result.First().loadErrors.First());
		}

		[Test]
		public void WriteToFile_NullTargetFile_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new WriteToFileAction()
				{
					TargetFile = null,
					Content = new[] {
						new WriteContent()
						{
							StartOffset = 1,
							EndOffset = 2
						}
					}
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("WriteToFile - TargetFile: Provided path must not be NULL or empty", result.First().loadErrors.First());
		}

		[Test]
		public void WriteToFile_ModTargetFile_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new WriteToFileAction()
				{
					TargetFile = "[Mod]\\resource1.pak",
					Content = new[] {
						new WriteContent()
						{
							StartOffset = 1,
							EndOffset = 2
						}
					}
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("WriteToFile - TargetFile: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		// Quick BMS Extract

		[Test]
		public void QuickBMSExtract_TargetFilesSet_ValidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new QuickBMSExtractAction()
				{
					TargetFiles = new[] {"[Game]\\resource1.pak" }
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void QuickBMSExtract_TargetFilesSet_Empty_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new QuickBMSExtractAction()
				{
					TargetFiles = new string[0]
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("QuickBMSExtract - TargetFiles: No items provided in 'TargetFiles' list", result.First().loadErrors.First());
		}

		[Test]
		public void QuickBMSExtract_TargetFilesNull_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new QuickBMSExtractAction()
				{
					TargetFiles = null
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("QuickBMSExtract - TargetFiles: No items provided in 'TargetFiles' list", result.First().loadErrors.First());
		}

		[Test]
		public void QuickBMSExtract_TargetFilesSet_ModPath_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new QuickBMSExtractAction()
				{
					TargetFiles = new[] {"[Mod]\\resource1.pak" }
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("QuickBMSExtract - TargetFiles: All provided paths for QuickBMS extraction must be in the [GAME] folder", result.First().loadErrors.First());
		}

		// Unluac Decompile

		[Test]
		public void UnluacDecompile_TargetFilesSet_ValidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new UnluacDecompileAction()
				{
					TargetFiles = new[] {"[Game]\\resource1.pak" }
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void UnluacDecompile_TargetFilesSet_Empty_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new UnluacDecompileAction()
				{
					TargetFiles = new string[0]
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("UnluacDecompile - TargetFiles: No items provided in 'TargetFiles' list", result.First().loadErrors.First());
		}

		[Test]
		public void UnluacDecompile_TargetFilesNull_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new UnluacDecompileAction()
				{
					TargetFiles = null
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("UnluacDecompile - TargetFiles: No items provided in 'TargetFiles' list", result.First().loadErrors.First());
		}

		[Test]
		public void UnluacDecompile_TargetFilesSet_ModPath_InvalidConfig()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new UnluacDecompileAction()
				{
					TargetFiles = new[] {"[Mod]\\resource1.pak" }
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("UnluacDecompile - TargetFiles: All provided paths for Unluac decompile must be in the [GAME] folder", result.First().loadErrors.First());
		}

		// Zip Files

		[Test]
		public void ZipFiles_NullFiles_ConfigInvalid()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new ZipFilesAction()
				{
					FilesToInclude = null
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("ZipFiles - FilesToInclude: No items provided in 'FilesToInclude' list", result.First().loadErrors.First());
		}

		[Test]
		public void ZipFiles_EmptyFiles_ConfigInvalid()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new ZipFilesAction()
				{
					FilesToInclude = new string[0]
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("ZipFiles - FilesToInclude: No items provided in 'FilesToInclude' list", result.First().loadErrors.First());
		}

		[Test]
		public void ZipFiles_InvalidFiles_NonGame_ConfigInvalid()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new ZipFilesAction()
				{
					FilesToInclude = new[]
					{
						"[MOD]\\test.txt"
					}
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("ZipFiles - FilesToInclude: All provided paths for inclusion in the zip archive must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ZipFiles_InvalidFiles_NonGameAlt_ConfigInvalid()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new ZipFilesAction()
				{
					FilesToInclude = new[]
					{
						"C:\\test.txt"
					}
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("ZipFiles - FilesToInclude: All provided paths for inclusion in the zip archive must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ZipFiles_DestinationPathNull_ConfigInvalid()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new ZipFilesAction()
				{
					FilesToInclude = new[]
					{
						"[GAME]\\test.txt"
					},
					DestinationPath = null
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("ZipFiles - DestinationPath: Destination path must be supplied", result.First().loadErrors.First());
		}

		[Test]
		public void ZipFiles_DestinationPathInvalid_ConfigInvalid()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new ZipFilesAction()
				{
					FilesToInclude = new[]
					{
						"[GAME]\\test.txt"
					},
					DestinationPath = "C:\\test.zip"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("ZipFiles - DestinationPath: Destination path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ZipFiles_DestinationPathInvalid_Alt_ConfigInvalid()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new ZipFilesAction()
				{
					FilesToInclude = new[]
					{
						"[GAME]\\test.txt"
					},
					DestinationPath = "[MOD]\\test.zip"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("ZipFiles - DestinationPath: Destination path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ZipFiles_ConfigValid()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new ZipFilesAction()
				{
					FilesToInclude = new[]
					{
						"[GAME]\\test.txt",
						"[GAME]\\test2.txt",
					},
					DestinationPath = "[GAME]\\test.zip"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		// Zip Directory

		[Test]
		public void ZipDirectory_NullDirectoryPath_ConfigInvalid()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new ZipDirectoryAction()
				{
					DirectoryPath = null
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("ZipDirectory - DirectoryPath: Directory path must be supplied", result.First().loadErrors.First());
		}

		[Test]
		public void ZipDirectory_DirectoryPathInvalid_ConfigInvalid()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new ZipDirectoryAction()
				{
					DirectoryPath = "C:\\test"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("ZipDirectory - DirectoryPath: Directory path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ZipDirectory_DirectoryPathInvalid_Alt_ConfigInvalid()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new ZipDirectoryAction()
				{
					DirectoryPath = "[MOD]\\test"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("ZipDirectory - DirectoryPath: Directory path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ZipDirectory_DestinationPathNull_ConfigInvalid()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new ZipDirectoryAction()
				{
					DirectoryPath = "[GAME]\\test",
					DestinationPath = null
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("ZipDirectory - DestinationPath: Destination path must be supplied", result.First().loadErrors.First());
		}

		[Test]
		public void ZipDirectory_DestinationPathInvalid_ConfigInvalid()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new ZipDirectoryAction()
				{
					DirectoryPath = "[GAME]\\test",
					DestinationPath = "C:\\test.zip"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("ZipDirectory - DestinationPath: Destination path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ZipDirectory_DestinationPathInvalid_Alt_ConfigInvalid()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new ZipDirectoryAction()
				{
					DirectoryPath = "[GAME]\\test",
					DestinationPath = "[MOD]\\test.zip"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("ZipDirectory - DestinationPath: Destination path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ZipDirectory_ConfigValid()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new ZipDirectoryAction()
				{
					DirectoryPath = "[GAME]\\test",
					DestinationPath = "[GAME]\\test.zip"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		// Unzip File

		[Test]
		public void UnzipFile_NullDirectoryPath_ConfigInvalid()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new UnzipFileAction()
				{
					TargetFile = null
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("UnzipFile - TargetFile: Target file must be supplied", result.First().loadErrors.First());
		}

		[Test]
		public void UnzipFile_DirectoryPathInvalid_ConfigInvalid()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new UnzipFileAction()
				{
					TargetFile = "C:\\test.zip"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("UnzipFile - TargetFile: Target file must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void UnzipFile_DirectoryPathInvalid_Alt_ConfigInvalid()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new UnzipFileAction()
				{
					TargetFile = "[MOD]\\test.zip"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("UnzipFile - TargetFile: Target file must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void UnzipFile_DestinationPathNull_ConfigInvalid()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new UnzipFileAction()
				{
					TargetFile = "[GAME]\\test.zip",
					DestinationPath = null
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("UnzipFile - DestinationPath: Destination path must be supplied", result.First().loadErrors.First());
		}

		[Test]
		public void UnzipFile_DestinationPathInvalid_ConfigInvalid()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new UnzipFileAction()
				{
					TargetFile = "[GAME]\\test.zip",
					DestinationPath = "C:\\test"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("UnzipFile - DestinationPath: Destination path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void UnzipFile_DestinationPathInvalid_Alt_ConfigInvalid()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new UnzipFileAction()
				{
					TargetFile = "[GAME]\\test.zip",
					DestinationPath = "[MOD]\\test"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("UnzipFile - DestinationPath: Destination path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void UnzipFile_ConfigValid()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new UnzipFileAction()
				{
					TargetFile = "[GAME]\\test.zip",
					DestinationPath = "[GAME]\\test"
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		// Unzip Files

		[Test]
		public void UnzipFiles_NullDirectoryPath_ConfigInvalid()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new UnzipFilesAction()
				{
					TargetFiles = null
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("UnzipFiles - TargetFiles: No items provided in 'TargetFiles' list", result.First().loadErrors.First());
		}

		[Test]
		public void UnzipFiles_EmptyDirectoryPath_ConfigInvalid()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new UnzipFilesAction()
				{
					TargetFiles = new string[0]
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("UnzipFiles - TargetFiles: No items provided in 'TargetFiles' list", result.First().loadErrors.First());
		}

		[Test]
		public void UnzipFiles_DirectoryPathInvalid_ConfigInvalid()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new UnzipFilesAction()
				{
					TargetFiles = new string[] { "C:\\test.zip" }
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("UnzipFiles - TargetFiles: All provided paths for unzip must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void UnzipFiles_DirectoryPathInvalid_Alt_ConfigInvalid()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new UnzipFilesAction()
				{
					TargetFiles = new string[] { "[MOD]\\test.zip" }
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			ClassicAssert.AreEqual("UnzipFiles - TargetFiles: All provided paths for unzip must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void UnzipFiles_ConfigValid()
		{
			var modConfig = CreateTestModConfig("test-mod");
			modConfig.InstallActions = new[]
			{
				new UnzipFilesAction()
				{
					TargetFiles = new string[] { "[GAME]\\test.zip" }
				}
			};

			var modPath = CreateTempModFiles(modConfig);
			var modLoader = new ModLoader(new[] { this.integration });
			var result = modLoader.Load(modPath);

			ClassicAssert.AreEqual(1, result.Count());
			ClassicAssert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}
	}
}
