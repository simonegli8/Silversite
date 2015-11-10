using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Text;
using System.Globalization;
using System.Diagnostics.Contracts;

[assembly: Silversite.Services.DependsOn(typeof(Silversite.Services.Log))]
[assembly: Silversite.Services.DependsOn(typeof(Silversite.Services.LogProvider))]
[assembly: Silversite.Services.DependsOn(typeof(Silversite.Services.DbLogProvider))]

namespace Silversite.Services {

	/// <summary>
	/// A log entry.
	/// </summary>
	public class LogMessage {

		public LogMessage() { Date = DateTime.Now; Key = -1; }

		/// <summary>
		/// The database key.
		/// </summary>
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Key { get; set; }
		/// <summary>
		/// Date of the message.
		/// </summary>
		[DataType(DataType.DateTime)]
		public DateTime Date { get; set; }

		[MaxLength, Column("Text"), DataType(DataType.Html)]
		public string html { get; set; }
		/// <summary>
		/// The log text.
		/// </summary>
		[NotMapped]
		public string Text {
			get { return HttpUtility.HtmlDecode(Key >= 0 ? html.Replace("{#key}", Key.ToString()) : html); }
			set { html = value.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Select(l => HttpUtility.HtmlEncode(l)).StringList("<br/>" + Environment.NewLine); }
		}
		/// <summary>
		/// The message category.
		/// </summary>
		[MaxLength(128), DataType(DataType.Text)]
		public string Category { get; set; }
		/// <summary>
		/// Used internally to store the Exception property to the database.
		/// </summary>
		/// <value>
		/// The exception data that gets stored to the database.
		/// </value>
		[MaxLength]
		public byte[] ExceptionData { get; set; }
		[MaxLength]
		public byte[] CustomData { get; set; }
		[MaxLength]
		public string SourceText { get; set; }
		[MaxLength(512)]
		public string SourceFile { get; set; }
		public int SourceLine { get; set; }

		Exception ex = null;
		/// <summary>
		/// An optional exception that occured.
		/// </summary>
		[NotMapped]
		public Exception Exception {
			get {
				if (ExceptionData == null) return null;
				if (ex == null) {
					using (var s = new MemoryStream(ExceptionData)) {
						var f = new BinaryFormatter();
						ex = (Exception)f.Deserialize(s);
					}
				}
				return ex;
			}
			set {
				ex = value;
				if (ex == null) ExceptionData = null;
				else {
					string sourceFile, sourceText, sourceMethod;
					int sourceLine;

					ex.Info(out sourceMethod, out sourceFile, out sourceLine, out sourceText);

					SourceFile = sourceFile;
					SourceLine = sourceLine;
					SourceText = sourceText;

					if (ex is System.Runtime.Serialization.ISerializable || ex.GetType().GetAttribute<SerializableAttribute>() != null) {
						try {
							using (var s = new MemoryStream()) {
								var f = new BinaryFormatter();
								f.Serialize(s, ex);
								var buf = new byte[s.Length];
								if (s.Length > int.MaxValue / 100) throw new ArgumentOutOfRangeException("Size of Exception too big.");
								ExceptionData = s.ToArray();
							}
						} catch { }
					}
				}
			}
		}

		object custom = null;
		/// <summary>
		/// An optional exception that occured.
		/// </summary>
		[NotMapped]
		public object Custom {
			get {
				if (CustomData == null) return null;
				if (custom == null) {
					using (var s = new MemoryStream(CustomData)) {
						var f = new BinaryFormatter();
						custom = (Exception)f.Deserialize(s);
					}
				}
				return custom;
			}
			set {
				custom = value;
				if (custom == null) CustomData = null;
				else {
					if (custom is System.Runtime.Serialization.ISerializable || custom.GetType().GetAttribute<SerializableAttribute>() != null) {
						try {
							using (var s = new MemoryStream()) {
								var f = new BinaryFormatter();
								f.Serialize(s, custom);
								if (s.Length > int.MaxValue / 100) throw new ArgumentOutOfRangeException("Size of Custom too big.");
								CustomData = s.ToArray();
							}
						} catch { }
					}
				}
			}
		}

