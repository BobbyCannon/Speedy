#region References

using System;
using System.Runtime.Serialization;

#endregion

namespace Speedy.Exceptions
{
	[Serializable]
	public class ValidationException : Exception
	{
		#region Constructors

		public ValidationException()
		{
		}

		public ValidationException(string message) : base(message)
		{
		}

		public ValidationException(string message, Exception inner) : base(message, inner)
		{
		}

		protected ValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		#endregion
	}
}