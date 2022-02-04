#region References

using System;

#endregion

namespace Speedy.Extensions
{
	/// <summary>
	/// Extensions for the Type object.
	/// </summary>
	public static class TypeExtensions
	{
		#region Methods

		/// <summary>
		/// Determines if a type is nullable.
		/// </summary>
		/// <param name="type"> The type to be tested. </param>
		/// <returns> True if the type is nullable otherwise false. </returns>
		public static bool IsNullable(this Type type)
		{
			return type.IsClass
				|| !type.IsValueType
				|| (Nullable.GetUnderlyingType(type) != null);
		}

		#endregion
	}
}