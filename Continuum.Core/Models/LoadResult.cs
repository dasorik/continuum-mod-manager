using Continuum.Core.Enums;
using Continuum.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Continuum.Core.Models
{
	public class LoadResult<T> where T : IVersionLoadableData
	{
		public T data;
		public readonly string fileName;
		public readonly LoadStatus status;
		public readonly IEnumerable<string> loadErrors;

		public LoadResult(string fileName, T data, LoadStatus status, params string[] loadErrors)
		{
			this.fileName = fileName;
			this.data = data;
			this.status = status;
			this.loadErrors = loadErrors ?? new string[0];
		}
	}
}
