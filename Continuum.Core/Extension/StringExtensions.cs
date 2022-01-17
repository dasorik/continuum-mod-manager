using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Continuum.Core.Extension
{
	public static class StringExtensions
	{
		public static bool SafeEquals(this string text, string comparison, bool ignoreCase = false, bool nullsAreEqual = false)
		{
			if (text == null)
				return nullsAreEqual ? comparison == null : false;

			if (ignoreCase)
				return text.Equals(comparison, StringComparison.InvariantCultureIgnoreCase);
			else
				return text.Equals(comparison);
		}

		public static bool IsNumber(this string text)
		{
			return decimal.TryParse(text, out decimal value);
		}

		public static decimal ToDecimal(this string text)
		{
			if (!decimal.TryParse(text, out decimal value))
				throw new System.Exception("Provided text is not a numeric value");

			return value;
		}
	}
}