		/// <summary>
		/// Returns Text in html form, including a link to an error page if Exception contains an exception.
		/// </summary>
		[NotMapped]
		public string Html {
			get { return Key >= 0 ? html.Replace("[#key]", Key.ToString()) : html; }
			set { html = value; }
		}
	}

	[Configuration.Section(Path = LogConfiguration.ConfigPath)]
	public class LogConfiguration : Configuration.Section {

		public const string ConfigPath = ConfigRoot + "/Silversite.config";

		[System.Configuration.ConfigurationProperty("debug", IsRequired = false, DefaultValue = true)]
		public bool Debug { get { return (bool)(this["debug"] ?? true); } set { this["debug"] = value; } }

		[System.Configuration.ConfigurationProperty("enabled", IsRequired = false, DefaultValue = true)]
		public bool Enabled { get { return (bool)(this["enabled"] ?? true); } set { this["enabled"] = value; } }

		[System.Configuration.ConfigurationProperty("severeExceptions", IsRequired = false, DefaultValue = "")]
		public string SevereExceptions { get { return (string)(this["severeExceptions"] ?? ""); } set { this["severeExceptions"] = value; } }

		[System.Configuration.ConfigurationProperty("daysToArchive", IsRequired = false, DefaultValue = 180)]
		public int DaysToArchive { get { return (int)(this["daysToArchive"] ?? 180); } set { this["daysToArchive"] = value; } }

	}

	/// <summary>
	/// A severe exception. If you add this exception as an inner exception to an Exception, the Exception is treated as severe.
	/// </summary>
	[Serializable]
	public class SevereException : Exception {
		/// <summary>
		/// Creates a severe exception.
		/// </summary>
		public SevereException() : base() { }
		/// <summary>
		/// Creates a severe exception.
		/// </summary>
		/// <param name="msg">A message.</param>
		public SevereException(string msg) : base(msg) { }
		/// <summary>
		/// Creates a severe exception.
		/// </summary>
		/// <param name="msg">A message.</param>
		/// <param name="innerException">An inner exception.</param>
		public SevereException(string msg, Exception innerException) : base(msg, innerException) { }
	}

	[Serializable]
	public class FatalException : SevereException {
		/// <summary>
		/// Creates a severe exception.
		/// </summary>
		public FatalException() : base() { }
		/// <summary>
		/// Creates a severe exception.
		/// </summary>
		/// <param name="msg">A message.</param>
		public FatalException(string msg) : base(msg) { }
		/// <summary>
		/// Creates a severe exception.
		/// </summary>
		/// <param name="msg">A message.</param>
		/// <param name="innerException">An inner exception.</param>
		public FatalException(string msg, Exception innerException) : base(msg, innerException) { }
	}

	public class LogEventArgs : EventArgs {
		public LogMessage Message { get; set; }
	}

	/// <summary>
	/// A service that implement logging facilities.
	/// </summary>
	public class Log: StaticService<Log, LogProvider>, IAutostart, IEnumerable<LogMessage> {

		public static LogConfiguration Configuration;
		static readonly TimeSpan FlushInterval;
		static ConcurrentQueue<LogMessage> queue;
		static bool Initialized { get { return Timer != null; } }
		static HashSet<Type> severe;
		public static bool Enabled { get; set; }
		public static event EventHandler<LogEventArgs> Watch;

		static Log() {
			severe = new HashSet<Type>();
			TimerLock = new object();
			queue = new ConcurrentQueue<LogMessage>();
			FlushInterval = 2.Minutes();
			Interval = FlushInterval;
			initException = null;
			Severe<SevereException>();
			Configuration = null;
			Enabled = true;
		}


		[ThreadStatic]
		static int critical;
		public static void Severe<T>() where T : Exception { lock (severe) severe.Add(typeof(T)); }

		public static void Critical(Action a) { critical++; a(); critical--; }
		public static void Critical(Action a, Action<Exception> acatch) { try { critical++; a(); } catch (Exception ex) { acatch(ex); } finally { critical--; } }

