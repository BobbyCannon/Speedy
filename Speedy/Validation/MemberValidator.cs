#region References

using System;
using System.Collections.Generic;
using System.Reflection;

#endregion

namespace Speedy.Validation
{
	/// <summary>
	/// Validation for an object member.
	/// </summary>
	public class MemberValidator<T> : MemberValidator
	{
		#region Constructors

		/// <summary>
		/// Creates an instance of a member validator.
		/// </summary>
		internal MemberValidator(MemberInfo info) : base(info)
		{
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public override MemberValidator AreEqual(object value, string message)
		{
			var validation = new Validation<object>(this, message, x => Equals(x, value));
			Validations.Add(validation);
			return this;
		}

		/// <inheritdoc />
		public override MemberValidator HasMinMaxRange(int minimum, int maximum, string message)
		{
			if (typeof(T) == typeof(int))
			{
				return AddMinMaxRange<int>(x => (x >= minimum) && (x <= maximum), message);
			}
			if (typeof(T) == typeof(uint))
			{
				return AddMinMaxRange<uint>(x => (x >= minimum) && (x <= maximum), message);
			}
			if (typeof(T) == typeof(string))
			{
				return AddMinMaxRange<string>(x => (x.Length >= minimum) && (x.Length <= maximum), message);
			}

			throw new NotSupportedException($"The type is not supported for {nameof(HasMinMaxRange)}.");
		}

		/// <inheritdoc />
		public override MemberValidator IsRequired(bool required, string message)
		{
			MemberRequired = required;

			if (MemberRequired == false)
			{
				return this;
			}

			if (typeof(T).IsValueType)
			{
				var validation = new Validation<T>(this, message, x => !Equals(x, default(T)));
				Validations.Add(validation);
				return this;
			}
			else
			{
				var validation = new Validation<T>(this, message, x => x != null);
				Validations.Add(validation);
				return this;
			}
		}

		/// <inheritdoc />
		public override bool TryValidate(object value)
		{
			for (var i = 0; i < Validations.Count; i++)
			{
				if (i >= Validations.Count)
				{
					return true;
				}

				var validation = Validations[i];

				if (!validation.TryValidate(value))
				{
					return false;
				}
			}

			return true;
		}

		private MemberValidator AddMinMaxRange<T1>(Func<T1, bool> validate, string message)
		{
			var validation = new Validation<T1>(this, message, validate);
			Validations.Add(validation);
			return this;
		}

		#endregion
	}

	/// <summary>
	/// Validation for an object member.
	/// </summary>
	public abstract class MemberValidator
	{
		#region Constructors

		/// <summary>
		/// Creates an instance of a member validator.
		/// </summary>
		protected MemberValidator(MemberInfo info)
		{
			Info = info;
			Validations = new List<IValidation>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// The name of the validator
		/// </summary>
		public string Name => Info.Name;

		/// <summary>
		/// The validations for the validator.
		/// </summary>
		public IList<IValidation> Validations { get; }

		/// <summary>
		/// The info for the member.
		/// </summary>
		protected internal MemberInfo Info { get; }

		/// <summary>
		/// Get the required status
		/// </summary>
		/// <returns> </returns>
		protected internal bool MemberRequired { get; protected set; }

		#endregion

		#region Methods

		/// <summary>
		/// Validates that a member matches the provided value.
		/// </summary>
		/// <param name="value"> The value to compare with. </param>
		/// <returns> True if the value is and within the provided range. </returns>
		public MemberValidator AreEqual(object value)
		{
			return AreEqual(value, $"{Info.Name} does not equal the provided value.");
		}

		/// <summary>
		/// Validates that a member matches the provided value.
		/// </summary>
		/// <param name="value"> The value to compare with. </param>
		/// <param name="message"> The message for failed validation. </param>
		/// <returns> True if the value is and within the provided range. </returns>
		public abstract MemberValidator AreEqual(object value, string message);

		/// <summary>
		/// Validate an object within a range.
		/// </summary>
		/// <param name="minimum"> The inclusive minimum value. </param>
		/// <param name="maximum"> The inclusive maximum value. </param>
		/// <returns> True if the value is and within the provided range. </returns>
		public MemberValidator HasMinMaxRange(int minimum, int maximum)
		{
			return HasMinMaxRange(minimum, maximum, $"{Info.Name} is not within the provided min max range values.");
		}

		/// <summary>
		/// Validate an object within a range.
		/// </summary>
		/// <param name="minimum"> The inclusive minimum value. </param>
		/// <param name="maximum"> The inclusive maximum value. </param>
		/// <param name="message"> The message for failed validation. </param>
		/// <returns> True if the value is and within the provided range. </returns>
		public abstract MemberValidator HasMinMaxRange(int minimum, int maximum, string message);

		/// <summary>
		/// Configure this member as not required.
		/// </summary>
		public MemberValidator IsNotRequired()
		{
			return IsRequired(false, string.Empty);
		}

		/// <summary>
		/// Configure this member as not required.
		/// </summary>
		public MemberValidator IsRequired()
		{
			return IsRequired(true, $"{Info.Name} is required but was not provided.");
		}

		/// <summary>
		/// Configure this member as not required.
		/// </summary>
		/// <param name="message"> The message for failed validation. </param>
		public MemberValidator IsRequired(string message)
		{
			return IsRequired(true, message);
		}

		/// <summary>
		/// Validate a member to determine if it is required.
		/// </summary>
		/// <param name="required"> The flag to determine if it is required. </param>
		/// <param name="message"> The message for failed validation. </param>
		public abstract MemberValidator IsRequired(bool required, string message);

		/// <summary>
		/// Tries to validate the property.
		/// </summary>
		/// <returns> Returns true if the validations pass otherwise false. </returns>
		public abstract bool TryValidate(object value);

		/// <summary>
		/// Process the validations.
		/// </summary>
		/// <param name="propertyValue"> The property value. </param>
		/// <param name="failedValidation"> The list of failed validations. </param>
		internal void ProcessValidations(object propertyValue, ICollection<IValidation> failedValidation)
		{
			for (var i = 0; i < Validations.Count; i++)
			{
				if (i >= Validations.Count)
				{
					return;
				}

				var validation = Validations[i];

				if (!validation.TryValidate(propertyValue))
				{
					failedValidation.Add(validation);
				}
			}
		}

		#endregion
	}
}