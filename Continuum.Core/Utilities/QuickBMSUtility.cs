using Continuum.Core.Extension;
using System;
using System.Diagnostics;
using Continuum.Common.Logging;
using System.IO;
using System.Threading.Tasks;
using System.Text;

namespace Continuum.Core.Utilities
{
	public class QuickBMSUtility
	{
		public static void ExtractFiles(string inputPath, string outputPath, string toolPath, string scriptPath)
		{
			var quickBmsPath = Path.Combine(toolPath, "quickbms\\quickbms.exe");

			Logger.Log($"Extracting file {inputPath} with QuickBMS", LogSeverity.Info);

			StringBuilder outputBuilder = new StringBuilder();
			StringBuilder errorBuilder = new StringBuilder();

			try
			{
				using (var quickBms = new Process())
				{
					quickBms.OutputDataReceived += (sender, e) => outputBuilder.AppendLine(e.Data);
					quickBms.ErrorDataReceived += (sender, e) => errorBuilder.AppendLine(e.Data);

					var startInfo = new ProcessStartInfo();
					startInfo.FileName = quickBmsPath;
					//startInfo.Arguments = $"\"{scriptPath}\" \"{inputPath}\" \"{outputPath}\" --quiet";
					startInfo.Arguments = $"\"{scriptPath}\" \"{inputPath}\" \"{outputPath}\"";
					startInfo.UseShellExecute = false;
					startInfo.CreateNoWindow = true;

					quickBms.StartInfo = startInfo;
					quickBms.Start();
					
					quickBms.WaitForExit();
				}

				Logger.Log($"Finished extracting: {inputPath}", LogSeverity.Info);
			}
			catch (Exception ex)
			{
				Console.Write($"[ERROR - QUICKBMS]: {ex}");
			}
		}
	}
}
