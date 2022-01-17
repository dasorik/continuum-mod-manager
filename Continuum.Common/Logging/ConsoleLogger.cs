using System;
using System.Collections.Generic;
using System.Text;

namespace Continuum.Common.Logging
{
	public class ConsoleLogger : ILogger
	{
		public void Log(string message, LogSeverity severity)
		{
			Console.WriteLine($"{GetLogPrefix(severity)} {message}");
		}

		string GetLogPrefix(LogSeverity severity)
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
	}
}
