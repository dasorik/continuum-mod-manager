using System;
using System.Collections.Generic;
using System.Text;

namespace Continuum.Core.Extension
{
	public static class ObjectExtensions
	{
		public static bool TryCast<T>(this object obj, out T castObj)
		{
			castObj = default(T);

			if (obj == null)
				return false;

			if (obj.GetType() == typeof(T))
			{
				castObj = (T)obj;
				return true;
			}

			return false;
		}
	}
}
