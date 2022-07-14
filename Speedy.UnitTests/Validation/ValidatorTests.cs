#region References

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
	public class ValidatorTests
	{
		#region Methods

		[TestMethod]
		public void HasMinMaxRange()
		{
			var sample = new Sample { Date1 = DateTime.MinValue };
			var validator = new Validator<Sample>();
			validator.Property(x => x.Date1).HasMinMaxRange(DateTime.MinValue, DateTime.MaxValue, true);
			TestHelper.ExpectedException<ValidationException>(() => validator.Validate(sample), "Date1 is not within the provided range values.");
			sample.Date1 = DateTime.MaxValue;
			TestHelper.ExpectedException<ValidationException>(() => validator.Validate(sample), "Date1 is not within the provided range values.");

			sample = new Sample { Date2 = DateTimeOffset.MinValue };
			validator = new Validator<Sample>();
			validator.Property(x => x.Date2).HasMinMaxRange(DateTimeOffset.MinValue, DateTimeOffset.MaxValue, true);
			TestHelper.ExpectedException<ValidationException>(() => validator.Validate(sample), "Date2 is not within the provided range values.");
			sample.Date2 = DateTimeOffset.MaxValue;
			TestHelper.ExpectedException<ValidationException>(() => validator.Validate(sample), "Date2 is not within the provided range values.");

			sample = new Sample { TimeTag1 = OscTimeTag.MinValue };
			validator = new Validator<Sample>();
			validator.Property(x => x.TimeTag1).HasMinMaxRange(OscTimeTag.MinValue, OscTimeTag.MaxValue, true);
			TestHelper.ExpectedException<ValidationException>(() => validator.Validate(sample), "TimeTag1 is not within the provided range values.");
			sample.TimeTag1 = OscTimeTag.MaxValue;
			TestHelper.ExpectedException<ValidationException>(() => validator.Validate(sample), "TimeTag1 is not within the provided range values.");

			sample = new Sample { Elapsed = TimeSpan.MinValue };
			validator = new Validator<Sample>();
			validator.Property(x => x.Elapsed).HasMinMaxRange(TimeSpan.MinValue, TimeSpan.MaxValue, true);
			TestHelper.ExpectedException<ValidationException>(() => validator.Validate(sample), "Elapsed is not within the provided range values.");
			sample.Elapsed = TimeSpan.MaxValue;
			TestHelper.ExpectedException<ValidationException>(() => validator.Validate(sample), "Elapsed is not within the provided range values.");
		}

		[TestMethod]
		public void HasValidValue()
		{
			var update = new PartialUpdate<Sample>();
			update.Validate(x => x.LogLevel).HasValidValue();
			update.Validate();
			update.Set(x => x.LogLevel, (LogLevel) 99);
			TestHelper.ExpectedException<ValidationException>(() => update.Validate(), "LogLevel does not contain a valid value.");
			
			update = new PartialUpdate<Sample>();
			update.Validate(x => x.OptionalLogLevel).HasValidValue();
			update.Validate();
			update.Set(x => x.OptionalLogLevel, (LogLevel?) 99);
			TestHelper.ExpectedException<ValidationException>(() => update.Validate(), "LogLevel does not contain a valid value.");
			
			update = new PartialUpdate<Sample>();
			update.Validate(x => x.OptionalLogLevel).HasValidValue();
			update.Validate();
			update.Set(nameof(Sample.OptionalLogLevel), (LogLevel) 99);
			TestHelper.ExpectedException<ValidationException>(() => update.Validate(), "LogLevel does not contain a valid value.");
		}

		[TestMethod]
		public void IsNotNull()
		{
			var sample = new Sample { Name = null };
			var validator = new Validator<Sample>();
			validator.Property(x => x.Name).IsNotNull();
			TestHelper.ExpectedException<ValidationException>(() => validator.Validate(sample), "Name is null.");
			validator.Property(x => x.Name).IsNotNull("FooBar");
			TestHelper.ExpectedException<ValidationException>(() => validator.Validate(sample), "FooBar");
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
				TestHelper.ExpectedException<ValidationException>(() => validator.Validate(sample), "Name is null or whitespace.");
			}
		}

		[TestMethod]
		public void IsOptional()
		{
			var update = new PartialUpdate<Sample>();
			update.Validate(x => x.Name).IsOptional();
			var actual = update.TryValidate(out var failures);
			Assert.AreEqual(true, actual);
			Assert.AreEqual(0, failures.Count);
		}

		[TestMethod]
		public void IsRequired()
		{
			var update = new PartialUpdate<Sample>();
			update.Validate(x => x.Name).IsRequired();
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
		public void NoLessThan()
		{
			var sample = new Sample { Date1 = DateTime.Today };
			var validator = new Validator<Sample>();
			validator.Property(x => x.Date1).NoLessThan(DateTime.Today);
			validator.Validate(sample);
			sample.Date1 -= TimeSpan.FromTicks(1);
			TestHelper.ExpectedException<ValidationException>(() => validator.Validate(sample), "Date1 is less than the provided minimum value.");

			sample = new Sample { Date2 = DateTime.Today };
			validator = new Validator<Sample>();
			validator.Property(x => x.Date2).NoLessThan(DateTime.Today);
			validator.Validate(sample);
			sample.Date2 -= TimeSpan.FromTicks(1);
			TestHelper.ExpectedException<ValidationException>(() => validator.Validate(sample), "Date2 is less than the provided minimum value.");

			sample = new Sample { TimeTag1 = new OscTimeTag(new DateTime(2022, 05, 03, 12, 00, 00, DateTimeKind.Utc)) };
			validator = new Validator<Sample>();
			validator.Property(x => x.TimeTag1).NoLessThan(sample.TimeTag1);
			validator.Validate(sample);
			sample.TimeTag1 -= TimeSpan.FromTicks(5);
			TestHelper.ExpectedException<ValidationException>(() => validator.Validate(sample), "TimeTag1 is less than the provided minimum value.");

			sample = new Sample { Elapsed = TimeSpan.FromSeconds(10) };
			validator = new Validator<Sample>();
			validator.Property(x => x.Elapsed).NoLessThan(sample.Elapsed);
			validator.Validate(sample);
			sample.Elapsed -= TimeSpan.FromTicks(1);
			TestHelper.ExpectedException<ValidationException>(() => validator.Validate(sample), "Elapsed is less than the provided minimum value.");
		}

		[TestMethod]
		public void NoMoreThan()
		{
			var sample = new Sample { Date1 = DateTime.Today };
			var validator = new Validator<Sample>();
			validator.Property(x => x.Date1).NoMoreThan(DateTime.Today);
			validator.Validate(sample);
			sample.Date1 += TimeSpan.FromTicks(1);
			TestHelper.ExpectedException<ValidationException>(() => validator.Validate(sample), "Date1 is greater than the provided maximum value.");

			sample = new Sample { Date2 = DateTime.Today };
			validator = new Validator<Sample>();
			validator.Property(x => x.Date2).NoMoreThan(DateTime.Today);
			validator.Validate(sample);
			sample.Date2 += TimeSpan.FromTicks(1);
			TestHelper.ExpectedException<ValidationException>(() => validator.Validate(sample), "Date2 is greater than the provided maximum value.");

			sample = new Sample { TimeTag1 = new OscTimeTag(new DateTime(2022, 05, 03, 12, 00, 00, DateTimeKind.Utc)) };
			validator = new Validator<Sample>();
			validator.Property(x => x.TimeTag1).NoMoreThan(sample.TimeTag1);
			validator.Validate(sample);
			sample.TimeTag1 += TimeSpan.FromTicks(5);
			TestHelper.ExpectedException<ValidationException>(() => validator.Validate(sample), "TimeTag1 is greater than the provided maximum value.");

			sample = new Sample { Elapsed = TimeSpan.FromSeconds(10) };
			validator = new Validator<Sample>();
			validator.Property(x => x.Elapsed).NoMoreThan(sample.Elapsed);
			validator.Validate(sample);
			sample.Elapsed += TimeSpan.FromTicks(1);
			TestHelper.ExpectedException<ValidationException>(() => validator.Validate(sample), "Elapsed is greater than the provided maximum value.");
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

			public LogLevel? OptionalLogLevel { get; set; }

			public string Name { get; set; }

			public OscTimeTag TimeTag1 { get; set; }

			#endregion
		}

		#endregion
	}
}