		public static WeakValueDictionary<object, Exception> Exceptions = new WeakValueDictionary<object, Exception>();
		static int ExceptionKey = 0;

		/// <summary>
		/// Flush the log cache to the database.
		/// </summary>
		/// <param name="minqueue">The minimal size of the cache in order  to start flushing.</param>
		public static void Flush(int minqueue) {
			if (Initialized && queue.Count > minqueue) {

				if (HasProvider && Provider.IsReady) {
					var entry = queue.Dequeue();
					while (entry != null) {
						Provider.Message(entry);
						entry = queue.Dequeue();
					}
				}
			} else while (queue.Count > 1000) queue.Dequeue();
		}

		/// <summary>
		/// Flushes the log cache to the database.
		/// </summary>
		public static void Flush() { Flush(0); }

		/// <summary>
		/// Flushes the log cache or blocks if the cache is empty.
		/// </summary>
		public static void FlushOrBlock() {

			while (!Initialized) Thread.Sleep(100);

			var blocked = new EventHandler((sender, args) => Thread.Sleep(3000));
			if (HasProvider && Provider.IsReady) {
				queue.Blocked += blocked;
				var entry = queue.DequeueOrBlock();
				while (entry != null) {
					Provider.Message(entry);
					entry = queue.DequeueOrBlock();
				}
				queue.Blocked -= blocked;
			}
		}
		/// <summary>
		/// Writes a message to the log.
		/// </summary>
		/// <param name="category">The message's category</param>
		/// <param name="msg">The message text.</param>
		/// <param name="e">An optional Exception</param>
		/// <param name="args">Arguments that are passed to String.Format for the message text.</param>
		public static void Write(string category, string msg, Exception e, params object[] args) {

			if (msg == null) msg = "";
			if (args != null) msg = string.Format(msg, args);

#if DEBUG
			if (!category.StartsWith("Debug.")) category = "Debug." + category;
			if (Configuration == null || Configuration.Debug) Debug.Message(e == null ? 0 : 4, category, "(" + category + "): " + msg);
#else
			if  (Configuration != null && !Configuration.Debug && category.StartsWith("Debug")) return;
#endif
			if (!Enabled) return;
			var message = new LogMessage { Date = DateTime.Now, Text = msg ?? "", Category = category ?? "", Exception = e };
			if (Watch != null) Watch(null, new LogEventArgs { Message = message });
			queue.Enqueue(message);
		}
		/// <summary>
		/// Writes a message to the log.
		/// </summary>
		/// <param name="category">The message's cetgory.</param>
		/// <param name="msg">The message text.</param>
		/// <param name="args">Arguments that are passed to String.Format for the message text.</param>
		public static void Write(string category, string msg, params object[] args) { Write(category, msg, null, args); }
		/// <summary>
		/// Writes a message to the log.
		/// </summary>
		/// <param name="category">The message's cetgory.</param>
		/// <param name="msg">The message text.</param>
		public static void Write(string category, string msg) { Write(category, msg, null, null); }
		/// <summary>Writes a custom object to the log.</summary>
		public static void Write(object custom) { queue.Enqueue(new LogMessage { Custom = custom }); }
		/// <summary>
		/// The class of an error. Pass one of those values as an optional argument to Log.Error.
		/// </summary>
		public enum ErrorClass {
			/// <summary>
			/// A normal error. Such errors are logged under the category "Errors".
			/// </summary>
			Normal,
			/// <summary>
			/// A severe error. Causes a call to Debug.Break() and is logged under the category "Severe Errors".
			/// </summary>
			Severe,
			/// <summary>
			/// A fatal error. Causes a call to Debug.Break(), is logged under the category "Severe Errors"and causes notification of the website admin via email.
			/// </summary>
			Fatal
		};

