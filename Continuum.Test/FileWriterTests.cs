using NUnit.Framework;
using NUnit.Framework.Legacy;
using Continuum.Core;
using Continuum.Core.Utilities;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace Continuum.Core.Test
{
	public class FileWriterTests
	{
		string tempFile;
		FileWriter fileWriter;

		[SetUp]
		public void Setup()
		{
			fileWriter = new FileWriter();

			var executionPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			tempFile = Path.Combine(executionPath, "TEST_0526c915-4036-480b-90c5-3a8ef2088de6.txt");

			if (File.Exists(tempFile))
				File.Delete(tempFile);

			File.WriteAllText(tempFile, "abcdefghijklmnopqrstuvqxyz1234567890");
		}

		[TearDown]
		public void TearDown()
		{
			if (File.Exists(tempFile))
				File.Delete(tempFile);
		}

		[Test]
		public void CheckWriteData_SingleWrite()
		{
			var write = fileWriter.WriteToFile(tempFile, "Test", 5, false, false);

			ClassicAssert.AreEqual(4, write.bytesWritten);
			ClassicAssert.AreEqual(5, write.localStartOffset);
			ClassicAssert.AreEqual(4, write.bytesAdded);
			ClassicAssert.AreEqual(5, write.localEndOffset);
		}

		[Test]
		public void CheckWriteData_MultipleWrites_After()
		{
			var write = fileWriter.WriteToFile(tempFile, "Test1", 7, false, false);
			var write2 = fileWriter.WriteToFile(tempFile, "Test2", 10, false, false);

			ClassicAssert.AreEqual(5, write.bytesWritten);
			ClassicAssert.AreEqual(7, write.localStartOffset);
			ClassicAssert.AreEqual(5, write.bytesAdded);
            ClassicAssert.AreEqual(7, write.localEndOffset);

			ClassicAssert.AreEqual(5, write2.bytesWritten);
			ClassicAssert.AreEqual(10, write2.localStartOffset);
			ClassicAssert.AreEqual(5, write2.bytesAdded);
            ClassicAssert.AreEqual(10, write2.localEndOffset);
		}

		[Test]
		public void CheckWriteData_MultipleWrites_Before()
		{
			var write = fileWriter.WriteToFile(tempFile, "Test1", 10, false, false);
			var write2 = fileWriter.WriteToFile(tempFile, "Test2", 7, false, false);

			ClassicAssert.AreEqual(5, write.bytesWritten);
			ClassicAssert.AreEqual(10, write.localStartOffset);
			ClassicAssert.AreEqual(5, write.bytesAdded);
            ClassicAssert.AreEqual(10, write.localEndOffset);

			ClassicAssert.AreEqual(5, write2.bytesWritten);
			ClassicAssert.AreEqual(7, write2.localStartOffset);
			ClassicAssert.AreEqual(5, write2.bytesAdded);
            ClassicAssert.AreEqual(7, write2.localEndOffset);
		}

		[Test]
		public void ValidateText_SingleWrite()
		{
			var write = fileWriter.WriteToFile(tempFile, "Test", 8, false, false);
			var result = File.ReadAllText(tempFile);

            ClassicAssert.AreEqual("abcdefghTestijklmnopqrstuvqxyz1234567890", result);
		}

		[Test]
		public void ValidateText_SingleWrite_LongText()
		{
			var write = fileWriter.WriteToFile(tempFile, "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", 8, false, false);
			var result = File.ReadAllText(tempFile);

            ClassicAssert.AreEqual("abcdefgh!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!ijklmnopqrstuvqxyz1234567890", result);
		}

		[Test]
		public void ValidateText_MultipleWrites_After()
		{
			var write = fileWriter.WriteToFile(tempFile, "Test1", 8, false, false);
			var write2 = fileWriter.WriteToFile(tempFile, "Test2", 10, false, false);
			var result = File.ReadAllText(tempFile);

            ClassicAssert.AreEqual("abcdefghTest1ijTest2klmnopqrstuvqxyz1234567890", result);
		}

		[Test]
		public void ValidateText_MultipleWrites_Before()
		{
			var write = fileWriter.WriteToFile(tempFile, "Test1", 12, false, false);
			var write2 = fileWriter.WriteToFile(tempFile, "Test2", 6, false, false);
			var result = File.ReadAllText(tempFile);

            ClassicAssert.AreEqual("abcdefTest2ghijklTest1mnopqrstuvqxyz1234567890", result);
		}

		[Test]
		public void ValidateText_MultipleWrites_SameOffset()
		{
			var write = fileWriter.WriteToFile(tempFile, "Test1", 12, false, false);
			var write2 = fileWriter.WriteToFile(tempFile, "Test2", 12, false, false);
			var write3 = fileWriter.WriteToFile(tempFile, "Test3", 12, false, false);
			var result = File.ReadAllText(tempFile);

            ClassicAssert.AreEqual("abcdefghijklTest1Test2Test3mnopqrstuvqxyz1234567890", result);
		}

		[Test]
		public void ValidateText_MultipleWrites_Random()
		{
			var write = fileWriter.WriteToFile(tempFile, "Test1", 12, false, false);
			var write2 = fileWriter.WriteToFile(tempFile, "Test2", 2, false, false);
			var write3 = fileWriter.WriteToFile(tempFile, "Test3", 30, false, false);
			var write4 = fileWriter.WriteToFile(tempFile, "Test4", 20, false, false);
			var write5 = fileWriter.WriteToFile(tempFile, "Test5", 20, false, false);
			var write6 = fileWriter.WriteToFile(tempFile, "Test6", 21, false, false);
			var result = File.ReadAllText(tempFile);

            ClassicAssert.AreEqual("abTest2cdefghijklTest1mnopqrstTest4Test5uTest6vqxyz1234Test3567890", result);
		}

		[Test]
		public void ValidateText_MultipleWrites_Override()
		{
			var write = fileWriter.WriteToFile(tempFile, "Test1", 8, true, false); // Don't insert text
			var write2 = fileWriter.WriteToFile(tempFile, "Test2", 10, false, false);
			var result = File.ReadAllText(tempFile);

            ClassicAssert.AreEqual("abcdefghTeTest2st1nopqrstuvqxyz1234567890", result);
		}

		[Test]
		public void CheckWriteData_SingleWrite_Replace()
		{
			var write = fileWriter.WriteToFile(tempFile, "Test", 5, true, false);

			ClassicAssert.AreEqual(4, write.bytesWritten);
			ClassicAssert.AreEqual(5, write.localStartOffset);
			ClassicAssert.AreEqual(0, write.bytesAdded);
            ClassicAssert.AreEqual(9, write.localEndOffset);
		}

		[Test]
		public void CheckWriteData_MultipleWrites_After_Replace()
		{
			var write = fileWriter.WriteToFile(tempFile, "Test1", 7, true, false);
			var write2 = fileWriter.WriteToFile(tempFile, "Test2", 10, true, false);

			ClassicAssert.AreEqual(5, write.bytesWritten);
			ClassicAssert.AreEqual(7, write.localStartOffset);
			ClassicAssert.AreEqual(0, write.bytesAdded);
            ClassicAssert.AreEqual(12, write.localEndOffset);

			ClassicAssert.AreEqual(5, write2.bytesWritten);
			ClassicAssert.AreEqual(10, write2.localStartOffset);
			ClassicAssert.AreEqual(0, write2.bytesAdded);
            ClassicAssert.AreEqual(15, write2.localEndOffset);
		}

		[Test]
		public void CheckWriteData_MultipleWrites_Before_Replace()
		{
			var write = fileWriter.WriteToFile(tempFile, "Test1", 10, true, false);
			var write2 = fileWriter.WriteToFile(tempFile, "Test2", 7, true, false);

			ClassicAssert.AreEqual(5, write.bytesWritten);
			ClassicAssert.AreEqual(10, write.localStartOffset);
			ClassicAssert.AreEqual(0, write.bytesAdded);
			ClassicAssert.AreEqual(15, write.localEndOffset);

			ClassicAssert.AreEqual(5, write2.bytesWritten);
			ClassicAssert.AreEqual(7, write2.localStartOffset);
			ClassicAssert.AreEqual(0, write2.bytesAdded);
            ClassicAssert.AreEqual(12, write2.localEndOffset);
		}

		[Test]
		public void ValidateText_SingleWrite_Replace()
		{
			var write = fileWriter.WriteToFile(tempFile, "Test", 8, true, false);
			var result = File.ReadAllText(tempFile);

            ClassicAssert.AreEqual("abcdefghTestmnopqrstuvqxyz1234567890", result);
		}

		[Test]
		public void ValidateText_SingleWrite_Replace_LongText()
		{
            ClassicAssert.Throws<System.ArgumentException>(() => fileWriter.WriteToFile(tempFile, "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", 8, true, false));
		}

		[Test]
		public void ValidateText_MultipleWrites_After_Replace()
		{
			var write = fileWriter.WriteToFile(tempFile, "Test1", 8, true, false);
			var write2 = fileWriter.WriteToFile(tempFile, "Test2", 10, true, false);
			var result = File.ReadAllText(tempFile);

            ClassicAssert.AreEqual("abcdefghTeTest2pqrstuvqxyz1234567890", result);
		}

		[Test]
		public void ValidateText_MultipleWrites_Before_Replace()
		{
			var write = fileWriter.WriteToFile(tempFile, "Test1", 12, true, false);
			var write2 = fileWriter.WriteToFile(tempFile, "Test2", 6, true, false);
			var result = File.ReadAllText(tempFile);

            ClassicAssert.AreEqual("abcdefTest2lTest1rstuvqxyz1234567890", result);
		}

		[Test]
		public void ValidateText_MultipleWrites_SameOffset_Replace()
		{
			var write = fileWriter.WriteToFile(tempFile, "Test1", 12, true, false);
			var write2 = fileWriter.WriteToFile(tempFile, "Test2", 12, true, false);
			var write3 = fileWriter.WriteToFile(tempFile, "Test3", 12, true, false);
			var result = File.ReadAllText(tempFile);

            ClassicAssert.AreEqual("abcdefghijklTest3rstuvqxyz1234567890", result);
		}

		[Test]
		public void ValidateText_MultipleWrites_Random_Replace()
		{
			var write = fileWriter.WriteToFile(tempFile, "Test1", 12, true, false);
			var write2 = fileWriter.WriteToFile(tempFile, "Test2", 2, true, false);
			var write3 = fileWriter.WriteToFile(tempFile, "Test3", 30, true, false);
			var write4 = fileWriter.WriteToFile(tempFile, "Test4", 20, true, false);
			var write5 = fileWriter.WriteToFile(tempFile, "Test5", 20, true, false);
			var write6 = fileWriter.WriteToFile(tempFile, "Test6", 21, true, false);
			var result = File.ReadAllText(tempFile);

            ClassicAssert.AreEqual("abTest2hijklTest1rstTTest61234Test30", result);
		}

		[Test]
		public void ValidateText_MultipleWrites_Override_Replace()
		{
			var write = fileWriter.WriteToFile(tempFile, "Test1", 8, false, false); // Don't insert text
			var write2 = fileWriter.WriteToFile(tempFile, "Test2", 10, true, false);
			var result = File.ReadAllText(tempFile);

            ClassicAssert.AreEqual("abcdefghTest1ijTest2pqrstuvqxyz1234567890", result);
		}

		[Test]
		public void CheckWriteData_WriteRange_SameLength()
		{
			var write = fileWriter.WriteToFileRange(tempFile, "Test", 4, 8, false);

            ClassicAssert.AreEqual(4, write.bytesWritten);
            ClassicAssert.AreEqual(0, write.bytesAdded);
            ClassicAssert.AreEqual(4, write.localStartOffset);
		}

		[Test]
		public void CheckWriteData_WriteRange_LargerLength()
		{
			var write = fileWriter.WriteToFileRange(tempFile, "Test", 6, 20, false);

			ClassicAssert.AreEqual(4, write.bytesWritten);
			ClassicAssert.AreEqual(-10, write.bytesAdded);
            ClassicAssert.AreEqual(6, write.localStartOffset);
		}

		[Test]
		public void CheckWriteData_WriteRange_SmallerLength()
		{
			var write = fileWriter.WriteToFileRange(tempFile, "Test", 6, 8, false);

            ClassicAssert.AreEqual(4, write.bytesWritten);
            ClassicAssert.AreEqual(2, write.bytesAdded);
            ClassicAssert.AreEqual(6, write.localStartOffset);
		}

		[Test]
		public void ValidateText_WriteRange_SameLength()
		{
			var write = fileWriter.WriteToFileRange(tempFile, "Test", 4, 8, false);
			var result = File.ReadAllText(tempFile);

            ClassicAssert.AreEqual("abcdTestijklmnopqrstuvqxyz1234567890", result);
		}

		[Test]
		public void ValidateText_WriteRange_LargerLength()
		{
			var write = fileWriter.WriteToFileRange(tempFile, "Test", 6, 20, false);
			var result = File.ReadAllText(tempFile);

            ClassicAssert.AreEqual("abcdefTestuvqxyz1234567890", result);
		}

		[Test]
		public void ValidateText_WriteRange_SmallerLength()
		{
			var write = fileWriter.WriteToFileRange(tempFile, "Test", 6, 8, false);
			var result = File.ReadAllText(tempFile);

            ClassicAssert.AreEqual("abcdefTestijklmnopqrstuvqxyz1234567890", result);
		}

		[Test]
		public void ValidateText_WriteRange_MultipleWrites_NoOverlap_SameFirstLength()
		{
			var write = fileWriter.WriteToFileRange(tempFile, "Test1", 4, 9, false);
			var write2 = fileWriter.WriteToFileRange(tempFile, "Test2", 12, 13, false);

			var result = File.ReadAllText(tempFile);

            ClassicAssert.AreEqual("abcdTest1jklTest2nopqrstuvqxyz1234567890", result);
		}

		[Test]
		public void ValidateText_WriteRange_MultipleWrites_NoOverlap_SmallerFirstLength()
		{
			var write = fileWriter.WriteToFileRange(tempFile, "Test1", 4, 5, false);
			var write2 = fileWriter.WriteToFileRange(tempFile, "Test2", 12, 13, false);

			var result = File.ReadAllText(tempFile);

            ClassicAssert.AreEqual("abcdTest1fghijklTest2nopqrstuvqxyz1234567890", result);
		}

		[Test]
		public void ValidateText_WriteRange_MultipleWrites_NoOverlap_LargerFirstLength()
		{
			var write = fileWriter.WriteToFileRange(tempFile, "Test1", 4, 12, false);
			var write2 = fileWriter.WriteToFileRange(tempFile, "Test2", 14, 20, false);

			var result = File.ReadAllText(tempFile);

            ClassicAssert.AreEqual("abcdTest1mnTest2uvqxyz1234567890", result);
		}

		[Test]
		public void ValidateText_WriteRange_MultipleWrites_Overlap()
		{
			var write = fileWriter.WriteToFileRange(tempFile, "Test1", 4, 9, false);
			var write2 = fileWriter.WriteToFileRange(tempFile, "Test2", 8, 10, false);

			var result = File.ReadAllText(tempFile);

            ClassicAssert.AreEqual("abcdTestTest2klmnopqrstuvqxyz1234567890", result);
		}
	}
}