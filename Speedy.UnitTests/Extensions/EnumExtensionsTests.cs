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
		public void SetClearHasFlag()
		{
			var status = MyEnum.Unknown;
			Assert.IsFalse(status.HasFlag(MyEnum.One), "Flag was set");
			status = status.SetFlag(MyEnum.One);
			Assert.IsTrue(status.HasFlag(MyEnum.One), "Flag was not set");
			status = status.ClearFlag(MyEnum.One);
			Assert.IsFalse(status.HasFlag(MyEnum.One), "Flag was set");

			var status2 = MyEnumUnsigned.Unknown;
			Assert.IsFalse(status2.HasFlag(MyEnumUnsigned.One), "Flag was set");
			status2 = status2.SetFlag(MyEnumUnsigned.One);
			Assert.IsTrue(status2.HasFlag(MyEnumUnsigned.One), "Flag was not set");
			status2 = status2.ClearFlag(MyEnumUnsigned.One);
			Assert.IsFalse(status2.HasFlag(MyEnumUnsigned.One), "Flag was set");

			var status3 = MyEnumLongUnsigned.Unknown;
			Assert.IsFalse(status3.HasFlag(MyEnumLongUnsigned.One), "Flag was set");
			status3 = status3.SetFlag(MyEnumLongUnsigned.One);
			Assert.IsTrue(status3.HasFlag(MyEnumLongUnsigned.One), "Flag was not set");
			status3 = status3.ClearFlag(MyEnumLongUnsigned.One);
			Assert.IsFalse(status3.HasFlag(MyEnumLongUnsigned.One), "Flag was set");

			var status4 = MyEnumByte.Unknown;
			Assert.IsFalse(status4.HasFlag(MyEnumByte.One), "Flag was set");
			status4 = status4.SetFlag(MyEnumByte.One);
			Assert.IsTrue(status4.HasFlag(MyEnumByte.One), "Flag was not set");
			status4 = status4.ClearFlag(MyEnumByte.One);
			Assert.IsFalse(status4.HasFlag(MyEnumByte.One), "Flag was set");

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