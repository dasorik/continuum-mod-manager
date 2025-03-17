using Continuum.Core;
using Continuum.Core.Enums;
using Continuum.Core.Models;
using Continuum.GUI;
using NUnit.Framework.Legacy;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Continuum.Core.Test
{
	[TestFixture]
	class ModCollisionTests
	{
		string testFile1;
		string testFile2;
		string exampleFile1;
		string exampleFile2;
		string exampleFile3;
		string exampleDestination1;
		string exampleDestination2;
		string exampleDestination3_exampleDestination1;

		ModInstallInfo GetModInstallationInfo(ModConfiguration config)
		{
			return new ModInstallInfo(config, new GameIntegration() { IntegrationID = "test.integration" });
		}

		[SetUp]
		public void SetUp()
		{
			// Create some test files in a temp directory
			string path = Path.Combine(Global.APP_DATA_FOLDER, "TestTemp");
			Directory.CreateDirectory(path);

			testFile1 = Path.Combine(path, "Test1.txt");
			testFile2 = Path.Combine(path, "Test2.txt");
			exampleFile1 = Path.Combine(path, "ExampleFile1.txt");
			exampleFile2 = Path.Combine(path, "ExampleFile2.txt");
			exampleFile3 = Path.Combine(path, "ExampleFile3.txt");
			exampleDestination1 = Path.Combine(path, "ExampleDestination1.txt");
			exampleDestination2 = Path.Combine(path, "ExampleDestination2.txt");
			exampleDestination3_exampleDestination1 = Path.Combine(path, "ExampleDestination3.txt");

			File.WriteAllText(testFile1, "ascdefghijklmnopqrstuvqxyz");
			File.WriteAllText(testFile2, "1234567890");
			File.WriteAllText(exampleFile1, "ExampleText");
			File.WriteAllText(exampleFile2, "ExampleText2");
			File.WriteAllText(exampleFile3, "ExampleText3");

			// For when we're expecting data to read (we aren't moving these files around)
			File.WriteAllText(exampleDestination1, "DestinationText");
			File.WriteAllText(exampleDestination2, "DestinationText2");
			File.WriteAllText(exampleDestination3_exampleDestination1, "DestinationText");
		}

		[TearDown]
		public void TearDown()
		{
			string path = Path.Combine(Global.APP_DATA_FOLDER, "TestTemp");
			Directory.Delete(path, true);
		}

		// File Writes

		[Test]
		public void CheckFileWriteWithDelete()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleFile1, new FileModification(null, FileModificationType.Deleted, "Test.Delete", false));

			var fileWriter = new FileWriter();
			var content = new WriteContent() { Text = "Test", StartOffset = 12 };
			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Edit" });

			bool hasCollision = ModCollisionTracker.HasEditCollision(mod, exampleFile1, content, fileWriter, actions, out var collision);

			ClassicAssert.AreEqual(true, hasCollision);
			ClassicAssert.AreEqual(collision.severity, ModCollisionSeverity.Clash);
			ClassicAssert.AreEqual(collision.modID, "Test.Delete");
            ClassicAssert.IsTrue(collision.description.Contains("Attempting to write to a file that has been deleted by another mod"), $"Actual: {collision.description}");
		}

		[Test]
		public void CheckFileWriteWithMove()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleFile1, new FileModification(exampleDestination1, FileModificationType.Moved, "Test.Move", false));

			var fileWriter = new FileWriter();
			var content = new WriteContent() { Text = "Test", StartOffset = 12 };
			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Edit" });

			bool hasCollision = ModCollisionTracker.HasEditCollision(mod, exampleFile1, content, fileWriter, actions, out var collision);

			ClassicAssert.AreEqual(true, hasCollision);
			ClassicAssert.AreEqual(collision.severity, ModCollisionSeverity.Clash);
			ClassicAssert.AreEqual(collision.modID, "Test.Move");
            ClassicAssert.IsTrue(collision.description.Contains("Attempting to write to a file that has been moved by another mod"), $"Actual: {collision.description}");
		}

		[Test]
		public void CheckFileWriteWithReplace()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleFile1, new FileModification(null, FileModificationType.Replaced, "Test.Move", false));

			var fileWriter = new FileWriter();
			var content = new WriteContent() { Text = "Test", StartOffset = 12 };
			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Edit" });

			bool hasCollision = ModCollisionTracker.HasEditCollision(mod, exampleFile1, content, fileWriter, actions, out var collision);

			ClassicAssert.AreEqual(true, hasCollision);
			ClassicAssert.AreEqual(collision.severity, ModCollisionSeverity.Clash);
			ClassicAssert.AreEqual(collision.modID, "Test.Move");
            ClassicAssert.IsTrue(collision.description.Contains("Attempting to write to a file that has been replaced by another mod"), $"Actual: {collision.description}");
		}


		// File Moves

		[Test]
		public void CheckFileMoveWithMove_DifferentTarget()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleFile1, new FileModification(exampleDestination2, FileModificationType.Moved, "Test.Move", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Move2" });
			bool hasCollision = ModCollisionTracker.HasMoveCollision(mod, exampleFile1, exampleDestination1, actions, out var collision);

			ClassicAssert.AreEqual(true, hasCollision);
			ClassicAssert.AreEqual(collision.severity, ModCollisionSeverity.Clash);
			ClassicAssert.AreEqual(collision.modID, "Test.Move");
            ClassicAssert.IsTrue(collision.description.Contains("Attempting to move a file that has been moved by another mod"), $"Actual: {collision.description}");
		}

		[Test]
		public void CheckFileMoveWithMove_SameTarget()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleFile1, new FileModification(exampleDestination1, FileModificationType.Moved, "Test.Move", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Move2" });
			bool hasCollision = ModCollisionTracker.HasMoveCollision(mod, exampleFile1, exampleDestination1, actions, out var collision);

			ClassicAssert.AreEqual(false, hasCollision);
		}

		[Test]
		public void CheckFileMoveWithMove_DiffSource_SameDest_DiffData()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleFile1, new FileModification(exampleDestination1, FileModificationType.Moved, "Test.Move", false));
			actions.AddModification(exampleDestination1, new FileModification(null, FileModificationType.Added, "Test.Move", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Move2" });
			bool hasCollision = ModCollisionTracker.HasMoveCollision(mod, exampleFile2, exampleDestination1, actions, out var collision);

			ClassicAssert.AreEqual(true, hasCollision);
			ClassicAssert.AreEqual(collision.severity, ModCollisionSeverity.Clash);
			ClassicAssert.AreEqual(collision.modID, "Test.Move");
            ClassicAssert.IsTrue(collision.description.Contains("Attempting to move a file that has been added by another mod (with different data)"), $"Actual: {collision.description}");
		}

		[Test]
		public void CheckFileMoveWithMove_DiffSource_SameDest_SameData()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleFile1, new FileModification(exampleDestination1, FileModificationType.Moved, "Test.Move", false));
			actions.AddModification(exampleDestination1, new FileModification(null, FileModificationType.Added, "Test.Move", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Move2" });
			bool hasCollision = ModCollisionTracker.HasMoveCollision(mod, exampleDestination3_exampleDestination1, exampleDestination1, actions, out var collision);

			ClassicAssert.AreEqual(false, hasCollision);
		}

		[Test]
		public void CheckFileMoveWithDelete()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleFile1, new FileModification(null, FileModificationType.Deleted, "Test.Delete", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Move" });
			bool hasCollision = ModCollisionTracker.HasMoveCollision(mod, exampleFile1, exampleDestination1, actions, out var collision);

			ClassicAssert.AreEqual(true, hasCollision);
			ClassicAssert.AreEqual(collision.severity, ModCollisionSeverity.Clash);
			ClassicAssert.AreEqual(collision.modID, "Test.Delete");
            ClassicAssert.IsTrue(collision.description.Contains("Attempting to move a file that has been deleted by another mod"), $"Actual: {collision.description}");
		}

		[Test]
		public void CheckFileMoveWithReplace_DifferentData()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleFile1, new FileModification(null, FileModificationType.Replaced, "Test.Replace", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Move2" });
			bool hasCollision = ModCollisionTracker.HasMoveCollision(mod, exampleFile1, exampleDestination1, actions, out var collision);

			ClassicAssert.AreEqual(true, hasCollision);
			ClassicAssert.AreEqual(collision.severity, ModCollisionSeverity.Clash);
			ClassicAssert.AreEqual(collision.modID, "Test.Replace");
			ClassicAssert.IsTrue(collision.description.Contains("Attempting to move a file that has been replaced by another mod"), $"Actual: {collision.description}");
		}

		[Test]
		public void CheckFileMoveWithReplace_SameData()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleDestination3_exampleDestination1, new FileModification(null, FileModificationType.Replaced, "Test.Replace", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Move2" });
			bool hasCollision = ModCollisionTracker.HasMoveCollision(mod, exampleDestination1, exampleDestination3_exampleDestination1, actions, out var collision);

			ClassicAssert.AreEqual(false, hasCollision);
		}

		[Test]
		public void CheckFileMoveWithWrite_DifferentData()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleFile1, new FileModification(null, FileModificationType.Edited, "Test.Edit", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Move2" });
			bool hasCollision = ModCollisionTracker.HasMoveCollision(mod, exampleFile1, exampleDestination1, actions, out var collision);

			ClassicAssert.AreEqual(true, hasCollision);
			ClassicAssert.AreEqual(collision.severity, ModCollisionSeverity.Clash);
			ClassicAssert.AreEqual(collision.modID, "Test.Edit");
            ClassicAssert.IsTrue(collision.description.Contains("Attempting to move a file that has been written to by another mod"), $"Actual: {collision.description}");
		}

		[Test]
		public void CheckFileMoveWithWrite_SameData()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleDestination3_exampleDestination1, new FileModification(null, FileModificationType.Edited, "Test.Edit", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Move2" });
			bool hasCollision = ModCollisionTracker.HasMoveCollision(mod, exampleDestination1, exampleDestination3_exampleDestination1, actions, out var collision);

			ClassicAssert.AreEqual(false, hasCollision);
		}

		[Test]
		public void CheckFileMoveWithAdd_SameDest_DifferentData()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleFile1, new FileModification(null, FileModificationType.Added, "Test.Add", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Move" });
			bool hasCollision = ModCollisionTracker.HasMoveCollision(mod, exampleFile2, exampleFile1, actions, out var collision);

			ClassicAssert.AreEqual(true, hasCollision);
			ClassicAssert.AreEqual(collision.severity, ModCollisionSeverity.Clash);
			ClassicAssert.AreEqual(collision.modID, "Test.Add");
            ClassicAssert.IsTrue(collision.description.Contains("Attempting to move a file that has been added by another mod (with different data)"), $"Actual: {collision.description}");
		}

		[Test]
		public void CheckFileMoveWithAdd_DiffDest()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleFile2, new FileModification(null, FileModificationType.Added, "Test.Add", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Move" });
			bool hasCollision = ModCollisionTracker.HasMoveCollision(mod, exampleFile3, exampleFile1, actions, out var collision);

			ClassicAssert.AreEqual(false, hasCollision);
		}

		// File Replacements

		[Test]
		public void CheckFileReplaceWithMove()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleFile1, new FileModification("C:\\Example\\ExampleFile.txt", FileModificationType.Moved, "Test.Move", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Replace" });
			bool hasCollision = ModCollisionTracker.HasReplaceCollision(mod, exampleFile1, exampleFile2, actions, out var collision);

			ClassicAssert.AreEqual(true, hasCollision);
			ClassicAssert.AreEqual(collision.severity, ModCollisionSeverity.Clash);
			ClassicAssert.AreEqual(collision.modID, "Test.Move");
            ClassicAssert.IsTrue(collision.description.Contains("Attempting to replace a file that has been moved by another mod"), $"Actual: {collision.description}");
		}

		[Test]
		public void CheckFileReplaceWithDelete()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleFile1, new FileModification(null, FileModificationType.Deleted, "Test.Delete", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Replace" });
			bool hasCollision = ModCollisionTracker.HasReplaceCollision(mod, exampleFile1, exampleFile2, actions, out var collision);

			ClassicAssert.AreEqual(true, hasCollision);
			ClassicAssert.AreEqual(collision.severity, ModCollisionSeverity.Clash);
			ClassicAssert.AreEqual(collision.modID, "Test.Delete");
            ClassicAssert.IsTrue(collision.description.Contains("Attempting to replace a file that has been deleted by another mod"), $"Actual: {collision.description}");
		}

		[Test]
		public void CheckFileReplaceWithWrite()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleFile1, new FileModification(null, FileModificationType.Edited, "Test.Edit", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Replace" });
			bool hasCollision = ModCollisionTracker.HasReplaceCollision(mod, exampleFile1, exampleFile2, actions, out var collision);

			ClassicAssert.AreEqual(true, hasCollision);
			ClassicAssert.AreEqual(collision.severity, ModCollisionSeverity.Clash);
			ClassicAssert.AreEqual(collision.modID, "Test.Edit");
            ClassicAssert.IsTrue(collision.description.Contains("Attempting to replace a file that has been written to by another mod"), $"Actual: {collision.description}");
		}

		[Test]
		public void CheckFileReplaceWithReplace_DifferentData()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleFile1, new FileModification(null, FileModificationType.Replaced, "Test.Replace1", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Replace2" });
			bool hasCollision = ModCollisionTracker.HasReplaceCollision(mod, exampleFile1, testFile2, actions, out var collision);

			ClassicAssert.AreEqual(true, hasCollision);
			ClassicAssert.AreEqual(collision.severity, ModCollisionSeverity.Clash);
			ClassicAssert.AreEqual(collision.modID, "Test.Replace1");
            ClassicAssert.IsTrue(collision.description.Contains("Attempting to replace a file that has been replaced by another mod (with different data)"), $"Actual: {collision.description}");
		}

		[Test]
		public void CheckFileReplaceWithReplace_SameData()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleDestination1, new FileModification(null, FileModificationType.Replaced, "Test.Replace1", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Replace2" });
			bool hasCollision = ModCollisionTracker.HasReplaceCollision(mod, exampleFile1, exampleDestination3_exampleDestination1, actions, out var collision);

			ClassicAssert.AreEqual(false, hasCollision);
		}

		// Zip

		[Test]
		public void CheckZipWithReplace_SameDestination()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleDestination1, new FileModification(null, FileModificationType.Replaced, "Test.Replace", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Zip" });
			string[] files = new[] { exampleFile1, exampleFile2 };

			bool hasCollision = ModCollisionTracker.HasZipCollision(mod, files, exampleDestination1, actions, out var collision);

			ClassicAssert.AreEqual(true, hasCollision);
		}

		[Test]
		public void CheckZipWithMove_SameDestination()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleFile1, new FileModification(exampleDestination1, FileModificationType.Moved, "Test.Move", false));
			actions.AddModification(exampleDestination1, new FileModification(null, FileModificationType.Added, "Test.Move", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Zip" });
			string[] files = new[] { exampleFile1, exampleFile2 };

			bool hasCollision = ModCollisionTracker.HasZipCollision(mod, files, exampleDestination1, actions, out var collision);

			ClassicAssert.AreEqual(true, hasCollision);
		}

		[Test]
		public void CheckZipWithMove_SameTarget()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleFile3, new FileModification(exampleDestination1, FileModificationType.Moved, "Test.Move", false));
			actions.AddModification(exampleDestination1, new FileModification(null, FileModificationType.Added, "Test.Move", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Zip" });
			string[] files = new[] { exampleFile1, exampleFile2 };

			bool hasCollision = ModCollisionTracker.HasZipCollision(mod, files, exampleFile3, actions, out var collision);

			ClassicAssert.AreEqual(false, hasCollision);
		}

		[Test]
		public void CheckZipWithAdd_SameDestination()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleDestination1, new FileModification(null, FileModificationType.Added, "Test.Add", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Zip" });
			string[] files = new[] { exampleFile1, exampleFile2 };

			bool hasCollision = ModCollisionTracker.HasZipCollision(mod, files, exampleDestination1, actions, out var collision);

			ClassicAssert.AreEqual(true, hasCollision);
		}

		[Test]
		public void CheckZipWithDelete_SameDestination()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleDestination1, new FileModification(null, FileModificationType.Deleted, "Test.Delete", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Zip" });
			string[] files = new[] { exampleFile1, exampleFile2 };

			bool hasCollision = ModCollisionTracker.HasZipCollision(mod, files, exampleDestination1, actions, out var collision);

			ClassicAssert.AreEqual(false, hasCollision);
		}

		[Test]
		public void CheckZipWithEdit_SameDestination()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleDestination1, new FileModification(null, FileModificationType.Edited, "Test.Edit", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Zip" });
			string[] files = new[] { exampleFile1, exampleFile2 };

			bool hasCollision = ModCollisionTracker.HasZipCollision(mod, files, exampleDestination1, actions, out var collision);

			ClassicAssert.AreEqual(true, hasCollision);
		}

		[Test]
		public void CheckZipWithDelete_NoConflicts()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleDestination2, new FileModification(null, FileModificationType.Deleted, "Test.Example", false));
			actions.AddModification(exampleDestination1, new FileModification(null, FileModificationType.Added, "Test.Example", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Zip" });
			string[] files = new[] { exampleFile1, exampleFile2 };

			bool hasCollision = ModCollisionTracker.HasZipCollision(mod, files, exampleFile3, actions, out var collision);

			ClassicAssert.AreEqual(false, hasCollision);
		}

		[Test]
		public void CheckZipFiles_OneFileMoved()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleFile1, new FileModification(exampleDestination1, FileModificationType.Moved, "Test.Move", false));
			actions.AddModification(exampleDestination1, new FileModification(null, FileModificationType.Added, "Test.Move", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Zip" });
			string[] files = new[] { exampleFile1, exampleFile2 };

			bool hasCollision = ModCollisionTracker.HasZipCollision(mod, files, exampleFile3, actions, out var collision);

			ClassicAssert.AreEqual(true, hasCollision);
		}

		[Test]
		public void CheckZipFiles_OneFileDeleted()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleFile1, new FileModification(null, FileModificationType.Deleted, "Test.Delete", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Zip" });
			string[] files = new[] { exampleFile1, exampleFile2 };

			bool hasCollision = ModCollisionTracker.HasZipCollision(mod, files, exampleFile3, actions, out var collision);

			ClassicAssert.AreEqual(true, hasCollision);
		}

		[Test]
		public void CheckZipFiles_OneFileReplaced()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleFile1, new FileModification(null, FileModificationType.Replaced, "Test.Replace", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Zip" });
			string[] files = new[] { exampleFile1, exampleFile2 };

			bool hasCollision = ModCollisionTracker.HasZipCollision(mod, files, exampleFile3, actions, out var collision);

			ClassicAssert.AreEqual(true, hasCollision);
		}

		[Test]
		public void CheckZipFiles_OneFileEdited()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleFile1, new FileModification(null, FileModificationType.Edited, "Test.Edit", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Zip" });
			string[] files = new[] { exampleFile1, exampleFile2 };

			bool hasCollision = ModCollisionTracker.HasZipCollision(mod, files, exampleFile3, actions, out var collision);

			ClassicAssert.AreEqual(true, hasCollision);
		}

		[Test]
		public void CheckZipFiles_OneFileAdded()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleFile1, new FileModification(null, FileModificationType.Added, "Test.Add", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Zip" });
			string[] files = new[] { exampleFile1, exampleFile2 };

			bool hasCollision = ModCollisionTracker.HasZipCollision(mod, files, exampleFile3, actions, out var collision);

			ClassicAssert.AreEqual(false, hasCollision);
		}

		[Test]
		public void CheckZipFiles_NoFilesModified()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleFile1, new FileModification(null, FileModificationType.Edited, "Test.Example", false));
			actions.AddModification(exampleFile1, new FileModification(null, FileModificationType.Deleted, "Test.Example", false));
			actions.AddModification(exampleFile1, new FileModification(null, FileModificationType.Added, "Test.Example", false));
			actions.AddModification(exampleFile1, new FileModification(exampleDestination1, FileModificationType.Moved, "Test.Example", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Zip" });
			string[] files = new[] { exampleFile2 };

			bool hasCollision = ModCollisionTracker.HasZipCollision(mod, files, exampleFile3, actions, out var collision);

			ClassicAssert.AreEqual(false, hasCollision);
		}

		// Unzip

		[Test]
		public void CheckUnzipFileWithDelete_SamePath()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleFile1, new FileModification(null, FileModificationType.Deleted, "Test.Delete", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Unzip" });
			bool hasCollision = ModCollisionTracker.HasUnzipCollision(mod, exampleFile1, actions, out var collision);

			ClassicAssert.AreEqual(true, hasCollision);
		}

		[Test]
		public void CheckUnzipFileWithReplace_SamePath()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleFile1, new FileModification(null, FileModificationType.Replaced, "Test.Replace", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Unzip" });
			bool hasCollision = ModCollisionTracker.HasUnzipCollision(mod, exampleFile1, actions, out var collision);

			ClassicAssert.AreEqual(true, hasCollision);
		}

		[Test]
		public void CheckUnzipFileWithEdit_SamePath()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleFile1, new FileModification(null, FileModificationType.Edited, "Test.Edit", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Unzip" });
			bool hasCollision = ModCollisionTracker.HasUnzipCollision(mod, exampleFile1, actions, out var collision);

			ClassicAssert.AreEqual(true, hasCollision);
		}

		[Test]
		public void CheckUnzipFileWithMove_SamePath()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleFile1, new FileModification(exampleDestination1, FileModificationType.Moved, "Test.Move", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Replace2" });
			bool hasCollision = ModCollisionTracker.HasUnzipCollision(mod, exampleFile1, actions, out var collision);

			ClassicAssert.AreEqual(true, hasCollision);
		}

		[Test]
		public void CheckUnzipFileWithAdd_SamePath()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleFile1, new FileModification(null, FileModificationType.Added, "Test.Add", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Unzip" });
			bool hasCollision = ModCollisionTracker.HasUnzipCollision(mod, exampleFile1, actions, out var collision);

			ClassicAssert.AreEqual(false, hasCollision);
		}

		// File Copies
			
		[Test]
		public void CheckFileCopyWithAdd_SameSourceFile()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleDestination3_exampleDestination1, new FileModification(null, FileModificationType.Added, "Test.Copy1", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Copy2" });
			bool hasCollision = ModCollisionTracker.HasCopyCollision(mod, exampleDestination1, exampleDestination3_exampleDestination1, actions, out var collision);

			ClassicAssert.AreEqual(false, hasCollision);
		}

		public void CheckFileCopyWithAdd_DifferentSourceFile()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleDestination3_exampleDestination1, new FileModification(null, FileModificationType.Added, "Test.Copy1", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Copy2" });
			bool hasCollision = ModCollisionTracker.HasCopyCollision(mod, exampleDestination2, exampleDestination3_exampleDestination1, actions, out var collision);

			ClassicAssert.AreEqual(true, hasCollision);
			ClassicAssert.AreEqual(collision.severity, ModCollisionSeverity.Clash);
			ClassicAssert.AreEqual(collision.modID, "Test.Copy1");
		}

		public void CheckFileCopyWithReplace_DifferentContents()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleDestination2, new FileModification(null, FileModificationType.Replaced, "Test.Replace", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Copy" });
			bool hasCollision = ModCollisionTracker.HasCopyCollision(mod, exampleDestination1, exampleDestination2, actions, out var collision);

			ClassicAssert.AreEqual(true, hasCollision);
			ClassicAssert.AreEqual(collision.severity, ModCollisionSeverity.Clash);
			ClassicAssert.AreEqual(collision.modID, "Test.Replace");
		}

		public void CheckFileCopyWithReplace_SameContents()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleDestination3_exampleDestination1, new FileModification(null, FileModificationType.Replaced, "Test.Replace", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Copy" });
			bool hasCollision = ModCollisionTracker.HasCopyCollision(mod, exampleDestination1, exampleDestination3_exampleDestination1, actions, out var collision);

			ClassicAssert.AreEqual(false, hasCollision);
		}

		public void CheckFileCopyWithDelete()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleDestination2, new FileModification(null, FileModificationType.Deleted, "Test.Delete", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Copy" });
			bool hasCollision = ModCollisionTracker.HasCopyCollision(mod, exampleDestination1, exampleDestination2, actions, out var collision);

			ClassicAssert.AreEqual(false, hasCollision);
		}

		public void CheckFileCopyWithMove()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleDestination1, new FileModification(exampleDestination2, FileModificationType.Moved, "Test.Add", false));
			actions.AddModification(exampleDestination2, new FileModification(null, FileModificationType.Added, "Test.Move", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Copy" });
			bool hasCollision = ModCollisionTracker.HasCopyCollision(mod, exampleDestination1, exampleDestination2, actions, out var collision);

			ClassicAssert.AreEqual(true, hasCollision);
			ClassicAssert.AreEqual(collision.severity, ModCollisionSeverity.Clash);
			ClassicAssert.AreEqual(collision.modID, "Test.Add");
		}

		public void CheckFileCopyWithEdit_DifferentContents()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleDestination2, new FileModification(null, FileModificationType.Edited, "Test.Edit", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Copy" });
			bool hasCollision = ModCollisionTracker.HasCopyCollision(mod, exampleDestination1, exampleDestination2, actions, out var collision);

			ClassicAssert.AreEqual(true, hasCollision);
			ClassicAssert.AreEqual(collision.severity, ModCollisionSeverity.Clash);
			ClassicAssert.AreEqual(collision.modID, "Test.Edit");
		}

		public void CheckFileCopyWithEdit_SameContents()
		{
			var actions = new FileModificationCache();
			actions.AddModification(exampleDestination3_exampleDestination1, new FileModification(null, FileModificationType.Edited, "Test.Edit", false));

			var mod = GetModInstallationInfo(new ModConfiguration() { ModID = "Test.Copy" });
			bool hasCollision = ModCollisionTracker.HasCopyCollision(mod, exampleDestination1, exampleDestination3_exampleDestination1, actions, out var collision);

			ClassicAssert.AreEqual(false, hasCollision);
		}
	}
}
