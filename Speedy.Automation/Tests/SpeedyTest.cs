#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using KellermanSoftware.CompareNetObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;
using Speedy.Storage;

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
	/// Sets the clipboard provider.
	/// </summary>
	/// <param name="provider"> The provider to be set. </param>
	public static void SetClipboardProvider(Action<string> provider)
	{
		_clipboardProvider = provider;
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