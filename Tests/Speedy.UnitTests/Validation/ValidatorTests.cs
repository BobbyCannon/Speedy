﻿#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Data.SyncApi;
using Speedy.Exceptions;
using Speedy.Protocols.Osc;
using Speedy.Validation;

#endregion

namespace Speedy.UnitTests.Validation
{
	[TestClass]
	public class ValidatorTests : SpeedyUnitTest
	{
		#region Methods

		[TestMethod]
		public void HasMinMaxRange()
		{
			(Func<Validator, PropertyValidator> setup, object min, object max)[] scenarios =
			{
				(x => x.Property<Sample, DateTime>(p => p.Date1), min: DateTime.MinValue, max: DateTime.MaxValue),
				(x => x.Property<Sample, OscTimeTag>(p => p.TimeTag1), min: OscTimeTag.MinValue, max: OscTimeTag.MaxValue),
				(x => x.Property<Sample, TimeSpan>(p => p.Elapsed),TimeSpan.MinValue, TimeSpan.MaxValue),
				(x => x.Property<Sample, double>(p => p.Total), -123.456, 123.456),
				(x => x.Property<Sample, float>(p => p.Precision), -654.321f, 654.321f),
				(x => x.Property<Sample, decimal>(p => p.Price), -789.35m, 789.35m),
			};

			foreach (var scenario in scenarios)
			{
				var sample = new Sample();
				var validator = new Validator<Sample>();
				var p = scenario.setup(validator);
				var message = ValidationException.GetErrorMessage(ValidationExceptionType.MinMaxRange, p.Info.Name);

				// We will exclude the ranges so the min and max should NOT be valid
				p.HasMinMaxRange(scenario.min, scenario.max, true);

				ExpectedException<ValidationException>(() =>
				{
					p.SetValue(sample, scenario.min);
					Assert.AreEqual(scenario.min, p.GetValue(sample));
					validator.Validate(sample);
				}, message);

				ExpectedException<ValidationException>(() =>
				{
					p.SetValue(sample, scenario.max);
					Assert.AreEqual(scenario.max, p.GetValue(sample));
					validator.Validate(sample);
				}, message);

				// We will no longer exclude the ranges so the min and max should be valid
				validator = new Validator<Sample>();
				p = scenario.setup(validator);
				p.HasMinMaxRange(scenario.min, scenario.max, false);

				p.SetValue(sample, scenario.min);
				Assert.AreEqual(scenario.min, p.GetValue(sample));
				validator.Validate(sample);

				p.SetValue(sample, scenario.max);
				Assert.AreEqual(scenario.max, p.GetValue(sample));
				validator.Validate(sample);
			}
		}

		[TestMethod]
		public void HasValidValue()
		{
			var update = new PartialUpdate<Sample>();
			update.ValidateProperty(x => x.LogLevel).HasEnumValue();
			update.Validate();
			update.Set(x => x.LogLevel, (LogLevel) 99);
			ExpectedException<ValidationException>(() => update.Validate(), 
				ValidationException.GetErrorMessage(ValidationExceptionType.EnumRange, nameof(Sample.LogLevel)));

			update = new PartialUpdate<Sample>();
			update.ValidateProperty(x => x.OptionalLogLevel).HasEnumValue();
			update.Validate();
			update.Set(x => x.OptionalLogLevel, (LogLevel?) 99);
			ExpectedException<ValidationException>(() => update.Validate(),
				ValidationException.GetErrorMessage(ValidationExceptionType.EnumRange, nameof(Sample.LogLevel)));

			update = new PartialUpdate<Sample>();
			update.ValidateProperty(x => x.OptionalLogLevel).HasEnumValue();
			update.Validate();
			update.Set(nameof(Sample.OptionalLogLevel), (LogLevel) 99);
			ExpectedException<ValidationException>(() => update.Validate(),
				ValidationException.GetErrorMessage(ValidationExceptionType.EnumRange, nameof(Sample.LogLevel)));
		}

		[TestMethod]
		public void IsFalse()
		{
			var validator = new Validator<Sample>();
			var sample = new Sample { Age = 20 };
			var expectedMessage = "Age must be less than 21.";
			validator.IsTrue(x => x.Age < 21, expectedMessage);
			validator.Validate(sample);

			sample.Age = 21;
			ExpectedException<ValidationException>(() => validator.Validate(sample), expectedMessage);
		}

		[TestMethod]
		public void IsNotNull()
		{
			var sample = new Sample { Name = null };
			var validator = new Validator<Sample>();
			validator.Property(x => x.Name).IsNotNull();
			ExpectedException<ValidationException>(() => validator.Validate(sample),
				ValidationException.GetErrorMessage(ValidationExceptionType.IsNotNull, "Name"));
			validator.Property(x => x.Name).IsNotNull("FooBar");
			ExpectedException<ValidationException>(() => validator.Validate(sample),
				ValidationException.GetErrorMessage(ValidationExceptionType.IsNotNull, "Name"));
		}

		[TestMethod]
		public void IsNotNullOrWhitespace()
		{
			var scenarios = new[] { null, string.Empty, "", " ", "   " };

			foreach (var scenario in scenarios)
			{
				var sample = new Sample { Name = scenario };
				var validator = new Validator<Sample>();
				validator.Property(x => x.Name).IsNotNullOrWhitespace();
				ExpectedException<ValidationException>(() => validator.Validate(sample), "Name is null or whitespace.");
			}
		}

		[TestMethod]
		public void IsOptional()
		{
			var update = new PartialUpdate<Sample>();
			update.ValidateProperty(x => x.Name).IsOptional();
			var actual = update.TryValidate(out var failures);
			Assert.AreEqual(true, actual);
			Assert.AreEqual(0, failures.Count);
		}

