using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Silversite {

	public static class TimeSpanExtensions {

		public static string ToShortTimeString(this TimeSpan t) {
			return string.Format("{0:00}:{1:00}", t.TotalHours, t.Minutes);
		}

		public static string ToLongTimeString(this TimeSpan t) {
			return string.Format("{0:00}:{1:00}:{2:00}", t.TotalHours, t.Minutes, t.Seconds);
		}
	}

}
