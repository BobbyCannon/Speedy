#region References

using System;
using Speedy.Extensions;
using Speedy.Validation;

#endregion

namespace Speedy.Exceptions;

/// <summary>
/// Represents an validation issue.
/// </summary>
[Serializable]
public class ValidationException : SpeedyException
{
	#region Constructors

	/// <summary>
	/// Instantiates an instance of the validation exception.
	/// </summary>
	public ValidationException()
	{
	}

	/// <summary>
	/// Instantiates an instance of the validation exception.
	/// </summary>
	public ValidationException(string message) : base(message)
	{
	}

	/// <summary>
	/// Instantiates an instance of the validation exception.
	/// </summary>
	public ValidationException(string message, Exception inner) : base(message, inner)
	{
	}

	#endregion

	#region Methods

	/// <summary>
	/// Get the error message for AreEqual validation.
	/// </summary>
	/// <param name="type"> The exception type. </param>
	/// <param name="name"> The property name. </param>
	/// <returns> The error message. </returns>
	public static string GetErrorMessage(ValidationExceptionType type, string name)
	{
		var attribute = type.GetEnumDetails();
		return string.Format(attribute.Description, name);
	}

	#endregion
}