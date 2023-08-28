#region References

using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Speedy.Data;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests.Data;

[TestClass]
public class RuntimeInformationShouldCache
{
	#region Methods

	[TestMethod]
	public void GetRuntimeInformation()
	{
		var mock = new Mock<RuntimeInformation>();
		var scenarios = new Test[]
		{
			new MockSetup<RuntimeInformation, Bitness>(mock, "ApplicationBitness", Bitness.X86),
			new MockSetup<RuntimeInformation, bool>(mock, "ApplicationIsDevelopmentBuild", true),
			new MockSetup<RuntimeInformation, bool>(mock, "ApplicationIsElevated", true),
			new MockSetup<RuntimeInformation, string>(mock, "ApplicationDataLocation", "C:\\test"),
			new MockSetup<RuntimeInformation, string>(mock, "ApplicationLocation", "C:\\test"),
			new MockSetup<RuntimeInformation, string>(mock, "ApplicationName", "Foo Bar"),
			new MockSetup<RuntimeInformation, Version>(mock, "ApplicationVersion", new Version(1, 2, 3, 4)),
			new MockSetup<RuntimeInformation, string>(mock, "DeviceId", "ABC-123"),
			new MockSetup<RuntimeInformation, string>(mock, "DeviceManufacturer", "Sony"),
			new MockSetup<RuntimeInformation, string>(mock, "DeviceModel", "TV-Two"),
			new MockSetup<RuntimeInformation, string>(mock, "DeviceName", "Vision"),
			new MockSetup<RuntimeInformation, DevicePlatform>(mock, "DevicePlatform", DevicePlatform.Windows),
			new MockSetup<RuntimeInformation, Bitness>(mock, "DevicePlatformBitness", Bitness.X64),
			new MockSetup<RuntimeInformation, Version>(mock, "DevicePlatformVersion", new Version(4, 3, 2, 1)),
			new MockSetup<RuntimeInformation, DeviceType>(mock, "DeviceType", DeviceType.Desktop)
		};

		var allProperties = typeof(RuntimeInformation)
			.GetCachedProperties()
			.Select(x => x.Name)
			.ToList();
		var missingProperties = allProperties
			.Except(scenarios.Select(x => x.Name))
			.ToList();

		Assert.IsTrue(missingProperties.Count == 0, string.Join("\r\n", missingProperties));

		foreach (var scenario in scenarios)
		{
			var actual = scenario.GetValue();
			Assert.AreEqual(scenario.Original, actual);
			Assert.AreEqual(scenario.Value, actual);

			// Reverse should not break cache
			scenario.Reverse();
			actual = scenario.GetValue();
			Assert.AreEqual(scenario.Original, actual);
			Assert.AreNotEqual(scenario.Value, actual);

			// Reset cache should allow properties to get reread
			mock.Object.ResetCache();
			actual = scenario.GetValue();
			Assert.AreNotEqual(scenario.Original, actual);
			Assert.AreEqual(scenario.Value, actual);
		}
	}

	#endregion

	#region Classes

	public class MockSetup<T, T2> : Test where T : class
	{
		#region Fields

		private readonly Mock<T> _p;

		#endregion

		#region Constructors

		public MockSetup(Mock<T> p, string name, T2 value)
		{
			_p = p;

			Name = name;
			Original = value;
			Value = value;

			p.Protected().Setup<T2>("Get" + name).Returns(() => (T2) Value);

			PropertyInfo = typeof(T).GetCachedProperty(name);
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public override object GetValue()
		{
			return PropertyInfo.GetValue(_p.Object);
		}

		#endregion
	}

	public abstract class Test
	{
		#region Properties

		public string Name { get; protected set; }

		public object Original { get; set; }

		public PropertyInfo PropertyInfo { get; set; }

		public object Value { get; set; }

		#endregion

		#region Methods

		public abstract object GetValue();

		public void Reverse()
		{
			Value = Value switch
			{
				bool bValue => !bValue,
				string sValue => sValue.ReverseString(),
				Bitness bitness => bitness == Bitness.X64 ? Bitness.X86 : Bitness.X64,
				DevicePlatform p => p == DevicePlatform.Windows ? DevicePlatform.Android : DevicePlatform.Windows,
				DeviceType t => t == DeviceType.Desktop ? DeviceType.Phone : DeviceType.Desktop,
				Version v => new Version(v.Revision, v.Build, v.Minor, v.Major),
				_ => Value
			};
		}

		#endregion
	}

	#endregion
}