#region References

using System;
using System.Collections.Generic;
using Speedy.Profiling;
using ICloneable = Speedy.ICloneable;

#endregion

namespace Speedy.UnitTests
{
    public static class CloneableHelper
	{
		#region Methods

		internal static void BaseShouldCloneTest<T>(IEnumerable<T> testItems, Action<T, T> additionalDeepCloneValidations = null) where T : ICloneable
		{
			foreach (var testItem in testItems)
			{
				// ReSharper disable once InvokeAsExtensionMethod
				var (deepClone, timer2) = Timer.Create(() => (T) testItem.DeepClone());
				var (shallowClone, timer3) = Timer.Create(() => (T) testItem.ShallowClone());

				TestHelper.AreEqual(testItem, deepClone);
				TestHelper.AreEqual(testItem, shallowClone);

				// Shallow clone will not work with deep clone validations
				additionalDeepCloneValidations?.Invoke(testItem, deepClone);
			}
		}

		#endregion
	}
}