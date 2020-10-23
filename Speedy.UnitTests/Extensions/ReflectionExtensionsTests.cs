#region References

using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;
#pragma warning disable 169

#endregion

namespace Speedy.UnitTests.Extensions
{
	[TestClass]
	public class ReflectionExtensionsTests
	{
		#region Methods

		[TestMethod]
		public void FieldTests()
		{
			var expected = new[] { "PublicField" };
			var type = typeof(ReflectionClassTest);
			var fields = type.GetCachedFields().Select(x => x.Name).ToArray();
			TestHelper.AreEqual(expected, fields);

			expected = new[] { "_privateField", "<PublicProperty>k__BackingField", "<PrivateProperty>k__BackingField" };
			fields = type.GetCachedFields(BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic).Select(x => x.Name).ToArray();
			TestHelper.AreEqual(expected, fields);

			var type2 = typeof(ReflectionStructTest);
			expected = new[] { "PublicField2" };
			fields = type2.GetCachedFields().Select(x => x.Name).ToArray();
			TestHelper.AreEqual(expected, fields);

			expected = new[] { "_privateField2", "<PublicProperty2>k__BackingField", "<PrivateProperty2>k__BackingField" };
			fields = type2.GetCachedFields(BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic).Select(x => x.Name).ToArray();
			TestHelper.AreEqual(expected, fields);
		}

		[TestMethod]
		public void PropertyTests()
		{
			// By class type
			var expected = new[] { "PublicProperty" };
			var type = typeof(ReflectionClassTest);
			var properties = type.GetCachedProperties().Select(x => x.Name).ToArray();
			TestHelper.AreEqual(expected, properties);
			expected = new[] { "PrivateProperty" };
			properties = type.GetCachedProperties(BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic).Select(x => x.Name).ToArray();
			TestHelper.AreEqual(expected, properties);
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
}