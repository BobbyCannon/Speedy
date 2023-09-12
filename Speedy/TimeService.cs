#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Speedy.Extensions;

#endregion

namespace Speedy;

/// <summary>
/// Represents the service to provide time. Allows control for when the system is being tested.
/// </summary>
public class TimeService : IDateTimeProvider
{
	#region Fields

	private static readonly List<IDateTimeProvider> _providers;
	private static bool _serviceLocked;
	private static readonly TimeService _defaultProvider;

	#endregion

	#region Constructors

	static TimeService()
	{
		_providers = new List<IDateTimeProvider>();
		_defaultProvider = new TimeService();

		Reset();
	}

	private TimeService()
	{
	}

	#endregion

	#region Properties

	/// <summary>
	/// Gets the ID of the Now provider.
	/// </summary>
	public static IDateTimeProvider CurrentProvider => _providers.LastOrDefault() ?? _defaultProvider;

	/// <summary>
	/// Gets the ID of the Now provider.
	/// </summary>
	public static Guid CurrentProviderId => CurrentProvider.GetProviderId();

	/// <summary>
	/// Gets the date time in the format of the current time zone.
	/// </summary>
	public static DateTime Now => GetNow();

	/// <summary>
	/// Gets the date time in the format of UTC time zone.
	/// </summary>
	public static DateTime UtcNow => GetUtcNow();

	#endregion

	#region Methods

	/// <inheritdoc />
	public DateTime GetDateTime()
	{
		return _providers.LastOrDefault()?.GetDateTime() ?? DateTime.Now;
	}

	/// <inheritdoc />
	public Guid GetProviderId()
	{
		return _providers.LastOrDefault()?.GetProviderId()
			?? Guid.Parse("48E21BDA-9E7A-4767-8E3B-B218203C9A71");
	}

	/// <inheritdoc />
	public DateTime GetUtcDateTime()
	{
		return _providers.LastOrDefault()?.GetUtcDateTime() ?? DateTime.UtcNow;
	}

	/// <summary>
	/// Log the service to not allow the time service to be mocked / changed.
	/// </summary>
	public static void LockService()
	{
		_serviceLocked = true;
	}

	/// <summary>
	/// Remove the current provider from the stack
	/// </summary>
	public static void PopProvider()
	{
		if (_serviceLocked)
		{
			throw new InvalidOperationException("The time service has been locked.");
		}

		var provider = _providers.LastOrDefault();
		_providers.Remove(provider);
	}

	/// <summary>
	/// Add a new DateTime provider onto the stack.
	/// </summary>
	public static DateTimeProvider PushProvider(Func<DateTime> provider)
	{
		if (_serviceLocked)
		{
			throw new InvalidOperationException("The time service has been locked.");
		}

		var response = new DateTimeProvider(provider);
		_providers.Add(response);
		return response;
	}

	/// <summary>
	/// Add a new DateTime provider onto the stack.
	/// </summary>
	public static void PushProvider(IDateTimeProvider provider)
	{
		if (_serviceLocked)
		{
			throw new InvalidOperationException("The time service has been locked.");
		}

		_providers.Add(provider);
	}

	/// <summary>
	/// Remove the provider from the stack
	/// </summary>
	public static void RemoveProvider(Guid providerId)
	{
		_providers.Remove(x => x.GetProviderId() == providerId);
	}

	/// <summary>
	/// Remove the provider from the stack
	/// </summary>
	public static void RemoveProvider(DateTimeProvider provider)
	{
		_providers.Remove(provider);
	}

	/// <summary>
	/// Resets the providers to the default values.
	/// </summary>
	public static void Reset()
	{
		_providers.Clear();
	}

	private static DateTime GetNow()
	{
		return CurrentProvider.GetDateTime();
	}

	private static DateTime GetUtcNow()
	{
		return CurrentProvider.GetUtcDateTime();
	}

	#endregion
}