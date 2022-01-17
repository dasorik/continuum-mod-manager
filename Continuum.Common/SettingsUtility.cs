using System.IO;
using Newtonsoft.Json;

namespace Continuum.Core.Utilities
{
	public class SettingsUtility
	{
		public static void SaveSettings<T>(T settings, string path)
		{
			var dataDirectory = Path.GetDirectoryName(path);

			var json = JsonConvert.SerializeObject(settings);

			if (!Directory.Exists(dataDirectory))
				Directory.CreateDirectory(dataDirectory);

			File.WriteAllText(path, json);
		}

		public static T LoadSettings<T>(string path)
			where T : new()
		{
			if (!File.Exists(path))
			{
				var settings = new T();
				SaveSettings(settings, path);
				return settings;
			}

			var jsonString = File.ReadAllText(path);
			return JsonConvert.DeserializeObject<T>(jsonString);
		}
	}
}
