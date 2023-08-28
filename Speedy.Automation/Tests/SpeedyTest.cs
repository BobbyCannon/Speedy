#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using KellermanSoftware.CompareNetObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;

#endregion

#pragma warning disable CA1416

namespace Speedy.Automation.Tests;

/// <summary>
/// Represents a Speedy test.
/// </summary>
public abstract class SpeedyTest<T> : SpeedyTest where T : new()
{
	#region Methods

	/// <summary>
	/// Get a model of the provided type.
	/// </summary>
	/// <returns> An instance of the type. </returns>
	protected T GetModel()
	{
		var response = new T();
		return response;
	}

	/// <summary>
	/// Create a new instance of the type then update the object with non default values.
	/// </summary>
	/// <returns> The instance of the type with non default values. </returns>
	protected T GetModelWithNonDefaultValues()
	{
		return GetModelWithNonDefaultValues<T>();
	}

	#endregion
}

/// <summary>
/// Represents a Speedy test.
/// </summary>
public abstract class SpeedyTest
{
	#region Fields

	private static Action<string> _clipboardProvider;
	private static DateTime? _currentTime;
	private static uint? _lastUtcNowProviderId;

	#endregion

	#region Constructors

	static SpeedyTest()
	{
		// Default start date
		StartDateTime = new DateTime(2022, 12, 29, 08, 00, 00, DateTimeKind.Utc);
	}

	#endregion

	#region Properties

	/// <summary>
	/// Represents the current time returned by TimeService.UtcNow;
	/// </summary>
	public static DateTime CurrentTime
	{
		get => _currentTime ?? DateTime.UtcNow;
		set => ResetCurrentTime(value);
	}

	/// <summary>
	/// Returns true if the debugger is attached.
	/// </summary>
	public static bool IsDebugging => Debugger.IsAttached;

	/// <summary>
	/// Represents the current time returned by TimeService.UtcNow;
	/// </summary>
	public static DateTime StartDateTime { get; set; }

	/// <summary>
	/// The timeout to use when waiting for a test state to be hit.
	/// </summary>
	public static int WaitTimeout => IsDebugging ? 100000 : 1000;

	private static Func<DateTime> GetCurrentTime => () => CurrentTime.ToLocalTime();

	private static Func<DateTime> GetCurrentTimeUtc => () => CurrentTime;

	#endregion

	#region Methods

	/// <summary>
	/// Validates that the expected and actual are equal.
	/// </summary>
	public virtual void AreEqual<T>(T expected, T actual, Action<bool> process = null, params string[] membersToIgnore)
	{
		var configuration = new ComparisonConfig { IgnoreObjectTypes = true, MaxDifferences = int.MaxValue };
		configuration.MembersToIgnore.AddRange(membersToIgnore);
		var logic = new CompareLogic(configuration);
		var result = logic.Compare(expected, actual);
		process?.Invoke(result.AreEqual);
		Assert.IsTrue(result.AreEqual, result.DifferencesString);
	}

