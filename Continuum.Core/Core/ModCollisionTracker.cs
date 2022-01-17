using Continuum.Core.Enums;
using Continuum.Core.Interfaces;
using Continuum.Core.Models;
using Continuum.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Continuum.Core
{
	public class ModCollisionTracker
	{
		public static bool HasMoveCollision(ModInstallInfo currentMod, string file, string destinationPath, FileModificationCache modifications, out ModCollision collision)
		{
			if (modifications.HasAnyModifications(file))
			{
				FileModification action;
				if (modifications.HasModification(file, FileModificationType.Moved, out action) && destinationPath != action.DestinationPath)
					return AddModCollision(currentMod, ModInstallActionEnum.Move, FileModificationType.Moved, action.ModID, ModCollisionSeverity.Clash, out collision);
				else if (modifications.HasModification(file, FileModificationType.Replaced, out action))
					return AddModCollision(currentMod, ModInstallActionEnum.Move, FileModificationType.Replaced, action.ModID, ModCollisionSeverity.Clash, out collision);
				else if (modifications.HasModification(file, FileModificationType.Edited, out action))
					return AddModCollision(currentMod, ModInstallActionEnum.Move, FileModificationType.Edited, action.ModID, ModCollisionSeverity.Clash, out collision);
				else if (modifications.HasModification(file, FileModificationType.Deleted, out action))
					return AddModCollision(currentMod, ModInstallActionEnum.Move, FileModificationType.Deleted, action.ModID, ModCollisionSeverity.Clash, out collision);
			}

			if (modifications.HasAnyModifications(destinationPath))
			{
				FileModification action;
				string fileMD5 = null, destinationMD5 = null;

				if (modifications.HasModification(destinationPath, FileModificationType.Replaced, out action) && GetMD5(file, ref fileMD5) != GetMD5(destinationPath, ref destinationMD5))
					return AddModCollision(currentMod, ModInstallActionEnum.Move, FileModificationType.Replaced, action.ModID, ModCollisionSeverity.Clash, out collision, suffix: "(with different data)");
				else if (modifications.HasModification(destinationPath, FileModificationType.Edited, out action) && GetMD5(file, ref fileMD5) != GetMD5(destinationPath, ref destinationMD5))
					return AddModCollision(currentMod, ModInstallActionEnum.Move, FileModificationType.Edited, action.ModID, ModCollisionSeverity.Clash, out collision, suffix: "(with different data)");
				else if (modifications.HasModification(destinationPath, FileModificationType.Added, out action) && GetMD5(file, ref fileMD5) != GetMD5(destinationPath, ref destinationMD5))
					return AddModCollision(currentMod, ModInstallActionEnum.Move, FileModificationType.Added, action.ModID, ModCollisionSeverity.Clash, out collision, suffix: "(with different data)");
			}

			collision = null;
			return false;
		}

		public static bool HasCopyCollision(ModInstallInfo currentMod, string file, string destinationPath, FileModificationCache modifications, out ModCollision collision)
		{
			if (modifications.HasAnyModifications(file))
			{
				FileModification action;
				if (modifications.HasModification(file, FileModificationType.Moved, out action) && destinationPath != action.DestinationPath)
					return AddModCollision(currentMod, ModInstallActionEnum.Copy, FileModificationType.Moved, action.ModID, ModCollisionSeverity.Clash, out collision);
				else if (modifications.HasModification(file, FileModificationType.Replaced, out action))
					return AddModCollision(currentMod, ModInstallActionEnum.Copy, FileModificationType.Replaced, action.ModID, ModCollisionSeverity.Clash, out collision);
				else if (modifications.HasModification(file, FileModificationType.Edited, out action))
					return AddModCollision(currentMod, ModInstallActionEnum.Copy, FileModificationType.Edited, action.ModID, ModCollisionSeverity.Clash, out collision);
				else if (modifications.HasModification(file, FileModificationType.Deleted, out action))
					return AddModCollision(currentMod, ModInstallActionEnum.Copy, FileModificationType.Deleted, action.ModID, ModCollisionSeverity.Clash, out collision);
			}

			if (modifications.HasAnyModifications(destinationPath))
			{
				FileModification action;
				string fileMD5 = null, destinationMD5 = null;

				if (modifications.HasModification(destinationPath, FileModificationType.Replaced, out action) && GetMD5(file, ref fileMD5) != GetMD5(destinationPath, ref destinationMD5))
					return AddModCollision(currentMod, ModInstallActionEnum.Copy, FileModificationType.Replaced, action.ModID, ModCollisionSeverity.Clash, out collision, suffix: "(with different data)");
				else if (modifications.HasModification(destinationPath, FileModificationType.Edited, out action) && GetMD5(file, ref fileMD5) != GetMD5(destinationPath, ref destinationMD5))
					return AddModCollision(currentMod, ModInstallActionEnum.Copy, FileModificationType.Edited, action.ModID, ModCollisionSeverity.Clash, out collision, suffix: "(with different data)");
				else if (modifications.HasModification(destinationPath, FileModificationType.Added, out action) && GetMD5(file, ref fileMD5) != GetMD5(destinationPath, ref destinationMD5))
					return AddModCollision(currentMod, ModInstallActionEnum.Copy, FileModificationType.Added, action.ModID, ModCollisionSeverity.Clash, out collision, suffix: "(with different data)");
			}

			collision = null;
			return false;
		}

		public static bool HasReplaceCollision(ModInstallInfo currentMod, string file, string replacementFile, FileModificationCache modifications, out ModCollision collision)
		{
			if (modifications.HasAnyModifications(file))
			{
				FileModification action;
				if (modifications.HasModification(file, FileModificationType.Moved, out action))
					return AddModCollision(currentMod, ModInstallActionEnum.Replace, FileModificationType.Moved, action.ModID, ModCollisionSeverity.Clash, out collision);
				else if (modifications.HasModification(file, FileModificationType.Replaced, out action) && MD5Utility.CalculateMD5Hash(file) != MD5Utility.CalculateMD5Hash(replacementFile))
					return AddModCollision(currentMod, ModInstallActionEnum.Replace, FileModificationType.Replaced, action.ModID, ModCollisionSeverity.Clash, out collision, suffix: "(with different data)");
				else if (modifications.HasModification(file, FileModificationType.Edited, out action))
					return AddModCollision(currentMod, ModInstallActionEnum.Replace, FileModificationType.Edited, action.ModID, ModCollisionSeverity.Clash, out collision);
				else if (modifications.HasModification(file, FileModificationType.Deleted, out action))
					return AddModCollision(currentMod, ModInstallActionEnum.Replace, FileModificationType.Deleted, action.ModID, ModCollisionSeverity.Clash, out collision);
			}

			collision = null;
			return false;
		}

		public static bool HasEditCollision(ModInstallInfo currentMod, string file, IWriteContent content, FileWriter fileWriter, FileModificationCache modifications, out ModCollision collision)
		{
			if (modifications.HasAnyModifications(file))
			{
				FileModification action;
				if (modifications.HasModification(file, FileModificationType.Moved, out action))
					return AddModCollision(currentMod, ModInstallActionEnum.Edit, FileModificationType.Moved, action.ModID, ModCollisionSeverity.Clash, out collision);
				else if (modifications.HasModification(file, FileModificationType.Replaced, out action))
					return AddModCollision(currentMod, ModInstallActionEnum.Edit, FileModificationType.Replaced, action.ModID, ModCollisionSeverity.Clash, out collision);
				else if (modifications.HasModification(file, FileModificationType.Edited, out action) && !fileWriter.CanWrite(file, content))
					return AddModCollision(currentMod, ModInstallActionEnum.Edit, FileModificationType.Edited, action.ModID, ModCollisionSeverity.Clash, out collision);
				else if (modifications.HasModification(file, FileModificationType.Deleted, out action))
					return AddModCollision(currentMod, ModInstallActionEnum.Edit, FileModificationType.Deleted, action.ModID, ModCollisionSeverity.Clash, out collision);
			}

			collision = null;
			return false;
		}

		public static bool HasZipCollision(ModInstallInfo currentMod, IEnumerable<string> files, string destinationPath, FileModificationCache modifications, out ModCollision collision)
		{
			if (modifications.HasAnyModifications(destinationPath))
			{
				FileModification action;
				if (modifications.HasModification(destinationPath, FileModificationType.Added, out action))
					return AddModCollision(currentMod, ModInstallActionEnum.Zip, FileModificationType.Added, action.ModID, ModCollisionSeverity.Clash, out collision);
				else if (modifications.HasModification(destinationPath, FileModificationType.Replaced, out action))
					return AddModCollision(currentMod, ModInstallActionEnum.Zip, FileModificationType.Replaced, action.ModID, ModCollisionSeverity.Clash, out collision);
				else if (modifications.HasModification(destinationPath, FileModificationType.Edited, out action))
					return AddModCollision(currentMod, ModInstallActionEnum.Zip, FileModificationType.Edited, action.ModID, ModCollisionSeverity.Clash, out collision);
			}

			FileModification fileAction;
			foreach (var file in files)
			{
				if (!modifications.HasAnyModifications(file))
					continue;

				// ie. We're attempting to add a file to a .zip that is deleted by another mod in the chain
				if (modifications.HasModification(file, FileModificationType.Deleted, out fileAction))
					return AddModCollision(currentMod, ModInstallActionEnum.Zip, FileModificationType.Deleted, fileAction.ModID, ModCollisionSeverity.Clash, out collision);
				else if (modifications.HasModification(file, FileModificationType.Moved, out fileAction))
					return AddModCollision(currentMod, ModInstallActionEnum.Zip, FileModificationType.Moved, fileAction.ModID, ModCollisionSeverity.Clash, out collision);
				else if (modifications.HasModification(file, FileModificationType.Replaced, out fileAction))
					return AddModCollision(currentMod, ModInstallActionEnum.Zip, FileModificationType.Replaced, fileAction.ModID, ModCollisionSeverity.Clash, out collision);
				else if (modifications.HasModification(file, FileModificationType.Edited, out fileAction))
					return AddModCollision(currentMod, ModInstallActionEnum.Zip, FileModificationType.Edited, fileAction.ModID, ModCollisionSeverity.Clash, out collision);
			}

			collision = null;
			return false;
		}

		public static bool HasUnzipCollision(ModInstallInfo currentMod, string file, FileModificationCache modifications, out ModCollision collision)
		{
			if (modifications.HasAnyModifications(file))
			{
				FileModification action;
				if (modifications.HasModification(file, FileModificationType.Deleted, out action))
					return AddModCollision(currentMod, ModInstallActionEnum.Zip, FileModificationType.Deleted, action.ModID, ModCollisionSeverity.Clash, out collision);
				else if (modifications.HasModification(file, FileModificationType.Moved, out action))
					return AddModCollision(currentMod, ModInstallActionEnum.Zip, FileModificationType.Moved, action.ModID, ModCollisionSeverity.Clash, out collision);
				else if (modifications.HasModification(file, FileModificationType.Replaced, out action))
					return AddModCollision(currentMod, ModInstallActionEnum.Zip, FileModificationType.Replaced, action.ModID, ModCollisionSeverity.Clash, out collision);
				else if (modifications.HasModification(file, FileModificationType.Edited, out action))
					return AddModCollision(currentMod, ModInstallActionEnum.Zip, FileModificationType.Edited, action.ModID, ModCollisionSeverity.Clash, out collision);
			}

			collision = null;
			return false;
		}

		private static bool AddModCollision(ModInstallInfo mod, ModInstallActionEnum action, FileModificationType collisionReason, string collidingModID, ModCollisionSeverity severity, out ModCollision collision, string suffix = "")
		{
			string collisionReasonDescription = GetCollisionDescription(collisionReason);
			string actionDescription = GetModificationDescription(action);

			return AddModCollision(mod, actionDescription, collisionReasonDescription, collidingModID, severity, out collision, suffix: suffix);
		}

		private static bool AddModCollision(ModInstallInfo mod, string actionDescription, FileModificationType collisionReason, string collidingModID, ModCollisionSeverity severity, out ModCollision collision, string suffix = "")
		{
			string collisionReasonDescription = GetCollisionDescription(collisionReason);
			return AddModCollision(mod, actionDescription, collisionReasonDescription, collidingModID, severity, out collision, suffix: suffix);
		}

		private static bool AddModCollision(ModInstallInfo mod, ModInstallActionEnum action, string collisionReasonDescription, string collidingModID, ModCollisionSeverity severity, out ModCollision collision, string suffix = "")
		{
			string actionDescription = GetModificationDescription(action);
			return AddModCollision(mod, actionDescription, collisionReasonDescription, collidingModID, severity, out collision, suffix: suffix);
		}

		private static bool AddModCollision(ModInstallInfo mod, string actionDescription, string collisionReasonDescription, string collidingModID, ModCollisionSeverity severity, out ModCollision collision, string suffix = "")
		{
			string modPrefix = mod != null ? $"Mod collision detected while installing mod ({mod.Config.ModID})" : $"Mod collision detected while applying changes";

			collision = new ModCollision(collidingModID, severity, $"{modPrefix}) Attempting to {actionDescription} that has been {collisionReasonDescription} another mod{(string.IsNullOrEmpty(suffix) ? "" : $" {suffix}")} (conflicting mod - {collidingModID})");
			return true;
		}

		private static string GetMD5(string file, ref string md5)
		{
			if (md5 != null)
				return md5;

			md5 = MD5Utility.CalculateMD5Hash(file);
			return md5;
		}

		private static string GetModificationDescription(ModInstallActionEnum action)
		{
			switch (action)
			{
				case ModInstallActionEnum.Copy:
					return "copy a file";
				case ModInstallActionEnum.Delete:
					return "delete a file";
				case ModInstallActionEnum.Edit:
					return "write to a file";
				case ModInstallActionEnum.Move:
					return "move a file";
				case ModInstallActionEnum.Replace:
					return "replace a file";
				case ModInstallActionEnum.QuickBMS:
					return "QuickBMS extract a file";
				case ModInstallActionEnum.Unzip:
					return "unzip a file";
				case ModInstallActionEnum.Zip:
					return "zip a file/folder";
				case ModInstallActionEnum.Unluac:
					return "Unluac decompile";
			}

			return "perform unknown action to a file";
		}

		private static string GetCollisionDescription(FileModificationType collisionReason)
		{
			switch (collisionReason)
			{
				case FileModificationType.Added:
					return "added by";
				case FileModificationType.Deleted:
					return "deleted by";
				case FileModificationType.Edited:
					return "written to by";
				case FileModificationType.Moved:
					return "moved by";
				case FileModificationType.Replaced:
					return "replaced by";
			}

			return "perform unknown action to a file";
		}
	}
}

