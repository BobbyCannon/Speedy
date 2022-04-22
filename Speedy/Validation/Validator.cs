#region References

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Speedy.Exceptions;
using Speedy.Extensions;

#endregion

namespace Speedy.Validation
{
	/// <summary>
	/// Validation for a typed object.
	/// </summary>
	public class Validator<T> : Validator
	{
		#region Constructors

		/// <summary>
		/// Creates an instance of a Validator.
		/// </summary>
		public Validator() : base(typeof(T).Name)
		{
		}

		/// <summary>
		/// Creates an instance of a Validator.
		/// </summary>
		/// <param name="name"> The name of the validator. </param>
		public Validator(string name) : base(name)
		{
		}

		#endregion

		#region Methods

		/// <summary>
		/// Configure a validation for a property.
		/// </summary>
		public MemberValidator<TProperty> Property<TProperty>(Expression<Func<T, TProperty>> expression)
		{
			var propertyExpression = (MemberExpression) expression.Body;
			var response = new MemberValidator<TProperty>(propertyExpression.Member);
			PropertyValidators.Add(response);
			return response;
		}

		#endregion
	}

	/// <summary>
	/// Validation for an object.
	/// </summary>
	public abstract class Validator
	{
		#region Constructors

		/// <summary>
		/// Creates an instance of a Validator.
		/// </summary>
		protected Validator(string name)
		{
			Name = name;
			PropertyValidators = new List<MemberValidator>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// The name of the validator
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The validations for the validator.
		/// </summary>
		public IList<MemberValidator> PropertyValidators { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Runs the validator to check the value.
		/// </summary>
		/// <returns> A list of failed validations. </returns>
		public IEnumerable<IValidation> Process(object value)
		{
			var failedValidations = new List<IValidation>();

			ProcessValidator(this, value, failedValidations);

			return failedValidations;
		}

		/// <summary>
		/// Throws a new exception.
		/// </summary>
		/// <typeparam name="T"> The type of exception to throw. </typeparam>
		/// <param name="message"> The message of the exception. </param>
		public static void ThrowException<T>(string message) where T : Exception, new()
		{
			var constructor = typeof(T).GetConstructor(new[] { typeof(string) });
			var newException = (T) constructor?.Invoke(new object[] { message })
				?? new Exception("Failed to throw custom validator exception due to missing constructor...");
			throw newException;
		}

		/// <summary>
		/// Tries to validate
		/// </summary>
		/// <returns> Returns true if the validation passes otherwise false. </returns>
		public bool TryValidate(object value)
		{
			if (PropertyValidators.Count == 0)
			{
				return true;
			}

			for (var i = 0; i < PropertyValidators.Count; i++)
			{
				if (i >= PropertyValidators.Count)
				{
					return true;
				}

				var validator = PropertyValidators[i];
				var memberValue = validator.Info.GetMemberValue(value);

				if (!validator.TryValidate(memberValue))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Runs the validator to check the parameter.
		/// </summary>
		public void Validate(object value)
		{
			var failedValidations = new List<IValidation>();

			ProcessValidator(this, value, failedValidations);

			if (failedValidations.Count <= 0)
			{
				return;
			}

			var builder = new StringBuilder();

			foreach (var validation in failedValidations)
			{
				builder.AppendLine(validation.Message);
			}

			ThrowException<ValidationException>(builder.ToString());
		}

		private static void ProcessValidator(Validator validator, object value, ICollection<IValidation> failedValidation)
		{
			for (var i = 0; i < validator.PropertyValidators.Count; i++)
			{
				if (i >= validator.PropertyValidators.Count)
				{
					break;
				}

				var propertyValidator = validator.PropertyValidators[i];
				var propertyValue = propertyValidator.Info.GetMemberValue(value);

				if (propertyValue == null && !propertyValidator.MemberRequired)
				{
					continue;
				}

				propertyValidator.ProcessValidations(propertyValue, failedValidation);
			}
		}

		#endregion
	}
}