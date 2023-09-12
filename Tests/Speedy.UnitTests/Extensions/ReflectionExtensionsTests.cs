#region References

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Converters;
using Speedy.Extensions;
using Speedy.Protocols.Osc;
using Speedy.Website.Data.Entities;

#pragma warning disable 169

#endregion

namespace Speedy.UnitTests.Extensions;

[TestClass]
public class ReflectionExtensionsTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void CreateInstanceForGenerics()
	{
		var scenarios = new ICollection<int>[]
		{
			new Collection<int>(new[] { 1, 2, 3 }),
			new List<int>(new[] { 2, 4, 5 }),
			new HashSet<int>(new[] { 6, 7, 8 })
		};

		foreach (var list in scenarios)
		{
			var type = list.GetType();
			type.FullName.Dump();
			AreEqual(list, type.GetDefaultValue(list), type.FullName);

			if (!list.IsReadOnly)
			{
				list.Clear();
				AreEqual(list, type.GetDefaultValue(), type.FullName);
			}
		}
	}

	[TestMethod]
	public void FieldTests()
	{
		var expected = new[] { "PublicField" };
		var type = typeof(ReflectionClassTest);
		var fields = type.GetCachedFields().Select(x => x.Name).ToArray();
		AreEqual(expected, fields);

		expected = new[] { "_privateField", "<PublicProperty>k__BackingField", "<PrivateProperty>k__BackingField" };
		fields = type.GetCachedFields(BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic).Select(x => x.Name).ToArray();
		AreEqual(expected, fields);

		var type2 = typeof(ReflectionStructTest);
		expected = new[] { "PublicField2" };
		fields = type2.GetCachedFields().Select(x => x.Name).ToArray();
		AreEqual(expected, fields);

		expected = new[] { "_privateField2", "<PublicProperty2>k__BackingField", "<PrivateProperty2>k__BackingField" };
		fields = type2.GetCachedFields(BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic).Select(x => x.Name).ToArray();
		AreEqual(expected, fields);
	}

	[TestMethod]
	public void GetDefaultValue()
	{
		var scenarios = new Dictionary<Type, object>
		{
			{ typeof(bool), false },
			{ typeof(byte), (byte) 0 },
			{ typeof(sbyte), (sbyte) 0 },
			{ typeof(char), (char) 0 },
			{ typeof(short), (short) 0 },
			{ typeof(ushort), (ushort) 0 },
			{ typeof(int), 0 },
			{ typeof(int?), new int?() },
			{ typeof(uint), (uint) 0 },
			{ typeof(long), (long) 0 },
			{ typeof(ulong), (ulong) 0 },
			{ typeof(float), (float) 0 },
			{ typeof(double), (double) 0 },
			{ typeof(decimal), (decimal) 0 },
			{ typeof(string), null },
			{ typeof(void), null },
			{ typeof(IntPtr), IntPtr.Zero },
			{ typeof(UIntPtr), UIntPtr.Zero },
			{ typeof(DateTime), DateTime.MinValue },
			{ typeof(DateTimeOffset), DateTimeOffset.MinValue },
			{ typeof(Guid), Guid.Empty },
			{ typeof(ShortGuid), ShortGuid.Empty },
			{ typeof(TimeSpan), TimeSpan.Zero },
			{ typeof(OscTimeTag), OscTimeTag.MinValue }
		};

		var missingTypes = ObjectConverter
			.AllTypes
			.Except(scenarios.Keys)
			.ToList();

		if (missingTypes.Any())
		{
			string.Join(", ", missingTypes).Dump();
			Assert.Fail("Missing types...");
		}

		foreach (var scenario in scenarios)
		{
			scenario.Key.FullName.Dump();
			Assert.AreEqual(scenario.Value, scenario.Key.GetDefaultValue(), scenario.Key.FullName);
		}
	}

	[TestMethod]
	public void GetNonDefaultValue()
	{
		var types = ObjectConverter.AllTypes.ToList();

		foreach (var type in ObjectConverter.AllTypes)
		{
			if (!type.IsNullable())
			{
				types.Add(typeof(Nullable<>).MakeGenericType(type));
			}
		}

		foreach (var type in types)
		{
			var defaultValue = type.GetDefaultValue();
			var nonDefaultValue = type.CreateInstanceWithNonDefaultValue();

			if (type.IsValueType && (nonDefaultValue == null))
			{
				throw new InvalidOperationException($"{type.FullName}: Non-default value is null but data type is value type");
			}

			AreNotEqual(defaultValue, nonDefaultValue, () => type.FullName);
		}
	}

	[TestMethod]
	public void IsAbstractAndIsVirtual()
	{
		//var type = typeof(MockTests.ITestMockPropertyGetOnly);
		//var info = type.GetCachedProperty(nameof(MockTests.ITestMockPropertyGetOnly.IsMock));
		//IsTrue(info.IsAbstract);
		//IsTrue(info.IsVirtual);

		var type = typeof(Entity<int>);
		var info = type.GetCachedProperty(nameof(Entity<int>.Id));
		IsTrue(info.IsAbstract);
		IsTrue(info.IsVirtual);

		type = typeof(AccountEntity);
		info = type.GetCachedProperty(nameof(AccountEntity.Id));
		IsFalse(info.IsAbstract);
		IsFalse(info.IsVirtual);
	}

	[TestMethod]
	public void MethodTests()
	{
		// Get Method Info by Name
		var type = typeof(ReflectionClassTest);
		var methodInfo = type.GetCachedMethod("MethodOne");
		Assert.AreNotEqual(null, methodInfo);
		Assert.AreEqual("MethodOne", methodInfo.Name);
	}

	[TestMethod]
	public void PropertyTests()
	{
		// By class type
		var expected = new[] { "PublicProperty" };
		var type = typeof(ReflectionClassTest);
		var properties = type.GetCachedProperties().Select(x => x.Name).ToArray();
		AreEqual(expected, properties);
		expected = new[] { "PrivateProperty" };
		properties = type.GetCachedProperties(BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic).Select(x => x.Name).ToArray();
		AreEqual(expected, properties);
	}

	[TestMethod]
	public void RemoveEventHandlers()
	{
		var account = new AccountEntity();
		var eventInfos = account.GetCachedEventFields();
		var propertyChangedEventInfo = eventInfos.FirstOrDefault(x => x.Name == nameof(PropertyChanged));
		Assert.IsNotNull(propertyChangedEventInfo);

		var actual = propertyChangedEventInfo.GetValue(account);
		Assert.IsNull(actual);

		account.PropertyChanged += (sender, args) => { };

		actual = propertyChangedEventInfo.GetValue(account);
		Assert.IsNotNull(actual);

		account.RemoveEventHandlers();

		actual = propertyChangedEventInfo.GetValue(account);
		Assert.IsNull(actual);
	}

	#endregion

	#region Classes

	public class ReflectionClassTest
	{
		#region Fields

		public int PublicField;

		private string _privateField;

		#endregion

		#region Properties

		public double PublicProperty { get; set; }

		private double PrivateProperty { get; set; }

		#endregion

		#region Methods

		public void MethodOne()
		{
		}

		#endregion
	}

	#endregion

	#region Structures

	public struct ReflectionStructTest
	{
		#region Fields

		public int PublicField2;

		private string _privateField2;

		#endregion

		#region Properties

		public double PublicProperty2 { get; set; }

		private double PrivateProperty2 { get; set; }

		#endregion
	}

	#endregion
}