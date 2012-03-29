using System;
using System.Collections.Generic;

namespace SpiderRT
{
	public static class HelperExtensions
	{
		public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
		{
			foreach(var item in enumeration)
			{
				action(item);
			}
		}
	}
}