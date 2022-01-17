using Continuum.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Continuum.GUI.Services
{
	public class ToastService
	{
		List<ToastData> toastList = new List<ToastData>();
		public IEnumerable<ToastData> ToastList => toastList;

		public delegate void ReloadPersistentToastPopupsEvent();
		public event ReloadPersistentToastPopupsEvent OnReloadPersistentToastPopups;

		public delegate void ToastsChangedEvent();
		public event ToastsChangedEvent OnToastsChanged;

		public ToastData AddToast(ToastData toast)
		{
			toastList.Add(toast);

			OnToastsChanged?.Invoke();

			return toast;
		}

		public void RemoveToast(ToastData toast)
		{
			int index = toastList.IndexOf(toast);
			toastList.RemoveAt(index);

			OnToastsChanged?.Invoke();
		}

		private void ClearPersistentNotifications()
		{
			toastList.RemoveAll(t => !t.canClose);
		}

		public void ReloadPersistentToastPopups()
		{
			ClearPersistentNotifications();
			OnReloadPersistentToastPopups?.Invoke();
		}
	}
}
