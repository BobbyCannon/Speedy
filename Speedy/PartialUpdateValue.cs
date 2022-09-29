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
		#region Constructors

		/// <summary>
		/// Instantiates a partial update value.
		/// </summary>
		public PartialUpdateValue()
		{
		}

		/// <summary>
		/// Instantiates a partial update value.
		/// </summary>
		/// <param name="name"> The property name of the update. </param>
		/// <param name="value"> The value to set the property to. </param>
		public PartialUpdateValue(string name, object value)
			: this(name, value.GetType(), value)
		{
		}

		/// <summary>
		/// Instantiates a partial update value.
		/// </summary>
		/// <param name="name"> The property name of the update. </param>
		/// <param name="type"> The type for the property. </param>
		/// <param name="value"> The value to set the property to. </param>
		public PartialUpdateValue(string name, Type type, object value)
		{
			Name = name;
			Type = type;
			Value = value;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The name of the member for the update.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The type of the property.
		/// </summary>
		public Type Type { get; set; }

		/// <summary>
		/// The value being apply.
		/// </summary>
		public object Value { get; set; }

		#endregion
	}
}