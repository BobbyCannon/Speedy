#region References

using System;
using System.Collections.Generic;
using System.Reflection;
using Speedy.Converters;
using Speedy.Exceptions;
using Speedy.Extensions;
using Speedy.Protocols.Osc;

#endregion

namespace Speedy.Validation
{
	/// <summary>
	/// Validation for an object property.
	/// </summary>
	public class PropertyValidator<T> : PropertyValidator
	{
		#region Constructors

		/// <summary>
		/// Creates an instance of a property validator.
		/// </summary>
		internal PropertyValidator(PropertyInfo info) : base(info)
		{
		}

		#endregion

		#region Methods

		/// <summary>
		/// Validates that a property matches the provided value.
		/// </summary>
		/// <param name="value"> The value to compare with. </param>
		/// <param name="message"> The message for failed validation. </param>
		public PropertyValidator<T> AreEqual(T value, string message)
		{
			var validation = new Validation<T>(Name, message, x => Equals(x, value));
			Validations.Add(validation);
			return this;
		}

		/// <inheritdoc />
		public override PropertyValidator AreEqual(object value, string message)
		{
			var validation = new Validation<T>(Name, message, x => Equals(x, value));
			Validations.Add(validation);
			return this;
		}

		/// <summary>
		/// Validate a property is within a range.
		/// </summary>
		/// <param name="minimum"> The minimum value. </param>
		/// <param name="maximum"> The maximum value. </param>
		/// <param name="excludeRangeValues"> The option to exclude the minimum and maximum values </param>
		public PropertyValidator<T> HasMinMaxRange(T minimum, T maximum, bool excludeRangeValues = false)
		{
			return (PropertyValidator<T>) AddMinMaxRange(minimum, maximum, 
				ValidationException.GetErrorMessage(ValidationExceptionType.MinMaxRange, Info.Name), excludeRangeValues);
		}

		/// <summary>
		/// Validate a property is within a range.
		/// </summary>
		/// <param name="minimum"> The minimum value. </param>
		/// <param name="maximum"> The maximum value. </param>
		/// <param name="message"> The message for failed validation. </param>
		/// <param name="excludeRangeValues"> The option to exclude the minimum and maximum values </param>
		public PropertyValidator<T> HasMinMaxRange(T minimum, T maximum, string message, bool excludeRangeValues = false)
		{
			return (PropertyValidator<T>) AddMinMaxRange(minimum, maximum, message, excludeRangeValues);
		}

		/// <summary>
		/// Validate a property is within a range.
		/// </summary>
		/// <param name="minimum"> The minimum value. </param>
		/// <param name="maximum"> The maximum value. </param>
		/// <param name="excludeRangeValues"> The option to exclude the minimum and maximum values </param>
		public PropertyValidator<T> HasMinMaxRange(int minimum, int maximum, bool excludeRangeValues = false)
		{
			return (PropertyValidator<T>) AddMinMaxRange(minimum, maximum,
				ValidationException.GetErrorMessage(ValidationExceptionType.MinMaxRange, Info.Name), excludeRangeValues);
		}

		/// <summary>
		/// Validate a property is within a range.
		/// </summary>
		/// <param name="minimum"> The minimum value. </param>
		/// <param name="maximum"> The maximum value. </param>
		/// <param name="message"> The message for failed validation. </param>
		/// <param name="excludeRangeValues"> The option to exclude the minimum and maximum values </param>
		public PropertyValidator<T> HasMinMaxRange(int minimum, int maximum, string message, bool excludeRangeValues = false)
		{
			return (PropertyValidator<T>) AddMinMaxRange(minimum, maximum, message, excludeRangeValues);
		}

		/// <inheritdoc />
		public override PropertyValidator HasMinMaxRange(object minimum, object maximum, bool excludeRangeValues)
		{
			return HasMinMaxRange((T) minimum, (T) maximum, excludeRangeValues);
		}

