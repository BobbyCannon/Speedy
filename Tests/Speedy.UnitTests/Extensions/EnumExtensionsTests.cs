#if NET6_0_OR_GREATER

#region References

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Extensions;
using Speedy.Sync;

#endregion

namespace Speedy.UnitTests.Extensions
{
	[TestClass]
	public class EnumExtensionsTests : SpeedyUnitTest
	{
		#region Methods

		[TestMethod]
		public void ClearFlag()
		{
			var allSettableFlags = EnumExtensions
				.GetFlagValues<SyncResultStatus>()
				.Except(new[] { SyncResultStatus.Unknown })
				.ToList();

			var actual = SyncResultStatus.Cancelled;
			allSettableFlags.ForEach(x => actual = actual.SetFlag(x));

			foreach (var flag in allSettableFlags)
			{
				Assert.IsTrue(actual.HasFlag(flag));
				actual = actual.ClearFlag(flag);
				Assert.IsFalse(actual.HasFlag(flag));
			}
		}

		[TestMethod]
		public void Count()
		{
			var value = FullValues.Unknown;
			Assert.AreEqual(4, value.Count());
			Assert.AreEqual(4, EnumExtensions.Count<FullValues>());
		}

		[TestMethod]
		public void GetDescription()
		{
			Assert.AreEqual("This is an unknown object.", FullValues.Unknown.GetDescription());
			Assert.AreEqual("This is the first value.", FullValues.Value1.GetDescription());
			Assert.AreEqual("This is the second value.", FullValues.Value2.GetDescription());
			Assert.AreEqual("Value3", FullValues.Value3.GetDescription());

			Assert.AreEqual("This is an unknown object.", ((Enum) FullValues.Unknown).GetDescription());
			Assert.AreEqual("This is the first value.", ((Enum) FullValues.Value1).GetDescription());
			Assert.AreEqual("This is the second value.", ((Enum) FullValues.Value2).GetDescription());
			Assert.AreEqual("Value3", ((Enum) FullValues.Value3).GetDescription());
		}

		[TestMethod]
		public void GetDisplayName()
		{
			Assert.AreEqual("Unknown", FullValues.Unknown.GetDisplayName());
			Assert.AreEqual("Value One", FullValues.Value1.GetDisplayName());
			Assert.AreEqual("Value Two", FullValues.Value2.GetDisplayName());
			Assert.AreEqual("Value3", FullValues.Value3.GetDisplayName());

			Assert.AreEqual("Unknown", ((Enum) FullValues.Unknown).GetDisplayName());
			Assert.AreEqual("Value One", ((Enum) FullValues.Value1).GetDisplayName());
			Assert.AreEqual("Value Two", ((Enum) FullValues.Value2).GetDisplayName());
			Assert.AreEqual("Value3", ((Enum) FullValues.Value3).GetDisplayName());
		}

		[TestMethod]
		public void GetDisplayShortName()
		{
			Assert.AreEqual("U", FullValues.Unknown.GetDisplayShortName());
			Assert.AreEqual("1", FullValues.Value1.GetDisplayShortName());
			Assert.AreEqual("2", FullValues.Value2.GetDisplayShortName());
			Assert.AreEqual("Value3", FullValues.Value3.GetDisplayShortName());

			Assert.AreEqual("U", ((Enum) FullValues.Unknown).GetDisplayShortName());
			Assert.AreEqual("1", ((Enum) FullValues.Value1).GetDisplayShortName());
			Assert.AreEqual("2", ((Enum) FullValues.Value2).GetDisplayShortName());
			Assert.AreEqual("Value3", ((Enum) FullValues.Value3).GetDisplayShortName());
		}

		[TestMethod]
		public void GetFlaggedString()
		{
			var scenarios = new Dictionary<FlaggedSample, string>
			{
				{ FlaggedSample.Unknown, "Unknown" },
				{ FlaggedSample.One, "One" },
				{ FlaggedSample.Two, "Two" },
				{ FlaggedSample.Four, "Four" },
				{ FlaggedSample.Eight, "Eight" },
				{ FlaggedSample.All, "One, Two, Four, Eight" },
				{ FlaggedSample.One | FlaggedSample.Four, "One, Four" },
				{ FlaggedSample.Eight | FlaggedSample.Two, "Two, Eight" }
			};

			foreach (var scenario in scenarios)
			{
				AreEqual(scenario.Value, scenario.Key.ToFlagsString());
			}
		}

