using System;
using System.IO;

namespace Continuum.Core.Utilities
{
	public class DirectoryUtility
	{
		public static void DeleteAndRecreateFolder(string path)
		{
			if (Directory.Exists(path))
				Directory.Delete(path, true);

			Directory.CreateDirectory(path);
		}

		public static string GenerateTempFolder(string basePath)
		{
			var folderPath = Path.Combine(basePath, Guid.NewGuid().ToString());

			if (Directory.Exists(folderPath))
				return GenerateTempFolder(basePath);

			Directory.CreateDirectory(folderPath);
			return folderPath;
		}
	}
}