		/// <summary>
		/// Writes an error message to the log.
		/// </summary>
		/// <param name="msg">The message text.</param>
		/// <param name="e">An optional Exception.</param>
		/// <param name="args">Arguments that are passed to String.Format for the message text.</param>
		public static void Error(string msg, Exception e, params object[] args) {
			if (e != null) {
				string page = string.Empty;
				try {
					if (HttpContext.Current != null) page = "Page: " + HttpUtility.HtmlEncode(HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath) + "<br/>";
				} catch { }
				if (args != null) msg = string.Format(msg, args);
				var link = "<a href=\"" + Services.Paths.Url("~/Silversite/Admin/exception.aspx?log=[#key]") + "\">Details... <img alt=\"\" title=\"Details\" style=\"vertical-align:middle\" src=\"" + Services.Paths.Url("~/Silversite/Images/lupe.gif") + "\" /></a>";
				msg = "Exception: " + HttpUtility.HtmlEncode(msg) +  link + "<br/>" + HttpUtility.HtmlEncode(page + e.GetType().FullName) + ": " + HttpUtility.HtmlEncode(e.Message) + "<div class='collapsed'>" +
					HttpUtility.HtmlEncode(e.StackTrace);
				if (e is System.Data.Entity.Validation.DbEntityValidationException) {
					var ve = (System.Data.Entity.Validation.DbEntityValidationException)e;
					foreach (var verr in ve.EntityValidationErrors.SelectMany(err => err.ValidationErrors)) {
						msg += "<br/>" + verr.ErrorMessage;
					}
				}
				msg +="</div>";
				var sev = critical > 0 || (args != null && args.Contains(ErrorClass.Severe));
				var fatal = sev && (e is FatalException || args.Contains(ErrorClass.Fatal));
				lock (severe) {
					if (!sev) {
						var ex = e;
						while (ex != null) {
							if (severe.Contains(ex.GetType()) || severe.Any(s => s.IsInstanceOfType(ex))) {
								sev = true;
								break;
							}
							ex = ex.InnerException;
						}
					}
				}
				sev &= args == null || !args.Contains(ErrorClass.Normal);
				if (sev) {
					Write("Severe Errors", msg, e);
					if (fatal) {
						int exkey;
						lock (Exceptions) exkey = ExceptionKey++;
						var mail = new Mail()
							.Template("~/Silversite/Extensions/Silversite.Core/Emails/FatalException.aspx?key=" + exkey)
							.SendTo(Mail.Configuration.Admin);
					}
					Debug.Break();
				} else Write("Errors", msg, e);
				//if (msg != "InnerException") SendAppError(e);
				if (e.InnerException != null && e.InnerException != e) Error("InnerException:", e.InnerException);
			} else Write("Errors", msg, args);
		}
		/// <summary>
		/// Writes an error message to the log.
		/// </summary>
		/// <param name="msg">The message text.</param>
		/// <param name="args">Arguments that are passed to String.Format for the message text.</param>
		public static void Error(string msg, params object[] args) { Error(msg, null, args); }
		/// <summary>
		/// Writes an error message to the log.
		/// </summary>
		/// <param name="msg">The message text.</param>
		/// <param name="e">An optional Exception.</param>
		public static void Error(string msg, Exception ex) { Error(msg, ex, null); }
		/// <summary>
		/// Writes an error message to the log.
		/// </summary>
		/// <param name="msg">The message text.</param>
		public static void Error(string msg) { Error(msg, null, null); }
		/* 
				const string ErrorMailTemplate = "~/emailtemplates/errormail.html";

				static void DoSendAppError(object state) {
					Exception e = state as Exception;
					if (Server.IsTestServer || e is ViewStateException) return;
					try {
						TimeSpan dt = DateTime.Now - startup;
						string page = "?";
						if (HttpContext.Current != null) page = HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath;

						string msg = File.ReadAllText(Application.MapPath(ErrorMailTemplate));
						msg = msg.Replace("[HOME]", Application.Home)
							.Replace("[DAYS]", dt.TotalDays.ToString("F0"))
							.Replace("[TIME]", dt.Hours.ToString("00") + ":" + dt.Minutes.ToString("00") + ":" + dt.Seconds.ToString("00"))
							.Replace("[PAGE]", page);

						if (e != null) {
							msg = msg.Replace("[EXCEPTION]", HttpUtility.HtmlEncode(e.Message))
								.Replace("[STACK]", HttpUtility.HtmlEncode(e.StackTrace));
							if (e.InnerException != null) {
								msg = msg.Replace("[IEXCEPTION]", HttpUtility.HtmlEncode(e.InnerException.Message))
								.Replace("[ISTACK]", HttpUtility.HtmlEncode(e.InnerException.StackTrace));
							} else {
								msg = msg.Replace("[IEXCEPTION]", "-")
									.Replace("[ISTACK]", "-");
							}
						} else {
							msg = msg.Replace("[EXCEPTION]", "-")
								.Replace("[STACK]", "-");
						}

						Mail mail = new Mail(e.Message, msg, Mail.Configuration.AdminAddress);
						mail.Send();
					} catch { }
				}

				public static void SendAppError(Exception e) {
					ThreadPool.QueueUserWorkItem(new WaitCallback(DoSendAppError), e);
				}
				*/
		/// <summary>Returns a specific log entry.</summary>
		public static LogMessage This(long key) { return Provider.This(key); return null; }
		/// <summary>Returns an IEnumerable with all log entries sorted after Date in descending order.</summary>
		public static IEnumerable<LogMessage> All() { return Provider.All(); }
		/// <summary>Clear entries older than Configuration.DaysToArchive.</summary>
		public static void Cleanup() { Provider.Cleanup(); }
		/// <summary>Clears the entire log.</summary>
		public static void Clear() { Provider.Clear(); }
		/// <summary>Clears an entire log category.</summary>
		/// <param name="category">The log category.</param>
		public static void Clear(string category) { Provider.Clear(category); }

