#region References

using System;
using System.Collections.Concurrent;
using System.Linq;

#endregion

namespace Speedy;

/// <summary>
/// Represents the service to provide time. Allows control for when the system is being tested.
/// </summary>
public static class TimeService
{
	#region Fields

	private static Func<DateTime> _currentNowProvider;
	private static Func<DateTime> _currentUtcNowProvider;
	private static uint _nowIndex, _utcNowIndex;
	private static readonly ConcurrentDictionary<uint, Func<DateTime>> _nowProviders;
	private static bool _serviceLocked;
	private static readonly ConcurrentDictionary<uint, Func<DateTime>> _utcNowProviders;

	#endregion

	#region Constructors

	static TimeService()
	{
		_nowProviders = new ConcurrentDictionary<uint, Func<DateTime>>();
		_utcNowProviders = new ConcurrentDictionary<uint, Func<DateTime>>();

		Reset();
	}

	#endregion

	#region Properties

	/// <summary>
	/// Gets the ID of the Now provider.
	/// </summary>
	public static uint? CurrentNowProviderId => _nowProviders.Keys.LastOrDefault();

	/// <summary>
	/// Gets the ID of the UtcNow provider.
	/// </summary>
	public static uint? CurrentUtcNowProviderId => _utcNowProviders.Keys.LastOrDefault();

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

	/// <summary>
	/// Add a new DateTime.Now provider onto the stack.
	/// </summary>
	/// <returns>
	/// The id of the provider. Use this id to remove it from the stack.
	/// </returns>
	public static uint? AddNowProvider(Func<DateTime> provider)
	{
		if (_serviceLocked)
		{
			throw new InvalidOperationException("The time service has been locked.");
		}

		return TryAddNowProvider(provider, out var id) ? id : null;
	}

	/// <summary>
	/// Add a new DateTime.UtcNow provider onto the stack.
	/// </summary>
	/// <returns>
	/// The id of the provider. Use this id to remove it from the stack.
	/// </returns>
	public static uint? AddUtcNowProvider(Func<DateTime> provider)
	{
		if (_serviceLocked)
		{
			throw new InvalidOperationException("The time service has been locked.");
		}

		return TryAddUtcNowProvider(provider, out var id) ? id : null;
	}

	/// <summary>
	/// Log the service to not allow the time service to be mocked / changed.
	/// </summary>
	public static void LockService()
	{
		_serviceLocked = true;
	}

	/// <summary>
	/// Remove the DateTime.Now provider from the stack by the provided id.
	/// </summary>
	public static void RemoveNowProvider(uint id)
	{
		if (_serviceLocked)
		{
			throw new InvalidOperationException("The time service has been locked.");
		}

		if (!_nowProviders.TryRemove(id, out var provider))
		{
			return;
		}

		if (provider == _currentNowProvider)
		{
			_currentNowProvider = _nowProviders.LastOrDefault().Value;
		}
	}

	/// <summary>
	/// Remove the DateTime.UtcNow provider from the stack by the provided id.
	/// </summary>
	public static void RemoveUtcNowProvider(uint id)
	{
		if (_serviceLocked)
		{
			throw new InvalidOperationException("The time service has been locked.");
		}

		if (!_utcNowProviders.TryRemove(id, out var provider))
		{
			return;
		}

		if (provider == _currentUtcNowProvider)
		{
			_currentUtcNowProvider = _utcNowProviders.LastOrDefault().Value;
		}
	}

	/// <summary>
	/// Resets the providers to the default values.
	/// </summary>
	public static void Reset()
	{
		_utcNowProviders.Clear();
		_currentUtcNowProvider = null;
		_nowProviders.Clear();
		_currentNowProvider = null;
	}

	/// <summary>
	/// Try to add a new DateTime.Now provider onto the stack.
	/// </summary>
	/// <param name="provider"> The provider to add. </param>
	/// <param name="id"> The id of the added provider. Returns null if provider not added. </param>
	public static bool TryAddNowProvider(Func<DateTime> provider, out uint? id)
	{
		if (_serviceLocked)
		{
			throw new InvalidOperationException("The time service has been locked.");
		}

		var key = _nowIndex++;
		var result = _nowProviders.TryAdd(key, provider);

		if (result)
		{
			_currentNowProvider = provider;
			id = key;
		}
		else
		{
			id = null;
		}

		return result;
	}

	/// <summary>
	/// Try to add a new DateTime.UtcNow provider onto the stack.
	/// </summary>
	/// <param name="provider"> The provider to add. </param>
	/// <param name="id"> The id of the added provider. Returns null if provider not added. </param>
	public static bool TryAddUtcNowProvider(Func<DateTime> provider, out uint? id)
	{
		if (_serviceLocked)
		{
			throw new InvalidOperationException("The time service has been locked.");
		}

		var key = _utcNowIndex++;
		var result = _utcNowProviders.TryAdd(key, provider);

		if (result)
		{
			_currentUtcNowProvider = provider;
			id = key;
		}
		else
		{
			id = null;
		}

		return result;
	}

	private static DateTime GetNow()
	{
		if (_serviceLocked)
		{
			return DateTime.Now;
		}

		return _currentNowProvider?.Invoke() ?? DateTime.Now;
	}

	private static DateTime GetUtcNow()
	{
		if (_serviceLocked)
		{
			return DateTime.UtcNow;
		}

		return _currentUtcNowProvider?.Invoke() ?? DateTime.UtcNow;
	}

	#endregion
}