#region References

using System;

#endregion

namespace Speedy
{
	/// <summary>
	/// A value of a partial update.
	/// </summary>
	public class PartialUpdateValue
	{
		#region Properties

		/// <summary>
		/// The path of the update.
		/// </summary>
		public string Path { get; set; }

		/// <summary>
		/// The type of the value being apply.
		/// </summary>
		public object TypeValue { get; set; }

		/// <summary>
		/// The update delegate.
		/// </summary>
		public Delegate Update { get; set; }

		/// <summary>
		/// The value.
		/// </summary>
		public IConvertible Value { get; set; }

		#endregion
	}
}