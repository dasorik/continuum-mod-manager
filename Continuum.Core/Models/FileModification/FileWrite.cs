
namespace Continuum.Core
{
	public struct FileWrite
	{
		public readonly long localStartOffset;
		public readonly long localEndOffset;
		public readonly long bytesWritten;
		public readonly long bytesAdded;

		public FileWrite(long localStartOffset, long localEndOffset, long bytesWritten, long bytesAdded)
		{
			this.localStartOffset = localStartOffset;
			this.localEndOffset = localEndOffset;
			this.bytesWritten = bytesWritten;
			this.bytesAdded = bytesAdded;
		}
	}
}