		[TestMethod]
		public void GetFlaggedValueOfInstance()
		{
			var scenarios = new Dictionary<FlaggedSample, FlaggedSample[]>
			{
				{ FlaggedSample.Unknown, [] },
				{ FlaggedSample.One, [FlaggedSample.One] },
				{ FlaggedSample.Two, [FlaggedSample.Two] },
				{ FlaggedSample.Four, [FlaggedSample.Four] },
				{ FlaggedSample.Eight, [FlaggedSample.Eight] },
				{ FlaggedSample.All, [FlaggedSample.One, FlaggedSample.Two, FlaggedSample.Four, FlaggedSample.Eight] }
			};

			foreach (var scenario in scenarios)
			{
				AreEqual(scenario.Value, scenario.Key.GetFlaggedValues());
			}
		}

		[TestMethod]
		public void GetFlaggedValues()
		{
			var valuesByte = EnumExtensions.GetFlagValues<MyEnumByte>();
			var expectedByte = new[] { MyEnumByte.One, MyEnumByte.Two };
			AreEqual(expectedByte, valuesByte);

			var valuesSByte = EnumExtensions.GetFlagValues<MyEnumSByte>();
			var expectedSByte = new[] { MyEnumSByte.One, MyEnumSByte.Two };
			AreEqual(expectedSByte, valuesSByte);

			var valuesShort = EnumExtensions.GetFlagValues<MyEnumShort>();
			var expectedShort = new[] { MyEnumShort.One, MyEnumShort.Two };
			AreEqual(expectedShort, valuesShort);

			var valuesUnsignedShort = EnumExtensions.GetFlagValues<MyEnumUnsignedShort>();
			var expectedUnsignedShort = new[] { MyEnumUnsignedShort.One, MyEnumUnsignedShort.Two };
			AreEqual(expectedUnsignedShort, valuesUnsignedShort);

			var values1 = EnumExtensions.GetFlagValues<MyEnum>();
			var expected1 = new[] { MyEnum.One, MyEnum.Two };
			AreEqual(expected1, values1);

			var valuesUnsigned = EnumExtensions.GetFlagValues<MyEnumUnsigned>();
			var expectedUnsigned = new[] { MyEnumUnsigned.One, MyEnumUnsigned.Two };
			AreEqual(expectedUnsigned, valuesUnsigned);

			var valuesLong = EnumExtensions.GetFlagValues<MyEnumLong>();
			var expectedLong = new[] { MyEnumLong.One, MyEnumLong.Two };
			AreEqual(expectedLong, valuesLong);

			var valuesLongUnsigned = EnumExtensions.GetFlagValues<MyEnumLongUnsigned>();
			var expectedLongUnsigned = new[] { MyEnumLongUnsigned.One, MyEnumLongUnsigned.Two };
			AreEqual(expectedLongUnsigned, valuesLongUnsigned);
		}

