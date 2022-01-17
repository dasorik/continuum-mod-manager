using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Continuum.GUI
{
	public class Global
	{
		public static readonly string APP_DATA_FOLDER;
		public static string CONTENT_ROOT;

		static Global()
		{
			string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			APP_DATA_FOLDER = Path.Combine(appDataPath, "continuum-mod-manager\\Data");

			if (!Directory.Exists(APP_DATA_FOLDER))
				Directory.CreateDirectory(APP_DATA_FOLDER);
		}
	}
}
