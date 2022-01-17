using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Continuum.GUI
{
	public class ToastData : IEquatable<ToastData>
	{
		public Guid id = Guid.NewGuid();
		public string message;
		public AlertLevel level;
		public bool canClose;
		float? duration;
		public ToastDataContextButton contextButton;

		public ToastData()
		{

		}

		public ToastData(Guid? id, string message, AlertLevel level, float? duration, bool canClose, ToastDataContextButton contextButton)
		{
			this.id = id ?? Guid.NewGuid();
			this.message = message;
			this.level = level;
			this.canClose = canClose;
			this.duration = duration;
			this.contextButton = contextButton;
		}

		public ToastData(Guid id, string message, AlertLevel level, float? duration, bool canClose, ToastDataContextButton contextButton)
		{
			this.id = id;
			this.message = message;
			this.level = level;
			this.canClose = canClose;
			this.duration = duration;
			this.contextButton = contextButton;
		}

		public ToastData(string message, AlertLevel level, float? duration, bool canClose, ToastDataContextButton contextButton)
		{
			this.id = Guid.NewGuid();
			this.message = message;
			this.level = level;
			this.canClose = canClose;
			this.duration = duration;
			this.contextButton = contextButton;
		}

		public bool Equals([AllowNull] ToastData other)
		{
			return id.Equals(other.id);
		}
	}

	public class ToastDataContextButton
	{
		public Action action;
		public string text;

		public ToastDataContextButton(Action action, string text)
		{
			this.action = action;
			this.text = text;
		}
	}
}