		[TestMethod]
		public void IsRequired()
		{
			var update = new PartialUpdate<Sample>();
			update.ValidateProperty(x => x.Name).IsRequired();
			var actual = update.TryValidate(out var failures);
			Assert.AreEqual(false, actual);
			Assert.AreEqual(1, failures.Count);
			Assert.AreEqual("Name", failures[0].Name);

			update.Set(x => x.Name, null);
			actual = update.TryValidate(out failures);
			Assert.AreEqual(true, actual);
			Assert.AreEqual(0, failures.Count);
		}

		[TestMethod]
		public void IsTrue()
		{
			var validator = new Validator<Sample>();
			var sample = new Sample { Age = 21 };
			var expectedMessage = "Age must be 21 or older.";
			validator.IsTrue(x => x.Age >= 21, expectedMessage);
			validator.Validate(sample);

			sample.Age = 20;
			ExpectedException<ValidationException>(() => validator.Validate(sample), expectedMessage);
		}

		[TestMethod]
		public void NoLessThan()
		{
			var sample = new Sample { Date1 = DateTime.Today };
			var validator = new Validator<Sample>();
			validator.Property(x => x.Date1).NoLessThan(DateTime.Today);
			validator.Validate(sample);
			sample.Date1 -= TimeSpan.FromTicks(1);
			ExpectedException<ValidationException>(() => validator.Validate(sample),
				ValidationException.GetErrorMessage(ValidationExceptionType.NoLessThan, nameof(Sample.Date1)));

			sample = new Sample { Date2 = DateTime.Today };
			validator = new Validator<Sample>();
			validator.Property(x => x.Date2).NoLessThan(DateTime.Today);
			validator.Validate(sample);
			sample.Date2 -= TimeSpan.FromTicks(1);
			ExpectedException<ValidationException>(() => validator.Validate(sample),
				ValidationException.GetErrorMessage(ValidationExceptionType.NoLessThan, nameof(Sample.Date2)));

			sample = new Sample { TimeTag1 = new OscTimeTag(new DateTime(2022, 05, 03, 12, 00, 00, DateTimeKind.Utc)) };
			validator = new Validator<Sample>();
			validator.Property(x => x.TimeTag1).NoLessThan(sample.TimeTag1);
			validator.Validate(sample);
			sample.TimeTag1 -= TimeSpan.FromTicks(5);
			ExpectedException<ValidationException>(() => validator.Validate(sample),
				ValidationException.GetErrorMessage(ValidationExceptionType.NoLessThan, nameof(Sample.TimeTag1)));

			sample = new Sample { Elapsed = TimeSpan.FromSeconds(10) };
			validator = new Validator<Sample>();
			validator.Property(x => x.Elapsed).NoLessThan(sample.Elapsed);
			validator.Validate(sample);
			sample.Elapsed -= TimeSpan.FromTicks(1);
			ExpectedException<ValidationException>(() => validator.Validate(sample),
				ValidationException.GetErrorMessage(ValidationExceptionType.NoLessThan, nameof(Sample.Elapsed)));
		}

		[TestMethod]
		public void NoMoreThan()
		{
			var sample = new Sample { Date1 = DateTime.Today };
			var validator = new Validator<Sample>();
			validator.Property(x => x.Date1).NoMoreThan(DateTime.Today);
			validator.Validate(sample);
			sample.Date1 += TimeSpan.FromTicks(1);
			ExpectedException<ValidationException>(() => validator.Validate(sample),
				ValidationException.GetErrorMessage(ValidationExceptionType.NoMoreThan, nameof(Sample.Date1)));

			sample = new Sample { Date2 = DateTime.Today };
			validator = new Validator<Sample>();
			validator.Property(x => x.Date2).NoMoreThan(DateTime.Today);
			validator.Validate(sample);
			sample.Date2 += TimeSpan.FromTicks(1);
			ExpectedException<ValidationException>(() => validator.Validate(sample),
				ValidationException.GetErrorMessage(ValidationExceptionType.NoMoreThan, nameof(Sample.Date2)));

			sample = new Sample { TimeTag1 = new OscTimeTag(new DateTime(2022, 05, 03, 12, 00, 00, DateTimeKind.Utc)) };
			validator = new Validator<Sample>();
			validator.Property(x => x.TimeTag1).NoMoreThan(sample.TimeTag1);
			validator.Validate(sample);
			sample.TimeTag1 += TimeSpan.FromTicks(5);
			ExpectedException<ValidationException>(() => validator.Validate(sample),
				ValidationException.GetErrorMessage(ValidationExceptionType.NoMoreThan, nameof(Sample.TimeTag1)));

			sample = new Sample { Elapsed = TimeSpan.FromSeconds(10) };
			validator = new Validator<Sample>();
			validator.Property(x => x.Elapsed).NoMoreThan(sample.Elapsed);
			validator.Validate(sample);
			sample.Elapsed += TimeSpan.FromTicks(1);
			ExpectedException<ValidationException>(() => validator.Validate(sample),
				ValidationException.GetErrorMessage(ValidationExceptionType.NoMoreThan, nameof(Sample.Elapsed)));
		}

		#endregion

		#region Classes

		public class Sample
		{
			#region Properties

			public int Age { get; set; }

			public DateTime Date1 { get; set; }

			public DateTimeOffset Date2 { get; set; }

			public TimeSpan Elapsed { get; set; }

			public LogLevel LogLevel { get; set; }

			public string Name { get; set; }

			public LogLevel? OptionalLogLevel { get; set; }

			public float Precision { get; set; }

			public decimal Price { get; set; }

			public OscTimeTag TimeTag1 { get; set; }

			public double Total { get; set; }

			#endregion
		}

		#endregion
	}
}