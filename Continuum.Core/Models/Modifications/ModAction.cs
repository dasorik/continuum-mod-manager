using Continuum.Core.InstallActions;
using Continuum.Core.Extension;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Continuum.Core.Models
{
	public class ModActionLink<T>
		where T : ModInstallAction
	{
		public readonly ModInstallInfo mod;
		public readonly T action;

		public ModActionLink(ModInstallInfo mod, T action)
		{
			this.mod = mod;
			this.action = action;
		}
	}

	public class ModActionCollection
	{
		public List<ModActionLink<QuickBMSExtractAction>> extractActions = new List<ModActionLink<QuickBMSExtractAction>>();
		public List<ModActionLink<UnluacDecompileAction>> decompileActions = new List<ModActionLink<UnluacDecompileAction>>();
		public List<ModActionLink<UnzipFilesAction>> unzipFilesActions = new List<ModActionLink<UnzipFilesAction>>();
		public List<ModActionLink<UnzipFileAction>> unzipFileActions = new List<ModActionLink<UnzipFileAction>>();
		public List<ModActionLink<ZipFilesAction>> zipFilesActions = new List<ModActionLink<ZipFilesAction>>();
		public List<ModActionLink<ZipDirectoryAction>> zipDirectoryActions = new List<ModActionLink<ZipDirectoryAction>>();
		public List<ModActionLink<MoveFileAction>> fileMoveActions = new List<ModActionLink<MoveFileAction>>();
		public List<ModActionLink<MoveFilesAction>> bulkFileMoveActions = new List<ModActionLink<MoveFilesAction>>();
		public List<ModActionLink<DeleteFilesAction>> fileDeleteActions = new List<ModActionLink<DeleteFilesAction>>();
		public List<ModActionLink<WriteToFileAction>> fileWriteActions = new List<ModActionLink<WriteToFileAction>>();
		public List<ModActionLink<ReplaceFileAction>> fileReplaceActions = new List<ModActionLink<ReplaceFileAction>>();
		public List<ModActionLink<ReplaceFilesAction>> bulkFileReplaceActions = new List<ModActionLink<ReplaceFilesAction>>();
		public List<ModActionLink<CopyFileAction>> fileCopyActions = new List<ModActionLink<CopyFileAction>>();
		public List<ModActionLink<CopyFilesAction>> bulkFileCopyActions = new List<ModActionLink<CopyFilesAction>>();

		public void AddActions(ModInstallInfo mod)
		{
			var installActions = mod.Config.InstallActions ?? new ModInstallAction[] { };
			AddActions(mod, installActions);
		}

		public void AddActions(ModInstallInfo mod, ModInstallAction[] actions)
		{
			extractActions.AddRange(GetActions<QuickBMSExtractAction>(actions, mod));
			decompileActions.AddRange(GetActions<UnluacDecompileAction>(actions, mod));
			unzipFilesActions.AddRange(GetActions<UnzipFilesAction>(actions, mod));
			unzipFileActions.AddRange(GetActions<UnzipFileAction>(actions, mod));
			zipFilesActions.AddRange(GetActions<ZipFilesAction>(actions, mod));
			zipDirectoryActions.AddRange(GetActions<ZipDirectoryAction>(actions, mod));
			fileMoveActions.AddRange(GetActions<MoveFileAction>(actions, mod));
			bulkFileMoveActions.AddRange(GetActions<MoveFilesAction>(actions, mod));
			fileDeleteActions.AddRange(GetActions<DeleteFilesAction>(actions, mod));
			fileWriteActions.AddRange(GetActions<WriteToFileAction>(actions, mod));
			fileReplaceActions.AddRange(GetActions<ReplaceFileAction>(actions, mod));
			bulkFileReplaceActions.AddRange(GetActions<ReplaceFilesAction>(actions, mod));
			fileCopyActions.AddRange(GetActions<CopyFileAction>(actions, mod));
			bulkFileCopyActions.AddRange(GetActions<CopyFilesAction>(actions, mod));
		}

		private IEnumerable<ModActionLink<T>> GetActions<T>(ModInstallAction[] actions, ModInstallInfo mod)
			where T : ModInstallAction
		{
			return actions.Where(a => a.GetType() == typeof(T) && !ActionDisabled(a, mod)).Select(a => new ModActionLink<T>(mod, a as T));
		}

		private bool ActionDisabled(ModInstallAction action, ModInstallInfo mod)
		{
			return !string.IsNullOrWhiteSpace(action.Disabled) && mod.PredicateMatch(action.Disabled) == PredicateMatchType.MatchesPredicate;
		}
	}
}
