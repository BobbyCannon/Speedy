#region References

using System;
using System.Collections.Generic;
using Speedy.Extensions;

#endregion

namespace Speedy.Automation.Tests
{
	/// <summary>
	/// Extension for testing
	/// </summary>
	public static class TestingExtensions
	{
		#region Methods

		/// <summary>
		/// Dump each item to the Console.WriteLine().
		/// </summary>
		/// <typeparam name="T"> The type of the item in the list. </typeparam>
		/// <param name="items"> The items to dump. </param>
		/// <returns> The items that was dumped. </returns>
		public static IEnumerable<T> Dump<T>(this IEnumerable<T> items)
		{
			foreach (var item in items)
			{
				item.Dump();
			}

			return items;
		}

		/// <summary>
		/// Dump the item to the Console.WriteLine().
		/// </summary>
		/// <typeparam name="T"> The type of the item. </typeparam>
		/// <param name="item"> The item to dump. </param>
		/// <returns> The item that was dumped. </returns>
		public static T Dump<T>(this T item)
		{
			Console.WriteLine(item);
			return item;
		}

		/// <summary>
		/// Dump the item to the Console.WriteLine().
		/// </summary>
		/// <typeparam name="T"> The type of the item. </typeparam>
		/// <typeparam name="T2"> The type of the item in the collection. </typeparam>
		/// <param name="dictionary"> The dictionary to dump. </param>
		/// <returns> The dictionary that was dumped. </returns>
		public static IDictionary<T, ICollection<T2>> Dump<T, T2>(this IDictionary<T, ICollection<T2>> dictionary)
		{
			foreach (var result in dictionary)
			{
				result.Key.Dump();
				result.Value.ForEach(x => $"\tb.Property(x => x.{x}).IsRequired();".Dump());
			}

			return dictionary;
		}

		#endregion
	}
}