		/// <summary>
		/// Validate an object within a range.
		/// </summary>
		/// <param name="minimum"> The inclusive minimum value. </param>
		/// <param name="maximum"> The inclusive maximum value. </param>
		/// <param name="message"> The message for failed validation. </param>
		/// <param name="excludeRangeValues"> The option to exclude the minimum and maximum values </param>
		/// <returns> True if the value is and within the expected range. </returns>
		public override PropertyValidator HasMinMaxRange(object minimum, object maximum, string message, bool excludeRangeValues)
		{
			return AddMinMaxRange(minimum, maximum, message, excludeRangeValues);
		}

		/// <summary>
		/// Validates that a property has a valid value.
		/// </summary>
		public PropertyValidator<T> HasEnumValue()
		{
			return HasEnumValue(ValidationException.GetErrorMessage(ValidationExceptionType.EnumRange, Info.Name));
		}

		/// <summary>
		/// Validates that a property has a valid value.
		/// </summary>
		/// <param name="message"> The message for failed validation. </param>
		public PropertyValidator<T> HasEnumValue(string message)
		{
			var underlyingType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
			var validation = new Validation<T>(Name, message, x => Enum.IsDefined(underlyingType, x));
			Validations.Add(validation);
			return this;
		}

		/// <summary>
		/// Validate a property with provided test.
		/// </summary>
		/// <param name="validate"> The test to validate the property. </param>
		/// <param name="message"> The message for failed validation. </param>
		public PropertyValidator<T> IsFalse(Func<T, bool> validate, string message)
		{
			return AddValidation<T>(x => !validate(x), message);
		}

		/// <summary>
		/// Validate a property to ensure it is not null.
		/// </summary>
		public PropertyValidator<T> IsNotNull()
		{
			return IsNotNull(ValidationException.GetErrorMessage(ValidationExceptionType.IsNotNull, Info.Name));
		}

		/// <summary>
		/// Validate a property to ensure it is not null.
		/// </summary>
		/// <param name="message"> The message for failed validation. </param>
		public PropertyValidator<T> IsNotNull(string message)
		{
			var validation = new Validation<T>(Name, message, x => x != null);
			Validations.Add(validation);
			return this;
		}

		/// <summary>
		/// Configure this property as optional.
		/// </summary>
		public PropertyValidator<T> IsOptional()
		{
			return IsOptional(ValidationException.GetErrorMessage(ValidationExceptionType.IsOptional, Info.Name));
		}

		/// <summary>
		/// Configure this property as optional.
		/// </summary>
		/// <param name="message"> The message for failed validation. </param>
		public PropertyValidator<T> IsOptional(string message)
		{
			return IsRequired(false, message);
		}

		/// <summary>
		/// Configure this property as required.
		/// </summary>
		public PropertyValidator<T> IsRequired()
		{
			return IsRequired(ValidationException.GetErrorMessage(ValidationExceptionType.IsRequired, Info.Name));
		}

		/// <summary>
		/// Configure this property as required.
		/// </summary>
		/// <param name="message"> The message for failed validation. </param>
		public PropertyValidator<T> IsRequired(string message)
		{
			return IsRequired(true, message);
		}

		/// <summary>
		/// Validate an property with provided test.
		/// </summary>
		public PropertyValidator<T> IsTrue()
		{
			return IsTrue(ValidationException.GetErrorMessage(ValidationExceptionType.IsTrue, Info.Name));
		}

		/// <summary>
		/// Validate an property with provided test.
		/// </summary>
		/// <param name="message"> The message for failed validation. </param>
		public PropertyValidator<T> IsTrue(string message)
		{
			return IsTrue(x => x is bool vBool && vBool, message);
		}

		/// <summary>
		/// Validate an property with provided test.
		/// </summary>
		/// <param name="validate"> The test to validate the property. </param>
		/// <param name="message"> The message for failed validation. </param>
		public PropertyValidator<T> IsTrue(Func<T, bool> validate, string message)
		{
			return AddValidation(validate, message);
		}

		/// <summary>
		/// Validate an object is equal to or greater than provided minimum value.
		/// </summary>
		/// <param name="minimum"> The inclusive minimum value. </param>
		/// <returns> True if the value is equal to or greater than the provided value. </returns>
		public PropertyValidator<T> NoLessThan(T minimum)
		{
			return NoLessThan(minimum, ValidationException.GetErrorMessage(ValidationExceptionType.NoLessThan, Info.Name));
		}

