#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests.Extensions
{
	[TestClass]
	public class EnumExtensionsTests
	{
		#region Methods

		[TestMethod]
		public void MyEnumByteType()
		{
			var status = MyEnumByte.Unknown;
			status = status.SetFlag(MyEnumByte.One);

			Assert.IsTrue((status & MyEnumByte.One) == MyEnumByte.One, "Flag was not set");
		}

		[TestMethod]
		public void MyEnumLongType()
		{
			var status = MyEnumLong.Unknown;
			status = status.SetFlag(MyEnumLong.One);

			Assert.IsTrue((status & MyEnumLong.One) == MyEnumLong.One, "Flag was not set");
		}

		[TestMethod]
		public void MyEnumLongUnsignedType()
		{
			var status = MyEnumLongUnsigned.Unknown;
			status = status.SetFlag(MyEnumLongUnsigned.One);

			Assert.IsTrue((status & MyEnumLongUnsigned.One) == MyEnumLongUnsigned.One, "Flag was not set");
		}

		[TestMethod]
		public void MyEnumType()
		{
			var status = MyEnum.Unknown;
			status = status.SetFlag(MyEnum.One);

			Assert.IsTrue((status & MyEnum.One) == MyEnum.One, "Flag was not set");
		}

		[TestMethod]
		public void MyEnumUnsignedType()
		{
			var status = MyEnumUnsigned.Unknown;
			status = status.SetFlag(MyEnumUnsigned.One);

			Assert.IsTrue((status & MyEnumUnsigned.One) == MyEnumUnsigned.One, "Flag was not set");
		}

		#endregion

		#region Enumerations

		[Flags]
		public enum MyEnum
		{
			Unknown = 0,
			One = 0b00000000000000000000000000000001,
			Max = 0b01000000000000000000000000000000
		}

		[Flags]
		public enum MyEnumByte : byte
		{
			Unknown = 0,
			One = 0b00000001,
			Max = 0b10000000
		}

		[Flags]
		public enum MyEnumLong : long
		{
			Unknown = 0,
			One = 0b0000000000000000000000000000000000000000000000000000000000000001,
			Max = 0b0100000000000000000000000000000000000000000000000000000000000000
		}

		[Flags]
		public enum MyEnumLongUnsigned : ulong
		{
			Unknown = 0,
			One = 0b0000000000000000000000000000000000000000000000000000000000000001,
			Max = 0b1000000000000000000000000000000000000000000000000000000000000000
		}

		[Flags]
		public enum MyEnumUnsigned : uint
		{
			Unknown = 0,
			One = 0b00000000000000000000000000000001,
			Max = 0b01000000000000000000000000000000
		}

		#endregion
	}
}