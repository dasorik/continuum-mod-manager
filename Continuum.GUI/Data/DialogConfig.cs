using Continuum.Core.Models;
using Continuum.GUI;
using System;
using System.Threading.Tasks;

public interface IDialog { }

public abstract class BaseDialogConfig : IDialog
{
	public AlertLevel level;
	public string title;
	public string message;
	public bool showIcon;
}

public class AlertDialogConfig : BaseDialogConfig
{
	public string confirmButtonText;
	public Func<Task> confirmAction;
}

public class ConfirmationDialogConfig : BaseDialogConfig
{
	public string confirmButtonText;
	public string cancelButtonText;

	public Func<Task> confirmAction;
	public Func<Task> cancelAction;
}

public class ConfirmationDialogConfig<T> : ConfirmationDialogConfig
{
	new public Func<T, Task> confirmAction;
}

public class GenericDialogConfig : BaseDialogConfig
{
	public string[] buttonText;
	public Func<Task>[] buttonActions;
}

public class ModDetailsConfig : IDialog
{
	public GameIntegration integration;
	public ModConfiguration selectedMod;
	public Func<ModConfiguration, Task> onInstallSelected;
	public Func<Task> onClose;
}

public class LoadingDialogConfig : IDialog
{
	public string title;
	public Continuum.Core.ProgressTracker tracker;
	public int refreshTime = 100;
}

public class SettingsDialogConfig : IDialog
{
	public SettingsTab defaultTab;
	public Func<Task> onClose;
}