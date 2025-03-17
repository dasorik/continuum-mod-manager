using System;
using System.IO;
using System.Threading.Tasks;

namespace Continuum.Common.Utilities
{
	public class Base64Utility
	{
		public static async Task<string> ConvertFileToBase64Async(string filePath)
		{
			if (!File.Exists(filePath))
				return string.Empty;

			var imageFileInfo = new FileInfo(filePath);
			var imageBytes = await File.ReadAllBytesAsync(imageFileInfo.FullName);

			string fileExtension = imageFileInfo.Extension.TrimStart('.');
			string mimeType = fileExtension;
			
			// Add any additional mime types as needed
			switch(fileExtension)
			{
				case "svg":
					mimeType = "svg+xml";
					break;
			}

			return $"data:image/{mimeType};base64," + Convert.ToBase64String(imageBytes);
		}
	}
}