		static TimeSpan Interval;
		static Timer Timer;
		static object TimerLock;

		static Exception initException;

		/// <summary>Initializes a timer that periodically flushes the log to the log provider.</summary>
		public void Startup() {

			try {
				Configuration = new LogConfiguration();
				foreach (var s in Configuration.SevereExceptions.SplitList(',', ';')) {
					var t = Type.GetType(s);
					if (t != null) {
						lock (severe) severe.Add(t);
					}
				}
			} catch (Exception ex) {
				initException = ex;
			}

			Contract.Assert(!Initialized);
			if (initException != null) {
				Error("Error reading Log configuration.", initException, ErrorClass.Severe);
				initException = null;
			}
			if (!Initialized) {
				lock (TimerLock) {
					Timer = Tasks.Recurring(Interval, Interval, () => {
						if (Monitor.TryEnter(TimerLock, 100)) {
							try {
								Flush(100);
							} finally {
								Monitor.Exit(TimerLock);
							}
						}
					});
				}
			}
		}

		/// <summary>Disposes and cleans up the log. </summary>
		public void Shutdown() {
			if (Timer != null) {
				Debug.Message("Log Shutdown");
				lock (TimerLock) Flush();
				Timer.Dispose();
				Timer = null;
				Cleanup();
			}
			base.Dispose();
		}

