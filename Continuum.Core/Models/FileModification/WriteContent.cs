using Continuum.Core.Interfaces;

namespace Continuum.Core.Models
{
	public class WriteContent : IWriteContent
	{
		public long StartOffset;
		public long? EndOffset;
		public string DataFilePath;
		public string Text;
		public bool Replace;

		long IWriteContent.StartOffset => StartOffset;
		long? IWriteContent.EndOffset => EndOffset;
		string IWriteContent.DataFilePath => DataFilePath;
		string IWriteContent.Text => Text;
		bool IWriteContent.Replace => Replace;
	}
}
