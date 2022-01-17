using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Continuum.Core.Utilities
{
	public class ZipUtility
	{
		public static void ZipFiles(IEnumerable<string> files, string destination)
		{
            ZipArchive zip = ZipFile.Open(destination, ZipArchiveMode.Create);
            HashSet<string> appendedFiles = new HashSet<string>();

            foreach (string file in files)
			{
                FileInfo fileInfo = new FileInfo(file);

                if (appendedFiles.Contains(fileInfo.Name))
				{
                    zip.Dispose();
                    File.Delete(destination);

                    throw new System.Exception("Cannot add two files with the same name to a flat zip file");
				}

                zip.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
                appendedFiles.Add(fileInfo.Name);
			}

            zip.Dispose();
        }

        public static void ZipDirectory(string folder, string destination, bool includeBaseDirectory)
        {
            ZipFile.CreateFromDirectory(folder, destination, CompressionLevel.Fastest, includeBaseDirectory);
        }

        public static void Unzip(string archiveName, string extractFolder)
		{
			ZipFile.ExtractToDirectory(archiveName, extractFolder);
		}
    }
}
