﻿#region References

using System;
using System.Globalization;
using Speedy.Protocols.Osc;

#endregion

namespace Speedy.Extensions;

/// <summary>
/// Extensions for date time
/// </summary>
public static class DateTimeExtensions
{
	#region Constants

	/// <summary>
	/// The amount of ticks in the Max Date / Time value.
	/// </summary>
	public const long MaxDateTimeTicks = 3155378975999999999L;

	/// <summary>
	/// The amount of ticks in the Min Date / Time value.
	/// </summary>
	public const long MinDateTimeTicks = 0L;

	#endregion

	#region Methods

	/// <summary>
	/// Returns the larger of two specified date times.
	/// </summary>
	/// <param name="left"> The left date and time. </param>
	/// <param name="right"> The right date and time. </param>
	/// <returns> The larger of the two. </returns>
	public static DateTime Max(this DateTime left, DateTime right)
	{
		return left.Ticks >= right.Ticks ? left : right;
	}

	/// <summary>
	/// Returns the smaller of two specified date times.
	/// </summary>
	/// <param name="left"> The left date and time. </param>
	/// <param name="right"> The right date and time. </param>
	/// <returns> The smaller of the two. </returns>
	public static DateTime Min(this DateTime left, DateTime right)
	{
		return left.Ticks <= right.Ticks ? left : right;
	}

	/// <summary>
	/// Get the first of the month from the provided date.
	/// </summary>
	/// <param name="date"> The date to process. </param>
	/// <returns>
	/// The date of the first day of the month.
	/// </returns>
	public static DateTime MonthStart(this DateTime date)
	{
		return new DateTime(date.Year, date.Month, 1);
	}

	/// <summary>
	/// Get the first of the next month from the provided date.
	/// </summary>
	/// <param name="date"> The date to process. </param>
	/// <returns>
	/// The date of the first day of the next month.
	/// </returns>
	public static DateTime NextMonth(this DateTime date)
	{
		return date.MonthStart().AddMonths(1);
	}

	/// <summary>
	/// Calculate the time since a provided date until now.
	/// </summary>
	/// <param name="dateTime"> The date and time to process. </param>
	/// <param name="useTimeService"> Optional flag to use time service. </param>
	/// <returns> The time span between the date time and now. </returns>
	public static TimeSpan Since(this DateTime dateTime, bool useTimeService = false)
	{
		var utcNow = useTimeService ? TimeService.UtcNow : DateTime.UtcNow;
		if (dateTime > utcNow)
		{
			return TimeSpan.Zero;
		}

		var utcDateTime = dateTime.ToUtcDateTime();
		var response = utcNow - utcDateTime;
		return response < TimeSpan.Zero ? TimeSpan.Zero : response;
	}

	/// <summary>
	/// Convert a DateTime to an OscTimeTag.
	/// </summary>
	/// <param name="time"> The time to be converted. </param>
	/// <returns> The DateTime in OscTimeTag format. </returns>
	public static OscTimeTag ToOscTimeTag(this DateTime time)
	{
		return new OscTimeTag(time);
	}

	/// <summary>
	/// Converts the string representation of a date and time to its <see cref="T:System.DateTime"> </see> equivalent.
	/// </summary>
	/// <param name="value"> The string value. </param>
	/// <returns> The date time value. </returns>
	public static DateTime ToUtcDateTime(this string value)
	{
		return DateTime.Parse(value, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
	}

	/// <summary>
	/// Converts the string representation of a date and time to its <see cref="T:System.DateTime"> </see> equivalent.
	/// </summary>
	/// <param name="value"> The string value. </param>
	/// <param name="format"> The exact format to parse. </param>
	/// <returns> The date time value. </returns>
	public static DateTime ToUtcDateTime(this string value, string format)
	{
		return DateTime.ParseExact(value, format, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
	}

	/// <summary>
	/// Converts the date time to its UTC <see cref="T:System.DateTime"> </see> equivalent. Assumes
	/// "Unknown" is already UTC
	/// </summary>
	/// <param name="value"> The date time value. </param>
	/// <param name="unspecifiedIsUtc"> Assumes "Unspecified" kind is already UTC value. </param>
	/// <returns> The date time value. </returns>
	public static DateTime ToUtcDateTime(this DateTime value, bool unspecifiedIsUtc = true)
	{
		return value.Kind switch
		{
			DateTimeKind.Unspecified when unspecifiedIsUtc => DateTime.SpecifyKind(value, DateTimeKind.Utc),
			DateTimeKind.Unspecified => value.ToUniversalTime(),
			DateTimeKind.Local => value.ToUniversalTime(),
			_ => value
		};
	}

	/// <summary>
	/// Converts the date time into a ISO8601 format.
	/// </summary>
	/// <param name="dateTime"> </param>
	/// <returns> </returns>
	public static string ToUtcString(this DateTime dateTime)
	{
		string dateTimeString;
		if ((dateTime.Kind == DateTimeKind.Local)
			&& (dateTime != DateTime.MinValue)
			&& (dateTime != DateTime.MaxValue))
		{
			dateTimeString = dateTime.ToUniversalTime().ToString("O");
		}
		else
		{
			dateTimeString = dateTime.ToString("O");
		}

		if (!dateTimeString.EndsWith("Z"))
		{
			dateTimeString += "Z";
		}

		return dateTimeString;
	}

	#endregion
}