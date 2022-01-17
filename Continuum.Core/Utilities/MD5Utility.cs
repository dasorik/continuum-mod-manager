using System;
using System.IO;
using System.Security.Cryptography;

namespace Continuum.Core.Utilities
{
	public class MD5Utility
	{
		public static string CalculateMD5Hash(string filePath)
		{
			using (var md5 = MD5.Create())
			{
				using (var stream = File.OpenRead(filePath))
				{
					var hash = md5.ComputeHash(stream);
					return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
				}
			}
		}
	}
}
