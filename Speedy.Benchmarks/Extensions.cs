#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KellermanSoftware.CompareNetObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speed.Benchmarks.Properties;
using Speedy.EntityFramework;

#endregion

namespace Speed.Benchmarks
{
	public static class Extensions
	{
		#region Fields

		private static readonly string[] _loremIpsumWords =
		{
			"lorem", "ipsum", "dolor", "sit", "amet", "consectetuer", "adipiscing", "elit", "sed", "do", "eiusmod", "tempor", "incididunt", "ut", "labore",
			"et", "dolore", "magna", "aliqua", "enim", "ad", "minim", "veniam,", "quis", "nostrud", "exercitation", "ullamco", "laboris", "nisi",
			"aliquip", "ex", "ea", "commodo", "consequat", "duis", "aute", "irure", "in", "reprehenderit", "voluptate", "velit", "esse", "cillum",
			"eu", "fugiat", "nulla", "pariatur", "excepteur", "sint", "occaecat", "cupidatat", "non", "proident", "sunt", "culpa", "qui", "officia", "deserunt",
			"mollit", "anim", "id", "est", "laborum"
		};

		#endregion

		#region Constructors

		static Extensions()
		{
			Random = new Random();
		}

		#endregion

		#region Properties

		public static Random Random { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Compares two objects to see if they are equal.
		/// </summary>
		/// <typeparam name="T"> The type of the object. </typeparam>
		/// <param name="expected"> The item that is expected. </param>
		/// <param name="actual"> The item that is to be tested. </param>
		/// <param name="includeChildren"> True to include child complex types. </param>
		public static void AreEqual<T>(T expected, T actual, bool includeChildren = true)
		{
			var compareObjects = new CompareLogic();
			compareObjects.Config.MaxDifferences = int.MaxValue;
			compareObjects.Config.CompareChildren = includeChildren;
			//compareObjects.Config.MembersToIgnore = new List<string>(new[] { "Id" });

			var result = compareObjects.Compare(expected, actual);
			Assert.IsTrue(result.AreEqual, result.DifferencesString);
		}

		public static T ClearDatabase<T>(this T database) where T : EntityFrameworkDatabase
		{
			database.Database.ExecuteSqlCommand(Resources.ClearDatabase);
			return database;
		}

		public static T GetRandomItem<T>(this IEnumerable<T> collection, T exclude = null) where T : class
		{
			var list = collection.ToList();
			if (!list.Any() || (exclude != null && list.Count == 1))
			{
				return null;
			}

			var index = Random.Next(0, list.Count);

			while (list[index] == exclude)
			{
				index++;

				if (index >= list.Count)
				{
					index = 0;
				}
			}

			return list[index];
		}

		/// <summary>
		/// Create a random string containing the "lorem ipsum" words. This is very useful during testing.
		/// </summary>
		/// <param name="minWords"> </param>
		/// <param name="maxWords"> </param>
		/// <param name="minSentences"> </param>
		/// <param name="maxSentences"> </param>
		/// <param name="numParagraphs"> </param>
		/// <param name="prefix"> </param>
		/// <param name="suffix"> </param>
		/// <returns> </returns>
		public static string LoremIpsum(int minWords = 1, int maxWords = 25, int minSentences = 1, int maxSentences = 10, int numParagraphs = 1, string prefix = "", string suffix = "\r\n")
		{
			// todo: add argument validation;

			var numSentences = Random.Next(minSentences, maxSentences);
			var numWords = Random.Next(minWords, maxWords);
			var result = new StringBuilder();

			for (var p = 0; p < numParagraphs; p++)
			{
				if (prefix.Length > 0)
				{
					result.Append(prefix);
				}

				for (var s = 0; s < numSentences; s++)
				{
					for (var w = 0; w < numWords; w++)
					{
						if (w > 0)
						{
							result.Append(" ");
						}

						result.Append(_loremIpsumWords[Random.Next(0, _loremIpsumWords.Length - 1)]);
					}

					result.Append(". ");
				}

				if (suffix.Length > 0)
				{
					result.Append(suffix);
				}
			}

			if (suffix.Length > 0)
			{
				result.Remove(result.Length - suffix.Length - 1, suffix.Length + 1);
			}

			return result.ToString();
		}

		public static string LoremIpsumWord()
		{
			return _loremIpsumWords[Random.Next(0, _loremIpsumWords.Length - 1)];
		}

		#endregion
	}
}