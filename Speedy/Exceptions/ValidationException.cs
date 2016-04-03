#region References

using System;
using System.Runtime.Serialization;

#endregion

namespace Speedy.Exceptions
{
	/// <summary>
	/// Represents an validation issue.
	/// </summary>
	[Serializable]
	public class ValidationException : Exception
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

		/// <summary>
		/// Instantiates an instance of the validation exception.
		/// </summary>
		protected ValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		#endregion
	}
}