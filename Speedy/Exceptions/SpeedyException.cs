#region References

using System;

#endregion

namespace Speedy.Exceptions;

/// <summary>
/// The base exception for the Speedy framework.
/// </summary>
public class SpeedyException : Exception
{
	#region Constants

	/// <summary>
	/// Represents message for invalid sync clients.
	/// </summary>
	public const string ClientNotSupported = "This client is no longer supported. Please update to a supported version.";

	/// <summary>
	/// Represents message for key not found.
	/// </summary>
	public const string KeyNotFound = "Could not find the entry with the key.";

	/// <summary>
	/// Represents message for repository not found.
	/// </summary>
	public const string RepositoryNotFound = "The repository was not found.";

	/// <summary>
	/// Represents message for invalid sync entity type.
	/// </summary>
	public const string SyncEntityIncorrectType = "The sync entity is not the correct type.";

	#endregion

	#region Constructors

	/// <summary>
	/// Instantiates an instance of the speedy exception.
	/// </summary>
	public SpeedyException()
	{
	}

	/// <summary>
	/// Instantiates an instance of the speedy exception.
	/// </summary>
	public SpeedyException(string message) : base(message)
	{
	}

	/// <summary>
	/// Instantiates an instance of the speedy exception.
	/// </summary>
	public SpeedyException(string message, Exception inner) : base(message, inner)
	{
	}

	#endregion
}