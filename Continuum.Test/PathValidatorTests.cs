using Continuum.Core.Utilities;
using Continuum.GUI;
using NUnit.Framework.Legacy;
using NUnit.Framework;
using System.IO;

namespace Continuum.Core.Test
{
	public class PathValidatorTests
	{
		// Game Path

		[Test]
		public void ValidGamePath()
		{
			bool isValidPath = ModFilePathUtility.ValidGameFilePath("[Game]\\test\\test2\\file.txt");
			ClassicAssert.IsTrue(isValidPath);
		}

		[Test]
		public void ValidGamePath_BackTrack_Early()
		{
			bool isValidPath = ModFilePathUtility.ValidGameFilePath("[Game]\\test\\..\\file.txt");
			ClassicAssert.IsTrue(isValidPath);
		}

		[Test]
		public void InvalidGamePath_BackTrack_Early()
		{
			bool isValidPath = ModFilePathUtility.ValidGameFilePath("[Game]\\..\\test2\\file.txt");
			ClassicAssert.IsFalse(isValidPath);
		}

		[Test]
		public void InvalidGamePath_BackTrack_Late()
		{
			bool isValidPath = ModFilePathUtility.ValidGameFilePath("[Game]\\test\\test2\\..\\test\\..\\..\\check\\..\\..\\temp3");
			ClassicAssert.IsFalse(isValidPath);
		}

		[Test]
		public void InvalidGamePath_WrongStart_Mod()
		{
			bool isValidPath = ModFilePathUtility.ValidGameFilePath("[Mod]\\test\\test2\\file.txt");
			ClassicAssert.IsFalse(isValidPath);
		}

		[Test]
		public void InvalidGamePath_WrongStart_Integration()
		{
			bool isValidPath = ModFilePathUtility.ValidGameFilePath("[Integration]\\test\\test2\\file.txt");
			ClassicAssert.IsFalse(isValidPath);
		}

		[Test]
		public void InvalidGamePath_Backtrack_WithFile()
		{
			bool isValidPath = ModFilePathUtility.ValidGameFilePath("[Game]\\test\\..\\..\\file.txt");
			ClassicAssert.IsFalse(isValidPath);
		}

		[Test]
		public void InvalidGamePath_WrongStart_PhysicalPath()
		{
			bool isValidPath = ModFilePathUtility.ValidGameFilePath("C:\\test\\..\\..\\file.txt");
			ClassicAssert.IsFalse(isValidPath);
		}

		// Mod Path

		[Test]
		public void ValidModPath()
		{
			bool isValidPath = ModFilePathUtility.ValidModFilePath("[Mod]\\test\\test2\\file.txt");
			ClassicAssert.IsTrue(isValidPath);
		}

		[Test]
		public void ValidModPath_BackTrack_Early()
		{
			bool isValidPath = ModFilePathUtility.ValidModFilePath("[Mod]\\test\\..\\file.txt");
			ClassicAssert.IsTrue(isValidPath);
		}

		[Test]
		public void InvalidModPath_BackTrack_Early()
		{
			bool isValidPath = ModFilePathUtility.ValidModFilePath("[Mod]\\..\\test2\\file.txt");
			ClassicAssert.IsFalse(isValidPath);
		}

		[Test]
		public void InvalidModPath_BackTrack_Late()
		{
			bool isValidPath = ModFilePathUtility.ValidModFilePath("[Mod]\\test\\test2\\..\\test\\..\\..\\check\\..\\..\\temp3");
			ClassicAssert.IsFalse(isValidPath);
		}

		[Test]
		public void InvalidModPath_WrongStart_Game()
		{
			bool isValidPath = ModFilePathUtility.ValidModFilePath("[Game]\\test\\test2\\file.txt");
			ClassicAssert.IsFalse(isValidPath);
		}

		[Test]
		public void InvalidModPath_WrongStart_Integration()
		{
			bool isValidPath = ModFilePathUtility.ValidModFilePath("[Integration]\\test\\test2\\file.txt");
			ClassicAssert.IsFalse(isValidPath);
		}

		[Test]
		public void InvalidModPath_Backtrack_WithFile()
		{
			bool isValidPath = ModFilePathUtility.ValidModFilePath("[Mod]\\test\\..\\..\\file.txt");
			ClassicAssert.IsFalse(isValidPath);
		}

		[Test]
		public void InvalidModPath_WrongStart_PhysicalPath()
		{
			bool isValidPath = ModFilePathUtility.ValidModFilePath("C:\\test\\..\\..\\file.txt");
			ClassicAssert.IsFalse(isValidPath);
		}

		// Integration Path

		[Test]
		public void ValidIntegrationPath()
		{
			bool isValidPath = ModFilePathUtility.ValidIntegrationFilePath("[Integration]\\test\\test2\\file.txt");
			ClassicAssert.IsTrue(isValidPath);
		}

		[Test]
		public void ValidIntegrationPath_BackTrack_Early()
		{
			bool isValidPath = ModFilePathUtility.ValidIntegrationFilePath("[Integration]\\test\\..\\file.txt");
			ClassicAssert.IsTrue(isValidPath);
		}

		[Test]
		public void InvalidIntegrationPath_BackTrack_Early()
		{
			bool isValidPath = ModFilePathUtility.ValidIntegrationFilePath("[Integration]\\..\\test2\\file.txt");
			ClassicAssert.IsFalse(isValidPath);
		}

		[Test]
		public void InvalidIntegrationPath_BackTrack_Late()
		{
			bool isValidPath = ModFilePathUtility.ValidIntegrationFilePath("[Integration]\\test\\test2\\..\\test\\..\\..\\check\\..\\..\\temp3");
			ClassicAssert.IsFalse(isValidPath);
		}

		[Test]
		public void InvalidIntegrationPath_WrongStart_Mod()
		{
			bool isValidPath = ModFilePathUtility.ValidIntegrationFilePath("[Mod]\\test\\test2\\file.txt");
			ClassicAssert.IsFalse(isValidPath);
		}

		[Test]
		public void InvalidIntegrationPath_WrongStart_Game()
		{
			bool isValidPath = ModFilePathUtility.ValidIntegrationFilePath("[Game]\\test\\test2\\file.txt");
			ClassicAssert.IsFalse(isValidPath);
		}

		[Test]
		public void InvalidIntegrationPath_Backtrack_WithFile()
		{
			bool isValidPath = ModFilePathUtility.ValidIntegrationFilePath("[Integration]\\test\\..\\..\\file.txt");
			ClassicAssert.IsFalse(isValidPath);
		}

		[Test]
		public void InvalidIntegrationPath_WrongStart_PhysicalPath()
		{
			bool isValidPath = ModFilePathUtility.ValidIntegrationFilePath("C:\\test\\..\\..\\file.txt");
			ClassicAssert.IsFalse(isValidPath);
		}
	}
}
