using Continuum.Core;
using Continuum.Core.Enums;
using Continuum.Core.InstallActions;
using Continuum.Core.Models;
using Continuum.Core.Test;
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
	class IntegrationLoaderTests : BaseModTests
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
			var integration = CreateTestIntegration("test-game");

			integration.SetupActions = new[]
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

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void DeleteFiles_InvalidConfig_NullTargetFiles()
		{
			var integration = CreateTestIntegration("test-game");

			integration.SetupActions = new[]
			{
				new DeleteFilesAction()
				{
					TargetFiles = null
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("DeleteFiles - TargetFiles: No items provided in 'TargetFiles' list", result.First().loadErrors.First());
		}

		[Test]
		public void DeleteFiles_InvalidConfig_InvalidPath()
		{
			var integration = CreateTestIntegration("test-game");

			integration.SetupActions = new[]
			{
				new DeleteFilesAction()
				{
					TargetFiles = new[]
					{
						"[Mod]\\resource1.pak",
					}
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("DeleteFiles - TargetFiles: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void DeleteFiles_InvalidConfig_InvalidPath_Integration()
		{
			var integration = CreateTestIntegration("test-game");

			integration.SetupActions = new[]
			{
				new DeleteFilesAction()
				{
					TargetFiles = new[]
					{
						"[Integration]\\resource1.pak",
					}
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("DeleteFiles - TargetFiles: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void DeleteFiles_InvalidConfig_NullPath()
		{
			var integration = CreateTestIntegration("test-game");

			integration.SetupActions = new[]
			{
				new DeleteFilesAction()
				{
					TargetFiles = new string[]
					{
						null
					}
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("DeleteFiles - TargetFiles: One or more target files were NULL or Empty", result.First().loadErrors.First());
		}

		[Test]
		public void DeleteFiles_InvalidConfig_EmptyPath()
		{
			var integration = CreateTestIntegration("test-game");

			integration.SetupActions = new[]
			{
				new DeleteFilesAction()
				{
					TargetFiles = new[]
					{
						string.Empty
					}
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("DeleteFiles - TargetFiles: One or more target files were NULL or Empty", result.First().loadErrors.First());
		}

		[Test]
		public void DeleteFiles_InvalidConfig_EmptyContent()
		{
			var integration = CreateTestIntegration("test-game");

			integration.SetupActions = new[]
			{
				new DeleteFilesAction()
				{
					TargetFiles = new string[0]
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("DeleteFiles - TargetFiles: No items provided in 'TargetFiles' list", result.First().loadErrors.First());
		}


		// Move File

		[Test]
		public void MoveFile_GameToGame_ValidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Game]\\resource1.pak",
					DestinationPath = "[Game]\\resource3.pak"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void MoveFile_ModToGame_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Mod]\\assets\\mod_asset1.pak",
					DestinationPath = "[Game]\\assets\\mod_asset1.pak"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("MoveFile - TargetFile: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void MoveFile_GameToMod_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Game]\\assets\\mod_asset1.pak",
					DestinationPath = "[Mod]\\assets\\mod_asset1.pak"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("MoveFile - DestinationPath: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void MoveFile_ModToMod_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Mod]\\assets\\mod_asset1.pak",
					DestinationPath = "[Mod]\\assets\\mod_asset1.pak"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("MoveFile - DestinationPath: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void MoveFile_NullTargetFile_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = null,
					DestinationPath = "[Mod]\\assets\\mod_asset1.pak"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("MoveFile - TargetFile: Provided path must not be NULL or empty", result.First().loadErrors.First());
		}

		[Test]
		public void MoveFile_NullDestinationPath_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new MoveFileAction()
				{
					TargetFile = "[Mod]\\assets\\mod_asset1.pak",
					DestinationPath = null
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("MoveFile - DestinationPath: Provided path must not be NULL or empty", result.First().loadErrors.First());
		}


		// Move Files

		[Test]
		public void MoveFiles_GameToGame_ValidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new MoveFilesAction()
				{
					TargetPath = "[Game]\\resource1.pak",
					DestinationPath = "[Game]\\resource3.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void MoveFiles_ModToGame_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new MoveFilesAction()
				{
					TargetPath = "[Mod]\\assets\\mod_asset1.pak",
					DestinationPath = "[Game]\\assets\\mod_asset1.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("MoveFiles - TargetPath: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void MoveFiles_GameToMod_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new MoveFilesAction()
				{
					TargetPath = "[Game]\\assets\\mod_asset1.pak",
					DestinationPath = "[Mod]\\assets\\mod_asset1.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("MoveFiles - DestinationPath: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void MoveFiles_ModToMod_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new MoveFilesAction()
				{
					TargetPath = "[Mod]\\assets\\mod_asset1.pak",
					DestinationPath = "[Mod]\\assets\\mod_asset1.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("MoveFiles - DestinationPath: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void MoveFiles_NullTargetFile_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new MoveFilesAction()
				{
					TargetPath = null,
					DestinationPath = "[Mod]\\assets\\mod_asset1.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("MoveFiles - TargetPath: Provided path must not be NULL or empty", result.First().loadErrors.First());
		}

		[Test]
		public void MoveFiles_NullDestinationPath_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new MoveFilesAction()
				{
					TargetPath = "[Mod]\\assets\\mod_asset1.pak",
					DestinationPath = null,
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("MoveFiles - DestinationPath: Provided path must not be NULL or empty", result.First().loadErrors.First());
		}

		[Test]
		public void MoveFiles_NullFileFilter_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new MoveFilesAction()
				{
					TargetPath = "[Game]\\resource1.pak",
					DestinationPath = "[Game]\\resource3.pak",
					FileFilter = null
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("MoveFiles - FileFilter: Provided path must not be NULL or empty (This is a regex pattern expression, eg. \".*\")", result.First().loadErrors.First());
		}

		[Test]
		public void MoveFiles_EmptyFileFilter_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new MoveFilesAction()
				{
					TargetPath = "[Game]\\resource1.pak",
					DestinationPath = "[Game]\\resource3.pak",
					FileFilter = ""
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("MoveFiles - FileFilter: Provided path must not be NULL or empty (This is a regex pattern expression, eg. \".*\")", result.First().loadErrors.First());
		}


		// Copy File

		[Test]
		public void CopyFile_GameToGame_ValidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new CopyFileAction()
				{
					TargetFile = "[Game]\\resource1.pak",
					DestinationPath = "[Game]\\resource3.pak"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void CopyFile_ModToGame_InvalidConfig_Mod()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new CopyFileAction()
				{
					TargetFile = "[Mod]\\assets\\mod_asset1.pak",
					DestinationPath = "[Game]\\assets\\mod_asset1.pak"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("CopyFile - TargetFile: Provided path must be in the [GAME] or [INTEGRATION] folder", result.First().loadErrors.First());
		}

		[Test]
		public void CopyFile_IntegrationToGame_ValidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new CopyFileAction()
				{
					TargetFile = "[Integration]\\assets\\mod_asset1.pak",
					DestinationPath = "[Game]\\assets\\mod_asset1.pak"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void CopyFile_GameToMod_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new CopyFileAction()
				{
					TargetFile = "[Game]\\assets\\mod_asset1.pak",
					DestinationPath = "[Mod]\\assets\\mod_asset1.pak"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("CopyFile - DestinationPath: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void CopyFile_ModToMod_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new CopyFileAction()
				{
					TargetFile = "[Mod]\\assets\\mod_asset1.pak",
					DestinationPath = "[Mod]\\assets\\mod_asset1.pak"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("CopyFile - DestinationPath: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void CopyFile_NullTargetFile_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new CopyFileAction()
				{
					TargetFile = null,
					DestinationPath = "[Mod]\\assets\\mod_asset1.pak"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("CopyFile - TargetFile: Provided path must not be NULL or empty", result.First().loadErrors.First());
		}

		[Test]
		public void CopyFile_NullDestinationPath_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new CopyFileAction()
				{
					TargetFile = "[Mod]\\assets\\mod_asset1.pak",
					DestinationPath = null
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("CopyFile - DestinationPath: Provided path must not be NULL or empty", result.First().loadErrors.First());
		}


		// Copy Files

		[Test]
		public void CopyFiles_GameToGame_ValidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new CopyFilesAction()
				{
					TargetPath = "[Game]\\resource1.pak",
					DestinationPath = "[Game]\\resource3.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void CopyFiles_ModToGame_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new CopyFilesAction()
				{
					TargetPath = "[Mod]\\assets\\mod_asset1.pak",
					DestinationPath = "[Game]\\assets\\mod_asset1.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("CopyFiles - TargetPath: Provided path must be in the [GAME] or [INTEGRATION] folder", result.First().loadErrors.First());
		}

		[Test]
		public void CopyFiles_IntegrationToGame_ValidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new CopyFilesAction()
				{
					TargetPath = "[Integration]\\assets\\mod_asset1.pak",
					DestinationPath = "[Game]\\assets\\mod_asset1.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void CopyFiles_GameToMod_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new CopyFilesAction()
				{
					TargetPath = "[Game]\\assets\\mod_asset1.pak",
					DestinationPath = "[Mod]\\assets\\mod_asset1.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("CopyFiles - DestinationPath: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void CopyFiles_ModToMod_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new CopyFilesAction()
				{
					TargetPath = "[Mod]\\assets\\mod_asset1.pak",
					DestinationPath = "[Mod]\\assets\\mod_asset1.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("CopyFiles - DestinationPath: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void CopyFiles_NullTargetFile_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new CopyFilesAction()
				{
					TargetPath = null,
					DestinationPath = "[Mod]\\assets\\mod_asset1.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("CopyFiles - TargetPath: Provided path must not be NULL or empty", result.First().loadErrors.First());
		}

		[Test]
		public void CopyFiles_NullDestinationPath_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new CopyFilesAction()
				{
					TargetPath = "[Mod]\\assets\\mod_asset1.pak",
					DestinationPath = null,
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("CopyFiles - DestinationPath: Provided path must not be NULL or empty", result.First().loadErrors.First());
		}

		[Test]
		public void CopyFiles_NullFileFilter_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new CopyFilesAction()
				{
					TargetPath = "[Game]\\resource1.pak",
					DestinationPath = "[Game]\\resource3.pak",
					FileFilter = null
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("CopyFiles - FileFilter: Provided path must not be NULL or empty (This is a regex pattern expression, eg. \".*\")", result.First().loadErrors.First());
		}

		[Test]
		public void CopyFiles_EmptyFileFilter_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new CopyFilesAction()
				{
					TargetPath = "[Game]\\resource1.pak",
					DestinationPath = "[Game]\\resource3.pak",
					FileFilter = ""
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("CopyFiles - FileFilter: Provided path must not be NULL or empty (This is a regex pattern expression, eg. \".*\")", result.First().loadErrors.First());
		}


		// Replace File

		[Test]
		public void ReplaceFile_GameToGame_ValidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new ReplaceFileAction()
				{
					TargetFile = "[Game]\\resource1.pak",
					ReplacementFile = "[Game]\\resource3.pak"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ReplaceFile - ReplacementFile: Provided path must be in the [INTEGRATION] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ReplaceFile_ModToGame_InvalidConfig_Mod()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new ReplaceFileAction()
				{
					TargetFile = "[Game]\\assets\\mod_asset1.pak",
					ReplacementFile = "[Mod]\\assets\\mod_asset1.pak"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ReplaceFile - ReplacementFile: Provided path must be in the [INTEGRATION] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ReplaceFile_ModToGame_ValidConfig_Integration()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new ReplaceFileAction()
				{
					TargetFile = "[Game]\\assets\\mod_asset1.pak",
					ReplacementFile = "[Integration]\\assets\\mod_asset1.pak"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void ReplaceFile_GameToMod_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new ReplaceFileAction()
				{
					TargetFile = "[Mod]\\assets\\mod_asset1.pak",
					ReplacementFile = "[Game]\\assets\\mod_asset1.pak"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ReplaceFile - ReplacementFile: Provided path must be in the [INTEGRATION] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ReplaceFile_ModToMod_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new ReplaceFileAction()
				{
					TargetFile = "[Mod]\\assets\\mod_asset1.pak",
					ReplacementFile = "[Mod]\\assets\\mod_asset1.pak"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ReplaceFile - ReplacementFile: Provided path must be in the [INTEGRATION] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ReplaceFile_ModToMod_InvalidConfig_TargetFile()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new ReplaceFileAction()
				{
					TargetFile = "[Mod]\\assets\\mod_asset1.pak",
					ReplacementFile = "[Integration]\\assets\\mod_asset1.pak"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ReplaceFile - TargetFile: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ReplaceFile_NullTargetFile_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new ReplaceFileAction()
				{
					TargetFile = null,
					ReplacementFile = "[Mod]\\assets\\mod_asset1.pak"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ReplaceFile - TargetFile: Provided path must not be NULL or empty", result.First().loadErrors.First());
		}

		[Test]
		public void ReplaceFile_NullDestinationPath_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new ReplaceFileAction()
				{
					TargetFile = "[Mod]\\assets\\mod_asset1.pak",
					ReplacementFile = null
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ReplaceFile - ReplacementFile: Provided path must not be NULL or empty", result.First().loadErrors.First());
		}


		// Replace Files

		[Test]
		public void ReplaceFiles_GameToGame_InvalidConfig_Mod()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new ReplaceFilesAction()
				{
					ReplacementPath = "[Mod]\\resource1.pak",
					TargetPath = "[Game]\\resource3.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ReplaceFiles - ReplacementPath: Provided path must be in the [INTEGRATION] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ReplaceFiles_ModToGame_InvalidConfig_Mod()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new ReplaceFilesAction()
				{
					ReplacementPath = "[Mod]\\assets\\mod_asset1.pak",
					TargetPath = "[Game]\\assets\\mod_asset1.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ReplaceFiles - ReplacementPath: Provided path must be in the [INTEGRATION] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ReplaceFiles_GameToMod_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new ReplaceFilesAction()
				{
					ReplacementPath = "[Game]\\assets\\mod_asset1.pak",
					TargetPath = "[Mod]\\assets\\mod_asset1.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ReplaceFiles - ReplacementPath: Provided path must be in the [INTEGRATION] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ReplaceFiles_ModToMod_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new ReplaceFilesAction()
				{
					ReplacementPath = "[Mod]\\assets\\mod_asset1.pak",
					TargetPath = "[Mod]\\assets\\mod_asset1.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ReplaceFiles - ReplacementPath: Provided path must be in the [INTEGRATION] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ReplaceFiles_ModToMod_InvalidConfig_TargetPath()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new ReplaceFilesAction()
				{
					ReplacementPath = "[Integration]\\assets\\mod_asset1.pak",
					TargetPath = "[Mod]\\assets\\mod_asset1.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ReplaceFiles - TargetPath: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ReplaceFiles_NullTargetFile_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new ReplaceFilesAction()
				{
					ReplacementPath = null,
					TargetPath = "[Mod]\\assets\\mod_asset1.pak",
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ReplaceFiles - ReplacementPath: Provided path must not be NULL or empty", result.First().loadErrors.First());
		}

		[Test]
		public void ReplaceFiles_NullDestinationPath_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new ReplaceFilesAction()
				{
					ReplacementPath = "[Mod]\\assets\\mod_asset1.pak",
					TargetPath = null,
					FileFilter = ".*"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ReplaceFiles - TargetPath: Provided path must not be NULL or empty", result.First().loadErrors.First());
		}

		[Test]
		public void ReplaceFiles_NullFileFilter_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new ReplaceFilesAction()
				{
					ReplacementPath = "[Game]\\resource1.pak",
					TargetPath = "[Game]\\resource3.pak",
					FileFilter = null
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ReplaceFiles - FileFilter: Provided path must not be NULL or empty (This is a regex pattern expression, eg. \".*\")", result.First().loadErrors.First());
		}

		[Test]
		public void ReplaceFiles_EmptyFileFilter_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new ReplaceFilesAction()
				{
					ReplacementPath = "[Game]\\resource1.pak",
					TargetPath = "[Game]\\resource3.pak",
					FileFilter = ""
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ReplaceFiles - FileFilter: Provided path must not be NULL or empty (This is a regex pattern expression, eg. \".*\")", result.First().loadErrors.First());
		}


		// File Write

		[Test]
		public void WriteToFile_TextContent_ValidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
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

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void WriteToFile_TextContentNullEndOffset_ValidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
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

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void WriteToFile_FileContent_ValidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
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

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void WriteToFile_FileContent_InvalidConfig_Mod()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
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

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("WriteToFile - DataFilePath: Provided path must be in the [INTEGRATION] folder", result.First().loadErrors.First());
		}

		[Test]
		public void WriteToFile_FileContent_InvalidConfig_Game()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
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

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("WriteToFile - DataFilePath: Provided path must be in the [INTEGRATION] folder", result.First().loadErrors.First());
		}

		[Test]
		public void WriteToFile_FileAndTextContent_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
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

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("WriteToFile - DataFilePath: File write action must provide either 'Text' or 'DataFilePath' properties, not both", result.First().loadErrors.First());
		}

		[Test]
		public void WriteToFile_FileAndTextContentNull_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
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

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("WriteToFile - DataFilePath: File write action must provide either 'Text' or 'DataFilePath' properties", result.First().loadErrors.First());
		}

		[Test]
		public void WriteToFile_NullWriteContent_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new WriteToFileAction()
				{
					TargetFile = "[Game]\\resource1.pak",
					Content = null
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("WriteToFile - Content: No items provided in 'Content' list", result.First().loadErrors.First());
		}

		[Test]
		public void WriteToFile_EmptyWriteContent_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new WriteToFileAction()
				{
					TargetFile = "[Game]\\resource1.pak",
					Content = new WriteContent[] {}
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("WriteToFile - Content: No items provided in 'Content' list", result.First().loadErrors.First());
		}

		[Test]
		public void WriteToFile_EmptyTargetFile_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
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

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("WriteToFile - TargetFile: Provided path must not be NULL or empty", result.First().loadErrors.First());
		}

		[Test]
		public void WriteToFile_NullTargetFile_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
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

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("WriteToFile - TargetFile: Provided path must not be NULL or empty", result.First().loadErrors.First());
		}

		[Test]
		public void WriteToFile_ModTargetFile_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
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

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("WriteToFile - TargetFile: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		// Quick BMS Extract

		[Test]
		public void QuickBMSExtract_TargetFilesSet_ValidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new QuickBMSExtractAction()
				{
					TargetFiles = new[] {"[Game]\\resource1.pak" }
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void QuickBMSExtract_TargetFilesSet_Empty_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new QuickBMSExtractAction()
				{
					TargetFiles = new string[0]
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("QuickBMSExtract - TargetFiles: No items provided in 'TargetFiles' list", result.First().loadErrors.First());
		}

		[Test]
		public void QuickBMSExtract_TargetFilesNull_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new QuickBMSExtractAction()
				{
					TargetFiles = null
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("QuickBMSExtract - TargetFiles: No items provided in 'TargetFiles' list", result.First().loadErrors.First());
		}

		[Test]
		public void QuickBMSExtract_TargetFilesSet_ModPath_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new QuickBMSExtractAction()
				{
					TargetFiles = new[] {"[Mod]\\resource1.pak" }
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("QuickBMSExtract - TargetFiles: All provided paths for QuickBMS extraction must be in the [GAME] folder", result.First().loadErrors.First());
		}

		// Unluac Decompile

		[Test]
		public void UnluacDecompile_TargetFilesSet_ValidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new UnluacDecompileAction()
				{
					TargetFiles = new[] {"[Game]\\resource1.pak" }
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void UnluacDecompile_TargetFilesSet_Empty_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new UnluacDecompileAction()
				{
					TargetFiles = new string[0]
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("UnluacDecompile - TargetFiles: No items provided in 'TargetFiles' list", result.First().loadErrors.First());
		}

		[Test]
		public void UnluacDecompile_TargetFilesNull_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new UnluacDecompileAction()
				{
					TargetFiles = null
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("UnluacDecompile - TargetFiles: No items provided in 'TargetFiles' list", result.First().loadErrors.First());
		}

		[Test]
		public void UnluacDecompile_TargetFilesSet_ModPath_InvalidConfig()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new UnluacDecompileAction()
				{
					TargetFiles = new[] {"[Mod]\\resource1.pak" }
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("UnluacDecompile - TargetFiles: All provided paths for Unluac decompile must be in the [GAME] folder", result.First().loadErrors.First());
		}

		// Zip Files

		[Test]
		public void ZipFiles_NullFiles_ConfigInvalid()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new ZipFilesAction()
				{
					FilesToInclude = null
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ZipFiles - FilesToInclude: No items provided in 'FilesToInclude' list", result.First().loadErrors.First());
		}

		[Test]
		public void ZipFiles_EmptyFiles_ConfigInvalid()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new ZipFilesAction()
				{
					FilesToInclude = new string[0]
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ZipFiles - FilesToInclude: No items provided in 'FilesToInclude' list", result.First().loadErrors.First());
		}

		[Test]
		public void ZipFiles_InvalidFiles_NonGame_ConfigInvalid()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new ZipFilesAction()
				{
					FilesToInclude = new[]
					{
						"[MOD]\\test.txt"
					}
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ZipFiles - FilesToInclude: All provided paths for inclusion in the zip archive must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ZipFiles_InvalidFiles_NonGameAlt_ConfigInvalid()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new ZipFilesAction()
				{
					FilesToInclude = new[]
					{
						"C:\\test.txt"
					}
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ZipFiles - FilesToInclude: All provided paths for inclusion in the zip archive must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ZipFiles_DestinationPathNull_ConfigInvalid()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
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

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ZipFiles - DestinationPath: Destination path must be supplied", result.First().loadErrors.First());
		}

		[Test]
		public void ZipFiles_DestinationPathInvalid_ConfigInvalid()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
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

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ZipFiles - DestinationPath: Destination path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ZipFiles_DestinationPathInvalid_Alt_ConfigInvalid()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
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

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ZipFiles - DestinationPath: Destination path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ZipFiles_ConfigValid()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
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

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		// Zip Directory

		[Test]
		public void ZipDirectory_NullDirectoryPath_ConfigInvalid()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new ZipDirectoryAction()
				{
					DirectoryPath = null
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ZipDirectory - DirectoryPath: Directory path must be supplied", result.First().loadErrors.First());
		}

		[Test]
		public void ZipDirectory_DirectoryPathInvalid_ConfigInvalid()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new ZipDirectoryAction()
				{
					DirectoryPath = "C:\\test"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ZipDirectory - DirectoryPath: Directory path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ZipDirectory_DirectoryPathInvalid_Alt_ConfigInvalid()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new ZipDirectoryAction()
				{
					DirectoryPath = "[MOD]\\test"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ZipDirectory - DirectoryPath: Directory path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ZipDirectory_DestinationPathNull_ConfigInvalid()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new ZipDirectoryAction()
				{
					DirectoryPath = "[GAME]\\test",
					DestinationPath = null
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ZipDirectory - DestinationPath: Destination path must be supplied", result.First().loadErrors.First());
		}

		[Test]
		public void ZipDirectory_DestinationPathInvalid_ConfigInvalid()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new ZipDirectoryAction()
				{
					DirectoryPath = "[GAME]\\test",
					DestinationPath = "C:\\test.zip"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ZipDirectory - DestinationPath: Destination path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ZipDirectory_DestinationPathInvalid_Alt_ConfigInvalid()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new ZipDirectoryAction()
				{
					DirectoryPath = "[GAME]\\test",
					DestinationPath = "[MOD]\\test.zip"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ZipDirectory - DestinationPath: Destination path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void ZipDirectory_ConfigValid()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new ZipDirectoryAction()
				{
					DirectoryPath = "[GAME]\\test",
					DestinationPath = "[GAME]\\test.zip"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		// Unzip File

		[Test]
		public void UnzipFile_NullDirectoryPath_ConfigInvalid()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new UnzipFileAction()
				{
					TargetFile = null
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("UnzipFile - TargetFile: Target file must be supplied", result.First().loadErrors.First());
		}

		[Test]
		public void UnzipFile_DirectoryPathInvalid_ConfigInvalid()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new UnzipFileAction()
				{
					TargetFile = "C:\\test.zip"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("UnzipFile - TargetFile: Target file must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void UnzipFile_DirectoryPathInvalid_Alt_ConfigInvalid()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new UnzipFileAction()
				{
					TargetFile = "[MOD]\\test.zip"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("UnzipFile - TargetFile: Target file must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void UnzipFile_DestinationPathNull_ConfigInvalid()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new UnzipFileAction()
				{
					TargetFile = "[GAME]\\test.zip",
					DestinationPath = null
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("UnzipFile - DestinationPath: Destination path must be supplied", result.First().loadErrors.First());
		}

		[Test]
		public void UnzipFile_DestinationPathInvalid_ConfigInvalid()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new UnzipFileAction()
				{
					TargetFile = "[GAME]\\test.zip",
					DestinationPath = "C:\\test"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("UnzipFile - DestinationPath: Destination path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void UnzipFile_DestinationPathInvalid_Alt_ConfigInvalid()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new UnzipFileAction()
				{
					TargetFile = "[GAME]\\test.zip",
					DestinationPath = "[MOD]\\test"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("UnzipFile - DestinationPath: Destination path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void UnzipFile_ConfigValid()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new UnzipFileAction()
				{
					TargetFile = "[GAME]\\test.zip",
					DestinationPath = "[GAME]\\test"
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		// Unzip Files

		[Test]
		public void UnzipFiles_NullDirectoryPath_ConfigInvalid()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new UnzipFilesAction()
				{
					TargetFiles = null
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("UnzipFiles - TargetFiles: No items provided in 'TargetFiles' list", result.First().loadErrors.First());
		}

		[Test]
		public void UnzipFiles_EmptyDirectoryPath_ConfigInvalid()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new UnzipFilesAction()
				{
					TargetFiles = new string[0]
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("UnzipFiles - TargetFiles: No items provided in 'TargetFiles' list", result.First().loadErrors.First());
		}

		[Test]
		public void UnzipFiles_DirectoryPathInvalid_ConfigInvalid()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new UnzipFilesAction()
				{
					TargetFiles = new string[] { "C:\\test.zip" }
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("UnzipFiles - TargetFiles: All provided paths for unzip must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void UnzipFiles_DirectoryPathInvalid_Alt_ConfigInvalid()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new UnzipFilesAction()
				{
					TargetFiles = new string[] { "[MOD]\\test.zip" }
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("UnzipFiles - TargetFiles: All provided paths for unzip must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void UnzipFiles_ConfigValid()
		{
			var integration = CreateTestIntegration("test-game");
			integration.SetupActions = new[]
			{
				new UnzipFilesAction()
				{
					TargetFiles = new string[] { "[GAME]\\test.zip" }
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}


		#region Automapping

		[Test]
		public void Automapping_QuickBMS_CopyFile_GameToGame()
		{
			var integration = CreateTestIntegration("test-game");
			integration.QuickBMSAutoMappings = new[]
			{
				new AutoMapping()
				{
					TargetPath = "[Game]\\Test",
					FileFilter = ".*",
					Actions = new[]
					{
						new CopyFileAction()
						{
							TargetFile = "[GAME]\\test.zip",
							DestinationPath = "[GAME]\\test2.zip",
						}
					}
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void Automapping_QuickBMS_CopyFile_ModToGame()
		{
			var integration = CreateTestIntegration("test-game");
			integration.QuickBMSAutoMappings = new[]
			{
				new AutoMapping()
				{
					TargetPath = "[Game]\\Test",
					FileFilter = ".*",
					Actions = new[]
					{
						new CopyFileAction()
						{
							TargetFile = "[Mod]\\test.zip",
							DestinationPath = "[GAME]\\test2.zip",
						}
					}
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("CopyFile - TargetFile: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void Automapping_QuickBMS_CopyFile_IntegrationToGame()
		{
			var integration = CreateTestIntegration("test-game");
			integration.QuickBMSAutoMappings = new[]
			{
				new AutoMapping()
				{
					TargetPath = "[Game]\\Test",
					FileFilter = ".*",
					Actions = new[]
					{
						new CopyFileAction()
						{
							TargetFile = "[Integration]\\test.zip",
							DestinationPath = "[GAME]\\test2.zip",
						}
					}
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("CopyFile - TargetFile: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void Automapping_QuickBMS_CopyFiles_GameToGame()
		{
			var integration = CreateTestIntegration("test-game");
			integration.QuickBMSAutoMappings = new[]
			{
				new AutoMapping()
				{
					TargetPath = "[Game]\\Test",
					FileFilter = ".*",
					Actions = new[]
					{
						new CopyFilesAction()
						{
							TargetPath = "[GAME]\\test",
							DestinationPath = "[GAME]\\test2",
							FileFilter = ".*"
						}
					}
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void Automapping_QuickBMS_CopyFiles_ModToGame()
		{
			var integration = CreateTestIntegration("test-game");
			integration.QuickBMSAutoMappings = new[]
			{
				new AutoMapping()
				{
					TargetPath = "[Game]\\Test",
					FileFilter = ".*",
					Actions = new[]
					{
						new CopyFilesAction()
						{
							TargetPath = "[Mod]\\test",
							DestinationPath = "[GAME]\\test2",
							FileFilter = ".*"
						}
					}
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("CopyFiles - TargetPath: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}

		[Test]
		public void Automapping_QuickBMS_CopyFiles_IntegrationToGame()
		{
			var integration = CreateTestIntegration("test-game");
			integration.QuickBMSAutoMappings = new[]
			{
				new AutoMapping()
				{
					TargetPath = "[Game]\\Test",
					FileFilter = ".*",
					Actions = new[]
					{
						new CopyFilesAction()
						{
							TargetPath = "[Integration]\\test",
							DestinationPath = "[GAME]\\test2",
							FileFilter = ".*"
						}
					}
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("CopyFiles - TargetPath: Provided path must be in the [GAME] folder", result.First().loadErrors.First());
		}


		[Test]
		public void Automapping_QuickBMS_ReplaceFile_GameToGame()
		{
			var integration = CreateTestIntegration("test-game");
			integration.QuickBMSAutoMappings = new[]
			{
				new AutoMapping()
				{
					TargetPath = "[Game]\\Test",
					FileFilter = ".*",
					Actions = new[]
					{
						new ReplaceFileAction()
						{
							TargetFile = "[GAME]\\test.zip",
							ReplacementFile = "[GAME]\\test2.zip"
						}
					}
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ReplaceFile: Action cannot be defined for integration automappings", result.First().loadErrors.First());
		}

		[Test]
		public void Automapping_QuickBMS_ReplaceFile_ModToGame()
		{
			var integration = CreateTestIntegration("test-game");
			integration.QuickBMSAutoMappings = new[]
			{
				new AutoMapping()
				{
					TargetPath = "[Game]\\Test",
					FileFilter = ".*",
					Actions = new[]
					{
						new ReplaceFileAction()
						{
							TargetFile = "[Game]\\test.zip",
							ReplacementFile = "[Mod]\\test2.zip"
						}
					}
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ReplaceFile: Action cannot be defined for integration automappings", result.First().loadErrors.First());
		}

		[Test]
		public void Automapping_QuickBMS_ReplaceFile_IntegrationToGame()
		{
			var integration = CreateTestIntegration("test-game");
			integration.QuickBMSAutoMappings = new[]
			{
				new AutoMapping()
				{
					TargetPath = "[Game]\\Test",
					FileFilter = ".*",
					Actions = new[]
					{
						new ReplaceFileAction()
						{
							TargetFile = "[Game]\\test.zip",
							ReplacementFile = "[Integration]\\test2.zip"
						}
					}
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ReplaceFile: Action cannot be defined for integration automappings", result.First().loadErrors.First());
		}

		[Test]
		public void Automapping_QuickBMS_ReplaceFiles_GameToGame()
		{
			var integration = CreateTestIntegration("test-game");
			integration.QuickBMSAutoMappings = new[]
			{
				new AutoMapping()
				{
					TargetPath = "[Game]\\Test",
					FileFilter = ".*",
					Actions = new[]
					{
						new ReplaceFilesAction()
						{
							TargetPath = "[GAME]\\test",
							ReplacementPath = "[GAME]\\test2",
							FileFilter = ".*"
						}
					}
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ReplaceFiles: Action cannot be defined for integration automappings", result.First().loadErrors.First());
		}

		[Test]
		public void Automapping_QuickBMS_ReplaceFiles_ModToGame()
		{
			var integration = CreateTestIntegration("test-game");
			integration.QuickBMSAutoMappings = new[]
			{
				new AutoMapping()
				{
					TargetPath = "[Game]\\Test",
					FileFilter = ".*",
					Actions = new[]
					{
						new ReplaceFilesAction()
						{
							TargetPath = "[Game]\\test",
							ReplacementPath = "[Mod]\\test2",
							FileFilter = ".*"
						}
					}
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ReplaceFiles: Action cannot be defined for integration automappings", result.First().loadErrors.First());
		}

		[Test]
		public void Automapping_QuickBMS_ReplaceFiles_IntegrationToGame()
		{
			var integration = CreateTestIntegration("test-game");
			integration.QuickBMSAutoMappings = new[]
			{
				new AutoMapping()
				{
					TargetPath = "[Game]\\Test",
					FileFilter = ".*",
					Actions = new[]
					{
						new ReplaceFilesAction()
						{
							TargetPath = "[Game]\\test",
							ReplacementPath = "[Integration]\\test2",
							FileFilter = ".*"
						}
					}
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("ReplaceFiles: Action cannot be defined for integration automappings", result.First().loadErrors.First());
		}


		[Test]
		public void Automapping_QuickBMS_WriteToFile_DataFilePath_Mod()
		{
			var integration = CreateTestIntegration("test-game");
			integration.QuickBMSAutoMappings = new[]
			{
				new AutoMapping()
				{
					TargetPath = "[Game]\\Test.txt",
					FileFilter = ".*",
					Actions = new[]
					{
						new WriteToFileAction()
						{
							TargetFile = "[Game]\\test.txt",
							Content = new[]
							{
								new WriteContent()
								{
									StartOffset = 0,
									DataFilePath = "[Mod]\\Test.pak"
								}
							}
						}
					}
				}
			};

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("WriteToFile - DataFilePath: Action (with DataFilePath) cannot be defined for integration automappings", result.First().loadErrors.First());
		}

		#endregion

		[Test]
		public void InvalidAuthor_Primary_Null()
		{
			var integration = CreateTestIntegration("test-game");
			integration.Author = null;

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("Author name cannot be blank", result.First().loadErrors.First());
		}

		[Test]
		public void  ValidAuthor_Primary_NullRole()
		{
			var integration = CreateTestIntegration("test-game");
			integration.Author = new ModContributor() { Name = "ExampleAuthor", Role=null };

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}

		[Test]
		public void InvalidContributor_Null()
		{
			var integration = CreateTestIntegration("test-game");
			integration.Contributors = new ModContributor[] { null };

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("Contributor name cannot be blank", result.First().loadErrors.First());
		}

		[Test]
		public void InvalidContibutor_NullName()
		{
			var integration = CreateTestIntegration("test-game");
			integration.Contributors = new ModContributor[] { new ModContributor() { Name = null, Role = null } };

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.ConfigInvalid, result.First().status, result.First().loadErrors.FirstOrDefault());
			Assert.AreEqual("Contributor name cannot be blank", result.First().loadErrors.First());
		}

		[Test]
		public void ValidContibutor()
		{
			var integration = CreateTestIntegration("test-game");
			integration.Contributors = new ModContributor[] { new ModContributor() { Name = "Test Contributor", Role = "Assistance" } };

			var modPath = CreateTempIntegrationFiles(integration);
			var integrationLoader = new GameIntegrationLoader(integrationCacheFolder, "1.0", "1.0");
			var result = integrationLoader.Load(modPath);

			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(LoadStatus.Success, result.First().status, result.First().loadErrors.FirstOrDefault());
		}
	}
}
