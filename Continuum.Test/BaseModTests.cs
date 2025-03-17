using Continuum.Core.Enums;
using Continuum.Core.Models;
using System.IO;
using System.IO.Compression;
using System.Text;
using Continuum.Test;

namespace Continuum.Core.Test
{
	public class BaseModTests
	{
		protected ModInstallerConfiguration configuration { get; private set; }
		protected string integrationCacheFolder { get; set; }
		protected string modCacheFolder { get; set; }

		protected GameIntegration integration { get; private set; }

		protected void SetUpTestData()
		{
			this.integrationCacheFolder = Path.Combine(FileSystemSetup.TempPath, "TestIntegrationCacheTemp");
			this.modCacheFolder = Path.Combine(FileSystemSetup.TempPath, "TestModCacheTemp");

			this.configuration = CreateTempConfiguration();
			this.integration = CreateTestIntegration();

			CreateTempFolders(configuration);
			CreateTempGameFiles(configuration);
		}

		protected ModInstallerConfiguration CreateTempConfiguration()
		{
			string gameFolder = Path.Combine(FileSystemSetup.TempPath, "TestGameTemp");
			string backupFolder = Path.Combine(FileSystemSetup.TempPath, "TestBackupTemp");
			string tempFolder = Path.Combine(FileSystemSetup.TempPath, "TestTemp");
			string toolFolder = "..\\..\\..\\Tools";

			return new ModInstallerConfiguration()
			{
				CheckForCollisions = true,
				BackupFolder = backupFolder,
				TempFolder = tempFolder,
				TargetPath = gameFolder,
				ToolPath = toolFolder,
				MaxQuickBMSBatches = 4
			};
		}

		protected GameIntegration CreateTestIntegration(string integrationID = null)
		{
			var id = integrationID ?? "test-game";

			return new GameIntegration()
			{
				IntegrationID = id,
				Version = "1.0",
				Author = new ModContributor() { Name = "Test", Role="Test" },
				DisplayIcon = "test.jpg",
				DisplayImage = "test2.jpg",
				DisplayName = "Test",
				TargetApplicationVersion = "1.0",
				Settings = new ModSettingCategory[]
				{
					new ModSettingCategory()
					{
						Category = "General",
						Settings = new ModSetting[]
						{
							new ModSetting()
							{
								SettingID = "install-path",
								SettingName = "Install Path",
								DefaultValue = null,
								Options = null,
								Type = ModSettingType.Text
							}
						}
					}
				},
				CacheFolder = Path.Combine(this.integrationCacheFolder, id) + ".integration"
			};
		}

		protected ModConfiguration CreateTestModConfig(string modID)
		{
			return new ModConfiguration()
			{
				ModID = modID,
				Author = new ModContributor() { Name = "Test" },
				LinkedIntegrations = new[] { new LinkedIntegration() { IntegrationID = "test-game", MinimumVersion = "1.0", TargetVersion = "1.0" } },
				Version = "2.0",
				DisplayImage = "test.png",
				CacheFolder = Path.Combine(modCacheFolder, modID) + ".mod"
			};
		}

		protected ModInstallInfo GetModInstallationInfo(ModConfiguration config)
		{
			return new ModInstallInfo(config, new GameIntegration() { IntegrationID = "test.integration" });
		}

		protected void CreateTempFolders(ModInstallerConfiguration configuration)
		{
			TryDeleteTempFolders(configuration);

			Directory.CreateDirectory(configuration.TargetPath);
			Directory.CreateDirectory(configuration.BackupFolder);
			Directory.CreateDirectory(configuration.TempFolder);
			Directory.CreateDirectory(modCacheFolder);
			Directory.CreateDirectory(integrationCacheFolder);
		}

		protected void TryDeleteTempFolders(ModInstallerConfiguration configuration)
		{
			if (Directory.Exists(configuration.TargetPath))
				Directory.Delete(configuration.TargetPath, true);

			if (Directory.Exists(configuration.BackupFolder))
				Directory.Delete(configuration.BackupFolder, true);

			if (Directory.Exists(configuration.TempFolder))
				Directory.Delete(configuration.TempFolder, true);

			if (Directory.Exists(modCacheFolder))
				Directory.Delete(modCacheFolder, true);

			if (Directory.Exists(integrationCacheFolder))
				Directory.Delete(integrationCacheFolder, true);
		}

