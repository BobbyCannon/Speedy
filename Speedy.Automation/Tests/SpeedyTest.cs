#region References

using System;
using System.Collections.Generic;
using System.Threading;
using KellermanSoftware.CompareNetObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Speedy.Automation.Tests
{
	/// <summary>
	/// Represents a Speedy test.
	/// </summary>
	public abstract class SpeedyTest
	{
		#region Fields

		private static Action<string> _clipboardProvider;

		#endregion

		#region Methods

		/// <summary>
		/// Validates that the expected and actual are equal.
		/// </summary>
		public static void AreEqual<T>(T expected, T actual, Action<bool> process = null, params string[] membersToIgnore)
		{
			var configuration = new ComparisonConfig { IgnoreObjectTypes = true, MaxDifferences = int.MaxValue };
			configuration.MembersToIgnore.AddRange(membersToIgnore);
			var logic = new CompareLogic(configuration);
			var result = logic.Compare(expected, actual);
			process?.Invoke(result.AreEqual);
			Assert.IsTrue(result.AreEqual, result.ToString());
		}

		/// <summary>
		/// Validates that the expected and actual are equal.
		/// </summary>
		public static void AreEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, bool ignoreCollectionOrder = false, Action<bool> process = null, params string[] membersToIgnore)
		{
			var configuration = new ComparisonConfig { IgnoreObjectTypes = true, MaxDifferences = int.MaxValue, IgnoreCollectionOrder = ignoreCollectionOrder };
			configuration.MembersToIgnore.AddRange(membersToIgnore);
			var logic = new CompareLogic(configuration);
			var result = logic.Compare(expected, actual);
			process?.Invoke(result.AreEqual);
			Assert.IsTrue(result.AreEqual, result.ToString());
		}

		/// <summary>
		/// Validates that the expected and actual are equal.
		/// </summary>
		public static void AreEqual<T>(T[] expected, T[] actual, bool ignoreCollectionOrder = false, Action<bool> process = null, params string[] membersToIgnore)
		{
			var configuration = new ComparisonConfig { IgnoreObjectTypes = true, MaxDifferences = int.MaxValue, IgnoreCollectionOrder = ignoreCollectionOrder };
			configuration.MembersToIgnore.AddRange(membersToIgnore);
			var logic = new CompareLogic(configuration);
			var result = logic.Compare(expected, actual);
			process?.Invoke(result.AreEqual);
			Assert.IsTrue(result.AreEqual, result.ToString());
		}

		/// <summary>
		/// Validates that the expected and actual are equal.
		/// </summary>
		public static void AreEqual<T>(T expected, T actual, params string[] membersToIgnore)
		{
			var configuration = new ComparisonConfig { IgnoreObjectTypes = true, MaxDifferences = int.MaxValue };
			configuration.MembersToIgnore.AddRange(membersToIgnore);
			var logic = new CompareLogic(configuration);
			var result = logic.Compare(expected, actual);
			Assert.IsTrue(result.AreEqual, result.DifferencesString);
		}

		/// <summary>
		/// Validates that the expected and actual are equal with custom message an optional exceptions.
		/// </summary>
		public static void AreEqual<T>(T expected, T actual, Func<string> message, params string[] membersToIgnore)
		{
			var configuration = new ComparisonConfig { IgnoreObjectTypes = true, MaxDifferences = int.MaxValue };
			configuration.MembersToIgnore.AddRange(membersToIgnore);
			var logic = new CompareLogic(configuration);
			var result = logic.Compare(expected, actual);
			Assert.IsTrue(result.AreEqual, message?.Invoke() + Environment.NewLine + result.DifferencesString);
		}

		/// <summary>
		/// Validates that the expected and actual are not equal.
		/// </summary>
		public static void AreNotEqual<T>(T expected, T actual, params string[] membersToIgnore)
		{
			var configuration = new ComparisonConfig { IgnoreObjectTypes = true, MaxDifferences = int.MaxValue };
			configuration.MembersToIgnore.AddRange(membersToIgnore);
			var logic = new CompareLogic(configuration);
			var result = logic.Compare(expected, actual);
			Assert.IsFalse(result.AreEqual, result.DifferencesString);
		}

		/// <summary>
		/// Validates that the expected and actual are not equal.
		/// </summary>
		public static void AreNotEqual<T>(string message, T expected, T actual, params string[] membersToIgnore)
		{
			var configuration = new ComparisonConfig { IgnoreObjectTypes = true, MaxDifferences = int.MaxValue };
			configuration.MembersToIgnore.AddRange(membersToIgnore);
			var logic = new CompareLogic(configuration);
			var result = logic.Compare(expected, actual);
			Assert.IsFalse(result.AreEqual, message + Environment.NewLine + result.DifferencesString);
		}

		/// <summary>
		/// Copy the string version of the value to the clipboard.
		/// </summary>
		/// <typeparam name="T"> The type of the value. </typeparam>
		/// <param name="value"> The object to copy to the clipboard. Calls ToString on the value. </param>
		/// <returns> The value input to allow for method chaining. </returns>
		public T CopyToClipboard<T>(T value)
		{
			var thread = new Thread(() =>
			{
				try
				{
					_clipboardProvider?.Invoke(value?.ToString() ?? string.Empty);
				}
				catch
				{
					// Ignore the clipboard set issue...
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			thread.Join();

			return value;
		}

		/// <summary>
		/// Sets the clipboard provider.
		/// </summary>
		/// <param name="provider"> The provider to be set. </param>
		public static void SetClipboardProvider(Action<string> provider)
		{
			_clipboardProvider = provider;
		}

		#endregion
	}
}