	/// <summary>
	/// Validates that the expected and actual are equal.
	/// </summary>
	public virtual void AreEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, bool ignoreCollectionOrder = false, Action<bool> process = null, params string[] membersToIgnore)
	{
		var configuration = new ComparisonConfig
		{
			IgnoreObjectTypes = true,
			MaxDifferences = int.MaxValue,
			IgnoreCollectionOrder = ignoreCollectionOrder
		};
		configuration.MembersToIgnore.AddRange(membersToIgnore);
		var logic = new CompareLogic(configuration);
		var result = logic.Compare(expected, actual);
		process?.Invoke(result.AreEqual);
		Assert.IsTrue(result.AreEqual, result.DifferencesString);
	}

	/// <summary>
	/// Validates that the expected and actual are equal.
	/// </summary>
	public virtual void AreEqual<T>(T[] expected, T[] actual, bool ignoreCollectionOrder = false, Action<bool> process = null, params string[] membersToIgnore)
	{
		var configuration = new ComparisonConfig { IgnoreObjectTypes = true, MaxDifferences = int.MaxValue, IgnoreCollectionOrder = ignoreCollectionOrder };
		configuration.MembersToIgnore.AddRange(membersToIgnore);
		var logic = new CompareLogic(configuration);
		var result = logic.Compare(expected, actual);
		process?.Invoke(result.AreEqual);
		Assert.IsTrue(result.AreEqual, result.DifferencesString);
	}
	
	/// <summary>
	/// Validates that the expected and actual are equal.
	/// </summary>
	public virtual void AreEqual<T>(T[] expected, T[] actual, Func<string> message, bool ignoreCollectionOrder = false, Action<bool> process = null, params string[] membersToIgnore)
	{
		var configuration = new ComparisonConfig { IgnoreObjectTypes = true, MaxDifferences = int.MaxValue, IgnoreCollectionOrder = ignoreCollectionOrder };
		configuration.MembersToIgnore.AddRange(membersToIgnore);
		var logic = new CompareLogic(configuration);
		var result = logic.Compare(expected, actual);
		process?.Invoke(result.AreEqual);
		Assert.IsTrue(result.AreEqual, result.DifferencesString + Environment.NewLine + message());
	}

	/// <summary>
	/// Validates that the expected and actual are equal.
	/// </summary>
	public virtual void AreEqual<T>(T expected, T actual, params string[] membersToIgnore)
	{
		var configuration = new ComparisonConfig { IgnoreObjectTypes = true, MaxDifferences = int.MaxValue };
		configuration.MembersToIgnore.AddRange(membersToIgnore);
		var logic = new CompareLogic(configuration);
		var result = logic.Compare(expected, actual);
		Assert.IsTrue(result.AreEqual, result.DifferencesString);
	}

	/// <summary>
	/// Validates that the expected and actual are equal with custom message an optional exceptions.
	/// </summary>
	public virtual void AreEqual<T>(T expected, T actual, Func<string> message, params string[] membersToIgnore)
	{
		var configuration = new ComparisonConfig { IgnoreObjectTypes = true, MaxDifferences = int.MaxValue };
		configuration.MembersToIgnore.AddRange(membersToIgnore);
		var logic = new CompareLogic(configuration);
		var result = logic.Compare(expected, actual);
		Assert.IsTrue(result.AreEqual, message?.Invoke() + Environment.NewLine + result.DifferencesString);
	}

	/// <summary>
	/// Validates that the expected and actual are not equal.
	/// </summary>
	public virtual void AreNotEqual<T>(T expected, T actual, params string[] membersToIgnore)
	{
		var configuration = new ComparisonConfig { IgnoreObjectTypes = true, MaxDifferences = int.MaxValue };
		configuration.MembersToIgnore.AddRange(membersToIgnore);
		var logic = new CompareLogic(configuration);
		var result = logic.Compare(expected, actual);
		Assert.IsFalse(result.AreEqual, result.DifferencesString);
	}

	/// <summary>
	/// Validates that the expected and actual are not equal.
	/// </summary>
	public virtual void AreNotEqual<T>(T expected, T actual, Func<string> message, params string[] membersToIgnore)
	{
		var configuration = new ComparisonConfig { IgnoreObjectTypes = true, MaxDifferences = int.MaxValue };
		configuration.MembersToIgnore.AddRange(membersToIgnore);
		var logic = new CompareLogic(configuration);
		var result = logic.Compare(expected, actual);
		Assert.IsFalse(result.AreEqual, message?.Invoke() + Environment.NewLine + result.DifferencesString);
	}

	/// <summary>
	/// Copy the string version of the value to the clipboard.
	/// </summary>
	/// <typeparam name="T"> The type of the value. </typeparam>
	/// <param name="value"> The object to copy to the clipboard. Calls ToString on the value. </param>
	/// <returns> The value input to allow for method chaining. </returns>
	public T CopyToClipboard<T>(T value)
	{
		var thread = new Thread(() =>
		{
			try
			{
				_clipboardProvider?.Invoke(value?.ToString() ?? string.Empty);
			}
			catch
			{
				// Ignore the clipboard set issue...
			}
		});

		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		thread.Join();

		return value;
	}

	/// <summary>
	/// Test for an expected exception.
	/// </summary>
	/// <typeparam name="T"> The type of the exception. </typeparam>
	/// <param name="work"> The test. </param>
	/// <param name="messages"> A set of messages where at least one message should be found. </param>
	public static void ExpectedException<T>(Action work, params string[] messages) where T : Exception
	{
		ExpectedException<T>(work, null, messages);
	}

	/// <summary>
	/// Test for an expected exception.
	/// </summary>
	/// <typeparam name="T"> The type of the exception. </typeparam>
	/// <param name="work"> The test. </param>
	/// <param name="extraValidation"> An optional set of extra validation. </param>
	/// <param name="messages"> A set of messages where at least one message should be found. </param>
	public static void ExpectedException<T>(Action work, Action<T> extraValidation, params string[] messages) where T : Exception
	{
		try
		{
			work();
		}
		catch (T ex)
		{
			var detailedException = ex.ToDetailedString();
			var allErrors = "\"" + string.Join("\", \"", messages) + "\"";

			if (!messages.Any(x => detailedException.Contains(x)))
			{
				Assert.Fail("Actual <" + detailedException + "> does not contain expected <" + allErrors + ">.");
			}

			extraValidation?.Invoke(ex);
			return;
		}

		Assert.Fail("The expected exception was not thrown.");
	}

	/// <summary>
	/// Increment the current time. This only works if current time is set. Negative values will subtract time.
	/// </summary>
	/// <param name="days"> The days to increment by. </param>
	/// <param name="hours"> The hours to increment by. </param>
	/// <param name="minutes"> The minutes to increment by. </param>
	/// <param name="seconds"> The seconds to increment by. </param>
	/// <param name="milliseconds"> The milliseconds to increment by. </param>
	/// <param name="microseconds"> The microseconds to increment by. Only supported in .NET 7 or greater. </param>
	/// <param name="ticks"> The ticks to increment by. </param>
	public static void IncrementTime(int days = 0, int hours = 0, int minutes = 0, int seconds = 0, int milliseconds = 0, int microseconds = 0, long ticks = 0)
	{
		var currentTime = _currentTime;
		if (currentTime == null)
		{
			return;
		}

		if (days != 0)
		{
			currentTime += TimeSpan.FromDays(days);
		}

		if (hours != 0)
		{
			currentTime += TimeSpan.FromHours(hours);
		}

		if (minutes != 0)
		{
			currentTime += TimeSpan.FromMinutes(minutes);
		}

		if (seconds != 0)
		{
			currentTime += TimeSpan.FromSeconds(seconds);
		}

		if (milliseconds != 0)
		{
			currentTime += TimeSpan.FromMilliseconds(milliseconds);
		}

		#if NET7_0_OR_GREATER
		if (microseconds != 0)
		{
			currentTime += TimeSpan.FromMicroseconds(microseconds);
		}
		#endif

		if (ticks != 0)
		{
			currentTime += TimeSpan.FromTicks(ticks);
		}

		ResetCurrentTime(currentTime);
	}

	/// <summary>
	/// Increment the current time. This only works if current time is set. Negative values will subtract time.
	/// </summary>
	/// <param name="value"> The value to increment by. </param>
	public static void IncrementTime(TimeSpan value)
	{
		var currentTime = _currentTime;
		if (currentTime == null)
		{
			return;
		}

		ResetCurrentTime(currentTime + value);
	}

	/// <summary>
	/// Initialize the defaults
	/// </summary>
	public static void Initialize()
	{
		// Reset the current time back to the start time.
		ResetCurrentTime(StartDateTime);
	}

	/// <summary>
	/// Tests whether the specified condition is false and throws an exception if the condition is true.
	/// </summary>
	/// <param name="condition"> The condition the test expects to be false. </param>
	public virtual void IsFalse(Func<bool> condition)
	{
		IsFalse(condition());
	}

	/// <summary>
	/// Tests whether the specified condition is false and throws an exception if the condition is true.
	/// </summary>
	/// <param name="condition"> The condition the test expects to be false. </param>
	/// <param name="message"> The message is shown in test results. </param>
	public virtual void IsFalse(Func<bool> condition, string message)
	{
		IsFalse(condition(), message);
	}

	/// <summary>
	/// Tests whether the specified condition is false and throws an exception if the condition is true.
	/// </summary>
	/// <param name="condition"> The condition the test expects to be false. </param>
	public virtual void IsFalse(bool condition)
	{
		Assert.IsFalse(condition);
	}

	/// <summary>
	/// Tests whether the specified condition is false and throws an exception if the condition is true.
	/// </summary>
	/// <param name="condition"> The condition the test expects to be false. </param>
	/// <param name="message"> The message is shown in test results. </param>
	public virtual void IsFalse(bool condition, string message)
	{
		Assert.IsFalse(condition, message);
	}

	/// <summary>
	/// Tests whether the specified object is non-null and throws an exception if it is null.
	/// </summary>
	/// <param name="value"> The object the test expects not to be null. </param>
	public virtual void IsNotNull(object value)
	{
		Assert.IsNotNull(value);
	}

	/// <summary>
	/// Tests whether the specified object is non-null and throws an exception if it is null.
	/// </summary>
	/// <param name="value"> The object the test expects not to be null. </param>
	/// <param name="message"> The message is shown in test results. </param>
	public virtual void IsNotNull(object value, string message)
	{
		Assert.IsNotNull(value, message);
	}

	/// <summary>
	/// Tests whether the specified object is null and throws an exception if it is not null.
	/// </summary>
	/// <param name="value"> The object the test expects to be null. </param>
	public virtual void IsNull(object value)
	{
		Assert.IsNull(value);
	}

	/// <summary>
	/// Tests whether the specified object is null and throws an exception if it is not null.
	/// </summary>
	/// <param name="value"> The object the test expects to be null. </param>
	/// <param name="message"> The message is shown in test results. </param>
	public virtual void IsNull(object value, string message)
	{
		Assert.IsNull(value, message);
	}

	/// <summary>
	/// Tests whether the specified condition is true and throws an exception if the condition is false.
	/// </summary>
	/// <param name="condition"> The condition the test expects to be true. </param>
	public virtual void IsTrue(Func<bool> condition)
	{
		IsTrue(condition());
	}

	/// <summary>
	/// Tests whether the specified condition is true and throws an exception if the condition is false.
	/// </summary>
	/// <param name="condition"> The condition the test expects to be true. </param>
	/// <param name="message"> The message is shown in test results. </param>
	public virtual void IsTrue(Func<bool> condition, string message)
	{
		IsTrue(condition(), message);
	}

	/// <summary>
	/// Tests whether the specified condition is true and throws an exception if the condition is false.
	/// </summary>
	/// <param name="condition"> The condition the test expects to be true. </param>
	public virtual void IsTrue(bool condition)
	{
		Assert.IsTrue(condition);
	}

	/// <summary>
	/// Tests whether the specified condition is true and throws an exception if the condition is false.
	/// </summary>
	/// <param name="condition"> The condition the test expects to be true. </param>
	/// <param name="message"> The message is shown in test results. </param>
	public virtual void IsTrue(bool condition, string message)
	{
		Assert.IsTrue(condition, message);
	}

	/// <summary>
	/// Reset the <see cref="CurrentTime" /> back to using DateTime.
	/// </summary>
	/// <param name="currentTime"> An optional current time to reset to otherwise back to DateTime.UtcNow and Now. </param>
	public static void ResetCurrentTime(DateTime? currentTime = null)
	{
		_currentTime = currentTime?.ToUtcDateTime();

		if ((_lastUtcNowProviderId != null) && (_lastUtcNowProviderId == TimeService.CurrentUtcNowProviderId))
		{
			// Service has already been set.
			return;
		}

		TimeService.Reset();
		_lastUtcNowProviderId = TimeService.AddUtcNowProvider(GetCurrentTimeUtc);
		TimeService.AddNowProvider(GetCurrentTime);
	}

	/// <summary>
	/// Sets the clipboard provider.
	/// </summary>
	/// <param name="provider"> The provider to be set. </param>
	public static void SetClipboardProvider(Action<string> provider)
	{
		_clipboardProvider = provider;
	}

	/// <summary>
	/// Set the CurrentTime value.
	/// </summary>
	/// <param name="value"> The new time to set. </param>
	public static void SetTime(DateTime value)
	{
		ResetCurrentTime(value);
	}

	/// <summary>
	/// Runs the action until the action returns true or the timeout is reached. Will delay in between actions using the
	/// <see cref="WaitTimeout" /> property with a delay of 10ms.
	/// </summary>
	/// <param name="action"> The action to call. </param>
	/// <param name="useTimeService"> An optional flag to use the TimeService instead of DateTime. Defaults to false to use DateTime. </param>
	/// <returns> Returns true of the call completed successfully or false if it timed out. </returns>
	public static bool Wait(Func<bool> action, bool useTimeService = false)
	{
		return UtilityExtensions.WaitUntil(action, WaitTimeout, 10, useTimeService);
	}

	/// <summary>
	/// Runs the action until the action returns true or the timeout is reached. Will delay in between actions of the provided
	/// time.
	/// </summary>
	/// <param name="action"> The action to call. </param>
	/// <param name="timeout"> The timeout to attempt the action. This value is in milliseconds. </param>
	/// <param name="delay"> The delay in between actions. This value is in milliseconds. </param>
	/// <param name="useTimeService"> An optional flag to use the TimeService instead of DateTime. Defaults to false to use DateTime. </param>
	/// <returns> Returns true of the call completed successfully or false if it timed out. </returns>
	public static bool Wait(Func<bool> action, int timeout, int delay, bool useTimeService = false)
	{
		return UtilityExtensions.WaitUntil(action, timeout, delay, useTimeService);
	}

	/// <summary>
	/// Wait for a cancellation or for the value to time out.
	/// </summary>
	/// <param name="cancellationPending"> A check for cancellation. </param>
	/// <param name="value"> The value of time to wait for. </param>
	/// <param name="delay"> The delay between checks. </param>
	/// <param name="minimum"> The minimal time to wait. </param>
	/// <param name="maximum"> The maximum time to wait. </param>
	/// <param name="useTimeService"> An optional flag to use the TimeService instead of DateTime. Defaults to false to use DateTime. </param>
	/// <returns> True if the wait was completed, false if the wait was cancelled. </returns>
	public static bool Wait(Func<bool> cancellationPending, TimeSpan value, TimeSpan delay, TimeSpan minimum, TimeSpan maximum, bool useTimeService = false)
	{
		return UtilityExtensions.WaitUntil(cancellationPending, value, delay, minimum, maximum, useTimeService);
	}

	/// <summary>
	/// Gets a default value for the property info.
	/// </summary>
	/// <param name="propertyInfo"> The info for the property. </param>
	/// <param name="nonSupportedType"> An optional non supported type. </param>
	/// <returns> The default value. </returns>
	protected object GetDefaultValue(PropertyInfo propertyInfo, Func<PropertyInfo, object> nonSupportedType = null)
	{
		return propertyInfo.GetDefaultValue(nonSupportedType);
	}

	/// <summary>
	/// Create a new instance of the type then update the object with non default values.
	/// </summary>
	/// <param name="type"> The type to create. </param>
	/// <param name="exclusions"> An optional set of exclusions. </param>
	/// <returns> The instance of the type with non default values. </returns>
	protected object GetModelWithNonDefaultValues(Type type, params string[] exclusions)
	{
		var response = Activator.CreateInstance(type);
		response.UpdateWithNonDefaultValues(exclusions);
		return response;
	}

	/// <summary>
	/// Create a new instance of the type then update the object with non default values.
	/// </summary>
	/// <param name="type"> The type to create. </param>
	/// <param name="nonSupportedType"> An optional function to update non supported property value types. </param>
	/// <param name="exclusions"> An optional set of exclusions. </param>
	/// <returns> The instance of the type with non default values. </returns>
	protected object GetModelWithNonDefaultValues(Type type, Func<Type, object> nonSupportedType = null, params string[] exclusions)
	{
		var response = Activator.CreateInstance(type);
		response.UpdateWithNonDefaultValues(nonSupportedType, exclusions);
		return response;
	}

	/// <summary>
	/// Create a new instance of the type then update the object with non default values.
	/// </summary>
	/// <typeparam name="T"> The type to create. </typeparam>
	/// <returns> The instance of the type with non default values. </returns>
	protected T GetModelWithNonDefaultValues<T>() where T : new()
	{
		var response = new T();
		response.UpdateWithNonDefaultValues();
		return response;
	}

	/// <summary>
	/// Gets a non default value for the property info.
	/// </summary>
	/// <param name="propertyInfo"> The info for the property. </param>
	/// <param name="nonSupportedType"> An optional non supported type. </param>
	/// <returns> The non default value. </returns>
	protected object GetNonDefaultValue(PropertyInfo propertyInfo, Func<Type, object> nonSupportedType = null)
	{
		return propertyInfo.GetNonDefaultValue(nonSupportedType);
	}

	/// <summary>
	/// Sleep for a given amount of time with an optional cancellation token.
	/// </summary>
	/// <param name="seconds"> The seconds to increment by. </param>
	/// <param name="milliseconds"> The milliseconds to increment by. </param>
	/// <param name="microseconds"> The microseconds to increment by. Only supported in .NET 7 or greater. </param>
	/// <param name="ticks"> The ticks to increment by. </param>
	/// <param name="token"> An optional cancellation token. </param>
	protected void Sleep(int seconds = 0, int milliseconds = 0, int microseconds = 0, long ticks = 0, CancellationToken? token = null)
	{
		var delay = TimeSpan.FromSeconds(seconds)
			+ TimeSpan.FromMilliseconds(milliseconds)
			+ TimeSpan.FromTicks(ticks);

		#if NET7_0_OR_GREATER
		delay += TimeSpan.FromMicroseconds(microseconds);
		#endif

		Sleep(delay, token);
	}

	/// <summary>
	/// Sleep for a given amount of time with an optional cancellation token.
	/// </summary>
	/// <param name="delay"> The delay to sleep. </param>
	/// <param name="token"> An optional cancellation token. </param>
	protected void Sleep(TimeSpan delay, CancellationToken? token)
	{
		var timeout = TimeService.UtcNow + delay;

		while ((TimeService.UtcNow < timeout)
				&& token is not { IsCancellationRequested: true })
		{
			Thread.Sleep(0);
		}
	}

	/// <summary>
	/// Validate the object has all non default values.
	/// </summary>
	/// <param name="model"> The module to be validated </param>
	/// <param name="exclusions"> An optional set of exclusions. </param>
	protected void ValidateAllValuesAreNotDefault<T>(T model, params string[] exclusions)
	{
		model.ValidateAllValuesAreNotDefault(exclusions);
	}

	/// <summary>
	/// Validate a model that is "IUnwrappable"
	/// </summary>
	/// <param name="model"> The model to test. </param>
	/// <param name="exclusions"> An optional set of exclusions. </param>
	protected void ValidateUnwrap(object model, params string[] exclusions)
	{
		if (model is not Entity entity)
		{
			return;
		}

		var actual = entity.Unwrap();
		var allExclusions = new List<string>(exclusions);
		allExclusions.AddRange(
			entity.GetCachedProperties()
				.Where(x => x.IsVirtual())
				.Select(x => x.Name)
		);

		AreEqual(model, actual, () => model.GetType().FullName, allExclusions.ToArray());
	}

	/// <summary>
	/// Validate a model that is "IUpdateable"
	/// </summary>
	/// <param name="model"> The model to test. </param>
	/// <param name="exclusions"> An optional set of exclusions. </param>
	protected void ValidateUpdateableModel(object model, params string[] exclusions)
	{
		ValidateUnwrap(model, exclusions);
		ValidateUpdateWith(model, false, exclusions);
		ValidateUpdateWith(model, true, exclusions);
		model.ValidateAllValuesAreNotDefault(exclusions);
	}

	/// <summary>
	/// Validate a model's UpdateWith using the "IUpdateable" interface.
	/// </summary>
	/// <param name="update"> The model to test. </param>
	/// <param name="excludeVirtuals"> Exclude virtuals during the test. </param>
	/// <param name="exclusions"> An optional set of exclusions. </param>
	protected void ValidateUpdateWith(object update, bool excludeVirtuals, params string[] exclusions)
	{
		var updateType = update.GetType();
		var actual = Activator.CreateInstance(updateType) as IUpdateable;

		Assert.IsNotNull(actual);

		var allExclusions = excludeVirtuals
			? exclusions.AddRange(updateType.GetVirtualPropertyNames()).ToArray()
			: exclusions;

		actual.UpdateWith(update, allExclusions);

		AreEqual(update, actual, () => updateType.FullName, allExclusions);
	}

	#endregion
}