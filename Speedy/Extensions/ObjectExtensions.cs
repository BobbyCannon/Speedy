#region References

using System;

#endregion

namespace Speedy.Extensions
{
	/// <summary>
	/// Extensions for the string type.
	/// </summary>
	public static class ObjectExtensions
	{
		#region Methods

		/// <summary>
		/// Executes a provided action if the test is successful.
		/// </summary>
		/// <param name="test"> The test to determine action to take. </param>
		/// <param name="action1"> The action to perform if the test is true. </param>
		/// <param name="action2"> The action to perform if the test is false. </param>
		public static void IfThenElse(Func<bool> test, Action action1, Action action2)
		{
			if (test())
			{
				action1();
			}
			else
			{
				action2();
			}
		}

		#endregion
	}
}