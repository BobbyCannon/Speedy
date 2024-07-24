#region References

using System;

#endregion

namespace Speedy.Runtime;

/// <summary>
/// Represents a time provider
/// </summary>
public class DateTimeProvider : IDateTimeProvider
{
	#region Fields

	private bool _locked;
	private Func<DateTime> _provider;
	private readonly Guid _providerId;

	#endregion

	#region Constructors

	/// <summary>
	/// Initialize a provider for date time.
	/// </summary>
	public DateTimeProvider() : this(Guid.NewGuid(), null)
	{
	}

	/// <summary>
	/// Initialize a provider for date time.
	/// </summary>
	public DateTimeProvider(DateTime currentTime) : this(Guid.NewGuid(), () => currentTime)
	{
	}

	/// <summary>
	/// Initialize a provider for date time.
	/// </summary>
	public DateTimeProvider(Guid providerIdId, DateTime currentTime) : this(providerIdId, () => currentTime)
	{
	}

	/// <summary>
	/// Initialize a provider for date time.
	/// </summary>
	public DateTimeProvider(Func<DateTime> provider) : this(Guid.NewGuid(), provider)
	{
	}

	/// <summary>
	/// Initialize a provider for date time.
	/// </summary>
	public DateTimeProvider(Guid providerIdId, Func<DateTime> provider)
	{
		_provider = provider;
		_providerId = providerIdId;
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public DateTime GetDateTime()
	{
		return _provider?.Invoke().ToLocalTime() ?? DateTime.Now;
	}

	/// <inheritdoc />
	public Guid GetProviderId()
	{
		return _providerId;
	}

	/// <inheritdoc />
	public DateTime GetUtcDateTime()
	{
		return _provider?.Invoke().ToUniversalTime() ?? DateTime.UtcNow;
	}

	/// <summary>
	/// Lock the provider to not allow the time service to change. Once locked
	/// the provider can no longer be changes or updated.
	/// </summary>
	public void LockProvider()
	{
		_locked = true;
	}

	/// <summary>
	/// Update the time for the provider.
	/// </summary>
	/// <param name="time"> </param>
	public void UpdateDateTime(DateTime time)
	{
		if (_locked)
		{
			throw new InvalidOperationException("The DateTime provider is locked.");
		}

		_provider = () => time;
	}

	#endregion
}

/// <summary>
/// Represents the service to provide time. Allows control for when the system is being tested.
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