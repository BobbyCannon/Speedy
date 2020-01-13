#region References

using System;

#endregion

namespace Speedy.Website.WebApi
{
	/// <summary>
	/// Represents a key for a session key
	/// </summary>
	public class SessionKey : IComparable<SessionKey>, IComparable, IEquatable<SessionKey>
	{
		#region Constructors

		/// <summary>
		/// Instantiates an session key
		/// </summary>
		public SessionKey(int accountId, Guid sessionId)
		{
			AccountId = accountId;
			SessionId = sessionId;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The account ID for the key.
		/// </summary>
		public int AccountId { get; }

		/// <summary>
		/// The session ID for the key.
		/// </summary>
		public Guid SessionId { get; }

		#endregion

		#region Methods

		/// <inheritdoc />
		public int CompareTo(SessionKey other)
		{
			if (other == null)
			{
				return -1;
			}

			if (Equals(other))
			{
				return 0;
			}

			return -1;
		}

		/// <inheritdoc />
		public int CompareTo(object obj)
		{
			return CompareTo(obj as SessionKey);
		}

		/// <inheritdoc />
		public bool Equals(SessionKey other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return AccountId == other.AccountId && SessionId == other.SessionId;
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			return Equals((SessionKey) obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = AccountId.GetHashCode();
				hashCode = (hashCode * 397) ^ SessionId.GetHashCode();
				return hashCode;
			}
		}

		#endregion
	}
}