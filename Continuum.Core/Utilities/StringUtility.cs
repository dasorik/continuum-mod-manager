using System;
using System.Collections.Generic;
using System.Text;

namespace Continuum.Core.Utilities
{
	public class StringUtility
	{
		public static string GetSHAOfString(string text)
		{
			var sha1 = new System.Security.Cryptography.SHA1Managed();
			var plaintextBytes = System.Text.Encoding.UTF8.GetBytes(text);
			var hashBytes = sha1.ComputeHash(plaintextBytes);

			var sb = new StringBuilder();
			foreach (var hashByte in hashBytes)
				sb.AppendFormat("{0:x2}", hashByte);

			return sb.ToString();
		}
	}
}
