#region References

using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Speedy.Extensions;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a full ISO-8601 date time with duration support.
	/// </summary>
	public struct IsoDateTime : IComparable<IsoDateTime>, IComparable, IEquatable<IsoDateTime>
	{
		#region Constants

		private const string _durationExpression = "(P((?<Years>[0-9]+)?Y)?((?<Months>[0-9]+)M)?((?<Days>[0-9]+)D)?(T((?<Hours>[0-9]+)H)?((?<Minutes>[0-9]+)M)?((?<Seconds>[0-9]+(\\.[0-9]+)?)S)?)?)";

		#endregion

		#region Fields

		private static readonly Regex _durationRegex;

		#endregion

		#region Constructors

		static IsoDateTime()
		{
			_durationRegex = new Regex(_durationExpression, RegexOptions.Compiled);
		}

		#endregion

		#region Properties

		/// <summary>
		/// The date and time.
		/// </summary>
		public DateTime DateTime { get; set; }

		/// <summary>
		/// The duration of this date time.
		/// </summary>
		/// <remarks>
		/// https://www.w3.org/TR/xmlschema-2/#duration
		/// </remarks>
		public TimeSpan Duration { get; set; }

		/// <summary>
		/// True if the date time has expired.
		/// </summary>
		public bool IsExpired => TimeService.UtcNow > ExpiresOn;

		/// <summary>
		/// When this date time expires on.
		/// </summary>
		public DateTime ExpiresOn => DateTime.ToUniversalTime().Add(Duration);

		#endregion

		#region Methods

		/// <inheritdoc />
		public int CompareTo(IsoDateTime other)
		{
			var result = DateTime.CompareTo(other.DateTime);
			if (result != 0)
			{
				return result;
			}

			result = Duration.CompareTo(other.Duration);
			return result;
		}

		/// <inheritdoc />
		public int CompareTo(object obj)
		{
			if (obj is not IsoDateTime time)
			{
				return -1;
			}

			return CompareTo(time);
		}

		/// <inheritdoc />
		public bool Equals(IsoDateTime other)
		{
			return DateTime.Equals(other.DateTime)
				&& Duration.Equals(other.Duration);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			return obj is IsoDateTime other && Equals(other);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				return (DateTime.GetHashCode() * 397) ^ Duration.GetHashCode();
			}
		}

		/// <summary>
		/// Determines whether two specified instances of <see cref="IsoDateTime"> </see> are equal.
		/// </summary>
		/// <param name="d1"> The first object to compare. </param>
		/// <param name="d2"> The second object to compare. </param>
		/// <returns> True if they are equal otherwise false. </returns>
		public static bool operator ==(IsoDateTime d1, IsoDateTime d2)
		{
			return (d1.DateTime.Ticks == d2.DateTime.Ticks)
				&& (d1.Duration.Ticks == d2.Duration.Ticks);
		}

		/// <summary>
		/// Converts an IsoDateTime to a DateTime format only using the DateTime value.
		/// </summary>
		/// <param name="isoDateTime"> The ISO date time to convert. </param>
		public static implicit operator DateTime(IsoDateTime isoDateTime)
		{
			return isoDateTime.DateTime;
		}

		/// <summary>
		/// Converts an IsoDateTime to a TimeSpan format only using the Duration value.
		/// </summary>
		/// <param name="isoDateTime"> The ISO date time to convert. </param>
		public static implicit operator TimeSpan(IsoDateTime isoDateTime)
		{
			return isoDateTime.Duration;
		}

		/// <summary>
		/// Determines whether two specified instances of <see cref="IsoDateTime"> </see> are not equal.
		/// </summary>
		/// <param name="d1"> The first object to compare. </param>
		/// <param name="d2"> The second object to compare. </param>
		/// <returns> True if they are not equal otherwise false. </returns>
		public static bool operator !=(IsoDateTime d1, IsoDateTime d2)
		{
			return (d1.DateTime.Ticks != d2.DateTime.Ticks)
				|| (d1.Duration.Ticks != d2.Duration.Ticks);
		}

		/// <summary>
		/// Parse a string into an IsoDateTime value.
		/// </summary>
		/// <param name="data"> The data to parse. </param>
		/// <param name="style"> The style to return the date time value in. Defaults to AdjustToUniversal, AssumeUniversal. </param>
		/// <returns> The data in a IsoDateTime format. </returns>
		/// <exception cref="ArgumentNullException"> Throws if the data value is null. </exception>
		public static IsoDateTime Parse(string data, DateTimeStyles style = DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal)
		{
			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			var response = new IsoDateTime();
			var parts = data.Split('/');
			if (parts.Length >= 1)
			{
				response.DateTime = DateTime.Parse(parts[0], null, style);
			}
			if (parts.Length >= 2)
			{
				// todo: parse period and duration
				response.Duration = ParseDuration(response.DateTime, parts[1]);
			}

			return response;
		}

		/// <summary>
		/// Parse the duration string (ex. P1Y2M3DT1H2M3.245S) into a TimeSpan value.
		/// </summary>
		/// <param name="start"> The date and time the duration starts. </param>
		/// <param name="duration"> The duration string. </param>
		/// <returns> The timespan that represents the duration. </returns>
		public static TimeSpan ParseDuration(DateTime start, string duration)
		{
			var match = _durationRegex.Match(duration);

			int years = 0, months = 0, days = 0, hours = 0, minutes = 0;
			var seconds = 0.0;

			var groupNames = _durationRegex.GetGroupNames();

			foreach (var name in groupNames)
			{
				switch (name)
				{
					case "Years":
					{
						years = int.TryParse(match.Groups[name].Value, out var i) ? i : 0;
						break;
					}
					case "Months":
					{
						months = int.TryParse(match.Groups[name].Value, out var i) ? i : 0;
						break;
					}
					case "Days":
					{
						days = int.TryParse(match.Groups[name].Value, out var i) ? i : 0;
						break;
					}
					case "Hours":
					{
						hours = int.TryParse(match.Groups[name].Value, out var i) ? i : 0;
						break;
					}
					case "Minutes":
					{
						minutes = int.TryParse(match.Groups[name].Value, out var i) ? i : 0;
						break;
					}
					case "Seconds":
					{
						seconds = double.TryParse(match.Groups[name].Value, out var v) ? v : 0.0;
						break;
					}
				}
			}

			var end = start
				.AddYears(years)
				.AddMonths(months)
				.AddDays(days)
				.AddHours(hours)
				.AddMinutes(minutes)
				.AddSeconds(seconds);

			return end - start;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return Duration == TimeSpan.Zero
				? DateTime.ToString("O")
				: $"{DateTime:O}/{ToDuration(DateTime, Duration)}";
		}

		/// <summary>
		/// Calculate the ISO-8601 duration.
		/// </summary>
		/// <param name="startDate"> The date to start from. </param>
		/// <param name="duration"> The timespan for the duration. </param>
		/// <returns> </returns>
		public static string ToDuration(DateTime startDate, TimeSpan duration)
		{
			var builder = new StringBuilder("P", 24);
			var endDate = startDate.Add(duration);

			int days;
			var months = 0;
			var years = 0;

			if (endDate.Day >= startDate.Day)
			{
				days = endDate.Day - startDate.Day;
			}
			else
			{
				var tempDate = endDate.AddMonths(-1);
				var daysInMonth = DateTime.DaysInMonth(tempDate.Year, tempDate.Month);
				days = daysInMonth - (startDate.Day - endDate.Day);
				months--;
			}

			if (endDate.Month >= startDate.Month)
			{
				months += endDate.Month - startDate.Month;
			}
			else
			{
				months += 12 - (startDate.Month - endDate.Month);
				years--;
			}

			years += endDate.Year - startDate.Year;

			builder.IfThen(_ => years > 0, x => x.Append($"{years}Y"));
			builder.IfThen(_ => months > 0, x => x.Append($"{months}M"));
			builder.IfThen(_ => days > 0, x => x.Append($"{days}D"));

			builder.IfThen(_ => (duration.Hours > 0) || (duration.Minutes > 0) || (duration.Seconds > 0) || (duration.Milliseconds > 0),
				x =>
				{
					x.Append("T");
					x.IfThen(_ => duration.Hours > 0, _ => x.Append($"{duration.Hours}H"));
					x.IfThen(_ => duration.Minutes > 0, _ => x.Append($"{duration.Minutes}M"));
					x.IfThen(_ => (duration.Seconds > 0) || (duration.Milliseconds > 0), _ => x.Append($"{duration.Seconds}.{duration.Milliseconds}S"));
				}
			);

			return builder.ToString();
		}

		/// <summary>
		/// Implicit operator from DateTime to IsoDateTime with Zero duration.
		/// </summary>
		/// <param name="value"> The DateTime value to cast. </param>
		public static implicit operator IsoDateTime(DateTime value)
		{
			return new IsoDateTime { DateTime = value, Duration = TimeSpan.Zero };
		}

		#endregion
	}
}