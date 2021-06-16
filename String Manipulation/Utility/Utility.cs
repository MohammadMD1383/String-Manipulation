using System;
using System.Globalization;

namespace String_Manipulation.Utility
{
	internal static class Utility
	{
		public static int GetDecimalsCount(this string s)
		{
			var splitted = s.Split('.');
			return splitted.Length < 2 ? 0 : splitted[1].Length;
		}

		public static decimal ToDecimal(this string s) => decimal.Parse(s, CultureInfo.InvariantCulture);
		public static decimal ToDecimal(this double d) => Convert.ToDecimal(d);
		public static decimal GetSmallestDecimal(this string s) => Math.Pow(10, -s.GetDecimalsCount()).ToDecimal();
		public static decimal GetSmallestDecimal(this decimal d) => d.ToString(CultureInfo.InvariantCulture).GetSmallestDecimal();
	}
}