		protected virtual void CreateTempGameFiles(ModInstallerConfiguration configuration)
		{
			File.WriteAllText(Path.Combine(configuration.TargetPath, "resource1.pak"), "This is a test file");
			File.WriteAllText(Path.Combine(configuration.TargetPath, "resource2.pak"), "This is another test file");

			string assetPath = Path.Combine(configuration.TargetPath, "assets");
			Directory.CreateDirectory(assetPath);

			File.WriteAllText(Path.Combine(assetPath, "asset1.pak"), "{\"id\":1,\"name\":\"Test\"}");
			File.WriteAllText(Path.Combine(assetPath, "asset2.pak"), "{\"id\":2,\"name\":\"Example\"}");

			using (var fileStream = new FileStream(Path.Combine(configuration.TargetPath, "zip1.zip"), FileMode.Create))
			{
				using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
				{
					AddFileToArchive(archive, "game_resource1.pak", "This is a test file with additional content");
					AddFileToArchive(archive, "game_resource2.pak", "1234567890");
					AddFileToArchive(archive, "assets\\game_asset1.pak", "{\"id\":1000,\"name\":\"GameData\"}");
					AddFileToArchive(archive, "assets\\game_asset2.pak", "{\"id\":2000,\"name\":\"AdditionalGamData\"}");
				}
			}

			using (var fileStream = new FileStream(Path.Combine(configuration.TargetPath, "zip2.zip"), FileMode.Create))
			{
				using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
				{
					AddFileToArchive(archive, "game_resource3.pak", "abcdefg");
					AddFileToArchive(archive, "game_resource4.pak", "gfedcba");
				}
			}
		}

		protected virtual string CreateTempModFiles(ModConfiguration modConfig)
		{
			string jsonConfig = Newtonsoft.Json.JsonConvert.SerializeObject(modConfig);
			string filePathCacheDir = Path.Combine(modCacheFolder, modConfig.ModID) + ".mod";
            string filePath = filePathCacheDir + ".zip";

			using (var fileStream = new FileStream(filePath, FileMode.Create))
			{
				using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
				{
					AddFileToArchive(archive, "config.json", jsonConfig);
					AddFileToArchive(archive, "resources\\mod_resource1.pak", "This is a test file with additional content");
					AddFileToArchive(archive, "resources\\mod_resource2.pak", "1234567890");
					AddFileToArchive(archive, "resources\\assets\\mod_asset1.pak", "{\"id\":1000,\"name\":\"ModData\"}");
					AddFileToArchive(archive, "resources\\assets\\mod_asset2.pak", "{\"id\":2000,\"name\":\"AdditionalModData\"}");
				}
			}

            Directory.CreateDirectory(filePathCacheDir);
            ZipFile.ExtractToDirectory(filePath, filePathCacheDir);
            File.Delete(filePath);

            return filePathCacheDir;
		}

		protected virtual string CreateTempIntegrationFiles(GameIntegration integration)
		{
			string jsonConfig = Newtonsoft.Json.JsonConvert.SerializeObject(integration);
			string filePathCacheDir = Path.Combine(integrationCacheFolder, integration.IntegrationID) + ".integration";
			string filePath = filePathCacheDir + ".zip";

            using (var fileStream = new FileStream(filePath, FileMode.Create))
			{
				using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
				{
					AddFileToArchive(archive, "config.json", jsonConfig);
					AddFileToArchive(archive, "resources\\mod_resource1.pak", "This is a test file with additional content");
					AddFileToArchive(archive, "resources\\mod_resource2.pak", "1234567890");
					AddFileToArchive(archive, "resources\\assets\\mod_asset1.pak", "{\"id\":1000,\"name\":\"ModData\"}");
					AddFileToArchive(archive, "resources\\assets\\mod_asset2.pak", "{\"id\":2000,\"name\":\"AdditionalModData\"}");
				}
			}

			Directory.CreateDirectory(filePathCacheDir);
            ZipFile.ExtractToDirectory(filePath, filePathCacheDir);
            File.Delete(filePath);

			return filePathCacheDir;
		}

		private void AddFileToArchive(ZipArchive archive, string fileName, string text)
		{
			byte[] data = Encoding.UTF8.GetBytes(text);
			var zipArchiveEntry = archive.CreateEntry(fileName, CompressionLevel.Fastest);
			using (var zipStream = zipArchiveEntry.Open())
				zipStream.Write(data, 0, data.Length);
		}
	}
}
