using Continuum.Core.Interfaces;
using Continuum.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Continuum.Core
{
	public class FileWriter
	{
		private Dictionary<string, List<FileWrite>> writeCache = new Dictionary<string, List<FileWrite>>();

		public bool CanWrite(string targetFile, IWriteContent content)
		{
			// Check if we don't have any writes to the file yet
			if (!writeCache.TryGetValue(targetFile, out List<FileWrite> fileWrites))
				return true;

			foreach (var write in fileWrites)
			{
				if (!CanWrite(content, write))
					return false;
			}

			return true;
		}

		public bool CanWrite(IWriteContent content, FileWrite write)
		{
			long contentLength = content.EndOffset.HasValue ? content.EndOffset.Value - content.StartOffset : 0;
			long contentEndOffset = content.StartOffset + contentLength;

			bool startsInRange = content.StartOffset > write.localStartOffset && content.StartOffset < write.localEndOffset;
			bool endsInRange = contentEndOffset > write.localStartOffset && contentEndOffset < write.localEndOffset;

			// We're attempting to write to a section of the file that has been modified by another
			if (startsInRange || (content.Replace && endsInRange))
				return false;

			return true;
		}

		public FileWrite WriteToFile(string filePath, string text, long offset, bool replace, bool ignoreWriteCache = false)
		{
			byte[] buffer = Encoding.ASCII.GetBytes(text);
			return WriteToFileRange(filePath, buffer, offset, replace ? offset + buffer.Length : offset, ignoreWriteCache: ignoreWriteCache);
		}

		public FileWrite WriteToFile(string filePath, byte[] buffer, long offset, bool replace, bool ignoreWriteCache = false)
		{
			return WriteToFileRange(filePath, buffer, offset, replace ? offset + buffer.Length : offset, ignoreWriteCache: ignoreWriteCache);
		}

		public FileWrite WriteToFileRange(string filePath, string text, long startOffset, long endOffset, bool ignoreWriteCache = false)
		{
			byte[] buffer = Encoding.ASCII.GetBytes(text);
			return WriteToFileRange(filePath, buffer, startOffset, endOffset, ignoreWriteCache:ignoreWriteCache);
		}

		public FileWrite WriteToFileRange(string filePath, byte[] buffer, long startOffset, long endOffset, bool ignoreWriteCache = false)
		{
			var actualStartOffset = ignoreWriteCache ? startOffset : GetOffsetForFile(startOffset, filePath);
			var actualEndOffset = ignoreWriteCache ? endOffset : GetOffsetForFile(endOffset, filePath);
			var replaceRange = actualEndOffset - actualStartOffset;

			byte[] fileBytes = File.ReadAllBytes(filePath);
			byte[] tempBuffer = new byte[actualStartOffset + buffer.Length + ((fileBytes.LongLength - actualStartOffset) - (actualEndOffset - actualStartOffset))];

			// Copy the files with the new bytes inserted in the middle
			Array.Copy(fileBytes, 0, tempBuffer, 0, actualStartOffset);
			Array.Copy(buffer, 0, tempBuffer, actualStartOffset, buffer.Length);
			Array.Copy(fileBytes, actualEndOffset, tempBuffer, actualStartOffset + buffer.LongLength, fileBytes.LongLength - actualEndOffset);

			var writeInfo = InsertToWriteCache(filePath, startOffset, endOffset, buffer.Length, buffer.Length - replaceRange);
			File.WriteAllBytes(filePath, tempBuffer);

			return writeInfo;
		}

		// TODO: We need a nice way to deal with removal of text
		private FileWrite InsertToWriteCache(string filePath, long localStartOffset, long localEndOffset, long bytesWritten, long bytesAdded)
		{
			var writeInfo = new FileWrite(localStartOffset, localEndOffset, bytesWritten, bytesAdded);

			if (!writeCache.ContainsKey(filePath))
				writeCache.Add(filePath, new List<FileWrite>());

			writeCache[filePath].Add(writeInfo);
			return writeInfo;
		}

		private long GetOffsetForFile(long memoryOffset, string file)
		{
			if (!writeCache.ContainsKey(file))
				return memoryOffset;

			var orderedWrites = writeCache[file].OrderBy(w => w.localStartOffset);
			long newMemoryOffset = 0;

			foreach (var write in orderedWrites)
			{
				if (write.localStartOffset <= memoryOffset)
					newMemoryOffset += write.bytesAdded;
				else
					break;
			}

			return newMemoryOffset + memoryOffset;
		}

		public static string GetUniqueFilePath(string path)
		{
			if (!File.Exists(path))
				return path;

			var fileInfo = new FileInfo(path);
			var fileName = Path.GetFileNameWithoutExtension(path);
			var directory = fileInfo.DirectoryName;
			var files = new HashSet<string>(Directory.GetFiles(directory));

			var originalPath = path;
			int index = 1;

			do
			{
				path = Path.Combine(directory, $"{fileName}_{index++}{fileInfo.Extension}");
			}
			while (files.Contains(path));

			return path;
		}
	}
}
