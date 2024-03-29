#region References

using System;
using System.Collections.Generic;
using Speedy.Profiling;

#endregion

namespace Speedy.UnitTests;

/// <summary>
/// todo: move this to a single test of all cloneable items.
/// </summary>
public class CloneableSpeedUnitTests : SpeedyUnitTest
{
	#region Methods

	internal void BaseShouldCloneTest<T>(IEnumerable<T> testItems, Action<T, T> additionalDeepCloneValidations = null) where T : ICloneable
	{
		foreach (var testItem in testItems)
		{
			// ReSharper disable once InvokeAsExtensionMethod
			var (deepClone, timer2) = Timer.Create(() => (T) testItem.DeepClone());
			var (shallowClone, timer3) = Timer.Create(() => (T) testItem.ShallowClone());

			AreEqual(testItem, deepClone);
			AreEqual(testItem, shallowClone);

			// Shallow clone will not work with deep clone validations
			additionalDeepCloneValidations?.Invoke(testItem, deepClone);
		}
	}

	#endregion
}