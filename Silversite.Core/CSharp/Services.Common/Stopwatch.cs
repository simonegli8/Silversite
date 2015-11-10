using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Configuration;
using System.Diagnostics;
using System.Web.UI.WebControls;
using System.Drawing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Silversite;

[assembly: Silversite.Services.DependsOn(typeof(Silversite.Services.Stopwatch))]

namespace Silversite.Services {
	
	public class StopwatchRecord {

		public StopwatchRecord() { Date = Data.DateTime.Never; Time = new Data.TimeSpan(); Start = Data.DateTime.Never; Max = TimeSpan.MaxValue; Min = TimeSpan.MinValue; }

		[Key]
		public int Id { get; set; }
		[MaxLength(512)]
		public string Page { get; set; }
		[MaxLength(128)]
		public string Name { get; set; }
		public DateTime Date { get; set; }
		public double MeanTicks { get; set; }
		public int N { get; set; }
		public bool Test { get; set; }
		public Data.TimeSpan Time { get; set; }
		public Data.TimeSpan Max { get; set; }
		public Data.TimeSpan Min { get; set; }
		public DateTime Start { get; set; }
		public StopwatchRecord ParentPage { get; set; }

		[NotMapped]
		public string Key { get { return IsPage ? Page : Page + ": " + Name; } }
		public bool IsPage { get { return string.IsNullOrEmpty(Name); } }
		public string TimeString {
			get {
				return IsPage || ParentPage == null ?
						string.Format("{0:F1} s (max {1:F1} s, min {2:F1} s)", new TimeSpan((long)MeanTicks).TotalSeconds, ((TimeSpan)Max).TotalSeconds, ((TimeSpan)Min).TotalSeconds) :
						string.Format("{0:F0} %", MeanTicks / ParentPage.MeanTicks * 100);
			}
		}
		public System.Drawing.Color Color { get { return IsPage ? Color.Red : (ParentPage == this ? Color.Green : Color.Black); } }
	}

	public class Stopwatch: IAutostart {

		public static readonly DateTime Never = Data.DateTime.Never;

		public class StopwatchConfiguration: ConfigurationSection {
			[ConfigurationProperty("Enabled", IsRequired=false, DefaultValue=true)]
			public bool Enabled { get { return (bool)this["Enabled"]; } set { this["Enabled"] = value; } }
		}

		public class Times: KeyedCollection<string, StopwatchRecord> {
			public Times() : base() { }
			public Times(IEnumerable<StopwatchRecord> set) : base() { foreach (var e in set) Add(e); }

			protected override string GetKeyForItem(StopwatchRecord item) { return item.Key; }

			protected override void InsertItem(int index, StopwatchRecord e) { base.InsertItem(index, e); e.ParentPage = this[e.Page, string.Empty]; }
			protected override void SetItem(int index, StopwatchRecord e) { base.SetItem(index, e); e.ParentPage = this[e.Page, string.Empty]; }

			public StopwatchRecord this[string page, string name] {
				get {
					var e = new StopwatchRecord();
					e.Page = page;
					e.Name = name;
					if (Contains(e.Key)) return this[e.Key];
					else {
						e.Start = Never;
						e.Time = new TimeSpan(0);
						e.N = 0;
						e.Date = DateTime.Now;
						e.Test = Services.TestServer.IsTestServer;

						Add(e);
						return e;
					}
				}
			}
		}

		static Times times = new Times();
		HttpApplicationState state;

		public static StopwatchConfiguration Configuration = new StopwatchConfiguration();
		public static bool Enabled = Configuration.Enabled;

		static string CurrentPage {
			get {
				string page = string.Empty;
				if (HttpContext.Current != null) {
					try {
						page = HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath;
					} catch (Exception) { }
				}
				return page;
			}
		}

		public static void Start(string name) {
			if (Enabled) {
				var now = DateTime.Now;
				var t = times[CurrentPage, name];
				lock (t) t.Start = now;
			}
		}

		public static void Stop(string name) {
			if (Enabled) {
				var now = DateTime.Now;
				var t = times[CurrentPage, name];
				lock (t) {
					if (t.Start == Never) return;
					t.Time += now - t.Start;
					if ((TimeSpan)t.Time > (TimeSpan)t.Max) t.Max = t.Time;
					if ((TimeSpan)t.Time < (TimeSpan)t.Min) t.Min = t.Time;
					t.Start = default(DateTime);
				}
			}
		}

		public static void Save() {
			foreach (var t in times) {
				lock (t) {
					if (t.Time > new TimeSpan(0)) {
						double n = t.N, f2 = 1/(n+1), f1 = n*f2;
						t.MeanTicks = t.MeanTicks*f1 + t.Time.Ticks*f2;
						t.Time = new TimeSpan(0);
					}
				}
			}
		}

		void BeginRequest(object sender, EventArgs e) {
			try {
				if (CurrentPage.EndsWith(".aspx") || CurrentPage.EndsWith(".ashx") || CurrentPage.EndsWith(".asmx")) Start(string.Empty);
			} catch (Exception) {
			}
		}

		void EndRequest(object sender, EventArgs e) {
			try {
				if (CurrentPage.EndsWith(".aspx") || CurrentPage.EndsWith(".ashx") || CurrentPage.EndsWith(".asmx")) Stop(string.Empty);
				Save();
			} catch (Exception) {
			}
		}

		HttpApplication app;

		public void Init(HttpApplication app) {
			if (Enabled) {
				this.app = app;
				state = app.Application;
				app.BeginRequest += BeginRequest;
				app.EndRequest += EndRequest;
			}
		}

		// private static List<TimerResult> results = null;

		public static void Flush(int treshold) {
			if (times.Count > treshold) {
				using (var db = new Context()) {
					db.Stopwatch.AddRange<StopwatchRecord>(times);
					times.Clear();
					var lastMonth = DateTime.Now.AddMonths(-1);
					db.Stopwatch.Remove(t => t.Date < lastMonth);
				}
			}
		}

		public static void Flush() { Flush(100); }

		public void Dispose() { Flush(); }

		public static void Clear() {
			using (var db = new Context()) {
				times.Clear();
				var test = Services.TestServer.IsTestServer;
				db.Stopwatch.Remove(t => t.Test == test);
			}
		}

		public void Startup() { }
		public void Shutdown() { Flush(0); }
	}
}
