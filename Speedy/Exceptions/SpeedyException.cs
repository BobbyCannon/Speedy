#region References

using System;
using System.Runtime.Serialization;

#endregion

namespace Speedy.Exceptions
{
	public abstract class SpeedyException : Exception
	{
		#region Constructors

		/// <summary>
		/// Instantiates an instance of the speedy exception.
		/// </summary>
		protected SpeedyException()
		{
		}

		/// <summary>
		/// Instantiates an instance of the speedy exception.
		/// </summary>
		protected SpeedyException(string message) : base(message)
		{
		}

		/// <summary>
		/// Instantiates an instance of the speedy exception.
		/// </summary>
		protected SpeedyException(string message, Exception inner) : base(message, inner)
		{
		}

		/// <summary>
		/// Instantiates an instance of the speedy exception.
		/// </summary>
		protected SpeedyException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		#endregion
	}
}