#region References

using System;
using System.Collections.Generic;
using System.Text;
using Speedy.Extensions;
using Speedy.Serialization;

#endregion

namespace Speedy.Automation.Tests;

/// <summary>
/// Extension for testing
/// </summary>
public static class TestingExtensions
{
	#region Methods

	/// <summary>
	/// Dump the item to the Console.WriteLine().
	/// </summary>
	/// <param name="item"> The item to dump. </param>
	/// <returns> The string that was dumped. </returns>
	public static string Dump(this object item)
	{
		Console.WriteLine(item);
		return item.ToString();
	}

	/// <summary>
	/// Dump the objects to the Console.WriteLine().
	/// </summary>
	/// <param name="items"> The objects to dump. </param>
	/// <param name="prefix"> On optional prefix to write before each character. </param>
	/// <returns> The items that was dumped. </returns>
	public static string Dump(this IEnumerable<object> items, string prefix = null)
	{
		var builder = new StringBuilder();
		foreach (var item in items)
		{
			if (prefix != null)
			{
				builder.Append(prefix);
			}

			builder.Append(item);
		}
		Console.WriteLine(builder.ToString());
		return builder.ToString();
	}

	/// <summary>
	/// Dump the characters to the Console.WriteLine().
	/// </summary>
	/// <param name="items"> The characters to dump. </param>
	/// <param name="prefix"> On optional prefix to write before each character. </param>
	/// <returns> The items that was dumped. </returns>
	public static string Dump(this char[] items, string prefix = null)
	{
		var builder = new StringBuilder();
		foreach (var item in items)
		{
			if (prefix != null)
			{
				builder.Append(prefix);
			}

			builder.Append(item);
		}
		Console.WriteLine(builder.ToString());
		return builder.ToString();
	}

	/// <summary>
	/// Dump each item to the Console.WriteLine().
	/// </summary>
	/// <param name="items"> The items to dump. </param>
	/// <returns> The items that was dumped. </returns>
	public static void Dump(this IEnumerable<int> items)
	{
		foreach (var i in items)
		{
			Console.Write($"0x{i:X4},");
		}

		Console.WriteLine("");
	}

	/// <summary>
	/// Dump each item to the Console.WriteLine().
	/// </summary>
	/// <param name="item"> The byte array to write. The array will be formatted as a hex string. </param>
	/// <returns> The items that was dumped. </returns>
	public static string Dump(this byte[] item)
	{
		var response = item.ToHexString();
		Console.WriteLine(response);
		return response;
	}

	/// <summary>
	/// Dump the result of the action to the Console.WriteLine().
	/// </summary>
	/// <typeparam name="T"> The type of the item in the list. </typeparam>
	/// <param name="item"> The item to pass to the action. </param>
	/// <param name="action"> The action to process the item. </param>
	public static void Dump<T>(this T item, Func<T, object> action)
	{
		Console.WriteLine(action(item));
	}

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
	/// <param name="prefix"> An optional prefix. </param>
	/// <returns> The item that was dumped. </returns>
	public static T Dump<T>(this T item, string prefix = null)
	{
		if (!string.IsNullOrWhiteSpace(prefix))
		{
			Console.Write(prefix);
		}

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

	/// <summary>
	/// Dump the item to the Console.WriteLine().
	/// </summary>
	/// <param name="item"> The item to dump. </param>
	public static string DumpJson(this object item)
	{
		var json = item.ToRawJson(true);
		Console.WriteLine(json);
		return json;
	}

	#endregion
}