		/// <summary>
		/// Validate an object is equal to or greater than provided minimum value.
		/// </summary>
		/// <param name="minimum"> The inclusive minimum value. </param>
		/// <param name="message"> The message for failed validation. </param>
		/// <returns> True if the value is equal to or greater than the provided value. </returns>
		public PropertyValidator<T> NoLessThan(T minimum, string message)
		{
			return AddNoLessThan(minimum, message);
		}

		/// <summary>
		/// Validate an object is equal to or less than provided minimum value.
		/// </summary>
		/// <param name="maximum"> The inclusive maximum value. </param>
		/// <returns> True if the value is equal to or less than the provided value. </returns>
		public PropertyValidator<T> NoMoreThan(T maximum)
		{
			return NoMoreThan(maximum, ValidationException.GetErrorMessage(ValidationExceptionType.NoMoreThan, Info.Name));
		}

		/// <summary>
		/// Validate an object is equal to or less than provided minimum value.
		/// </summary>
		/// <param name="maximum"> The inclusive maximum value. </param>
		/// <param name="message"> The message for failed validation. </param>
		/// <returns> True if the value is equal to or less than the provided value. </returns>
		public PropertyValidator<T> NoMoreThan(T maximum, string message)
		{
			return AddNoMoreThan(maximum, message);
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

		private PropertyValidator<T1> AddMinMaxRange<T1>(T1 minimum, T1 maximum, Func<T1, T1, Func<T1, bool>> getValidate, string message)
		{
			return AddValidation(getValidate(minimum, maximum), message);
		}

		private PropertyValidator<T1> AddMinMaxRange<T1, T2>(T2 minimum, T2 maximum, Func<T2, T2, Func<T1, bool>> getValidate, string message)
		{
			return AddValidation(getValidate(minimum, maximum), message);
		}

		/// <summary>
		/// Validate an object within a range.
		/// </summary>
		/// <param name="minimum"> The inclusive minimum value. </param>
		/// <param name="maximum"> The inclusive maximum value. </param>
		/// <param name="message"> The message for failed validation. </param>
		/// <param name="excludeRangeValues"> The option to exclude the minimum and maximum values </param>
		/// <returns> True if the value is and within the expected range. </returns>
		private PropertyValidator AddMinMaxRange(object minimum, object maximum, string message, bool excludeRangeValues)
		{
			if (typeof(T) == typeof(short))
			{
				return AddMinMaxRange(minimum.ToInt16(), maximum.ToInt16(), (min, max) => excludeRangeValues ? x => (x > min) && (x < max) : x => (x >= min) && (x <= max), message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(ushort))
			{
				return AddMinMaxRange(minimum.ToUInt16(), maximum.ToUInt16(), (min, max) => excludeRangeValues ? x => (x > min) && (x < max) : x => (x >= min) && (x <= max), message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(int))
			{
				return AddMinMaxRange(minimum.ToInt32(), maximum.ToInt32(), (min, max) => excludeRangeValues ? x => (x > min) && (x < max) : x => (x >= min) && (x <= max), message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(uint))
			{
				return AddMinMaxRange(minimum.ToUInt32(), maximum.ToUInt32(), (min, max) => excludeRangeValues ? x => (x > min) && (x < max) : x => (x >= min) && (x <= max), message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(long))
			{
				return AddMinMaxRange(minimum.ToInt64(), maximum.ToInt64(), (min, max) => excludeRangeValues ? x => (x > min) && (x < max) : x => (x >= min) && (x <= max), message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(ulong))
			{
				return AddMinMaxRange(minimum.ToUInt64(), maximum.ToUInt64(), (min, max) => excludeRangeValues ? x => (x > min) && (x < max) : x => (x >= min) && (x <= max), message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(float))
			{
				return AddMinMaxRange(minimum.ToFloat(), maximum.ToFloat(), (min, max) => excludeRangeValues ? x => (x > min) && (x < max) : x => (x >= min) && (x <= max), message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(double))
			{
				return AddMinMaxRange(minimum.ToDouble(), maximum.ToDouble(), (min, max) => excludeRangeValues ? x => (x > min) && (x < max) : x => (x >= min) && (x <= max), message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(decimal))
			{
				return AddMinMaxRange(minimum.ToDecimal(), maximum.ToDecimal(), (min, max) => excludeRangeValues
						? x => (x > min) && (x < max)
						: x => (x >= min) && (x <= max)
					, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(byte))
			{
				return AddMinMaxRange(minimum.ToByte(), maximum.ToByte(), (min, max) => excludeRangeValues ? x => (x > min) && (x < max) : x => (x >= min) && (x <= max), message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(sbyte))
			{
				return AddMinMaxRange(minimum.ToSByte(), maximum.ToSByte(), (min, max) => excludeRangeValues ? x => (x > min) && (x < max) : x => (x >= min) && (x <= max), message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(char))
			{
				return AddMinMaxRange(minimum.ToChar(), maximum.ToChar(), (min, max) => excludeRangeValues ? x => (x > min) && (x < max) : x => (x >= min) && (x <= max), message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(string))
			{
				return AddMinMaxRange<string, int>(minimum.ToInt32(), maximum.ToInt32(),
					(min, max) => excludeRangeValues
						? x => (x != null) && (x.Length > min) && (x.Length < max)
						: x => (x != null) && (x.Length >= min) && (x.Length <= max)
					, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(DateTime))
			{
				return AddMinMaxRange(minimum.ToDateTime(), maximum.ToDateTime(), (min, max) => excludeRangeValues ? x => (x > min) && (x < max) : x => (x >= min) && (x <= max), message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(DateTimeOffset))
			{
				return AddMinMaxRange(minimum.ToDateTimeOffset(), maximum.ToDateTimeOffset(), (min, max) => excludeRangeValues ? x => (x > min) && (x < max) : x => (x >= min) && (x <= max), message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(OscTimeTag))
			{
				return AddMinMaxRange(minimum.ToOscTimeTag(), maximum.ToOscTimeTag(), (min, max) => excludeRangeValues ? x => (x > min) && (x < max) : x => (x >= min) && (x <= max), message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(TimeSpan))
			{
				return AddMinMaxRange(minimum.ToTimeSpan(), maximum.ToTimeSpan(), (min, max) => excludeRangeValues ? x => (x > min) && (x < max) : x => (x >= min) && (x <= max), message) as PropertyValidator<T>;
			}

			throw new NotSupportedException($"The type is not supported for {nameof(AddMinMaxRange)}.");
		}

		private PropertyValidator<T1> AddNoLessThan<T1>(T1 minimum, Func<T1, Func<T1, bool>> getValidate, string message)
		{
			return AddNoLessThan<T1, T1>(minimum, getValidate, message);
		}

		private PropertyValidator<T1> AddNoLessThan<T1, T2>(object minimum, Func<T2, Func<T1, bool>> getValidate, string message)
		{
			if (minimum is not T2 tMinimum)
			{
				throw new InvalidCastException("Minimum value is the incorrect type.");
			}

			return AddValidation(getValidate(tMinimum), message);
		}

		/// <summary>
		/// Validate an object is equal to or greater than provided minimum value.
		/// </summary>
		/// <param name="minimum"> The inclusive minimum value. </param>
		/// <param name="message"> The message for failed validation. </param>
		/// <returns> True if the value is equal to or greater than the provided value. </returns>
		private PropertyValidator<T> AddNoLessThan(object minimum, string message)
		{
			if (typeof(T) == typeof(short))
			{
				return AddNoLessThan(minimum.ToInt16(), min => x => x >= min, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(ushort))
			{
				return AddNoLessThan(minimum.ToUInt16(), min => x => x >= min, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(int))
			{
				return AddNoLessThan(minimum.ToInt32(), min => x => x >= min, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(uint))
			{
				return AddNoLessThan(minimum.ToUInt32(), min => x => x >= min, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(long))
			{
				return AddNoLessThan(minimum.ToInt64(), min => x => x >= min, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(ulong))
			{
				return AddNoLessThan(minimum.ToUInt64(), min => x => x >= min, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(float))
			{
				return AddNoLessThan(minimum.ToFloat(), min => x => x >= min, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(double))
			{
				return AddNoLessThan(minimum.ToDouble(), min => x => x >= min, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(decimal))
			{
				return AddNoLessThan(minimum.ToDecimal(), min => x => x >= min, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(byte))
			{
				return AddNoLessThan(minimum.ToByte(), min => x => x >= min, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(sbyte))
			{
				return AddNoLessThan(minimum.ToSByte(), min => x => x >= min, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(char))
			{
				return AddNoLessThan(minimum.ToChar(), min => x => x >= min, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(string))
			{
				return AddNoLessThan<string, int>(minimum.ToInt32(), min => x => x.Length >= min, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(DateTime))
			{
				return AddNoLessThan(minimum.ToDateTime(), min => x => x >= min, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(DateTimeOffset))
			{
				return AddNoLessThan(minimum.ToDateTimeOffset(), min => x => x >= min, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(OscTimeTag))
			{
				return AddNoLessThan(minimum.ToOscTimeTag(), min => x => x >= min, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(TimeSpan))
			{
				return AddNoLessThan(minimum.ToTimeSpan(), min => x => x >= min, message) as PropertyValidator<T>;
			}

			throw new NotSupportedException($"The type is not supported for {nameof(AddNoLessThan)}.");
		}

		private PropertyValidator<T1> AddNoMoreThan<T1>(object maximum, Func<T1, Func<T1, bool>> getValidate, string message)
		{
			return AddNoMoreThan<T1, T1>(maximum, getValidate, message);
		}

		private PropertyValidator<T1> AddNoMoreThan<T1, T2>(object maximum, Func<T2, Func<T1, bool>> getValidate, string message)
		{
			if (maximum is not T2 tMaximum)
			{
				throw new InvalidCastException("Maximum value is the incorrect type.");
			}

			return AddValidation(getValidate(tMaximum), message);
		}

		/// <summary>
		/// Validate an object is equal to or less than provided minimum value.
		/// </summary>
		/// <param name="maximum"> The inclusive maximum value. </param>
		/// <param name="message"> The message for failed validation. </param>
		/// <returns> True if the value is equal to or less than the provided value. </returns>
		private PropertyValidator<T> AddNoMoreThan(object maximum, string message)
		{
			if (typeof(T) == typeof(short))
			{
				return AddNoMoreThan<short>(maximum, max => x => x <= max, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(ushort))
			{
				return AddNoMoreThan<ushort>(maximum, max => x => x <= max, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(int))
			{
				return AddNoMoreThan<int>(maximum, max => x => x <= max, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(uint))
			{
				return AddNoMoreThan<uint>(maximum, max => x => x <= max, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(long))
			{
				return AddNoMoreThan<long>(maximum, max => x => x <= max, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(ulong))
			{
				return AddNoMoreThan<ulong>(maximum, max => x => x <= max, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(float))
			{
				return AddNoMoreThan<float>(maximum, max => x => x <= max, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(double))
			{
				return AddNoMoreThan<double>(maximum, max => x => x <= max, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(decimal))
			{
				return AddNoMoreThan<decimal>(maximum, max => x => x <= max, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(byte))
			{
				return AddNoMoreThan<byte>(maximum, max => x => x <= max, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(sbyte))
			{
				return AddNoMoreThan<sbyte>(maximum, max => x => x <= max, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(char))
			{
				return AddNoMoreThan<char>(maximum, max => x => x <= max, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(string))
			{
				return AddNoMoreThan<string, int>(maximum, max => x => x.Length <= max, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(DateTime))
			{
				return AddNoMoreThan<DateTime>(maximum, max => x => x <= max, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(DateTimeOffset))
			{
				return AddNoMoreThan<DateTimeOffset>(maximum, max => x => x <= max, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(OscTimeTag))
			{
				return AddNoMoreThan<OscTimeTag>(maximum, max => x => x <= max, message) as PropertyValidator<T>;
			}
			if (typeof(T) == typeof(TimeSpan))
			{
				return AddNoMoreThan<TimeSpan>(maximum, max => x => x <= max, message) as PropertyValidator<T>;
			}

			throw new NotSupportedException($"The type is not supported for {nameof(AddNoMoreThan)}.");
		}

		private PropertyValidator<T1> AddValidation<T1>(Func<T1, bool> validate, string message)
		{
			var validation = new Validation<T1>(Name, message, validate);
			Validations.Add(validation);
			return this as PropertyValidator<T1>;
		}

		/// <summary>
		/// Validate a property to determine if it is required.
		/// </summary>
		/// <param name="required"> The flag to determine if it is required. </param>
		/// <param name="message"> The message for failed validation. </param>
		private PropertyValidator<T> IsRequired(bool required, string message)
		{
			MemberRequired = required;
			MemberRequiredMessage = message;
			return this;
		}

		#endregion
	}

	/// <summary>
	/// Validation for an object property.
	/// </summary>
	public abstract class PropertyValidator
	{
		#region Constructors

		/// <summary>
		/// Creates an instance of a property validator.
		/// </summary>
		protected PropertyValidator(PropertyInfo info)
		{
			Info = info;
			Validations = new List<IValidation>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// The info for the property.
		/// </summary>
		public PropertyInfo Info { get; }

		/// <summary>
		/// The name of the validator
		/// </summary>
		public string Name => Info.Name;

		/// <summary>
		/// The validations for the validator.
		/// </summary>
		public IList<IValidation> Validations { get; }

		/// <summary>
		/// Get the required status.
		/// </summary>
		internal bool MemberRequired { get; set; }

		/// <summary>
		/// Get the required validation.
		/// </summary>
		internal string MemberRequiredMessage { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Validates that a property matches the provided value.
		/// </summary>
		/// <param name="value"> The value to compare with. </param>
		public PropertyValidator AreEqual(object value)
		{
			return AreEqual(value, ValidationException.GetErrorMessage(ValidationExceptionType.AreEqual, Info.Name));
		}

		/// <summary>
		/// Validates that a property matches the provided value.
		/// </summary>
		/// <param name="value"> The value to compare with. </param>
		/// <param name="message"> The message for failed validation. </param>
		public abstract PropertyValidator AreEqual(object value, string message);

		/// <summary>
		/// Tries to get the property using the provided value.
		/// </summary>
		/// <param name="entity"> The entity to get the value from. </param>
		/// <returns> The value of the property or default value if it cannot be read. </returns>
		public object GetValue(object entity)
		{
			if (Info.CanRead)
			{
				return Info.GetValue(entity);
			}

			return Info.PropertyType.GetDefaultValue();
		}

		/// <summary>
		/// Validate an object within a range.
		/// </summary>
		/// <param name="minimum"> The inclusive minimum value. </param>
		/// <param name="maximum"> The inclusive maximum value. </param>
		/// <param name="excludeRangeValues"> The option to exclude the minimum and maximum values </param>
		public abstract PropertyValidator HasMinMaxRange(object minimum, object maximum, bool excludeRangeValues);

		/// <summary>
		/// Validate an object within a range.
		/// </summary>
		/// <param name="minimum"> The inclusive minimum value. </param>
		/// <param name="maximum"> The inclusive maximum value. </param>
		/// <param name="message"> The message for failed validation. </param>
		/// <param name="excludeRangeValues"> The option to exclude the minimum and maximum values </param>
		public abstract PropertyValidator HasMinMaxRange(object minimum, object maximum, string message, bool excludeRangeValues);

		/// <summary>
		/// Tries to set the property using the provided value.
		/// </summary>
		/// <param name="entity"> </param>
		/// <param name="value"> </param>
		public PropertyValidator SetValue(object entity, object value)
		{
			if (Info.CanWrite)
			{
				Info.SetValue(entity, value);
			}

			return this;
		}

		/// <summary>
		/// Tries to validate the property.
		/// </summary>
		/// <returns> Returns true if the validations pass otherwise false. </returns>
		public abstract bool TryValidate(object value);

		#endregion
	}
}