		[TestMethod]
		public void SetClearHasFlag()
		{
			var status1 = MyEnumByte.Unknown;
			Assert.IsFalse(status1.HasFlag(MyEnumByte.One), "Flag was set");
			status1 = status1.SetFlag(MyEnumByte.One);
			Assert.IsTrue(status1.HasFlag(MyEnumByte.One), "Flag was not set");
			status1 = status1.ClearFlag(MyEnumByte.One);
			Assert.IsFalse(status1.HasFlag(MyEnumByte.One), "Flag was set");

			var status2 = MyEnumSByte.Unknown;
			Assert.IsFalse(status2.HasFlag(MyEnumSByte.One), "Flag was set");
			status2 = status2.SetFlag(MyEnumSByte.One);
			Assert.IsTrue(status2.HasFlag(MyEnumSByte.One), "Flag was not set");
			status2 = status2.ClearFlag(MyEnumSByte.One);
			Assert.IsFalse(status2.HasFlag(MyEnumSByte.One), "Flag was set");

			var status3 = MyEnumShort.Unknown;
			Assert.IsFalse(status3.HasFlag(MyEnumShort.One), "Flag was set");
			status3 = status3.SetFlag(MyEnumShort.One);
			Assert.IsTrue(status3.HasFlag(MyEnumShort.One), "Flag was not set");
			status3 = status3.ClearFlag(MyEnumShort.One);
			Assert.IsFalse(status3.HasFlag(MyEnumShort.One), "Flag was set");

			var status4 = MyEnumUnsignedShort.Unknown;
			Assert.IsFalse(status4.HasFlag(MyEnumUnsignedShort.One), "Flag was set");
			status4 = status4.SetFlag(MyEnumUnsignedShort.One);
			Assert.IsTrue(status4.HasFlag(MyEnumUnsignedShort.One), "Flag was not set");
			status4 = status4.ClearFlag(MyEnumUnsignedShort.One);
			Assert.IsFalse(status4.HasFlag(MyEnumUnsignedShort.One), "Flag was set");

			var status5 = MyEnum.Unknown;
			Assert.IsFalse(status5.HasFlag(MyEnum.One), "Flag was set");
			status5 = status5.SetFlag(MyEnum.One);
			Assert.IsTrue(status5.HasFlag(MyEnum.One), "Flag was not set");
			status5 = status5.ClearFlag(MyEnum.One);
			Assert.IsFalse(status5.HasFlag(MyEnum.One), "Flag was set");

			var status6 = MyEnumUnsigned.Unknown;
			Assert.IsFalse(status6.HasFlag(MyEnumUnsigned.One), "Flag was set");
			status6 = status6.SetFlag(MyEnumUnsigned.One);
			Assert.IsTrue(status6.HasFlag(MyEnumUnsigned.One), "Flag was not set");
			status6 = status6.ClearFlag(MyEnumUnsigned.One);
			Assert.IsFalse(status6.HasFlag(MyEnumUnsigned.One), "Flag was set");

			var status7 = MyEnumLong.Unknown;
			Assert.IsFalse(status7.HasFlag(MyEnumLong.One), "Flag was set");
			status7 = status7.SetFlag(MyEnumLong.One);
			Assert.IsTrue(status7.HasFlag(MyEnumLong.One), "Flag was not set");
			status7 = status7.ClearFlag(MyEnumLong.One);
			Assert.IsFalse(status7.HasFlag(MyEnumLong.One), "Flag was set");

			var status8 = MyEnumLongUnsigned.Unknown;
			Assert.IsFalse(status8.HasFlag(MyEnumLongUnsigned.One), "Flag was set");
			status8 = status8.SetFlag(MyEnumLongUnsigned.One);
			Assert.IsTrue(status8.HasFlag(MyEnumLongUnsigned.One), "Flag was not set");
			status8 = status8.ClearFlag(MyEnumLongUnsigned.One);
			Assert.IsFalse(status8.HasFlag(MyEnumLongUnsigned.One), "Flag was set");
		}

		[TestMethod]
		public void SetClearHasFlagTestGenerator()
		{
			var scenarios = new (Enum value, Enum flag)[]
			{
				(MyEnumByte.Unknown, MyEnumByte.One),
				(MyEnumSByte.Unknown, MyEnumSByte.One),
				(MyEnumShort.Unknown, MyEnumShort.One),
				(MyEnumUnsignedShort.Unknown, MyEnumUnsignedShort.One),
				(MyEnum.Unknown, MyEnum.One),
				(MyEnumUnsigned.Unknown, MyEnumUnsigned.One),
				(MyEnumLong.Unknown, MyEnumLong.One),
				(MyEnumLongUnsigned.Unknown, MyEnumLongUnsigned.One)
			};

			var builder = new StringBuilder();
			var count = 1;

			foreach (var scenario in scenarios)
			{
				var name = $"status{count++}";
				builder.AppendLine($"var {name} = {scenario.value.GetType().Name}.{scenario.value};");
				builder.AppendLine($"Assert.IsFalse({name}.HasFlag({scenario.flag.GetType().Name}.{scenario.flag}), \"Flag was set\");");
				builder.AppendLine($"{name} = {name}.SetFlag({scenario.flag.GetType().Name}.{scenario.flag});");
				builder.AppendLine($"Assert.IsTrue({name}.HasFlag({scenario.flag.GetType().Name}.{scenario.flag}), \"Flag was not set\");");
				builder.AppendLine($"{name} = {name}.ClearFlag({scenario.flag.GetType().Name}.{scenario.flag});");
				builder.AppendLine($"Assert.IsFalse({name}.HasFlag({scenario.flag.GetType().Name}.{scenario.flag}), \"Flag was set\");");
				builder.AppendLine();
			}

			builder.CopyToClipboard().Dump();
		}

