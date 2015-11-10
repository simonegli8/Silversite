using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Data.Entity;

namespace Silversite.Data {

	[ComplexType]
	public class DateTime {

		public static readonly System.DateTime MinValue = new System.DateTime(1753, 1, 1);
		public static readonly System.DateTime MaxValue = new System.DateTime(9999, 12, 31);
		public static readonly System.DateTime Never = MinValue;

		public System.DateTime Value { get; set; }

		public static implicit operator System.DateTime(DateTime d) { return d.Value == MinValue ? System.DateTime.MinValue : (d.Value == MaxValue ? System.DateTime.MaxValue : d.Value); }
		public static implicit operator DateTime(System.DateTime d) { return new DateTime() { Value = (d == System.DateTime.MinValue || d == default(System.DateTime)) ? MinValue : (d == System.DateTime.MaxValue ? MaxValue : d) }; }
		/*
		public override bool  Equals(object obj) { return Value.Equals((System.DateTime)obj); }
		public override int GetHashCode() { return Value.GetHashCode(); }
		public override string ToString() { return Value.ToString(); }
		*/

		public DateTime() { Value = default(System.DateTime); }

		// datetime api

		//
		// Summary:
		//     Initializes a new instance of the System.DateTime structure to a specified
		//     number of ticks.
		//
		// Parameters:
		//   ticks:
		//     A date and time expressed in 100-nanosecond units.
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     ticks is less than System.DateTime.MinValue or greater than System.DateTime.MaxValue.
		public DateTime(long ticks) { Value = new System.DateTime(ticks); }
		//
		// Summary:
		//     Initializes a new instance of the System.DateTime structure to a specified
		//     number of ticks and to Coordinated Universal Time (UTC) or local time.
		//
		// Parameters:
		//   ticks:
		//     A date and time expressed in 100-nanosecond units.
		//
		//   kind:
		//     One of the enumeration values that indicates whether ticks specifies a local
		//     time, Coordinated Universal Time (UTC), or neither.
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     ticks is less than System.DateTime.MinValue or greater than System.DateTime.MaxValue.
		//
		//   System.ArgumentException:
		//     kind is not one of the System.DateTimeKind values.
		public DateTime(long ticks, DateTimeKind kind) { Value = new System.DateTime(ticks, kind); }
		//
		// Summary:
		//     Initializes a new instance of the System.DateTime structure to the specified
		//     year, month, and day.
		//
		// Parameters:
		//   year:
		//     The year (1 through 9999).
		//
		//   month:
		//     The month (1 through 12).
		//
		//   day:
		//     The day (1 through the number of days in month).
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     year is less than 1 or greater than 9999.-or- month is less than 1 or greater
		//     than 12.-or- day is less than 1 or greater than the number of days in month.
		//
		//   System.ArgumentException:
		//     The specified parameters evaluate to less than System.DateTime.MinValue or
		//     more than System.DateTime.MaxValue.
		public DateTime(int year, int month, int day) { Value = new System.DateTime(year, month, day); }
		//
		// Summary:
		//     Initializes a new instance of the System.DateTime structure to the specified
		//     year, month, and day for the specified calendar.
		//
		// Parameters:
		//   year:
		//     The year (1 through the number of years in calendar).
		//
		//   month:
		//     The month (1 through the number of months in calendar).
		//
		//   day:
		//     The day (1 through the number of days in month).
		//
		//   calendar:
		//     The calendar that is used to interpret year, month, and day.
		//
		// Exceptions:
		//   System.ArgumentNullException:
		//     calendar is null.
		//
		//   System.ArgumentOutOfRangeException:
		//     year is not in the range supported by calendar.-or- month is less than 1
		//     or greater than the number of months in calendar.-or- day is less than 1
		//     or greater than the number of days in month.
		//
		//   System.ArgumentException:
		//     The specified parameters evaluate to less than System.DateTime.MinValue or
		//     more than System.DateTime.MaxValue.
		public DateTime(int year, int month, int day, Calendar calendar) { Value = new System.DateTime(year, month, day, calendar); }
		//
		// Summary:
		//     Initializes a new instance of the System.DateTime structure to the specified
		//     year, month, day, hour, minute, and second.
		//
		// Parameters:
		//   year:
		//     The year (1 through 9999).
		//
		//   month:
		//     The month (1 through 12).
		//
		//   day:
		//     The day (1 through the number of days in month).
		//
		//   hour:
		//     The hours (0 through 23).
		//
		//   minute:
		//     The minutes (0 through 59).
		//
		//   second:
		//     The seconds (0 through 59).
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     year is less than 1 or greater than 9999. -or- month is less than 1 or greater
		//     than 12. -or- day is less than 1 or greater than the number of days in month.-or-
		//     hour is less than 0 or greater than 23. -or- minute is less than 0 or greater
		//     than 59. -or- second is less than 0 or greater than 59.
		//
		//   System.ArgumentException:
		//     The specified parameters evaluate to less than System.DateTime.MinValue or
		//     more than System.DateTime.MaxValue.
		public DateTime(int year, int month, int day, int hour, int minute, int second) { Value = new System.DateTime(year, month, day, hour, minute, second); }
		//
		// Summary:
		//     Initializes a new instance of the System.DateTime structure to the specified
		//     year, month, day, hour, minute, and second for the specified calendar.
		//
		// Parameters:
		//   year:
		//     The year (1 through the number of years in calendar).
		//
		//   month:
		//     The month (1 through the number of months in calendar).
		//
		//   day:
		//     The day (1 through the number of days in month).
		//
		//   hour:
		//     The hours (0 through 23).
		//
		//   minute:
		//     The minutes (0 through 59).
		//
		//   second:
		//     The seconds (0 through 59).
		//
		//   calendar:
		//     The calendar that is used to interpret year, month, and day.
		//
		// Exceptions:
		//   System.ArgumentNullException:
		//     calendar is null.
		//
		//   System.ArgumentOutOfRangeException:
		//     year is not in the range supported by calendar.-or- month is less than 1
		//     or greater than the number of months in calendar.-or- day is less than 1
		//     or greater than the number of days in month.-or- hour is less than 0 or greater
		//     than 23 -or- minute is less than 0 or greater than 59. -or- second is less
		//     than 0 or greater than 59.
		//
		//   System.ArgumentException:
		//     The specified parameters evaluate to less than System.DateTime.MinValue or
		//     more than System.DateTime.MaxValue.
		public DateTime(int year, int month, int day, int hour, int minute, int second, Calendar calendar) { Value = new System.DateTime(year, month, day, hour, minute, second, calendar); }
		//
		// Summary:
		//     Initializes a new instance of the System.DateTime structure to the specified
		//     year, month, day, hour, minute, second, and Coordinated Universal Time (UTC)
		//     or local time.
		//
		// Parameters:
		//   year:
		//     The year (1 through 9999).
		//
		//   month:
		//     The month (1 through 12).
		//
		//   day:
		//     The day (1 through the number of days in month).
		//
		//   hour:
		//     The hours (0 through 23).
		//
		//   minute:
		//     The minutes (0 through 59).
		//
		//   second:
		//     The seconds (0 through 59).
		//
		//   kind:
		//     One of the enumeration values that indicates whether year, month, day, hour,
		//     minute and second specify a local time, Coordinated Universal Time (UTC),
		//     or neither.
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     year is less than 1 or greater than 9999. -or- month is less than 1 or greater
		//     than 12. -or- day is less than 1 or greater than the number of days in month.-or-
		//     hour is less than 0 or greater than 23. -or- minute is less than 0 or greater
		//     than 59. -or- second is less than 0 or greater than 59.
		//
		//   System.ArgumentException:
		//     The specified time parameters evaluate to less than System.DateTime.MinValue
		//     or more than System.DateTime.MaxValue. -or-kind is not one of the System.DateTimeKind
		//     values.
		public DateTime(int year, int month, int day, int hour, int minute, int second, DateTimeKind kind) { Value = new System.DateTime(year, month, day, hour, minute, second, kind); }
		//
		// Summary:
		//     Initializes a new instance of the System.DateTime structure to the specified
		//     year, month, day, hour, minute, second, and millisecond.
		//
		// Parameters:
		//   year:
		//     The year (1 through 9999).
		//
		//   month:
		//     The month (1 through 12).
		//
		//   day:
		//     The day (1 through the number of days in month).
		//
		//   hour:
		//     The hours (0 through 23).
		//
		//   minute:
		//     The minutes (0 through 59).
		//
		//   second:
		//     The seconds (0 through 59).
		//
		//   millisecond:
		//     The milliseconds (0 through 999).
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     year is less than 1 or greater than 9999.-or- month is less than 1 or greater
		//     than 12.-or- day is less than 1 or greater than the number of days in month.-or-
		//     hour is less than 0 or greater than 23.-or- minute is less than 0 or greater
		//     than 59.-or- second is less than 0 or greater than 59.-or- millisecond is
		//     less than 0 or greater than 999.
		//
		//   System.ArgumentException:
		//     The specified parameters evaluate to less than System.DateTime.MinValue or
		//     more than System.DateTime.MaxValue.
		public DateTime(int year, int month, int day, int hour, int minute, int second, int millisecond) { Value = new System.DateTime(year, month, day, hour, minute, second, millisecond); }
		//
		// Summary:
		//     Initializes a new instance of the System.DateTime structure to the specified
		//     year, month, day, hour, minute, second, and millisecond for the specified
		//     calendar.
		//
		// Parameters:
		//   year:
		//     The year (1 through the number of years in calendar).
		//
		//   month:
		//     The month (1 through the number of months in calendar).
		//
		//   day:
		//     The day (1 through the number of days in month).
		//
		//   hour:
		//     The hours (0 through 23).
		//
		//   minute:
		//     The minutes (0 through 59).
		//
		//   second:
		//     The seconds (0 through 59).
		//
		//   millisecond:
		//     The milliseconds (0 through 999).
		//
		//   calendar:
		//     The calendar that is used to interpret year, month, and day.
		//
		// Exceptions:
		//   System.ArgumentNullException:
		//     calendar is null.
		//
		//   System.ArgumentOutOfRangeException:
		//     year is not in the range supported by calendar.-or- month is less than 1
		//     or greater than the number of months in calendar.-or- day is less than 1
		//     or greater than the number of days in month.-or- hour is less than 0 or greater
		//     than 23.-or- minute is less than 0 or greater than 59.-or- second is less
		//     than 0 or greater than 59.-or- millisecond is less than 0 or greater than
		//     999.
		//
		//   System.ArgumentException:
		//     The specified parameters evaluate to less than System.DateTime.MinValue or
		//     more than System.DateTime.MaxValue.
		public DateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, Calendar calendar) { Value = new System.DateTime(year, month, day, hour, minute, second, millisecond, calendar); }
		//
		// Summary:
		//     Initializes a new instance of the System.DateTime structure to the specified
		//     year, month, day, hour, minute, second, millisecond, and Coordinated Universal
		//     Time (UTC) or local time.
		//
		// Parameters:
		//   year:
		//     The year (1 through 9999).
		//
		//   month:
		//     The month (1 through 12).
		//
		//   day:
		//     The day (1 through the number of days in month).
		//
		//   hour:
		//     The hours (0 through 23).
		//
		//   minute:
		//     The minutes (0 through 59).
		//
		//   second:
		//     The seconds (0 through 59).
		//
		//   millisecond:
		//     The milliseconds (0 through 999).
		//
		//   kind:
		//     One of the enumeration values that indicates whether year, month, day, hour,
		//     minute, second, and millisecond specify a local time, Coordinated Universal
		//     Time (UTC), or neither.
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     year is less than 1 or greater than 9999.-or- month is less than 1 or greater
		//     than 12.-or- day is less than 1 or greater than the number of days in month.-or-
		//     hour is less than 0 or greater than 23.-or- minute is less than 0 or greater
		//     than 59.-or- second is less than 0 or greater than 59.-or- millisecond is
		//     less than 0 or greater than 999.
		//
		//   System.ArgumentException:
		//     The specified time parameters evaluate to less than System.DateTime.MinValue
		//     or more than System.DateTime.MaxValue. -or-kind is not one of the System.DateTimeKind
		//     values.
		public DateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, DateTimeKind kind) { Value = new System.DateTime(year, month, day, hour, minute, second, millisecond, kind); }
		//
		// Summary:
		//     Initializes a new instance of the System.DateTime structure to the specified
		//     year, month, day, hour, minute, second, millisecond, and Coordinated Universal
		//     Time (UTC) or local time for the specified calendar.
		//
		// Parameters:
		//   year:
		//     The year (1 through the number of years in calendar).
		//
		//   month:
		//     The month (1 through the number of months in calendar).
		//
		//   day:
		//     The day (1 through the number of days in month).
		//
		//   hour:
		//     The hours (0 through 23).
		//
		//   minute:
		//     The minutes (0 through 59).
		//
		//   second:
		//     The seconds (0 through 59).
		//
		//   millisecond:
		//     The milliseconds (0 through 999).
		//
		//   calendar:
		//     The calendar that is used to interpret year, month, and day.
		//
		//   kind:
		//     One of the enumeration values that indicates whether year, month, day, hour,
		//     minute, second, and millisecond specify a local time, Coordinated Universal
		//     Time (UTC), or neither.
		//
		// Exceptions:
		//   System.ArgumentNullException:
		//     calendar is null.
		//
		//   System.ArgumentOutOfRangeException:
		//     year is not in the range supported by calendar.-or- month is less than 1
		//     or greater than the number of months in calendar.-or- day is less than 1
		//     or greater than the number of days in month.-or- hour is less than 0 or greater
		//     than 23.-or- minute is less than 0 or greater than 59.-or- second is less
		//     than 0 or greater than 59.-or- millisecond is less than 0 or greater than
		//     999.
		//
		//   System.ArgumentException:
		//     The specified time parameters evaluate to less than System.DateTime.MinValue
		//     or more than System.DateTime.MaxValue. -or-kind is not one of the System.DateTimeKind
		//     values.
		public DateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, Calendar calendar, DateTimeKind kind) { Value = new System.DateTime(year, month, day, hour, minute, second, millisecond, calendar, kind); }

		// Summary:
		//     Subtracts a specified date and time from another specified date and time
		//     and returns a time interval.
		//
		// Parameters:
		//   d1:
		//     A System.DateTime (the minuend).
		//
		//   d2:
		//     A System.DateTime (the subtrahend).
		//
		// Returns:
		//     A System.TimeSpan that is the time interval between d1 and d2; that is, d1
		//     minus d2.

		//public static System.TimeSpan operator -(DateTime d1, DateTime d2) { return d1.Value - d2.Value; }
		//
		// Summary:
		//     Subtracts a specified time interval from a specified date and time and returns
		//     a new date and time.
		//
		// Parameters:
		//   d:
		//     A System.DateTime.
		//
		//   t:
		//     A System.TimeSpan.
		//
		// Returns:
		//     A System.DateTime whose value is the value of d minus the value of t.
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     The resulting System.DateTime is less than System.DateTime.MinValue or greater
		//     than System.DateTime.MaxValue.
		//public static DateTime operator -(DateTime d, System.TimeSpan t) { return d.Value - t; }
		//
		// Summary:
		//     Determines whether two specified instances of System.DateTime are not equal.
		//
		// Parameters:
		//   d1:
		//     A System.DateTime.
		//
		//   d2:
		//     A System.DateTime.
		//
		// Returns:
		//     true if d1 and d2 do not represent the same date and time; otherwise, false.

		//public static bool operator !=(DateTime d1, DateTime d2) { return d1.Value != d2.Value; }
		//
		// Summary:
		//     Adds a specified time interval to a specified date and time, yielding a new
		//     date and time.
		//
		// Parameters:
		//   d:
		//     A System.DateTime.
		//
		//   t:
		//     A System.TimeSpan.
		//
		// Returns:
		//     A System.DateTime that is the sum of the values of d and t.
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     The resulting System.DateTime is less than System.DateTime.MinValue or greater
		//     than System.DateTime.MaxValue.
		//public static DateTime operator +(DateTime d, System.TimeSpan t) { return d.Value + t; }
		//
		// Summary:
		//     Determines whether one specified System.DateTime is less than another specified
		//     System.DateTime.
		//
		// Parameters:
		//   t1:
		//     A System.DateTime.
		//
		//   t2:
		//     A System.DateTime.
		//
		// Returns:
		//     true if t1 is less than t2; otherwise, false.

		//public static bool operator <(DateTime t1, DateTime t2) { return t1.Value < t2.Value; }
		//
		// Summary:
		//     Determines whether one specified System.DateTime is less than or equal to
		//     another specified System.DateTime.
		//
		// Parameters:
		//   t1:
		//     A System.DateTime.
		//
		//   t2:
		//     A System.DateTime.
		//
		// Returns:
		//     true if t1 is less than or equal to t2; otherwise, false.

		//public static bool operator <=(DateTime t1, DateTime t2) { return t1.Value <= t2.Value; }
		//
		// Summary:
		//     Determines whether two specified instances of System.DateTime are equal.
		//
		// Parameters:
		//   d1:
		//     A System.DateTime.
		//
		//   d2:
		//     A System.DateTime.
		//
		// Returns:
		//     true if d1 and d2 represent the same date and time; otherwise, false.

		//public static bool operator ==(DateTime d1, DateTime d2) { return d1.Value == d2.Value; }
		//
		// Summary:
		//     Determines whether one specified System.DateTime is greater than another
		//     specified System.DateTime.
		//
		// Parameters:
		//   t1:
		//     A System.DateTime.
		//
		//   t2:
		//     A System.DateTime.
		//
		// Returns:
		//     true if t1 is greater than t2; otherwise, false.

		//public static bool operator >(DateTime t1, DateTime t2) { return t1.Value > t2.Value; } 
		//
		// Summary:
		//     Determines whether one specified System.DateTime is greater than or equal
		//     to another specified System.DateTime.
		//
		// Parameters:
		//   t1:
		//     A System.DateTime.
		//
		//   t2:
		//     A System.DateTime.
		//
		// Returns:
		//     true if t1 is greater than or equal to t2; otherwise, false.

		//public static bool operator >=(DateTime t1, DateTime t2) { return t1.Value >= t2.Value; }

		// Summary:
		//     Gets the date component of this instance.
		//
		// Returns:
		//     A new System.DateTime with the same date as this instance, and the time value
		//     set to 12:00:00 midnight (00:00:00).
		public System.DateTime Date { get { return Value.Date; } }
		//
		// Summary:
		//     Gets the day of the month represented by this instance.
		//
		// Returns:
		//     The day component, expressed as a value between 1 and 31.
		public int Day { get { return Value.Day; } }
		//
		// Summary:
		//     Gets the day of the week represented by this instance.
		//
		// Returns:
		//     A System.DayOfWeek enumerated constant that indicates the day of the week
		//     of this System.DateTime value.
		public DayOfWeek DayOfWeek { get { return Value.DayOfWeek; } }
		//
		// Summary:
		//     Gets the day of the year represented by this instance.
		//
		// Returns:
		//     The day of the year, expressed as a value between 1 and 366.
		public int DayOfYear { get { return Value.DayOfYear; } }
		//
		// Summary:
		//     Gets the hour component of the date represented by this instance.
		//
		// Returns:
		//     The hour component, expressed as a value between 0 and 23.
		public int Hour { get { return Value.Hour; } }
		//
		// Summary:
		//     Gets a value that indicates whether the time represented by this instance
		//     is based on local time, Coordinated Universal Time (UTC), or neither.
		//
		// Returns:
		//     One of the System.DateTimeKind values. The default is System.DateTimeKind.Unspecified.
		public DateTimeKind Kind { get { return Value.Kind; } }
		//
		// Summary:
		//     Gets the milliseconds component of the date represented by this instance.
		//
		// Returns:
		//     The milliseconds component, expressed as a value between 0 and 999.
		public int Millisecond { get { return Value.Millisecond; } }
		//
		// Summary:
		//     Gets the minute component of the date represented by this instance.
		//
		// Returns:
		//     The minute component, expressed as a value between 0 and 59.
		public int Minute { get { return Value.Minute; } }
		//
		// Summary:
		//     Gets the month component of the date represented by this instance.
		//
		// Returns:
		//     The month component, expressed as a value between 1 and 12.
		public int Month { get { return Value.Month; } }
		//
		// Summary:
		//     Gets a System.DateTime object that is set to the current date and time on
		//     this computer, expressed as the local time.
		//
		// Returns:
		//     A System.DateTime whose value is the current local date and time.
		public static System.DateTime Now { get { return System.DateTime.Now; } }
		//
		// Summary:
		//     Gets the seconds component of the date represented by this instance.
		//
		// Returns:
		//     The seconds, between 0 and 59.
		public int Second { get { return Value.Second; } }
		//
		// Summary:
		//     Gets the number of ticks that represent the date and time of this instance.
		//
		// Returns:
		//     The number of ticks that represent the date and time of this instance. The
		//     value is between DateTime.MinValue.Ticks and DateTime.MaxValue.Ticks.
		public long Ticks { get { return Value.Ticks; } }
		//
		// Summary:
		//     Gets the time of day for this instance.
		//
		// Returns:
		//     A System.TimeSpan that represents the fraction of the day that has elapsed
		//     since midnight.
		public System.TimeSpan TimeOfDay { get { return Value.TimeOfDay; } }
		//
		// Summary:
		//     Gets the current date.
		//
		// Returns:
		//     A System.DateTime set to today's date, with the time component set to 00:00:00.
		public static System.DateTime Today { get { return System.DateTime.Today; } }
		//
		// Summary:
		//     Gets a System.DateTime object that is set to the current date and time on
		//     this computer, expressed as the Coordinated Universal Time (UTC).
		//
		// Returns:
		//     A System.DateTime whose value is the current UTC date and time.
		public static System.DateTime UtcNow { get { return System.DateTime.UtcNow; } }
		//
		// Summary:
		//     Gets the year component of the date represented by this instance.
		//
		// Returns:
		//     The year, between 1 and 9999.
		public int Year { get { return Value.Year; } }

		// Summary:
		//     Returns a new System.DateTime that adds the value of the specified System.TimeSpan
		//     to the value of this instance.
		//
		// Parameters:
		//   value:
		//     A System.TimeSpan object that represents a positive or negative time interval.
		//
		// Returns:
		//     A System.DateTime whose value is the sum of the date and time represented
		//     by this instance and the time interval represented by value.
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     The resulting System.DateTime is less than System.DateTime.MinValue or greater
		//     than System.DateTime.MaxValue.
		public System.DateTime Add(System.TimeSpan value) { return Value.Add(value); }
		//
		// Summary:
		//     Returns a new System.DateTime that adds the specified number of days to the
		//     value of this instance.
		//
		// Parameters:
		//   value:
		//     A number of whole and fractional days. The value parameter can be negative
		//     or positive.
		//
		// Returns:
		//     A System.DateTime whose value is the sum of the date and time represented
		//     by this instance and the number of days represented by value.
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     The resulting System.DateTime is less than System.DateTime.MinValue or greater
		//     than System.DateTime.MaxValue.
		public System.DateTime AddDays(double value) { return Value.AddDays(value); }
		//
		// Summary:
		//     Returns a new System.DateTime that adds the specified number of hours to
		//     the value of this instance.
		//
		// Parameters:
		//   value:
		//     A number of whole and fractional hours. The value parameter can be negative
		//     or positive.
		//
		// Returns:
		//     A System.DateTime whose value is the sum of the date and time represented
		//     by this instance and the number of hours represented by value.
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     The resulting System.DateTime is less than System.DateTime.MinValue or greater
		//     than System.DateTime.MaxValue.
		public System.DateTime AddHours(double value) { return Value.AddHours(value); }
		//
		// Summary:
		//     Returns a new System.DateTime that adds the specified number of milliseconds
		//     to the value of this instance.
		//
		// Parameters:
		//   value:
		//     A number of whole and fractional milliseconds. The value parameter can be
		//     negative or positive. Note that this value is rounded to the nearest integer.
		//
		// Returns:
		//     A System.DateTime whose value is the sum of the date and time represented
		//     by this instance and the number of milliseconds represented by value.
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     The resulting System.DateTime is less than System.DateTime.MinValue or greater
		//     than System.DateTime.MaxValue.
		public System.DateTime AddMilliseconds(double value) { return Value.AddMilliseconds(value); }
		//
		// Summary:
		//     Returns a new System.DateTime that adds the specified number of minutes to
		//     the value of this instance.
		//
		// Parameters:
		//   value:
		//     A number of whole and fractional minutes. The value parameter can be negative
		//     or positive.
		//
		// Returns:
		//     A System.DateTime whose value is the sum of the date and time represented
		//     by this instance and the number of minutes represented by value.
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     The resulting System.DateTime is less than System.DateTime.MinValue or greater
		//     than System.DateTime.MaxValue.
		public System.DateTime AddMinutes(double value) { return Value.AddMinutes(value); }
		//
		// Summary:
		//     Returns a new System.DateTime that adds the specified number of months to
		//     the value of this instance.
		//
		// Parameters:
		//   months:
		//     A number of months. The months parameter can be negative or positive.
		//
		// Returns:
		//     A System.DateTime whose value is the sum of the date and time represented
		//     by this instance and months.
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     The resulting System.DateTime is less than System.DateTime.MinValue or greater
		//     than System.DateTime.MaxValue.-or- months is less than -120,000 or greater
		//     than 120,000.
		public System.DateTime AddMonths(int months) { return Value.AddMonths(months); }
		//
		// Summary:
		//     Returns a new System.DateTime that adds the specified number of seconds to
		//     the value of this instance.
		//
		// Parameters:
		//   value:
		//     A number of whole and fractional seconds. The value parameter can be negative
		//     or positive.
		//
		// Returns:
		//     A System.DateTime whose value is the sum of the date and time represented
		//     by this instance and the number of seconds represented by value.
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     The resulting System.DateTime is less than System.DateTime.MinValue or greater
		//     than System.DateTime.MaxValue.
		public System.DateTime AddSeconds(double value) { return Value.AddSeconds(value); }
		//
		// Summary:
		//     Returns a new System.DateTime that adds the specified number of ticks to
		//     the value of this instance.
		//
		// Parameters:
		//   value:
		//     A number of 100-nanosecond ticks. The value parameter can be positive or
		//     negative.
		//
		// Returns:
		//     A System.DateTime whose value is the sum of the date and time represented
		//     by this instance and the time represented by value.
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     The resulting System.DateTime is less than System.DateTime.MinValue or greater
		//     than System.DateTime.MaxValue.
		public System.DateTime AddTicks(long value) { return Value.AddTicks(value); }
		//
		// Summary:
		//     Returns a new System.DateTime that adds the specified number of years to
		//     the value of this instance.
		//
		// Parameters:
		//   value:
		//     A number of years. The value parameter can be negative or positive.
		//
		// Returns:
		//     A System.DateTime whose value is the sum of the date and time represented
		//     by this instance and the number of years represented by value.
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     value or the resulting System.DateTime is less than System.DateTime.MinValue
		//     or greater than System.DateTime.MaxValue.
		public System.DateTime AddYears(int value) { return Value.AddYears(value); }
		//
		// Summary:
		//     Compares two instances of System.DateTime and returns an integer that indicates
		//     whether the first instance is earlier than, the same as, or later than the
		//     second instance.
		//
		// Parameters:
		//   t1:
		//     The first System.DateTime.
		//
		//   t2:
		//     The second System.DateTime.
		//
		// Returns:
		//     A signed number indicating the relative values of t1 and t2.Value Type Condition
		//     Less than zero t1 is earlier than t2. Zero t1 is the same as t2. Greater
		//     than zero t1 is later than t2.
		public static int Compare(System.DateTime t1, System.DateTime t2) { return System.DateTime.Compare(t1, t2); }
		//
		// Summary:
		//     Compares the value of this instance to a specified System.DateTime value
		//     and returns an integer that indicates whether this instance is earlier than,
		//     the same as, or later than the specified System.DateTime value.
		//
		// Parameters:
		//   value:
		//     A System.DateTime object to compare.
		//
		// Returns:
		//     A signed number indicating the relative values of this instance and the value
		//     parameter.Value Description Less than zero This instance is earlier than
		//     value. Zero This instance is the same as value. Greater than zero This instance
		//     is later than value.
		public int CompareTo(System.DateTime value) { return Value.CompareTo(value); }
		//
		// Summary:
		//     Compares the value of this instance to a specified object that contains a
		//     specified System.DateTime value, and returns an integer that indicates whether
		//     this instance is earlier than, the same as, or later than the specified System.DateTime
		//     value.
		//
		// Parameters:
		//   value:
		//     A boxed System.DateTime object to compare, or null.
		//
		// Returns:
		//     A signed number indicating the relative values of this instance and value.Value
		//     Description Less than zero This instance is earlier than value. Zero This
		//     instance is the same as value. Greater than zero This instance is later than
		//     value, or value is null.
		//
		// Exceptions:
		//   System.ArgumentException:
		//     value is not a System.DateTime.
		public int CompareTo(object value) { return Value.CompareTo((System.DateTime)value); }
		//
		// Summary:
		//     Returns the number of days in the specified month and year.
		//
		// Parameters:
		//   year:
		//     The year.
		//
		//   month:
		//     The month (a number ranging from 1 to 12).
		//
		// Returns:
		//     The number of days in month for the specified year.For example, if month
		//     equals 2 for February, the return value is 28 or 29 depending upon whether
		//     year is a leap year.
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     month is less than 1 or greater than 12.-or-year is less than 1 or greater
		//     than 9999.
		public static int DaysInMonth(int year, int month) { return System.DateTime.DaysInMonth(year, month); }
		//
		// Summary:
		//     Returns a value indicating whether this instance is equal to the specified
		//     System.DateTime instance.
		//
		// Parameters:
		//   value:
		//     A System.DateTime instance to compare to this instance.
		//
		// Returns:
		//     true if the value parameter equals the value of this instance; otherwise,
		//     false.
		public bool Equals(System.DateTime value) { return Value.Equals(value); }
		//
		// Summary:
		//     Returns a value indicating whether this instance is equal to a specified
		//     object.
		//
		// Parameters:
		//   value:
		//     An object to compare to this instance.
		//
		// Returns:
		//     true if value is an instance of System.DateTime and equals the value of this
		//     instance; otherwise, false.
		public override bool Equals(object value) { return Value.Equals((System.DateTime)value); }
		//
		// Summary:
		//     Returns a value indicating whether two instances of System.DateTime are equal.
		//
		// Parameters:
		//   t1:
		//     The first System.DateTime instance.
		//
		//   t2:
		//     The second System.DateTime instance.
		//
		// Returns:
		//     true if the two System.DateTime values are equal; otherwise, false.
		public static bool Equals(System.DateTime t1, System.DateTime t2) { return t1.Equals(t2); }
		//
		// Summary:
		//     Deserializes a 64-bit binary value and recreates an original serialized System.DateTime
		//     object.
		//
		// Parameters:
		//   dateData:
		//     A 64-bit signed integer that encodes the System.DateTime.Kind property in
		//     a 2-bit field and the System.DateTime.Ticks property in a 62-bit field.
		//
		// Returns:
		//     A System.DateTime object that is equivalent to the System.DateTime object
		//     that was serialized by the System.DateTime.ToBinary() method.
		//
		// Exceptions:
		//   System.ArgumentException:
		//     dateData is less than System.DateTime.MinValue or greater than System.DateTime.MaxValue.
		public static System.DateTime FromBinary(long dateData) { return System.DateTime.FromBinary(dateData); }
		//
		// Summary:
		//     Converts the specified Windows file time to an equivalent local time.
		//
		// Parameters:
		//   fileTime:
		//     A Windows file time expressed in ticks.
		//
		// Returns:
		//     A System.DateTime object that represents a local time equivalent to the date
		//     and time represented by the fileTime parameter.
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     fileTime is less than 0 or represents a time greater than System.DateTime.MaxValue.
		public static System.DateTime FromFileTime(long fileTime) { return System.DateTime.FromFileTime(fileTime); }
		//
		// Summary:
		//     Converts the specified Windows file time to an equivalent UTC time.
		//
		// Parameters:
		//   fileTime:
		//     A Windows file time expressed in ticks.
		//
		// Returns:
		//     A System.DateTime object that represents a UTC time equivalent to the date
		//     and time represented by the fileTime parameter.
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     fileTime is less than 0 or represents a time greater than System.DateTime.MaxValue.
		public static System.DateTime FromFileTimeUtc(long fileTime) { return System.DateTime.FromFileTimeUtc(fileTime); }
		//
		// Summary:
		//     Returns a System.DateTime equivalent to the specified OLE Automation Date.
		//
		// Parameters:
		//   d:
		//     An OLE Automation Date value.
		//
		// Returns:
		//     A System.DateTime that represents the same date and time as d.
		//
		// Exceptions:
		//   System.ArgumentException:
		//     The date is not a valid OLE Automation Date value.
		public static System.DateTime FromOADate(double d) { return System.DateTime.FromOADate(d); }
		//
		// Summary:
		//     Converts the value of this instance to all the string representations supported
		//     by the standard System.DateTime format specifiers.
		//
		// Returns:
		//     A string array where each element is the representation of the value of this
		//     instance formatted with one of the standard System.DateTime formatting specifiers.
		public string[] GetDateTimeFormats() { return Value.GetDateTimeFormats(); }
		//
		// Summary:
		//     Converts the value of this instance to all the string representations supported
		//     by the specified standard System.DateTime format specifier.
		//
		// Parameters:
		//   format:
		//     A standard date and time format string. (See Standard Date and Time Format
		//     Strings.)
		//
		// Returns:
		//     A string array where each element is the representation of the value of this
		//     instance formatted with the format standard System.DateTime formatting specifier.
		//
		// Exceptions:
		//   System.FormatException:
		//     format is not a valid standard date and time format specifier character.
		public string[] GetDateTimeFormats(char format) { return Value.GetDateTimeFormats(format); }
		//
		// Summary:
		//     Converts the value of this instance to all the string representations supported
		//     by the standard System.DateTime format specifiers and the specified culture-specific
		//     formatting information.
		//
		// Parameters:
		//   provider:
		//     An object that supplies culture-specific formatting information about this
		//     instance.
		//
		// Returns:
		//     A string array where each element is the representation of the value of this
		//     instance formatted with one of the standard System.DateTime formatting specifiers.
		public string[] GetDateTimeFormats(IFormatProvider provider) { return Value.GetDateTimeFormats(provider); }
		//
		// Summary:
		//     Converts the value of this instance to all the string representations supported
		//     by the specified standard System.DateTime format specifier and culture-specific
		//     formatting information.
		//
		// Parameters:
		//   format:
		//     A date and time format string.
		//
		//   provider:
		//     An object that supplies culture-specific formatting information about this
		//     instance.
		//
		// Returns:
		//     A string array where each element is the representation of the value of this
		//     instance formatted with one of the standard System.DateTime formatting specifiers.
		//
		// Exceptions:
		//   System.FormatException:
		//     format is not a valid standard date and time format specifier character.
		public string[] GetDateTimeFormats(char format, IFormatProvider provider) { return Value.GetDateTimeFormats(format, provider); }
		//
		// Summary:
		//     Returns the hash code for this instance.
		//
		// Returns:
		//     A 32-bit signed integer hash code.

		public override int GetHashCode() { return Value.GetHashCode(); }
		//
		// Summary:
		//     Returns the System.TypeCode for value type System.DateTime.
		//
		// Returns:
		//     The enumerated constant, System.TypeCode.DateTime.

		public TypeCode GetTypeCode() { return Value.GetTypeCode(); }
		//
		// Summary:
		//     Indicates whether this instance of System.DateTime is within the Daylight
		//     Saving Time range for the current time zone.
		//
		// Returns:
		//     true if System.DateTime.Kind is System.DateTimeKind.Local or System.DateTimeKind.Unspecified
		//     and the value of this instance of System.DateTime is within the Daylight
		//     Saving Time range for the current time zone. false if System.DateTime.Kind
		//     is System.DateTimeKind.Utc.
		public bool IsDaylightSavingTime() { return Value.IsDaylightSavingTime(); }
		//
		// Summary:
		//     Returns an indication whether the specified year is a leap year.
		//
		// Parameters:
		//   year:
		//     A 4-digit year.
		//
		// Returns:
		//     true if year is a leap year; otherwise, false.
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     year is less than 1 or greater than 9999.
		public static bool IsLeapYear(int year) { return System.DateTime.IsLeapYear(year); }
		//
		// Summary:
		//     Converts the specified string representation of a date and time to its System.DateTime
		//     equivalent.
		//
		// Parameters:
		//   s:
		//     A string containing a date and time to convert.
		//
		// Returns:
		//     A System.DateTime equivalent to the date and time contained in s.
		//
		// Exceptions:
		//   System.ArgumentNullException:
		//     s is null.
		//
		//   System.FormatException:
		//     s does not contain a valid string representation of a date and time.

		public static System.DateTime Parse(string s) { return System.DateTime.Parse(s); }
		//
		// Summary:
		//     Converts the specified string representation of a date and time to its System.DateTime
		//     equivalent using the specified culture-specific format information.
		//
		// Parameters:
		//   s:
		//     A string containing a date and time to convert.
		//
		//   provider:
		//     An object that supplies culture-specific format information about s.
		//
		// Returns:
		//     A System.DateTime equivalent to the date and time contained in s as specified
		//     by provider.
		//
		// Exceptions:
		//   System.ArgumentNullException:
		//     s is null.
		//
		//   System.FormatException:
		//     s does not contain a valid string representation of a date and time.

		public static System.DateTime Parse(string s, IFormatProvider provider) { return System.DateTime.Parse(s, provider); }
		//
		// Summary:
		//     Converts the specified string representation of a date and time to its System.DateTime
		//     equivalent using the specified culture-specific format information and formatting
		//     style.
		//
		// Parameters:
		//   s:
		//     A string containing a date and time to convert.
		//
		//   provider:
		//     An object that supplies culture-specific formatting information about s.
		//
		//   styles:
		//     A bitwise combination of the enumeration values that indicates the style
		//     elements that can be present in s for the parse operation to succeed and
		//     that defines how to interpret the parsed date in relation to the current
		//     time zone or the current date. A typical value to specify is System.Globalization.DateTimeStyles.None.
		//
		// Returns:
		//     A System.DateTime equivalent to the date and time contained in s as specified
		//     by provider and styles.
		//
		// Exceptions:
		//   System.ArgumentNullException:
		//     s is null.
		//
		//   System.FormatException:
		//     s does not contain a valid string representation of a date and time.
		//
		//   System.ArgumentException:
		//     styles contains an invalid combination of System.Globalization.DateTimeStyles
		//     values. For example, both System.Globalization.DateTimeStyles.AssumeLocal
		//     and System.Globalization.DateTimeStyles.AssumeUniversal.

		public static System.DateTime Parse(string s, IFormatProvider provider, DateTimeStyles styles) { return System.DateTime.Parse(s, provider, styles); }
		//
		// Summary:
		//     Converts the specified string representation of a date and time to its System.DateTime
		//     equivalent using the specified format and culture-specific format information.
		//     The format of the string representation must match the specified format exactly.
		//
		// Parameters:
		//   s:
		//     A string that contains a date and time to convert.
		//
		//   format:
		//     A format specifier that defines the required format of s.
		//
		//   provider:
		//     An object that supplies culture-specific format information about s.
		//
		// Returns:
		//     A System.DateTime equivalent to the date and time contained in s as specified
		//     by format and provider.
		//
		// Exceptions:
		//   System.ArgumentNullException:
		//     s or format is null.
		//
		//   System.FormatException:
		//     s or format is an empty string. -or- s does not contain a date and time that
		//     corresponds to the pattern specified in format. -or-The hour component and
		//     the AM/PM designator in s do not agree.
		public static System.DateTime ParseExact(string s, string format, IFormatProvider provider) { return System.DateTime.ParseExact(s, format, provider); }
		//
		// Summary:
		//     Converts the specified string representation of a date and time to its System.DateTime
		//     equivalent using the specified format, culture-specific format information,
		//     and style. The format of the string representation must match the specified
		//     format exactly or an exception is thrown.
		//
		// Parameters:
		//   s:
		//     A string containing a date and time to convert.
		//
		//   format:
		//     A format specifier that defines the required format of s.
		//
		//   provider:
		//     An object that supplies culture-specific formatting information about s.
		//
		//   style:
		//     A bitwise combination of the enumeration values that provides additional
		//     information about s, about style elements that may be present in s, or about
		//     the conversion from s to a System.DateTime value. A typical value to specify
		//     is System.Globalization.DateTimeStyles.None.
		//
		// Returns:
		//     A System.DateTime equivalent to the date and time contained in s as specified
		//     by format, provider, and style.
		//
		// Exceptions:
		//   System.ArgumentNullException:
		//     s or format is null.
		//
		//   System.FormatException:
		//     s or format is an empty string. -or- s does not contain a date and time that
		//     corresponds to the pattern specified in format. -or-The hour component and
		//     the AM/PM designator in s do not agree.
		//
		//   System.ArgumentException:
		//     style contains an invalid combination of System.Globalization.DateTimeStyles
		//     values. For example, both System.Globalization.DateTimeStyles.AssumeLocal
		//     and System.Globalization.DateTimeStyles.AssumeUniversal.
		public static System.DateTime ParseExact(string s, string format, IFormatProvider provider, DateTimeStyles style) { return System.DateTime.ParseExact(s, format, provider, style); }
		//
		// Summary:
		//     Converts the specified string representation of a date and time to its System.DateTime
		//     equivalent using the specified array of formats, culture-specific format
		//     information, and style. The format of the string representation must match
		//     at least one of the specified formats exactly or an exception is thrown.
		//
		// Parameters:
		//   s:
		//     A string containing one or more dates and times to convert.
		//
		//   formats:
		//     An array of allowable formats of s.
		//
		//   provider:
		//     An System.IFormatProvider that supplies culture-specific format information
		//     about s.
		//
		//   style:
		//     A bitwise combination of System.Globalization.DateTimeStyles values that
		//     indicates the permitted format of s. A typical value to specify is System.Globalization.DateTimeStyles.None.
		//
		// Returns:
		//     A System.DateTime equivalent to the date and time contained in s as specified
		//     by formats, provider, and style.
		//
		// Exceptions:
		//   System.ArgumentNullException:
		//     s or formats is null.
		//
		//   System.FormatException:
		//     s is an empty string. -or- an element of formats is an empty string. -or-
		//     s does not contain a date and time that corresponds to any element of formats.
		//     -or-The hour component and the AM/PM designator in s do not agree.
		//
		//   System.ArgumentException:
		//     style contains an invalid combination of System.Globalization.DateTimeStyles
		//     values. For example, both System.Globalization.DateTimeStyles.AssumeLocal
		//     and System.Globalization.DateTimeStyles.AssumeUniversal.
		public static System.DateTime ParseExact(string s, string[] formats, IFormatProvider provider, DateTimeStyles style) { return System.DateTime.ParseExact(s, formats, provider, style); }
		//
		// Summary:
		//     Creates a new System.DateTime object that has the same number of ticks as
		//     the specified System.DateTime, but is designated as either local time, Coordinated
		//     Universal Time (UTC), or neither, as indicated by the specified System.DateTimeKind
		//     value.
		//
		// Parameters:
		//   value:
		//     A date and time.
		//
		//   kind:
		//     One of the enumeration values that indicates whether the new object represents
		//     local time, UTC, or neither.
		//
		// Returns:
		//     A new object that has the same number of ticks as the object represented
		//     by the value parameter and the System.DateTimeKind value specified by the
		//     kind parameter.

		public static System.DateTime SpecifyKind(System.DateTime value, DateTimeKind kind) { return System.DateTime.SpecifyKind(value, kind); }
		//
		// Summary:
		//     Subtracts the specified date and time from this instance.
		//
		// Parameters:
		//   value:
		//     An instance of System.DateTime.
		//
		// Returns:
		//     A System.TimeSpan interval equal to the date and time represented by this
		//     instance minus the date and time represented by value.
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     The result is less than System.DateTime.MinValue or greater than System.DateTime.MaxValue.

		public System.TimeSpan Subtract(System.DateTime value) { return Value.Subtract(value); }
		//
		// Summary:
		//     Subtracts the specified duration from this instance.
		//
		// Parameters:
		//   value:
		//     An instance of System.TimeSpan.
		//
		// Returns:
		//     A System.DateTime equal to the date and time represented by this instance
		//     minus the time interval represented by value.
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     The result is less than System.DateTime.MinValue or greater than System.DateTime.MaxValue.
		public System.DateTime Subtract(System.TimeSpan value) { return Value.Subtract(value); }
		//
		// Summary:
		//     Serializes the current System.DateTime object to a 64-bit binary value that
		//     subsequently can be used to recreate the System.DateTime object.
		//
		// Returns:
		//     A 64-bit signed integer that encodes the System.DateTime.Kind and System.DateTime.Ticks
		//     properties.
		public long ToBinary() { return Value.ToBinary(); }
		//
		// Summary:
		//     Converts the value of the current System.DateTime object to a Windows file
		//     time.
		//
		// Returns:
		//     The value of the current System.DateTime object expressed as a Windows file
		//     time.
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     The resulting file time would represent a date and time before 12:00 midnight
		//     January 1, 1601 C.E. UTC.
		public long ToFileTime() { return Value.ToFileTime(); }
		//
		// Summary:
		//     Converts the value of the current System.DateTime object to a Windows file
		//     time.
		//
		// Returns:
		//     The value of the current System.DateTime object expressed as a Windows file
		//     time.
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     The resulting file time would represent a date and time before 12:00 midnight
		//     January 1, 1601 C.E. UTC.
		public long ToFileTimeUtc() { return Value.ToFileTimeUtc(); }
		//
		// Summary:
		//     Converts the value of the current System.DateTime object to local time.
		//
		// Returns:
		//     A System.DateTime object whose System.DateTime.Kind property is System.DateTimeKind.Local,
		//     and whose value is the local time equivalent to the value of the current
		//     System.DateTime object, or System.DateTime.MaxValue if the converted value
		//     is too large to be represented by a System.DateTime object, or System.DateTime.MinValue
		//     if the converted value is too small to be represented as a System.DateTime
		//     object.
		public System.DateTime ToLocalTime() { return Value.ToLocalTime(); }
		//
		// Summary:
		//     Converts the value of the current System.DateTime object to its equivalent
		//     long date string representation.
		//
		// Returns:
		//     A string that contains the long date string representation of the current
		//     System.DateTime object.

		public string ToLongDateString() { return Value.ToLongDateString(); }
		//
		// Summary:
		//     Converts the value of the current System.DateTime object to its equivalent
		//     long time string representation.
		//
		// Returns:
		//     A string that contains the long time string representation of the current
		//     System.DateTime object.

		public string ToLongTimeString() { return Value.ToLongTimeString(); }
		//
		// Summary:
		//     Converts the value of this instance to the equivalent OLE Automation date.
		//
		// Returns:
		//     A double-precision floating-point number that contains an OLE Automation
		//     date equivalent to the value of this instance.
		//
		// Exceptions:
		//   System.OverflowException:
		//     The value of this instance cannot be represented as an OLE Automation Date.
		public double ToOADate() { return Value.ToOADate(); }
		//
		// Summary:
		//     Converts the value of the current System.DateTime object to its equivalent
		//     short date string representation.
		//
		// Returns:
		//     A string that contains the short date string representation of the current
		//     System.DateTime object.

		public string ToShortDateString() { return Value.ToShortDateString(); }
		//
		// Summary:
		//     Converts the value of the current System.DateTime object to its equivalent
		//     short time string representation.
		//
		// Returns:
		//     A string that contains the short time string representation of the current
		//     System.DateTime object.

		public string ToShortTimeString() { return Value.ToShortTimeString(); }
		//
		// Summary:
		//     Converts the value of the current System.DateTime object to its equivalent
		//     string representation.
		//
		// Returns:
		//     A string representation of the value of the current System.DateTime object.

		public override string ToString() { return Value.ToString(); }
		//
		// Summary:
		//     Converts the value of the current System.DateTime object to its equivalent
		//     string representation using the specified culture-specific format information.
		//
		// Parameters:
		//   provider:
		//     An System.IFormatProvider that supplies culture-specific formatting information.
		//
		// Returns:
		//     A string representation of value of the current System.DateTime object as
		//     specified by provider.
		public string ToString(IFormatProvider provider) { return Value.ToString(provider); }
		//
		// Summary:
		//     Converts the value of the current System.DateTime object to its equivalent
		//     string representation using the specified format.
		//
		// Parameters:
		//   format:
		//     A standard or custom date and time format string.
		//
		// Returns:
		//     A string representation of value of the current System.DateTime object as
		//     specified by format.
		//
		// Exceptions:
		//   System.FormatException:
		//     The length of format is 1, and it is not one of the format specifier characters
		//     defined for System.Globalization.DateTimeFormatInfo.-or- format does not
		//     contain a valid custom format pattern.

		public string ToString(string format) { return Value.ToString(format); }
		//
		// Summary:
		//     Converts the value of the current System.DateTime object to its equivalent
		//     string representation using the specified format and culture-specific format
		//     information.
		//
		// Parameters:
		//   format:
		//     A standard or custom date and time format string.
		//
		//   provider:
		//     An object that supplies culture-specific formatting information.
		//
		// Returns:
		//     A string representation of value of the current System.DateTime object as
		//     specified by format and provider.
		//
		// Exceptions:
		//   System.FormatException:
		//     The length of format is 1, and it is not one of the format specifier characters
		//     defined for System.Globalization.DateTimeFormatInfo.-or- format does not
		//     contain a valid custom format pattern.

		public string ToString(string format, IFormatProvider provider) { return Value.ToString(format, provider); }
		//
		// Summary:
		//     Converts the value of the current System.DateTime object to Coordinated Universal
		//     Time (UTC).
		//
		// Returns:
		//     A System.DateTime object whose System.DateTime.Kind property is System.DateTimeKind.Utc,
		//     and whose value is the UTC equivalent to the value of the current System.DateTime
		//     object, or System.DateTime.MaxValue if the converted value is too large to
		//     be represented by a System.DateTime object, or System.DateTime.MinValue if
		//     the converted value is too small to be represented by a System.DateTime object.
		public DateTime ToUniversalTime() { return Value.ToUniversalTime(); }
		//
		// Summary:
		//     Converts the specified string representation of a date and time to its System.DateTime
		//     equivalent and returns a value that indicates whether the conversion succeeded.
		//
		// Parameters:
		//   s:
		//     A string containing a date and time to convert.
		//
		//   result:
		//     When this method returns, contains the System.DateTime value equivalent to
		//     the date and time contained in s, if the conversion succeeded, or System.DateTime.MinValue
		//     if the conversion failed. The conversion fails if the s parameter is null,
		//     is an empty string (""), or does not contain a valid string representation
		//     of a date and time. This parameter is passed uninitialized.
		//
		// Returns:
		//     true if the s parameter was converted successfully; otherwise, false.

		public static bool TryParse(string s, out DateTime result) { System.DateTime res; var ok = System.DateTime.TryParse(s, out res); result = (DateTime)res; return ok; }
		//
		// Summary:
		//     Converts the specified string representation of a date and time to its System.DateTime
		//     equivalent using the specified culture-specific format information and formatting
		//     style, and returns a value that indicates whether the conversion succeeded.
		//
		// Parameters:
		//   s:
		//     A string containing a date and time to convert.
		//
		//   provider:
		//     An object that supplies culture-specific formatting information about s.
		//
		//   styles:
		//     A bitwise combination of enumeration values that defines how to interpret
		//     the parsed date in relation to the current time zone or the current date.
		//     A typical value to specify is System.Globalization.DateTimeStyles.None.
		//
		//   result:
		//     When this method returns, contains the System.DateTime value equivalent to
		//     the date and time contained in s, if the conversion succeeded, or System.DateTime.MinValue
		//     if the conversion failed. The conversion fails if the s parameter is null,
		//     is an empty string (""), or does not contain a valid string representation
		//     of a date and time. This parameter is passed uninitialized.
		//
		// Returns:
		//     true if the s parameter was converted successfully; otherwise, false.
		//
		// Exceptions:
		//   System.ArgumentException:
		//     styles is not a valid System.Globalization.DateTimeStyles value.-or-styles
		//     contains an invalid combination of System.Globalization.DateTimeStyles values
		//     (for example, both System.Globalization.DateTimeStyles.AssumeLocal and System.Globalization.DateTimeStyles.AssumeUniversal).
		//
		//   System.NotSupportedException:
		//     provider is a neutral culture and cannot be used in a parsing operation.

		public static bool TryParse(string s, IFormatProvider provider, DateTimeStyles styles, out DateTime result) { System.DateTime res; var ok = System.DateTime.TryParse(s, provider, styles, out res); result = (DateTime)res; return ok; }
		//
		// Summary:
		//     Converts the specified string representation of a date and time to its System.DateTime
		//     equivalent using the specified format, culture-specific format information,
		//     and style. The format of the string representation must match the specified
		//     format exactly. The method returns a value that indicates whether the conversion
		//     succeeded.
		//
		// Parameters:
		//   s:
		//     A string containing a date and time to convert.
		//
		//   format:
		//     The required format of s.
		//
		//   provider:
		//     An System.IFormatProvider object that supplies culture-specific formatting
		//     information about s.
		//
		//   style:
		//     A bitwise combination of one or more enumeration values that indicate the
		//     permitted format of s.
		//
		//   result:
		//     When this method returns, contains the System.DateTime value equivalent to
		//     the date and time contained in s, if the conversion succeeded, or System.DateTime.MinValue
		//     if the conversion failed. The conversion fails if either the s or format
		//     parameter is null, is an empty string, or does not contain a date and time
		//     that correspond to the pattern specified in format. This parameter is passed
		//     uninitialized.
		//
		// Returns:
		//     true if s was converted successfully; otherwise, false.
		//
		// Exceptions:
		//   System.ArgumentException:
		//     styles is not a valid System.Globalization.DateTimeStyles value.-or-styles
		//     contains an invalid combination of System.Globalization.DateTimeStyles values
		//     (for example, both System.Globalization.DateTimeStyles.AssumeLocal and System.Globalization.DateTimeStyles.AssumeUniversal).
		public static bool TryParseExact(string s, string format, IFormatProvider provider, DateTimeStyles style, out DateTime result) { System.DateTime res; var ok = System.DateTime.TryParseExact(s, format, provider, style, out res); result = (DateTime)res; return ok; }
		//
		///<summary>
		//     Converts the specified string representation of a date and time to its System.DateTime
		//     equivalent using the specified array of formats, culture-specific format
		//     information, and style. The format of the string representation must match
		//     at least one of the specified formats exactly. The method returns a value
		//     that indicates whether the conversion succeeded.
		//</summary>
		// Parameters:
		//   s:
		//     A string containing one or more dates and times to convert.
		//
		//   formats:
		//     An array of allowable formats of s.
		//
		//   provider:
		//     An object that supplies culture-specific format information about s.
		//
		//   style:
		//     A bitwise combination of enumeration values that indicates the permitted
		//     format of s. A typical value to specify is System.Globalization.DateTimeStyles.None.
		//
		//   result:
		//     When this method returns, contains the System.DateTime value equivalent to
		//     the date and time contained in s, if the conversion succeeded, or System.DateTime.MinValue
		//     if the conversion failed. The conversion fails if s or formats is null, s
		//     or an element of formats is an empty string, or the format of s is not exactly
		//     as specified by at least one of the format patterns in formats. This parameter
		//     is passed uninitialized.
		//
		// Returns:
		//     true if the s parameter was converted successfully; otherwise, false.
		//
		// Exceptions:
		//   System.ArgumentException:
		//     styles is not a valid System.Globalization.DateTimeStyles value.-or-styles
		//     contains an invalid combination of System.Globalization.DateTimeStyles values
		//     (for example, both System.Globalization.DateTimeStyles.AssumeLocal and System.Globalization.DateTimeStyles.AssumeUniversal).
		public static bool TryParseExact(string s, string[] formats, IFormatProvider provider, DateTimeStyles style, out DateTime result) { System.DateTime res; var ok = System.DateTime.TryParseExact(s, formats, provider, style, out res); result = (DateTime)res; return ok; }
	}

	[ComplexType]
	public class TimeSpan {

		public long Value { get; set; }

		public static implicit operator System.TimeSpan(TimeSpan t) { return new System.TimeSpan(t.Value); }
		public static implicit operator TimeSpan(System.TimeSpan t) { return new TimeSpan { Value = t.Ticks }; }

		/* public override bool Equals(object obj) { return ((System.TimeSpan)this).Equals((System.TimeSpan)obj); }
		public override int GetHashCode() { return ((System.TimeSpan)this).GetHashCode(); }
		public override string ToString() { return ((System.TimeSpan)this).ToString(); }
		*/

		// TimeSpan API
		// Summary:
		//     Represents the number of ticks in 1 day. This field is constant.
		public const long TicksPerDay = 864000000000;
		//
		// Summary:
		//     Represents the number of ticks in 1 hour. This field is constant.
		public const long TicksPerHour = 36000000000;
		//
		// Summary:
		//     Represents the number of ticks in 1 millisecond. This field is constant.
		public const long TicksPerMillisecond = 10000;
		//
		// Summary:
		//     Represents the number of ticks in 1 minute. This field is constant.
		public const long TicksPerMinute = 600000000;
		//
		// Summary:
		//     Represents the number of ticks in 1 second.
		public const long TicksPerSecond = 10000000;

		// Summary:
		//     Represents the maximum System.TimeSpan value. This field is read-only.
		public static readonly TimeSpan MaxValue = System.TimeSpan.MaxValue;
		//
		// Summary:
		//     Represents the minimum System.TimeSpan value. This field is read-only.
		public static readonly TimeSpan MinValue = System.TimeSpan.MinValue;
		//
		// Summary:
		//     Represents the zero System.TimeSpan value. This field is read-only.
		public static readonly TimeSpan Zero = System.TimeSpan.Zero;


		public TimeSpan() { }

		public TimeSpan(System.TimeSpan t) { Value = t.Ticks; }
		//
		// Summary:
		//     Initializes a new System.TimeSpan to the specified number of ticks.
		//
		// Parameters:
		//   ticks:
		//     A time period expressed in 100-nanosecond units.
		public TimeSpan(long ticks) { Value = ticks; }
		//
		// Summary:
		//     Initializes a new System.TimeSpan to a specified number of hours, minutes,
		//     and seconds.
		//
		// Parameters:
		//   hours:
		//     Number of hours.
		//
		//   minutes:
		//     Number of minutes.
		//
		//   seconds:
		//     Number of seconds.
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     The parameters specify a System.TimeSpan value less than System.TimeSpan.MinValue
		//     or greater than System.TimeSpan.MaxValue.
		public TimeSpan(int hours, int minutes, int seconds) { Value = new System.TimeSpan(hours, minutes, seconds).Ticks; }
		//
		// Summary:
		//     Initializes a new System.TimeSpan to a specified number of days, hours, minutes,
		//     and seconds.
		//
		// Parameters:
		//   days:
		//     Number of days.
		//
		//   hours:
		//     Number of hours.
		//
		//   minutes:
		//     Number of minutes.
		//
		//   seconds:
		//     Number of seconds.
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     The parameters specify a System.TimeSpan value less than System.TimeSpan.MinValue
		//     or greater than System.TimeSpan.MaxValue.
		public TimeSpan(int days, int hours, int minutes, int seconds) { Value = new System.TimeSpan(days, hours, minutes, seconds).Ticks; }
		//
		// Summary:
		//     Initializes a new System.TimeSpan to a specified number of days, hours, minutes,
		//     seconds, and milliseconds.
		//
		// Parameters:
		//   days:
		//     Number of days.
		//
		//   hours:
		//     Number of hours.
		//
		//   minutes:
		//     Number of minutes.
		//
		//   seconds:
		//     Number of seconds.
		//
		//   milliseconds:
		//     Number of milliseconds.
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     The parameters specify a System.TimeSpan value less than System.TimeSpan.MinValue
		//     or greater than System.TimeSpan.MaxValue.
		public TimeSpan(int days, int hours, int minutes, int seconds, int milliseconds) { Value = new System.TimeSpan(days, hours, minutes, seconds, milliseconds).Ticks; }

		// Summary:
		//     Returns a System.TimeSpan whose value is the negated value of the specified
		//     instance.
		//
		// Parameters:
		//   t:
		//     The time interval to be negated.
		//
		// Returns:
		//     An object that has the same numeric value as this instance, but the opposite
		//     sign.
		//
		// Exceptions:
		//   System.OverflowException:
		//     The negated value of this instance cannot be represented by a System.TimeSpan;
		//     that is, the value of this instance is System.TimeSpan.MinValue.
		public static TimeSpan operator -(TimeSpan t) { return new TimeSpan { Value = -t.Value }; }
		//
		// Summary:
		//     Subtracts a specified System.TimeSpan from another specified System.TimeSpan.
		//
		// Parameters:
		//   t1:
		//     The minuend.
		//
		//   t2:
		//     The subtrahend.
		//
		// Returns:
		//     An object whose value is the result of the value of t1 minus the value of
		//     t2.
		//
		// Exceptions:
		//   System.OverflowException:
		//     The return value is less than System.TimeSpan.MinValue or greater than System.TimeSpan.MaxValue.

		//public static TimeSpan operator -(TimeSpan t1, TimeSpan t2) { return new TimeSpan { Value = t1.Value - t2.Value }; }
		//
		// Summary:
		//     Indicates whether two System.TimeSpan instances are not equal.
		//
		// Parameters:
		//   t1:
		//     The first time interval to compare.
		//
		//   t2:
		//     The second time interval to compare.
		//
		// Returns:
		//     true if the values of t1 and t2 are not equal; otherwise, false.

		//public static bool operator !=(TimeSpan t1, TimeSpan t2) { return t1.Value != t2.Value; }
		//
		// Summary:
		//     Returns the specified instance of System.TimeSpan.
		//
		// Parameters:
		//   t:
		//     The time interval to return.
		//
		// Returns:
		//     The time interval specified by t.
		public static TimeSpan operator +(TimeSpan t) { return new TimeSpan { Value = t.Value }; }
		//
		// Summary:
		//     Adds two specified System.TimeSpan instances.
		//
		// Parameters:
		//   t1:
		//     The first time interval to add.
		//
		//   t2:
		//     The second time interval to add.
		//
		// Returns:
		//     An object whose value is the sum of the values of t1 and t2.
		//
		// Exceptions:
		//   System.OverflowException:
		//     The resulting System.TimeSpan is less than System.TimeSpan.MinValue or greater
		//     than System.TimeSpan.MaxValue.
		//public static TimeSpan operator +(TimeSpan t1, TimeSpan t2) { return new TimeSpan { Value = t1.Value + t2.Value }; }
		//
		// Summary:
		//     Indicates whether a specified System.TimeSpan is less than another specified
		//     System.TimeSpan.
		//
		// Parameters:
		//   t1:
		//     The first time interval to compare.
		//
		//   t2:
		//     The second time interval to compare.
		//
		// Returns:
		//     true if the value of t1 is less than the value of t2; otherwise, false.

		//public static bool operator <(TimeSpan t1, TimeSpan t2) { return t1.Value < t2.Value; }
		//
		// Summary:
		//     Indicates whether a specified System.TimeSpan is less than or equal to another
		//     specified System.TimeSpan.
		//
		// Parameters:
		//   t1:
		//     The first time interval to compare.
		//
		//   t2:
		//     The second time interval to compare.
		//
		// Returns:
		//     true if the value of t1 is less than or equal to the value of t2; otherwise,
		//     false.

		//public static bool operator <=(TimeSpan t1, TimeSpan t2) { return t1.Value <= t2.Value; }
		//
		// Summary:
		//     Indicates whether two System.TimeSpan instances are equal.
		//
		// Parameters:
		//   t1:
		//     The first time interval to add.
		//
		//   t2:
		//     The second time interval to add.
		//
		// Returns:
		//     true if the values of t1 and t2 are equal; otherwise, false.

		//public static bool operator ==(TimeSpan t1, TimeSpan t2) { return t1.Value == t2.Value; }
		//
		// Summary:
		//     Indicates whether a specified System.TimeSpan is greater than another specified
		//     System.TimeSpan.
		//
		// Parameters:
		//   t1:
		//     The first time interval to compare.
		//
		//   t2:
		//     The second time interval to compare.
		//
		// Returns:
		//     true if the value of t1 is greater than the value of t2; otherwise, false.

		//public static bool operator >(TimeSpan t1, TimeSpan t2) { return t1.Value > t2.Value; }
		//
		// Summary:
		//     Indicates whether a specified System.TimeSpan is greater than or equal to
		//     another specified System.TimeSpan.
		//
		// Parameters:
		//   t1:
		//     The first time interval to compare.
		//
		//   t2:
		//     The second time interval to compare.
		//
		// Returns:
		//     true if the value of t1 is greater than or equal to the value of t2; otherwise,
		//     false.

		//public static bool operator >=(TimeSpan t1, TimeSpan t2) { return t1.Value >= t2.Value; }

		// Summary:
		//     Gets the days component of the time interval represented by the current System.TimeSpan
		//     structure.
		//
		// Returns:
		//     The day component of this instance. The return value can be positive or negative.
		public int Days { get { return ((System.TimeSpan)this).Days; } }
		//
		// Summary:
		//     Gets the hours component of the time interval represented by the current
		//     System.TimeSpan structure.
		//
		// Returns:
		//     The hour component of the current System.TimeSpan structure. The return value
		//     ranges from -23 through 23.
		public int Hours { get { return ((System.TimeSpan)this).Hours; } }
		//
		// Summary:
		//     Gets the milliseconds component of the time interval represented by the current
		//     System.TimeSpan structure.
		//
		// Returns:
		//     The millisecond component of the current System.TimeSpan structure. The return
		//     value ranges from -999 through 999.
		public int Milliseconds { get { return ((System.TimeSpan)this).Milliseconds; } }
		//
		// Summary:
		//     Gets the minutes component of the time interval represented by the current
		//     System.TimeSpan structure.
		//
		// Returns:
		//     The minute component of the current System.TimeSpan structure. The return
		//     value ranges from -59 through 59.
		public int Minutes { get { return ((System.TimeSpan)this).Minutes; } }
		//
		// Summary:
		//     Gets the seconds component of the time interval represented by the current
		//     System.TimeSpan structure.
		//
		// Returns:
		//     The second component of the current System.TimeSpan structure. The return
		//     value ranges from -59 through 59.
		public int Seconds { get { return ((System.TimeSpan)this).Seconds; } }
		//
		// Summary:
		//     Gets the number of ticks that represent the value of the current System.TimeSpan
		//     structure.
		//
		// Returns:
		//     The number of ticks contained in this instance.
		public long Ticks { get { return Value; } }
		//
		// Summary:
		//     Gets the value of the current System.TimeSpan structure expressed in whole
		//     and fractional days.
		//
		// Returns:
		//     The total number of days represented by this instance.
		public double TotalDays { get { return ((System.TimeSpan)this).TotalDays; } }
		//
		// Summary:
		//     Gets the value of the current System.TimeSpan structure expressed in whole
		//     and fractional hours.
		//
		// Returns:
		//     The total number of hours represented by this instance.
		public double TotalHours { get { return ((System.TimeSpan)this).TotalHours; } }
		//
		// Summary:
		//     Gets the value of the current System.TimeSpan structure expressed in whole
		//     and fractional milliseconds.
		//
		// Returns:
		//     The total number of milliseconds represented by this instance.
		public double TotalMilliseconds { get { return ((System.TimeSpan)this).TotalMilliseconds; } }
		//
		// Summary:
		//     Gets the value of the current System.TimeSpan structure expressed in whole
		//     and fractional minutes.
		//
		// Returns:
		//     The total number of minutes represented by this instance.
		public double TotalMinutes { get { return ((System.TimeSpan)this).TotalMinutes; } }
		//
		// Summary:
		//     Gets the value of the current System.TimeSpan structure expressed in whole
		//     and fractional seconds.
		//
		// Returns:
		//     The total number of seconds represented by this instance.
		public double TotalSeconds { get { return ((System.TimeSpan)this).TotalSeconds; } }

		// Summary:
		//     Adds the specified System.TimeSpan to this instance.
		//
		// Parameters:
		//   ts:
		//     The time interval to add.
		//
		// Returns:
		//     An object that represents the value of this instance plus the value of ts.
		//
		// Exceptions:
		//   System.OverflowException:
		//     The resulting System.TimeSpan is less than System.TimeSpan.MinValue or greater
		//     than System.TimeSpan.MaxValue.
		public System.TimeSpan Add(System.TimeSpan ts) { return ((System.TimeSpan)this).Add(ts); }
		//
		// Summary:
		//     Compares two System.TimeSpan values and returns an integer that indicates
		//     whether the first value is shorter than, equal to, or longer than the second
		//     value.
		//
		// Parameters:
		//   t1:
		//     The first time interval to compare.
		//
		//   t2:
		//     The second time interval to compare.
		//
		// Returns:
		//     One of the following values.Value Description -1 t1 is shorter than t2. 0
		//     t1 is equal to t2. 1 t1 is longer than t2.
		public static int Compare(System.TimeSpan t1, System.TimeSpan t2) { return System.TimeSpan.Compare(t1, t2); }
		//
		// Summary:
		//     Compares this instance to a specified object and returns an integer that
		//     indicates whether this instance is shorter than, equal to, or longer than
		//     the specified object.
		//
		// Parameters:
		//   value:
		//     An object to compare, or null.
		//
		// Returns:
		//     One of the following values.Value Description -1 This instance is shorter
		//     than value. 0 This instance is equal to value. 1 This instance is longer
		//     than value.-or- value is null.
		//
		// Exceptions:
		//   System.ArgumentException:
		//     value is not a System.TimeSpan.
		public int CompareTo(object value) { return ((System.TimeSpan)this).CompareTo(value); }
		//
		// Summary:
		//     Compares this instance to a specified System.TimeSpan object and returns
		//     an integer that indicates whether this instance is shorter than, equal to,
		//     or longer than the System.TimeSpan object.
		//
		// Parameters:
		//   value:
		//     An object to compare to this instance.
		//
		// Returns:
		//     A signed number indicating the relative values of this instance and value.Value
		//     Description A negative integer This instance is shorter than value. Zero
		//     This instance is equal to value. A positive integer This instance is longer
		//     than value.
		public int CompareTo(TimeSpan value) { return ((System.TimeSpan)this).CompareTo(value); }
		//
		// Summary:
		//     Returns a new System.TimeSpan object whose value is the absolute value of
		//     the current System.TimeSpan object.
		//
		// Returns:
		//     A new object whose value is the absolute value of the current System.TimeSpan
		//     object.
		//
		// Exceptions:
		//   System.OverflowException:
		//     The value of this instance is System.TimeSpan.MinValue.
		public System.TimeSpan Duration() { return ((System.TimeSpan)this).Duration(); }
		//
		// Summary:
		//     Returns a value indicating whether this instance is equal to a specified
		//     object.
		//
		// Parameters:
		//   value:
		//     An object to compare with this instance.
		//
		// Returns:
		//     true if value is a System.TimeSpan object that represents the same time interval
		//     as the current System.TimeSpan structure; otherwise, false.
		public override bool Equals(object value) { return ((System.TimeSpan)this).Equals(value); }
		//
		// Summary:
		//     Returns a value indicating whether this instance is equal to a specified
		//     System.TimeSpan object.
		//
		// Parameters:
		//   obj:
		//     An object to compare with this instance.
		//
		// Returns:
		//     true if obj represents the same time interval as this instance; otherwise,
		//     false.
		public bool Equals(System.TimeSpan obj) { return ((System.TimeSpan)this).Equals(obj); }
		//
		// Summary:
		//     Returns a value that indicates whether two specified instances of System.TimeSpan
		//     are equal.
		//
		// Parameters:
		//   t1:
		//     The first time interval to compare.
		//
		//   t2:
		//     The second time interval to compare.
		//
		// Returns:
		//     true if the values of t1 and t2 are equal; otherwise, false.
		public static bool Equals(System.TimeSpan t1, System.TimeSpan t2) { return System.TimeSpan.Equals(t1, t2); }
		//
		// Summary:
		//     Returns a System.TimeSpan that represents a specified number of days, where
		//     the specification is accurate to the nearest millisecond.
		//
		// Parameters:
		//   value:
		//     A number of days, accurate to the nearest millisecond.
		//
		// Returns:
		//     An object that represents value.
		//
		// Exceptions:
		//   System.OverflowException:
		//     value is less than System.TimeSpan.MinValue or greater than System.TimeSpan.MaxValue.
		//     -or-value is System.Double.PositiveInfinity.-or-value is System.Double.NegativeInfinity.
		//
		//   System.ArgumentException:
		//     value is equal to System.Double.NaN.
		public static System.TimeSpan FromDays(double value) { return System.TimeSpan.FromDays(value); }
		//
		// Summary:
		//     Returns a System.TimeSpan that represents a specified number of hours, where
		//     the specification is accurate to the nearest millisecond.
		//
		// Parameters:
		//   value:
		//     A number of hours accurate to the nearest millisecond.
		//
		// Returns:
		//     An object that represents value.
		//
		// Exceptions:
		//   System.OverflowException:
		//     value is less than System.TimeSpan.MinValue or greater than System.TimeSpan.MaxValue.
		//     -or-value is System.Double.PositiveInfinity.-or-value is System.Double.NegativeInfinity.
		//
		//   System.ArgumentException:
		//     value is equal to System.Double.NaN.
		public static System.TimeSpan FromHours(double value) { return System.TimeSpan.FromHours(value); }
		//
		// Summary:
		//     Returns a System.TimeSpan that represents a specified number of milliseconds.
		//
		// Parameters:
		//   value:
		//     A number of milliseconds.
		//
		// Returns:
		//     An object that represents value.
		//
		// Exceptions:
		//   System.OverflowException:
		//     value is less than System.TimeSpan.MinValue or greater than System.TimeSpan.MaxValue.-or-value
		//     is System.Double.PositiveInfinity.-or-value is System.Double.NegativeInfinity.
		//
		//   System.ArgumentException:
		//     value is equal to System.Double.NaN.
		public static System.TimeSpan FromMilliseconds(double value) { return System.TimeSpan.FromMilliseconds(value); }
		//
		// Summary:
		//     Returns a System.TimeSpan that represents a specified number of minutes,
		//     where the specification is accurate to the nearest millisecond.
		//
		// Parameters:
		//   value:
		//     A number of minutes, accurate to the nearest millisecond.
		//
		// Returns:
		//     An object that represents value.
		//
		// Exceptions:
		//   System.OverflowException:
		//     value is less than System.TimeSpan.MinValue or greater than System.TimeSpan.MaxValue.-or-value
		//     is System.Double.PositiveInfinity.-or-value is System.Double.NegativeInfinity.
		//
		//   System.ArgumentException:
		//     value is equal to System.Double.NaN.
		public static System.TimeSpan FromMinutes(double value) { return System.TimeSpan.FromMinutes(value); }
		//
		// Summary:
		//     Returns a System.TimeSpan that represents a specified number of seconds,
		//     where the specification is accurate to the nearest millisecond.
		//
		// Parameters:
		//   value:
		//     A number of seconds, accurate to the nearest millisecond.
		//
		// Returns:
		//     An object that represents value.
		//
		// Exceptions:
		//   System.OverflowException:
		//     value is less than System.TimeSpan.MinValue or greater than System.TimeSpan.MaxValue.-or-value
		//     is System.Double.PositiveInfinity.-or-value is System.Double.NegativeInfinity.
		//
		//   System.ArgumentException:
		//     value is equal to System.Double.NaN.
		public static System.TimeSpan FromSeconds(double value) { return System.TimeSpan.FromSeconds(value); }
		//
		// Summary:
		//     Returns a System.TimeSpan that represents a specified time, where the specification
		//     is in units of ticks.
		//
		// Parameters:
		//   value:
		//     A number of ticks that represent a time.
		//
		// Returns:
		//     An object that represents value.

		public static TimeSpan FromTicks(long value) { return System.TimeSpan.FromTicks(value); }
		//
		// Summary:
		//     Returns a hash code for this instance.
		//
		// Returns:
		//     A 32-bit signed integer hash code.
		public override int GetHashCode() { return ((System.TimeSpan)this).GetHashCode(); }
		//
		// Summary:
		//     Returns a System.TimeSpan whose value is the negated value of this instance.
		//
		// Returns:
		//     The same numeric value as this instance, but with the opposite sign.
		//
		// Exceptions:
		//   System.OverflowException:
		//     The negated value of this instance cannot be represented by a System.TimeSpan;
		//     that is, the value of this instance is System.TimeSpan.MinValue.
		public System.TimeSpan Negate() { return ((System.TimeSpan)this).Negate(); }
		//
		// Summary:
		//     Converts the string representation of a time interval to its System.TimeSpan
		//     equivalent.
		//
		// Parameters:
		//   s:
		//     A string that specifies the time interval to convert.
		//
		// Returns:
		//     A time interval that corresponds to s.
		//
		// Exceptions:
		//   System.ArgumentNullException:
		//     s is null.
		//
		//   System.FormatException:
		//     s has an invalid format.
		//
		//   System.OverflowException:
		//     s represents a number that is less than System.TimeSpan.MinValue or greater
		//     than System.TimeSpan.MaxValue.-or- At least one of the days, hours, minutes,
		//     or seconds components is outside its valid range.
		public static System.TimeSpan Parse(string s) { return System.TimeSpan.Parse(s); }
		//
		// Summary:
		//     Converts the string representation of a time interval to its System.TimeSpan
		//     equivalent by using the specified culture-specific format information.
		//
		// Parameters:
		//   input:
		//     A string that specifies the time interval to convert.
		//
		//   formatProvider:
		//     An object that supplies culture-specific formatting information.
		//
		// Returns:
		//     A time interval that corresponds to input, as specified by formatProvider.
		//
		// Exceptions:
		//   System.ArgumentNullException:
		//     input is null.
		//
		//   System.FormatException:
		//     input has an invalid format.
		//
		//   System.OverflowException:
		//     input represents a number that is less than System.TimeSpan.MinValue or greater
		//     than System.TimeSpan.MaxValue.-or- At least one of the days, hours, minutes,
		//     or seconds components in input is outside its valid range.
		public static System.TimeSpan Parse(string input, IFormatProvider formatProvider) { return System.TimeSpan.Parse(input, formatProvider); }
		//
		// Summary:
		//     Converts the string representation of a time interval to its System.TimeSpan
		//     equivalent by using the specified format and culture-specific format information.
		//     The format of the string representation must match the specified format exactly.
		//
		// Parameters:
		//   input:
		//     A string that specifies the time interval to convert.
		//
		//   format:
		//     A standard or custom format string that defines the required format of input.
		//
		//   formatProvider:
		//     An object that provides culture-specific formatting information.
		//
		// Returns:
		//     A time interval that corresponds to input, as specified by format and formatProvider.
		//
		// Exceptions:
		//   System.ArgumentNullException:
		//     input is null.
		//
		//   System.FormatException:
		//     input has an invalid format.
		//
		//   System.OverflowException:
		//     input represents a number that is less than System.TimeSpan.MinValue or greater
		//     than System.TimeSpan.MaxValue.-or- At least one of the days, hours, minutes,
		//     or seconds components in input is outside its valid range.
		public static System.TimeSpan ParseExact(string input, string format, IFormatProvider formatProvider) { return System.TimeSpan.ParseExact(input, format, formatProvider); }
		//
		// Summary:
		//     Converts the string representation of a time interval to its System.TimeSpan
		//     equivalent by using the specified array of format strings and culture-specific
		//     format information. The format of the string representation must match one
		//     of the specified formats exactly.
		//
		// Parameters:
		//   input:
		//     A string that specifies the time interval to convert.
		//
		//   formats:
		//     A array of standard or custom format strings that defines the required format
		//     of input.
		//
		//   formatProvider:
		//     An object that provides culture-specific formatting information.
		//
		// Returns:
		//     A time interval that corresponds to input, as specified by formats and formatProvider.
		//
		// Exceptions:
		//   System.ArgumentNullException:
		//     input is null.
		//
		//   System.FormatException:
		//     input has an invalid format.
		//
		//   System.OverflowException:
		//     input represents a number that is less than System.TimeSpan.MinValue or greater
		//     than System.TimeSpan.MaxValue.-or- At least one of the days, hours, minutes,
		//     or seconds components in input is outside its valid range.
		public static System.TimeSpan ParseExact(string input, string[] formats, IFormatProvider formatProvider) { return System.TimeSpan.ParseExact(input, formats, formatProvider); }
		//
		// Summary:
		//     Converts the string representation of a time interval to its System.TimeSpan
		//     equivalent by using the specified format, culture-specific format information,
		//     and styles. The format of the string representation must match the specified
		//     format exactly.
		//
		// Parameters:
		//   input:
		//     A string that specifies the time interval to convert.
		//
		//   format:
		//     A standard or custom format string that defines the required format of input.
		//
		//   formatProvider:
		//     An object that provides culture-specific formatting information.
		//
		//   styles:
		//     A bitwise combination of enumeration values that defines the style elements
		//     that may be present in input.
		//
		// Returns:
		//     A time interval that corresponds to input, as specified by format, formatProvider,
		//     and styles.
		//
		// Exceptions:
		//   System.ArgumentException:
		//     styles is an invalid System.Globalization.TimeSpanStyles value.
		//
		//   System.ArgumentNullException:
		//     input is null.
		//
		//   System.FormatException:
		//     input has an invalid format.
		//
		//   System.OverflowException:
		//     input represents a number that is less than System.TimeSpan.MinValue or greater
		//     than System.TimeSpan.MaxValue.-or- At least one of the days, hours, minutes,
		//     or seconds components in input is outside its valid range.
		public static System.TimeSpan ParseExact(string input, string format, IFormatProvider formatProvider, TimeSpanStyles styles) { return System.TimeSpan.ParseExact(input, format, formatProvider, styles); }
		//
		// Summary:
		//     Converts the string representation of a time interval to its System.TimeSpan
		//     equivalent by using the specified formats, culture-specific format information,
		//     and styles. The format of the string representation must match one of the
		//     specified formats exactly.
		//
		// Parameters:
		//   input:
		//     A string that specifies the time interval to convert.
		//
		//   formats:
		//     A array of standard or custom format strings that define the required format
		//     of input.
		//
		//   formatProvider:
		//     An object that provides culture-specific formatting information.
		//
		//   styles:
		//     A bitwise combination of enumeration values that defines the style elements
		//     that may be present in input.
		//
		// Returns:
		//     A time interval that corresponds to input, as specified by formats, formatProvider,
		//     and styles.
		//
		// Exceptions:
		//   System.ArgumentException:
		//     styles is an invalid System.Globalization.TimeSpanStyles value.
		//
		//   System.ArgumentNullException:
		//     input is null.
		//
		//   System.FormatException:
		//     input has an invalid format.
		//
		//   System.OverflowException:
		//     input represents a number that is less than System.TimeSpan.MinValue or greater
		//     than System.TimeSpan.MaxValue.-or- At least one of the days, hours, minutes,
		//     or seconds components in input is outside its valid range.
		public static System.TimeSpan ParseExact(string input, string[] formats, IFormatProvider formatProvider, TimeSpanStyles styles) { return System.TimeSpan.ParseExact(input, formats, formatProvider, styles); }
		//
		// Summary:
		//     Subtracts the specified System.TimeSpan from this instance.
		//
		// Parameters:
		//   ts:
		//     The time interval to be subtracted.
		//
		// Returns:
		//     A time interval whose value is the result of the value of this instance minus
		//     the value of ts.
		//
		// Exceptions:
		//   System.OverflowException:
		//     The return value is less than System.TimeSpan.MinValue or greater than System.TimeSpan.MaxValue.
		public System.TimeSpan Subtract(System.TimeSpan ts) { return ((System.TimeSpan)this).Subtract(ts); }
		//
		// Summary:
		//     Converts the value of the current System.TimeSpan object to its equivalent
		//     string representation.
		//
		// Returns:
		//     The string representation of the current System.TimeSpan value.
		public override string ToString() { return ((System.TimeSpan)this).ToString(); }
		//
		// Summary:
		//     Converts the value of the current System.TimeSpan object to its equivalent
		//     string representation by using the specified format.
		//
		// Parameters:
		//   format:
		//     A standard or custom System.TimeSpan format string.
		//
		// Returns:
		//     The string representation of the current System.TimeSpan value in the format
		//     specified by the format parameter.
		//
		// Exceptions:
		//   System.FormatException:
		//     The format parameter is not recognized or is not supported.
		public string ToString(string format) { return ((System.TimeSpan)this).ToString(format); }
		//
		// Summary:
		//     Converts the value of the current System.TimeSpan object to its equivalent
		//     string representation by using the specified format and culture-specific
		//     formatting information.
		//
		// Parameters:
		//   format:
		//     A standard or custom System.TimeSpan format string.
		//
		//   formatProvider:
		//     An object that supplies culture-specific formatting information.
		//
		// Returns:
		//     The string representation of the current System.TimeSpan value, as specified
		//     by format and formatProvider.
		//
		// Exceptions:
		//   System.FormatException:
		//     The format parameter is not recognized or is not supported.
		public string ToString(string format, IFormatProvider formatProvider) { return ((System.TimeSpan)this).ToString(format, formatProvider); }
		//
		// Summary:
		//     Converts the string representation of a time interval to its System.TimeSpan
		//     equivalent and returns a value that indicates whether the conversion succeeded.
		//
		// Parameters:
		//   s:
		//     A string that specifies the time interval to convert.
		//
		//   result:
		//     When this method returns, contains an object that represents the time interval
		//     specified by s, or System.TimeSpan.Zero if the conversion failed. This parameter
		//     is passed uninitialized.
		//
		// Returns:
		//     true if s was converted successfully; otherwise, false. This operation returns
		//     false if the s parameter is null or System.String.Empty, has an invalid format,
		//     represents a time interval that is less than System.TimeSpan.MinValue or
		//     greater than System.TimeSpan.MaxValue, or has at least one days, hours, minutes,
		//     or seconds component outside its valid range.
		public static bool TryParse(string s, out TimeSpan result) { System.TimeSpan res; var ok = System.TimeSpan.TryParse(s, out res); result = res; return ok; }
		//
		// Summary:
		//     Converts the string representation of a time interval to its System.TimeSpan
		//     equivalent by using the specified culture-specific formatting information,
		//     and returns a value that indicates whether the conversion succeeded.
		//
		// Parameters:
		//   input:
		//     A string that specifies the time interval to convert.
		//
		//   formatProvider:
		//     An object that supplies culture-specific formatting information.
		//
		//   result:
		//     When this method returns, contains an object that represents the time interval
		//     specified by input, or System.TimeSpan.Zero if the conversion failed. This
		//     parameter is passed uninitialized.
		//
		// Returns:
		//     true if input was converted successfully; otherwise, false. This operation
		//     returns false if the input parameter is null or System.String.Empty, has
		//     an invalid format, represents a time interval that is less than System.TimeSpan.MinValue
		//     or greater than System.TimeSpan.MaxValue, or has at least one days, hours,
		//     minutes, or seconds component outside its valid range.
		public static bool TryParse(string input, IFormatProvider formatProvider, out TimeSpan result) { System.TimeSpan res; var ok = System.TimeSpan.TryParse(input, formatProvider, out res); result = res; return ok; }
		//
		// Summary:
		//     Converts the string representation of a time interval to its System.TimeSpan
		//     equivalent by using the specified format and culture-specific format information,
		//     and returns a value that indicates whether the conversion succeeded. The
		//     format of the string representation must match the specified format exactly.
		//
		// Parameters:
		//   input:
		//     A string that specifies the time interval to convert.
		//
		//   format:
		//     A standard or custom format string that defines the required format of input.
		//
		//   formatProvider:
		//     An object that supplies culture-specific formatting information.
		//
		//   result:
		//     When this method returns, contains an object that represents the time interval
		//     specified by input, or System.TimeSpan.Zero if the conversion failed. This
		//     parameter is passed uninitialized.
		//
		// Returns:
		//     true if input was converted successfully; otherwise, false.
		public static bool TryParseExact(string input, string format, IFormatProvider formatProvider, out TimeSpan result) { System.TimeSpan res; var ok = System.TimeSpan.TryParseExact(input, format, formatProvider, out res); result = res; return ok; }
		//
		// Summary:
		//     Converts the specified string representation of a time interval to its System.TimeSpan
		//     equivalent by using the specified formats and culture-specific format information,
		//     and returns a value that indicates whether the conversion succeeded. The
		//     format of the string representation must match one of the specified formats
		//     exactly.
		//
		// Parameters:
		//   input:
		//     A string that specifies the time interval to convert.
		//
		//   formats:
		//     A array of standard or custom format strings that define the acceptable formats
		//     of input.
		//
		//   formatProvider:
		//     An object that provides culture-specific formatting information.
		//
		//   result:
		//     When this method returns, contains an object that represents the time interval
		//     specified by input, or System.TimeSpan.Zero if the conversion failed. This
		//     parameter is passed uninitialized.
		//
		// Returns:
		//     true if input was converted successfully; otherwise, false.
		public static bool TryParseExact(string input, string[] formats, IFormatProvider formatProvider, out TimeSpan result) { System.TimeSpan res; var ok = System.TimeSpan.TryParseExact(input, formats, formatProvider, out res); result = res; return ok; }
		//
		// Summary:
		//     Converts the string representation of a time interval to its System.TimeSpan
		//     equivalent by using the specified format, culture-specific format information,
		//     and styles, and returns a value that indicates whether the conversion succeeded.
		//     The format of the string representation must match the specified format exactly.
		//
		// Parameters:
		//   input:
		//     A string that specifies the time interval to convert.
		//
		//   format:
		//     A standard or custom format string that defines the required format of input.
		//
		//   formatProvider:
		//     An object that provides culture-specific formatting information.
		//
		//   styles:
		//     One or more enumeration values that indicate the style of input.
		//
		//   result:
		//     When this method returns, contains an object that represents the time interval
		//     specified by input, or System.TimeSpan.Zero if the conversion failed. This
		//     parameter is passed uninitialized.
		//
		// Returns:
		//     true if input was converted successfully; otherwise, false.
		public static bool TryParseExact(string input, string format, IFormatProvider formatProvider, TimeSpanStyles styles, out TimeSpan result) { System.TimeSpan res; var ok = System.TimeSpan.TryParseExact(input, format, formatProvider, styles, out res); result = res; return ok; }
		//
		// Summary:
		//     Converts the specified string representation of a time interval to its System.TimeSpan
		//     equivalent by using the specified formats, culture-specific format information,
		//     and styles, and returns a value that indicates whether the conversion succeeded.
		//     The format of the string representation must match one of the specified formats
		//     exactly.
		//
		// Parameters:
		//   input:
		//     A string that specifies the time interval to convert.
		//
		//   formats:
		//     A array of standard or custom format strings that define the acceptable formats
		//     of input.
		//
		//   formatProvider:
		//     An object that supplies culture-specific formatting information.
		//
		//   styles:
		//     One or more enumeration values that indicate the style of input.
		//
		//   result:
		//     When this method returns, contains an object that represents the time interval
		//     specified by input, or System.TimeSpan.Zero if the conversion failed. This
		//     parameter is passed uninitialized.
		//
		// Returns:
		//     true if input was converted successfully; otherwise, false.
		public static bool TryParseExact(string input, string[] formats, IFormatProvider formatProvider, TimeSpanStyles styles, out TimeSpan result) { System.TimeSpan res; var ok = System.TimeSpan.TryParseExact(input, formats, formatProvider, styles, out res); result = res; return ok; }
	}

	[ComplexType]
	public class Enum<TEnum> where TEnum : struct {

		public static readonly TEnum Default = default(TEnum);

		public Enum() {
			if (!typeof(TEnum).IsEnum)
				throw new ArgumentException("Not an enum");
		}

		//public TEnum Enum { get; set; }

		public int Value { get { return Convert.ToInt32(EnumValue); } set { EnumValue = (TEnum)(object)Value; } }
		[NotMapped]
		public TEnum EnumValue { get; set; }

		public static implicit operator TEnum(Enum<TEnum> w) { return w.EnumValue; }
		public static implicit operator Enum<TEnum>(TEnum e) { return new Enum<TEnum> { EnumValue = e }; }
		public static implicit operator int(Enum<TEnum> w) { return w.Value; }
		public override bool Equals(object obj) { return EnumValue.Equals((TEnum)obj); }
		public override int GetHashCode() { return EnumValue.GetHashCode(); }
		public override string ToString() { return EnumValue.ToString(); }
	}

}