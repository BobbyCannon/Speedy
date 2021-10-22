#region References

using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests.Protocols
{
	[TestClass]
	public class ExtensionTests
	{
		#region Methods

		[TestMethod]
		public void CalculateCrc16()
		{
			var actual = Encoding.UTF8.GetBytes("123456789").CalculateCrc16();
			actual.ToString("X4").Dump();
			Assert.AreEqual(0x2189, actual);
		}

		[TestMethod]
		public void EscapeString()
		{
			var items = GetItems();

			foreach (var item in items)
			{
				item.Key.Escape().Dump();
				Assert.AreEqual(item.Key, item.Value.Escape());
			}

			// One way escapes
			items = new List<(string, string)>
			{
				("\\'", "'"),
				("John\\\'s Tavern", "John's Tavern")
			};

			foreach (var item in items)
			{
				item.Key.Escape().Dump();
				Assert.AreEqual(item.Key, item.Value.Escape());
			}
		}

		[TestMethod]
		public void UnescapeString()
		{
			var items = GetItems();

			foreach (var item in items)
			{
				item.Key.Escape().Dump();
				Assert.AreEqual(item.Value, item.Key.Unescape());
			}

			// One way unescapes
			items = new List<(string, string)>
			{
				("\\u0000", "\0"),
				("\\'", "\'")
			};

			foreach (var item in items)
			{
				item.Key.Escape().Dump();
				Assert.AreEqual(item.Value, item.Key.Unescape());
			}
		}

		private static List<(string Key, string Value)> GetItems()
		{
			return new List<(string, string)>
			{
				("\\0", "\0"),
				("\\a", "\a"),
				("\\b", "\b"),
				("\\f", "\f"),
				("\\n", "\n"),
				("\\r", "\r"),
				("\\t", "\t"),
				("\\v", "\v"),
				("\\'", "\'"),
				("\\\"", "\""),
				("\\u0001", "\u0001"),
				("\\uffff", "\uFFFF"),
				("\\u1234", "\x1234"),
				("\\u0001", "\x1"),
				("\\u0012", "\x12"),
				("\\u0123", "\x123"),
				("\\u000e", "\xE"),
				("\\u00ec", "\xEC"),
				("\\u0daf", "\xDAF"),
				("\\u000f", "\xF"),
				("\\u00ef", "\xEF"),
				("\\u0cba", "\xCBA"),
				("\\udcba", "\xDCBA"),
				("\\udcbaF", "\xDCBAF"),
				("John\\'s Tavern", "John\'s Tavern")
			};
		}

		#endregion
	}
}