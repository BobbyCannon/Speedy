#region References

using System;
using Speedy.Exceptions;
using Speedy.Extensions;

#endregion

namespace Speedy
{
	/// <summary>
	/// Validation for Partial Update
	/// </summary>
	public class PartialValidation<T> : PartialValidation
	{
		#region Fields

		private Func<T, bool> _validate;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a validation for a partial update.
		/// </summary>
		/// <param name="name"> The name of the partial update. </param>
		public PartialValidation(string name) : base(name)
		{
		}

		#endregion

		#region Methods

		/// <summary>
		/// Set a minimum and maximum for the value.
		/// </summary>
		/// <param name="minimum"> The inclusive minimal value. </param>
		/// <param name="maximum"> The inclusive maximum value. </param>
		/// <returns> The validation for method chaining. </returns>
		public PartialValidation<T> HasMinMaxRange(int minimum, int maximum)
		{
			if (typeof(T) == typeof(string))
			{
				_validate = x => x.ValidateStringRange(minimum, maximum);
			}
			else if (typeof(T) == typeof(int))
			{
				_validate = x => x.ValidateIntRange(minimum, maximum);
			}

			return this;
		}

		/// <summary>
		/// Sets the flag to clear / set a required requirement.
		/// </summary>
		/// <param name="required"> </param>
		/// <returns> The validation for method chaining. </returns>
		public PartialValidation<T> IsRequired(bool required = true)
		{
			Required = required;
			return this;
		}

		/// <summary>
		/// Process a validation against an object.
		/// </summary>
		/// <param name="value"> The object to validate against. Must be of type T. </param>
		public override void Process(object value)
		{
			Process((T) value);
		}

		/// <summary>
		/// Process a validation against an object.
		/// </summary>
		/// <param name="value"> The value to validate against. </param>
		public virtual void Process(T value)
		{
			if (!_validate.Invoke(value))
			{
				throw new ValidationException(Message);
			}
		}

		/// <summary>
		/// The message to throw if the validation fails.
		/// </summary>
		/// <param name="message"> The text of the message that will be thrown if the validation fails. </param>
		/// <returns> The validation for method chaining. </returns>
		public PartialValidation<T> Throws(string message)
		{
			Message = message;
			return this;
		}

		#endregion
	}

	/// <summary>
	/// Validation for Partial Update
	/// </summary>
	public abstract class PartialValidation
	{
		#region Constructors

		/// <summary>
		/// Instantiates a validation for a partial update.
		/// </summary>
		protected PartialValidation(string name)
		{
			Name = name;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The text of the message that will be thrown if the validation fails.
		/// </summary>
		public string Message { get; protected set; }

		/// <summary>
		/// The name of the validation.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets or sets the flag to clear / set a required requirement.
		/// </summary>
		public bool Required { get; protected set; }

		#endregion

		#region Methods

		/// <summary>
		/// Process a value through an update validation.
		/// </summary>
		/// <param name="value"> The value to test against a validation. </param>
		public abstract void Process(object value);

		#endregion
	}
}