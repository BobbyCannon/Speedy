#region References

using System;

#endregion

namespace Speedy.Validation
{
	/// <summary>
	/// Represents a validation for an object.
	/// </summary>
	/// <typeparam name="T"> The type of the object to validate. </typeparam>
	public class Validation<T> : IValidation
	{
		#region Fields

		private readonly Func<T, bool> _validate;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of a validator.
		/// </summary>
		/// <param name="name"> The name of the validator. </param>
		/// <param name="message"> The message for failed validation. </param>
		/// <param name="validate"> The function to validate an object. </param>
		internal Validation(string name, string message, Func<T, bool> validate)
		{
			_validate = validate;

			Name = name;
			Message = message;
		}

		#endregion

		#region Properties

		/// <inheritdoc />
		public string Message { get; }

		/// <inheritdoc />
		public string Name { get; }

		#endregion

		#region Methods

		/// <inheritdoc />
		public bool TryValidate(object value)
		{
			if (value is not T tValue)
			{
				return false;
			}

			return _validate?.Invoke(tValue) ?? false;
		}

		/// <summary>
		/// Tries to validate
		/// </summary>
		/// <returns> Returns true if the validation passes otherwise false. </returns>
		public bool TryValidate(T value)
		{
			return _validate?.Invoke(value) ?? false;
		}

		#endregion
	}
}