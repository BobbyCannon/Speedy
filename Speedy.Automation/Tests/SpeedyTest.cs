﻿#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using KellermanSoftware.CompareNetObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;

#endregion

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

	#endregion

	#region Properties

	/// <summary>
	/// Represents the current time returned by TimeService.UtcNow;
	/// </summary>
	public static DateTime CurrentTime
	{
		get => _currentTime ?? DateTime.UtcNow;
		set => _currentTime = value;
	}

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
		Assert.IsTrue(result.AreEqual, result.ToString());
	}

	/// <summary>
	/// Validates that the expected and actual are equal.
	/// </summary>
	public virtual void AreEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, bool ignoreCollectionOrder = false, Action<bool> process = null, params string[] membersToIgnore)
	{
		var configuration = new ComparisonConfig { IgnoreObjectTypes = true, MaxDifferences = int.MaxValue, IgnoreCollectionOrder = ignoreCollectionOrder };
		configuration.MembersToIgnore.AddRange(membersToIgnore);
		var logic = new CompareLogic(configuration);
		var result = logic.Compare(expected, actual);
		process?.Invoke(result.AreEqual);
		Assert.IsTrue(result.AreEqual, result.ToString());
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
		Assert.IsTrue(result.AreEqual, result.ToString());
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
			return;
		}

		Assert.Fail("The expected exception was not thrown.");
	}

	/// <summary>
	/// Increment the current time. This only works if current time is set. Negative values will subtract time.
	/// </summary>
	/// <param name="seconds"> The seconds to increment by. </param>
	/// <param name="milliseconds"> The milliseconds to increment by. </param>
	/// <param name="microseconds"> The microseconds to increment by. Only supported in .NET 7 or greater. </param>
	/// <param name="ticks"> The ticks to increment by. </param>
	public static void IncrementTime(int seconds = 0, int milliseconds = 0, int microseconds = 0, long ticks = 0)
	{
		var currentTime = _currentTime;
		if (currentTime == null)
		{
			return;
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

		_currentTime = currentTime;
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
	public static void ResetCurrentTime()
	{
		TimeService.Reset();
		_currentTime = null;
		TimeService.AddUtcNowProvider(() => CurrentTime);
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
		return UtilityExtensions.Wait(action, timeout, delay, useTimeService);
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
		return UtilityExtensions.Wait(cancellationPending, value, delay, minimum, maximum, useTimeService);
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
	protected object GetModelWithNonDefaultValues(Type type, Func<PropertyInfo, object> nonSupportedType = null, params string[] exclusions)
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
	protected object GetNonDefaultValue(PropertyInfo propertyInfo, Func<PropertyInfo, object> nonSupportedType = null)
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
		var allExclusions = model.GetExclusions();
		allExclusions.AddRange(exclusions);

		var properties = model
			.GetCachedProperties()
			.Where(x => x.CanWrite)
			.Where(x => !allExclusions.Contains(x.Name))
			.ToList();

		foreach (var property in properties)
		{
			var value = property.GetValue(model);
			var defaultValue = property.PropertyType.GetDefaultValue();

			if (Equals(value, defaultValue))
			{
				throw new Exception($"Property {property.Name} should have been set but was not.");
			}
		}
	}

	/// <summary>
	/// Validate a model that is "IUnwrappable"
	/// </summary>
	/// <param name="model"> The model to test. </param>
	/// <param name="exclusions"> An optional set of exclusions. </param>
	protected void ValidateUnwrap(object model, params string[] exclusions)
	{
		if (model is IUnwrappable unwrappable)
		{
			var actual = unwrappable.Unwrap();
			AreEqual(model, actual, exclusions);
		}

		if (model is Entity entity)
		{
			var actual = entity.Unwrap();
			AreEqual(model, actual, exclusions);
		}
	}

	/// <summary>
	/// Validate a model that is "IUpdatable"
	/// </summary>
	/// <param name="model"> The model to test. </param>
	/// <param name="exclusions"> An optional set of exclusions. </param>
	protected void ValidateUpdatableModel(object model, params string[] exclusions)
	{
		ValidateUnwrap(model, exclusions);
		ValidateUpdateWith(model, false, exclusions);
		ValidateUpdateWith(model, true, exclusions);
		ValidateAllValuesAreNotDefault(model, exclusions);
	}

	/// <summary>
	/// Validate a model's UpdateWith using the "IUpdatable" interface.
	/// </summary>
	/// <param name="update"> The model to test. </param>
	/// <param name="excludeVirtuals"> Exclude virtuals during the test. </param>
	/// <param name="exclusions"> An optional set of exclusions. </param>
	protected void ValidateUpdateWith(object update, bool excludeVirtuals, params string[] exclusions)
	{
		var updateType = update.GetType();
		var actual = Activator.CreateInstance(updateType) as IUpdatable;
		var allExclusions = new List<string>(exclusions);

		Assert.IsNotNull(actual);

		if (excludeVirtuals)
		{
			actual.UpdateWith(update, true, exclusions);
			allExclusions.AddRange(updateType.GetVirtualPropertyNames());
		}
		else
		{
			actual.UpdateWith(update, exclusions);
		}

		AreEqual(update, actual, allExclusions.ToArray());
	}

	#endregion
}