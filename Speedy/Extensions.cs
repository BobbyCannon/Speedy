#region References

using System.Collections.Generic;

#endregion

namespace Speedy
{
	public static class Extensions
	{
		#region Methods

		public static void AddOrUpdate<T1, T2>(this Dictionary<T1, T2> dictionary, Dictionary<T1, T2> values)
		{
			foreach (var item in values)
			{
				dictionary.AddOrUpdate(item.Key, item.Value);
			}
		}

		public static void AddOrUpdate<T1, T2>(this Dictionary<T1, T2> dictionary, T1 key, T2 value)
		{
			if (dictionary.ContainsKey(key))
			{
				dictionary[key] = value;
				return;
			}

			dictionary.Add(key, value);
		}

		#endregion
	}
}