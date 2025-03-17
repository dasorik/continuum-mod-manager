using Continuum.Core.Utilities;
using Continuum.GUI;
using NUnit.Framework.Legacy;
using NUnit.Framework;
using System.IO;

namespace Continuum.Core.Test
{
	public class ZipUtilityTests
	{
		string tempArchiveFolder;
		string tempExtractFolder;

		[SetUp]
		protected void SetUpTestData()
		{
			CreateTempConfiguration();
		}

		[TearDown]
		protected void RemoveTestData()
		{
			if (Directory.Exists(tempArchiveFolder))
				Directory.Delete(tempArchiveFolder, true);

			if (Directory.Exists(tempExtractFolder))
				Directory.Delete(tempExtractFolder, true);
		}

		protected void CreateTempConfiguration()
		{
			this.tempArchiveFolder = Path.Combine(Global.APP_DATA_FOLDER, "TestArchiveTemp");
			this.tempExtractFolder = Path.Combine(Global.APP_DATA_FOLDER, "TestExtractTemp");

			if (!Directory.Exists(tempArchiveFolder))
				Directory.CreateDirectory(tempArchiveFolder);

			if (!Directory.Exists(tempExtractFolder))
				Directory.CreateDirectory(tempExtractFolder);
		}

		protected string CreateTestFile(string relativePath, string content)
		{
			string path = Path.Combine(tempArchiveFolder, relativePath);
			var fileInfo = new FileInfo(path);

			if (!Directory.Exists(fileInfo.Directory.FullName))
				Directory.CreateDirectory(fileInfo.Directory.FullName);

			File.WriteAllText(path, content);
			return path;
		}

		[Test]
		public void ZipFiles_UniqueNames_FlatFile()
		{
			string[] testFiles = new[]
			{
				CreateTestFile("test.txt", "Test 1"),
				CreateTestFile("test2.txt", "Test 2"),
			};

			string zipFileName = Path.Combine(tempArchiveFolder, "test.zip");
			ZipUtility.ZipFiles(testFiles, zipFileName);
			ZipUtility.Unzip(zipFileName, tempExtractFolder);

            ClassicAssert.IsTrue(File.Exists(Path.Combine(tempExtractFolder, "test.txt")));
            ClassicAssert.IsTrue(File.Exists(Path.Combine(tempExtractFolder, "test2.txt")));
		}

		[Test]
		public void ZipFiles_UniqueNames_WithDirectory()
		{
			string[] testFiles = new[]
			{
				CreateTestFile("test.txt", "Test 1"),
				CreateTestFile("test2.txt", "Test 2"),
				CreateTestFile("subdir\\test3.txt", "Test 3"),
				CreateTestFile("subdir\\subdir2\\test4.txt", "Test 4"),
			};

			string zipFileName = Path.Combine(tempArchiveFolder, "test.zip");
			ZipUtility.ZipFiles(testFiles, zipFileName);
			ZipUtility.Unzip(zipFileName, tempExtractFolder);

			ClassicAssert.IsTrue(File.Exists(Path.Combine(tempExtractFolder, "test.txt")));
			ClassicAssert.IsTrue(File.Exists(Path.Combine(tempExtractFolder, "test2.txt")));
			ClassicAssert.IsTrue(File.Exists(Path.Combine(tempExtractFolder, "test3.txt")));
            ClassicAssert.IsTrue(File.Exists(Path.Combine(tempExtractFolder, "test4.txt")));
		}

		[Test]
		public void ZipFiles_NotUniqueNames_WithDirectory()
		{
			string[] testFiles = new[]
			{
				CreateTestFile("test.txt", "Test 1"),
				CreateTestFile("test2.txt", "Test 2"),
				CreateTestFile("subdir\\test.txt", "Test 3"),
				CreateTestFile("subdir\\subdir2\\test4.txt", "Test 4"),
			};

			Assert.Throws(typeof(System.Exception), () =>
			{
				string zipFileName = Path.Combine(tempArchiveFolder, "test.zip");
				ZipUtility.ZipFiles(testFiles, zipFileName);
			});
		}

		[Test]
		public void ZipDirectory_WithRootFolder()
		{
			CreateTestFile("root\\test.txt", "Test 1");
			CreateTestFile("root\\test2.txt", "Test 2");
			CreateTestFile("root\\subdir\\test.txt", "Test 3");
			CreateTestFile("root\\subdir\\subdir2\\test4.txt", "Test 4");

			string zipFileName = Path.Combine(tempArchiveFolder, "test.zip");
			string zipArchiveRoot = Path.Combine(tempArchiveFolder, "root");

			ZipUtility.ZipDirectory(zipArchiveRoot, zipFileName, true);
			ZipUtility.Unzip(zipFileName, tempExtractFolder);

			ClassicAssert.IsTrue(Directory.Exists(Path.Combine(tempExtractFolder, "root")));
			ClassicAssert.IsTrue(File.Exists(Path.Combine(tempExtractFolder, "root\\test.txt")));
			ClassicAssert.IsTrue(File.Exists(Path.Combine(tempExtractFolder, "root\\test2.txt")));
			ClassicAssert.IsTrue(File.Exists(Path.Combine(tempExtractFolder, "root\\subdir\\test.txt")));
            ClassicAssert.IsTrue(File.Exists(Path.Combine(tempExtractFolder, "root\\subdir\\subdir2\\test4.txt")));
		}

		[Test]
		public void ZipDirectory_WithoutRootFolder()
		{
			CreateTestFile("root\\test.txt", "Test 1");
			CreateTestFile("root\\test2.txt", "Test 2");
			CreateTestFile("root\\subdir\\test.txt", "Test 3");
			CreateTestFile("root\\subdir\\subdir2\\test4.txt", "Test 4");

			string zipFileName = Path.Combine(tempArchiveFolder, "test.zip");
			string zipArchiveRoot = Path.Combine(tempArchiveFolder, "root");

			ZipUtility.ZipDirectory(zipArchiveRoot, zipFileName, false);
			ZipUtility.Unzip(zipFileName, tempExtractFolder);

			ClassicAssert.IsTrue(!Directory.Exists(Path.Combine(tempExtractFolder, "root")));
			ClassicAssert.IsTrue(File.Exists(Path.Combine(tempExtractFolder, "test.txt")));
			ClassicAssert.IsTrue(File.Exists(Path.Combine(tempExtractFolder, "test2.txt")));
			ClassicAssert.IsTrue(File.Exists(Path.Combine(tempExtractFolder, "subdir\\test.txt")));
            ClassicAssert.IsTrue(File.Exists(Path.Combine(tempExtractFolder, "subdir\\subdir2\\test4.txt")));
		}
	}
}
