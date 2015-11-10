using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.ComponentModel.DataAnnotations;
using Silversite.Services;

[assembly: Silversite.Services.DependsOn(typeof(Silversite.Services.Uptime))]

namespace Silversite.Services {

	[Serializable]
	public class UptimeRecord {

		public UptimeRecord() { Time = Data.DateTime.Now; Process = System.Diagnostics.Process.GetCurrentProcess().Id; AppDomain = System.AppDomain.CurrentDomain.Id; Test = Uptime.IsTest; }

		[Key]
		public int Key { get; set; }
		public DateTime Time { get; set; }
		public bool Running { get; set; }
		public bool Test { get; set; }
		public int Process { get; set; }
		public int AppDomain { get; set; }
	}

	public class Uptime:  IAutostart {

		internal static bool IsTest = false;
		static bool running = false;

		public static DateTime Startup;
		static Timer timer;

		void IAutostart.Startup() {
			running = true;
			Log.Write(new UptimeRecord { Time = DateTime.Now, Running = false });
		}

		void IAutostart.Shutdown() {
			running = false;
			Log.Write(new UptimeRecord { Time = DateTime.Now, Running = false });
			Cleanup();
		}

		public static List<UptimeRecord> All() {
			return Log.All()
				.Where(log => log.Custom != null && log.Custom is UptimeRecord)
				.Select(log => (UptimeRecord)log.Custom)
				.Where(t => t.Test == IsTest)
				.ToList();
		}

		public static List<UptimeRecord> Correct() {
			var times = All();

			for (int i = 0; i < times.Count-1; ) {
				if (times[i].Running && times[i+1].Running) times.RemoveAt(i+1);
				else if (!times[i].Running && !times[i+1].Running) times.RemoveAt(i);
				else i++;
			}
			return times;
		}

		public static void Cleanup() {
			using (var db = new Context()) {
				DateTime lastMonth = DateTime.Now.AddMonths(-1);
				db.Uptime.Remove(t => t.Time < lastMonth);
				db.SaveChanges();
			}
		}


		public static double Total {
			get {
				Cleanup();
				var times = Correct();

				TimeSpan up = new TimeSpan(0), down = new TimeSpan(0);
				var prev = new UptimeRecord { Time = DateTime.Now, Running = false };
				foreach (var t in times) {
					if (prev.Running != t.Running) {
						var dt = prev.Time - t.Time;
						if (t.Running) up += dt;
						else down += dt;
					}
					prev = t;
				}
				return (double)up.Ticks / (double)(up.Ticks + down.Ticks);
			}
		}

		public static TimeSpan AverageUptime {
			get {
				Cleanup();
				var times = Correct();

				TimeSpan up = new TimeSpan(0), down = new TimeSpan(0);
				int n = 0;
				var prev = new UptimeRecord { Time = DateTime.Now, Running = false };
				foreach (var t in times) {
					if (prev.Running != t.Running) {
						TimeSpan dt = prev.Time - t.Time;
						if (t.Running) {
							up += dt;
							n++;
						} else down += dt;
					}
					prev = t;
				}
				if (n == 0) return new TimeSpan(0);
				else return new TimeSpan(up.Ticks / n);
			}
		}

		public static double AverageMissedShutdowns {
			get {
				Cleanup();
				var times = All();

				int n = 0, missed = 0;
				var prev = new UptimeRecord { Time = DateTime.Now, Running = false };
				foreach (var t in times) {
					if (t.Running) n++;
					if (prev.Running == t.Running) {
						if (t.Running) missed++;
					}
					prev = t;
				}
				if (n == 0) return 0;
				else return (double)missed / (double)n;
			}
		}

		public static int Errors {
			get {
				Cleanup();
				var times = All();

				int n = 0;
				var prev = new UptimeRecord { Time = DateTime.Now, Running = false };
				foreach (var t in times) {
					if (prev.Running == t.Running) n++;
					prev = t;
				}
				return n;
			}
		}

		public static void Clear() {
			var times = All();
			using (var db = new Context()) {
				db.Uptime.Remove(times);
				db.SaveChanges();
			}
		}
	}
}