		[TestMethod]
		public void SetFlag()
		{
			var actual = SyncResultStatus.Unknown;
			var allButUnknown = EnumExtensions
				.GetFlagValues<SyncResultStatus>()
				.Except(new[] { actual })
				.ToList();

			foreach (var flag in allButUnknown)
			{
				Assert.IsFalse(actual.HasFlag(flag));
			}

			foreach (var flag in allButUnknown)
			{
				actual = SyncResultStatus.Unknown.SetFlag(flag);
				Assert.IsTrue(actual.HasFlag(flag));

				var allButFlag = allButUnknown.Except(new[] { flag });
				foreach (var fagNotSet in allButFlag)
				{
					Assert.IsFalse(actual.HasFlag(fagNotSet));
				}
			}
		}

		[TestMethod]
		public void UpdateFlag()
		{
			var allSettableFlags = EnumExtensions
				.GetFlagValues<SyncResultStatus>()
				.Except(new[] { SyncResultStatus.Unknown })
				.ToList();

			var allFlagsSet = SyncResultStatus.Cancelled;
			allSettableFlags.ForEach(x => allFlagsSet = allFlagsSet.SetFlag(x));

			foreach (var singleFlag in allSettableFlags)
			{
				var actual = SyncResultStatus.Unknown;
				actual = actual.UpdateFlag(allFlagsSet, singleFlag);
				Assert.AreEqual(singleFlag, actual);
			}
		}

		#endregion

		#region Enumerations

		[Flags]
		public enum FlaggedSample
		{
			Unknown = 0,
			One = 0b0001,
			Two = 0b0010,
			Four = 0b0100,
			Eight = 0b1000,
			All = 0b1111
		}

		public enum FullValues
		{
			[Display(Description = "This is an unknown object.", Name = "Unknown", ShortName = "U")]
			Unknown,

			[Display(Description = "This is the first value.", Name = "Value One", ShortName = "1")]
			Value1,

			[Display(Description = "This is the second value.", Name = "Value Two", ShortName = "2")]
			Value2,

			// No attributes
			Value3
		}

		[Flags]
		public enum MyEnum
		{
			Unknown = 0,

			//      10987654321098765432109876543210
			One = 0b00000000000000000000000000000001,
			Two = 0b00000000000000000000000000000010,
			Max = 0b01111111111111111111111111111111
		}

		[Flags]
		public enum MyEnumByte : byte
		{
			Unknown = 0,

			//      76543210
			One = 0b00000001,
			Two = 0b00000010,
			Max = 0b11111111
		}

		[Flags]
		public enum MyEnumLong : long
		{
			Unknown = 0,

			//      3210987654321098765432109876543210987654321098765432109876543210
			One = 0b0000000000000000000000000000000000000000000000000000000000000001,
			Two = 0b0000000000000000000000000000000000000000000000000000000000000010,
			Max = 0b0111111111111111111111111111111111111111111111111111111111111111
		}

		[Flags]
		public enum MyEnumLongUnsigned : ulong
		{
			Unknown = 0,

			//      3210987654321098765432109876543210987654321098765432109876543210
			One = 0b0000000000000000000000000000000000000000000000000000000000000001,
			Two = 0b0000000000000000000000000000000000000000000000000000000000000010,
			Max = 0b1111111111111111111111111111111111111111111111111111111111111111
		}

		[Flags]
		public enum MyEnumSByte : sbyte
		{
			Unknown = 0,

			//      76543210
			One = 0b00000001,
			Two = 0b00000010,
			Max = 0b01111111
		}

		[Flags]
		public enum MyEnumShort : short
		{
			Unknown = 0,

			//      5432109876543210
			One = 0b0000000000000001,
			Two = 0b0000000000000010,
			Max = 0b0111111111111111
		}

		[Flags]
		public enum MyEnumUnsigned : uint
		{
			Unknown = 0,

			//      10987654321098765432109876543210
			One = 0b00000000000000000000000000000001,
			Two = 0b00000000000000000000000000000010,
			Max = 0b11111111111111111111111111111111
		}

		[Flags]
		public enum MyEnumUnsignedShort : ushort
		{
			Unknown = 0,

			//      5432109876543210
			One = 0b0000000000000001,
			Two = 0b0000000000000010,
			Max = 0b1111111111111111
		}

		#endregion
	}
}

#endif