using System;
using System.Linq;

namespace Continuum.Core.Utilities
{
	public class ModFilePathUtility
	{
		public const string INTEGRATION_PATH_PREFIX = "[INTEGRATION]\\";
		public const string MOD_PATH_PREFIX = "[MOD]\\";
		public const string GAME_PATH_PREFIX = "[GAME]\\";

		public static bool ValidIntegrationFilePath(string path)
		{
			return path.StartsWith(INTEGRATION_PATH_PREFIX, StringComparison.InvariantCultureIgnoreCase) && ValidateBacktracks(path.Substring(INTEGRATION_PATH_PREFIX.Length, path.Length - INTEGRATION_PATH_PREFIX.Length));
		}

		public static bool ValidModFilePath(string path)
		{
			return path.StartsWith(MOD_PATH_PREFIX, StringComparison.InvariantCultureIgnoreCase) && ValidateBacktracks(path.Substring(MOD_PATH_PREFIX.Length, path.Length - MOD_PATH_PREFIX.Length));
		}

		public static bool ValidGameFilePath(string path)
		{
			return path.StartsWith(GAME_PATH_PREFIX, StringComparison.InvariantCultureIgnoreCase) && ValidateBacktracks(path.Substring(GAME_PATH_PREFIX.Length, path.Length - GAME_PATH_PREFIX.Length));
		}

		/// <summary>
		/// Ensure that the path provided can only access files in the [GAME]/[MOD] directory, and nothing below
		/// </summary>
		private static bool ValidateBacktracks(string path)
		{
			string[] pathSplit = path.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
			return ValidateBacktracks(pathSplit, 0, 0);
		}

		private static bool ValidateBacktracks(string[] split, int index, int level)
		{
			if (level < 0)
				return false;

			if (index == split.Length)
				return level >= 0;

			if (split[index] == "..")
				return ValidateBacktracks(split, ++index, --level);
			else
				return ValidateBacktracks(split, ++index, ++level);
		}
	}
}