		public IEnumerator<LogMessage> GetEnumerator() { return All().GetEnumerator(); }
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return ((System.Collections.IEnumerable)All()).GetEnumerator(); }
	}

	/// <summary>Base class for a custom log provider and also the standard log provider that writes to logfiles located in ~/Silversite/Log.</summary>
	[DefaultProvider]
	public class LogProvider : Provider<Log> {

		public const string LogRoot = Paths.Log;

		const long keyshift = 0x100000000;

		void Save(StreamWriter w, LogMessage m) {
			m.Key = w.BaseStream.Length + (m.Date - DateTime.MinValue).Days * keyshift;
			w.Write("<p style='font:Verdana, Helvetica, Sans-serif 9pt;><b>### Date:</b>"); w.WriteLine(m.Date.ToString("s"));
			w.Write("<br/><b>Category:</b>"); w.WriteLine(HttpUtility.HtmlEncode(m.Category));
			w.Write("<br/><b>Text:</b><br/>"); w.WriteLine(HttpUtility.HtmlEncode(m.Text).Replace(Environment.NewLine, "<br/>"));
			w.Write("<span style='display:none;'>Key:"); w.WriteLine(m.Key.ToString("X")); w.WriteLine("</span><br/>");
			if (!string.IsNullOrEmpty(m.SourceFile)) {
				w.Write("SourceFile: "); w.WriteLine(HttpUtility.HtmlEncode(m.SourceFile));
				w.Write("SourceLine: "); w.WriteLine(m.SourceLine.ToString());
			}
			if (!string.IsNullOrEmpty(m.SourceText)) { w.WriteLine("SourceText: "); w.WriteLine(HttpUtility.HtmlEncode(m.SourceText)); }
			if (m.Exception != null) {
				var link = "<a href=\"" + Services.Paths.Url("~/Silversite/Admin/exception.aspx?log={0}", m.Key) + "\">Details... <img alt=\"\" title=\"Details\" style=\"vertical-align:middle\" src=\"" + Services.Paths.Url("~/Silversite/Images/lupe.gif") + "\" /></a>";
				w.Write("Exception: "); w.WriteLine(link);
				w.WriteLine("StackTrace:");
				w.WriteLine(m.Exception.StackTrace);
				w.WriteLine("ExceptionData:");
				w.WriteLine(Convert.ToBase64String(m.ExceptionData));
				w.WriteLine("</p>");
			}
			if (m.Custom != null) {
				w.WriteLine("CustomData:");
				w.WriteLine(Convert.ToBase64String(m.CustomData));
			}
		}

		LogMessage Load(StreamReader r) {
			var m = new LogMessage();
			if (r.EndOfStream) return null;
			var line = r.ReadLine();
			m.Exception = null;
			m.Custom = null;
			while (!line.Contains("</p>")) {
				var token = line.UpTo(':');
				switch (token) {
					case "<p style='font:Verdana, Helvetica, Sans-serif 9pt;>### Date": m.Date = DateTime.Parse(line.FromOn(':')); break;
					case "Category": m.Category = HttpUtility.HtmlDecode(line.FromOn(": ")); break;
					case "Text": m.Text = HttpUtility.HtmlDecode(line.FromOn(": ")); break;
					case "Key": m.Key = long.Parse(line.FromOn(": ")); break;
					case "SourceFile": m.SourceFile = HttpUtility.HtmlDecode(line.FromOn(": ")); break;
					case "SourceLine": int linenr = 0;
						if (int.TryParse(HttpUtility.HtmlDecode(line.FromOn(": ")), out linenr)) m.SourceLine = linenr;
						break;
					case "SourceText": m.SourceText = HttpUtility.HtmlDecode(line.FromOn(": ")); break;
					case "ExceptionData":
						var exdata = new StringBuilder();
						line = r.ReadLine();
						while (!(line.Contains("</p>") || line.Contains(':'))) exdata.AppendLine(line);
						m.Exception = null;
						m.ExceptionData = Convert.FromBase64String(exdata.ToString());
						break;
					case "CustomData":
						var cdata = new StringBuilder();
						line = r.ReadLine();
						while (!(line.Contains("</p>") || line.Contains(':'))) cdata.AppendLine(line);
						m.Custom = null;
						m.CustomData = Convert.FromBase64String(cdata.ToString());
						break;
					default: break;
				}
				if (line.Contains("</p>") || r.EndOfStream) return m;
				line = r.ReadLine();
			}
			return m;
		}

		string LogFile(DateTime date) { return Paths.Combine(LogRoot, "Log." + date.ToString("yyyy-MM-dd") + ".html"); }
		string LogFile(LogMessage m) { return LogFile(m.Date); }

		void Save(LogMessage m) {
			try {
				using (var w = new StreamWriter(Files.Write(LogFile(m)), Encoding.UTF8, 4096)) Save(w, m);
			} catch (Exception ex) {
				if (!Files.DirectoryExists(LogRoot)) {
					Files.CreateDirectory(LogRoot);
					using (var w = new StreamWriter(Files.Write(LogFile(m)), Encoding.UTF8, 4096)) Save(w, m);
				}
			}
		}

		IEnumerable<LogMessage> Load(string logfile) {
			using (var r = new StreamReader(Files.Read(logfile), Encoding.UTF8, true)) {
				while (!r.EndOfStream) yield return Load(r);
			}
		}
		/// <summary>Returns a specific log entry.</summary>
		public virtual LogMessage This(long key) {
			Log.Flush();
			var Date = DateTime.MinValue.AddDays(key / keyshift);
			var entry = Load(LogFile(Date)).FirstOrDefault(log => log.Key >= key);
			if (entry == null || entry.Key > key) return null;
			return entry;
		}
		/// <summary>Returns all log entries ordered after Date in descending order.</summary>
		public virtual IEnumerable<LogMessage> All() {
			Log.Flush();
			foreach (var file in Files.All(LogRoot + "/*").OrderByDescending(f => f)) {
				var entries = Load(file).ToList();
				entries.Reverse();
				foreach (var entry in entries) yield return entry;
			}
		}
		/// <summary>Clears the entire log.</summary>
		public virtual void Clear() { Log.Flush(); Files.Delete(LogRoot + "/*"); }
		/// <summary>Clears a log category.</summary>
		/// <param name="category">The log category.</param>
		public virtual void Clear(string category) {
			Log.Flush();
			foreach (var file in Files.All(LogRoot + "/*").OrderByDescending(f => f)) {
				var logs = Load(file).Where(log => log.Category != category).ToList();
				using (var w = new StreamWriter(file, false, Encoding.UTF8)) {
					logs.Each(log => Save(w, log));
				}
			}
		}
		/// <summary>Deletes old entries. (Can be configured in the Configuration with the DaysToArchive Attribute).</summary>
		public virtual void Cleanup() {
			string lastfile = LogFile(DateTime.Now - Log.Configuration.DaysToArchive.Days());
			var files = Files.All(LogRoot + "/*")
				.Where<string>(file => lastfile.CompareTo(file) > 0);
			Files.Delete(files);
		}
		/// <summary>Writes a message to the custom log.</summary>
		/// <param name="msg">The message to log.</param>
		public virtual void Message(LogMessage msg) { Save(msg); }
		/// <summary>Returns false, when the provider is not yet ready, like during initialization of the database.</summary>
		public virtual bool IsReady { get { return true; } }
	}

	/// <summary>Base class for a custom log provider and also the standard log provider that writes to logfiles located in ~/Silversite/Log.</summary>
	public class DbLogProvider : LogProvider {

		/// <summary>Returns a specific log entry.</summary>
		public override LogMessage This(long key) {
			Log.Flush();
			using (var db = new Context()) {
				return db.LogMessages.FirstOrDefault(m => m.Key == key);
			}
		}
		/// <summary>Returns all log entries ordered after Date in descending order.</summary>
		public override IEnumerable<LogMessage> All() {
			Log.Flush();
			using (var db = new Context()) {
				return db.LogMessages;
			}
		}
		/// <summary>Clears the entire log.</summary>
		public override void Clear() {
			Log.Flush();
			using (var db = new Context()) {
				db.LogMessages.RemoveAll();
				db.SaveChanges();
			}
		}
		/// <summary>Clears a log category.</summary>
		/// <param name="category">The log category.</param>
		public override void Clear(string category) {
			Log.Flush();
			using (var db = new Context()) {
				db.LogMessages.Remove(m => m.Category == category);
				db.SaveChanges();
			}
		}
		/// <summary>Deletes old entries. (Can be configured in the Configuration with the DaysToArchive Attribute).</summary>
		public override void Cleanup() {
			var old = DateTime.Now - Log.Configuration.DaysToArchive.Days();
			using (var db = new Context()) {
				db.LogMessages.Remove(m => m.Date < old);
				db.SaveChanges();
			}
		}
		/// <summary>Writes a message to the custom log.</summary>
		/// <param name="msg">The message to log.</param>
		public override void Message(LogMessage msg) {
			using (var db = new Context()) {
				db.LogMessages.Add(msg);
				db.SaveChanges();
			}
		}
		/// <summary>Returns false, when the provider is not yet ready, like during initialization of the database.</summary>
		public override bool IsReady { get { return true; } }
	}
}
