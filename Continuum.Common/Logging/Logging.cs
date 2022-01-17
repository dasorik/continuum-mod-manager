using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;

namespace Continuum.Common.Logging
{
	public class Logger
	{
		private static ILogger _logger = new ConsoleLogger();

		public static void Log(string message, LogSeverity severity)
		{
			_logger.Log(message, severity);
		}

		public static void ConfigureLogger(ILogger logger)
		{
			_logger = logger;
		}
	}

	public enum LogSeverity
	{
		Info,
		Warning,
		Error
	}

	public interface ILogger
	{
		public void Log(string message, LogSeverity severity);
	}

	public struct Log
	{
		public readonly DateTime date;
		public readonly string message;
		public readonly LogSeverity severity;

		public Log(string message, LogSeverity severity)
		{
			this.message = message;
			this.severity = severity;
			this.date = DateTime.Now;
		}
	}
}
