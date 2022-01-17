using NUnit.Framework;
using Newtonsoft.Json;
using Continuum.GUI.Models;

namespace Continuum.Core.Test
{
	public class SerializationTests
	{
		[Test]
		public void SerailizeModCache()
		{
			var cache = new FileModificationCache();
			cache.AddModification("test\\test.txt", new Models.FileModification("test\\test2.txt", Models.FileModificationType.Moved, "test.mod", true));

			string json = JsonConvert.SerializeObject(cache);
			var newCache = JsonConvert.DeserializeObject<FileModificationCache>(json);

			Assert.IsTrue(newCache.HasModification("test\\test.txt", Models.FileModificationType.Moved));
		}

		[Test]
		public void SerailizeUserSettings_IntegrationData()
		{
			var data = new UserModData();

			var cache = new FileModificationCache();
			cache.AddModification("test\\test.txt", new Models.FileModification("test\\test2.txt", Models.FileModificationType.Moved, "test.mod", true));

			data.IntegrationData.Add(new IntegrationData()
			{
				IntegrationID = "test.gameid",
				IntegrationFileModifications = cache
			});

			string json = JsonConvert.SerializeObject(data);
			var newData = JsonConvert.DeserializeObject<UserModData>(json);

			Assert.AreEqual("test.gameid", newData.GetIntegration("test.gameid")?.IntegrationID);
			Assert.IsTrue(newData.GetIntegration("test.gameid").IntegrationFileModifications.HasModification("test\\test.txt", Models.FileModificationType.Moved));
		}

		[Test]
		public void SerailizeUserSettings_ModData()
		{
			var data = new UserModData();

			var cache = new FileModificationCache();
			cache.AddModification("test\\test.txt", new Models.FileModification("test\\test2.txt", Models.FileModificationType.Moved, "test.mod", true));

			data.IntegrationData.Add(new IntegrationData()
			{
				IntegrationID = "test.gameid",
				ModFileModifications = cache
			});

			string json = JsonConvert.SerializeObject(data);
			var newData = JsonConvert.DeserializeObject<UserModData>(json);

			Assert.AreEqual("test.gameid", newData.GetIntegration("test.gameid")?.IntegrationID);
			Assert.IsTrue(newData.GetIntegration("test.gameid").ModFileModifications.HasModification("test\\test.txt", Models.FileModificationType.Moved));
		}
	}
}