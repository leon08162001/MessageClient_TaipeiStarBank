using System;
using Foundation;

namespace Util
{
	public static class NSDateCalculateDay
	{
		public static nint daysOfMonth(this NSDate date)
		{
			NSCalendar calendar = new NSCalendar(NSCalendarType.Gregorian);
			NSRange range = calendar.Range(NSCalendarUnit.Day, NSCalendarUnit.Month, date);
			return range.Length;
		}
	}
}

