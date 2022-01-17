using Continuum.Core.Extension;
using Continuum.GUI.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Continuum.GUI.Services
{
	public class DialogService
	{
        Stack<IDialog> dialogStack = new Stack<IDialog>();

        public delegate void DialogChangedEvent();
        public event DialogChangedEvent OnDialogChanged;

        public AlertDialogConfig ShowAlertDialog(string title, string message, AlertLevel level, Func<Task> onClick = null, string buttonText = null, bool showIcon = true)
        {
            var config = new AlertDialogConfig()
            {
                title = title,
                message = message,
                level = level,
                confirmAction = onClick,
                confirmButtonText = buttonText,
                showIcon = showIcon
            };

            config.confirmAction = async () => { Remove(config); await onClick.InvokeSafe(); };

            dialogStack.Push(config);
            OnDialogChanged?.Invoke();

            return config;
        }

        public ConfirmationDialogConfig ShowConfirmDialog(string title, string message, AlertLevel level, Func<Task> confirmAction, Func<Task> cancelAction = null, string confirmText = null, string cancelText = null, bool showIcon = true)
        {
            var config = new ConfirmationDialogConfig()
            {
                title = title,
                message = message,
                level = level,
                confirmButtonText = confirmText,
                cancelButtonText = cancelText,
                showIcon = showIcon
            };

            config.confirmAction = async () => { Remove(config); await confirmAction.InvokeSafe(); };
            config.cancelAction = async () => { Remove(config); await cancelAction.InvokeSafe(); };

            dialogStack.Push(config);
            OnDialogChanged?.Invoke();

            return config;
        }

        public LoadingDialogConfig ShowLoadingDialog(string title, Continuum.Core.ProgressTracker progressTracker, int refreshTime)
		{
            var config = new LoadingDialogConfig()
            {
                title = title,
                tracker = progressTracker,
                refreshTime = refreshTime
            };

            dialogStack.Push(config);
            OnDialogChanged?.Invoke();

            return config;
		}

        public SettingsDialogConfig ShowOptionsPopup(SettingsTab defaultTab, Func<Task> onClose = null)
		{
            var config = new SettingsDialogConfig()
            {
                defaultTab = defaultTab
            };

            config.onClose = async () => { Remove(config); await onClose.InvokeSafe(); };

            dialogStack.Push(config);
            OnDialogChanged?.Invoke();

            return config;
		}

        public ModDetailsConfig ShowModDetailsPopup(Continuum.Core.Models.GameIntegration integration, Continuum.Core.Models.ModConfiguration mod, Func<Continuum.Core.Models.ModConfiguration, Task> onInstallSelected, Func<Task> onClose = null)
        {
            var config = new ModDetailsConfig()
            {
                integration = integration,
                selectedMod = mod,
                onInstallSelected = onInstallSelected
            };

            config.onClose = async () => { Remove(config); await onClose.InvokeSafe(); };

            dialogStack.Push(config);
            OnDialogChanged?.Invoke();

            return config;
        }

        public void Remove(IDialog dialogConfig)
		{
            dialogStack.RemoveItem(dialogConfig);
            OnDialogChanged?.Invoke();
        }

        public void Remove<T>() where T : IDialog
        {
            var config = dialogStack.LastOrDefault(ds => ds.GetType() == typeof(T));

            if (config != null)
                dialogStack.RemoveItem(config);

            OnDialogChanged?.Invoke();
        }

        public bool IsCurrentContext<T>(out T dialogConfig) where T : IDialog
		{
            if (dialogStack.Count == 0)
			{
                dialogConfig = default(T);
                return false;
			}

            if (dialogStack.Peek().GetType() == typeof(T))
			{
                dialogConfig = (T)dialogStack.Peek();
                return true;
            }
            else
			{
                dialogConfig = default(T);
                return false;
			}
		}
    }
}
