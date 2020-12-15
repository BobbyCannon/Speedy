#region References

using System;

#endregion

namespace Speedy.Logging
{
	/// <summary>
	/// Represents a value for an event.
	/// </summary>
	public class EventValue
	{
		#region Constructors

		/// <summary>
		/// Instantiates a new instance of the class.
		/// </summary>
		public EventValue()
		{
		}

		/// <summary>
		/// Instantiates a new instance of the class.
		/// </summary>
		public EventValue(string name, object value)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name), "The name cannot be null.");
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("The name is required.", nameof(name));
			}

			Name = name;
			Value = value?.ToString() ?? string.Empty;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		public string Value { get; set; }

		#endregion
	}
}