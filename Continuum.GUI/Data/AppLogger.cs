using Continuum.Common.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace Continuum.GUI
{
	public class AppLogger : ILogger
	{
		static string logFile;
		static Queue<Log> logs = new Queue<Log>();

		static AppLogger()
		{
			var logDirectory = Path.Combine(Global.APP_DATA_FOLDER, "Logs");
			logFile = Path.Combine(logDirectory, $"log_{DateTime.Now.ToString("yyyyMMddhhss")}.txt");

			Directory.CreateDirectory(logDirectory);
		}

		public void Log(string message, LogSeverity severity)
		{
			lock (logs)
			{
				if (logs.Count == 2000)
					logs.Dequeue();

				logs.Enqueue(new Log(message, severity));
				File.AppendAllText(logFile, $"{GetLogPrefix(severity)} {message}\n");

				Console.Write($"{GetLogPrefix(severity)} {message}");
			}
		}

		static string GetLogPrefix(LogSeverity severity)
		{
			switch (severity)
			{
				case LogSeverity.Info:
					return "[INFO]    ";
				case LogSeverity.Warning:
					return "[WARNING] ";
				case LogSeverity.Error:
					return "[ERROR]   ";
			}

			return string.Empty;
		}

		public static IEnumerable<Log> GetLogs()
		{
			return logs;
		}
	}
}
