#region References

using System;

#endregion

namespace Speedy;

/// <summary>
/// Represents a time provider
/// </summary>
public class DateTimeProvider : IDateTimeProvider
{
	#region Fields

	private Func<DateTime> _getCurrentTime;
	private readonly Guid _provider;

	#endregion

	#region Constructors

	/// <summary>
	/// Instantiate an provider for date time.
	/// </summary>
	public DateTimeProvider() : this(Guid.NewGuid(), null)
	{
	}

	/// <summary>
	/// Instantiate an provider for date time.
	/// </summary>
	public DateTimeProvider(DateTime currentTime) : this(Guid.NewGuid(), () => currentTime)
	{
	}

	/// <summary>
	/// Instantiate an provider for date time.
	/// </summary>
	public DateTimeProvider(Guid providerId, DateTime currentTime) : this(providerId, () => currentTime)
	{
	}

	/// <summary>
	/// Instantiate an provider for date time.
	/// </summary>
	public DateTimeProvider(Func<DateTime> getCurrentTime) : this(Guid.NewGuid(), getCurrentTime)
	{
	}

	/// <summary>
	/// Instantiate an provider for date time.
	/// </summary>
	public DateTimeProvider(Guid providerId, Func<DateTime> getCurrentTime)
	{
		_getCurrentTime = getCurrentTime;
		_provider = providerId;
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public DateTime GetDateTime()
	{
		return _getCurrentTime?.Invoke().ToLocalTime() ?? DateTime.Now;
	}

	/// <inheritdoc />
	public Guid GetProviderId()
	{
		return _provider;
	}

	/// <inheritdoc />
	public DateTime GetUtcDateTime()
	{
		return _getCurrentTime?.Invoke().ToUniversalTime() ?? DateTime.UtcNow;
	}

	/// <summary>
	/// Update the time for the provider.
	/// </summary>
	/// <param name="time"> </param>
	public void UpdateDateTime(DateTime time)
	{
		_getCurrentTime = () => time;
	}

	#endregion
}

/// <summary>
/// Represents a time provider
/// </summary>
public interface IDateTimeProvider : IProvider
{
	#region Methods

	/// <summary>
	/// Gets the DateTime in the current time zone.
	/// </summary>
	/// <returns> The current UTC date and time for the sync client. </returns>
	DateTime GetDateTime();

	/// <summary>
	/// Gets the DateTime in UTC.
	/// </summary>
	/// <returns> The current UTC date and time for the sync client. </returns>
	DateTime GetUtcDateTime();

	#endregion
}