#region References

using System;
using System.Runtime.Serialization;

#endregion

namespace Speedy.Exceptions
{
	/// <summary>
	/// Represents an update exception.
	/// </summary>
	[Serializable]
	public class UpdateException : SpeedyException
	{
		#region Constructors

		/// <summary>
		/// Instantiates an instance of the update exception.
		/// </summary>
		public UpdateException()
		{
		}

		/// <summary>
		/// Instantiates an instance of the update exception.
		/// </summary>
		public UpdateException(string message) : base(message)
		{
		}

		/// <summary>
		/// Instantiates an instance of the update exception.
		/// </summary>
		public UpdateException(string message, Exception inner) : base(message, inner)
		{
		}

		/// <summary>
		/// Instantiates an instance of the update exception.
		/// </summary>
		protected UpdateException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		#endregion
	}
}