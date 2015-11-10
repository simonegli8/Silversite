using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Net;
using System.Net.Mail;
using System.IO;
using System.Globalization;
using System.Reflection;
using System.Web.Configuration;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace Silversite.Services {

	//TODO implement background sending of mails, in a sepearate thread & buffering mails in the database, to support mail limit per hour properly.

	public class MailAddress : System.Net.Mail.MailAddress {
		public MailAddress(string email) : base(email) { }
		public MailAddress(string email, string name) : base(email, name) { }
		public MailAddress(string email, string name, Encoding encoding) : base(email, name, encoding) { }
		static readonly Regex mailAddress = new Regex("([.]*)<([.]*)>", RegexOptions.Compiled);
		public static implicit operator MailAddress(string email) {
			var name = "";
			var match = mailAddress.Match(email);
			if (match.Success) {
				name = match.Groups[1].Value;
				email = match.Groups[2].Value;
			}
			return new MailAddress(email, name, Encoding.UTF8);
		}
		public MailAddress(System.Net.Mail.MailAddress adr) : this(adr.Address, adr.DisplayName, Encoding.UTF8) { }
	}

	public class MailAddressCollection : System.Net.Mail.MailAddressCollection {
		public MailAddressCollection() : base() { }
		public MailAddressCollection(System.Net.Mail.MailAddressCollection nm) : base() { nm.ForEach(adr => Add(adr)); Base = nm; }

		System.Net.Mail.MailAddressCollection Base = null;

		protected override void ClearItems() {
			base.ClearItems();
			Base.Clear();
		}
		protected override void InsertItem(int index, System.Net.Mail.MailAddress item) {
			base.InsertItem(index, item);
			if (Base != null) { Base.Clear(); Base.AddRange(this); }
		}
		protected override void RemoveItem(int index) {
			base.RemoveItem(index);
			if (Base != null) { Base.Clear(); Base.AddRange(this); }
		}
		protected override void SetItem(int index, System.Net.Mail.MailAddress item) {
			base.SetItem(index, item);
			if (Base != null) { Base.Clear(); Base.AddRange(this); }
		}

		public static implicit operator MailAddressCollection(string emails) {
			var m = new MailAddressCollection();
			emails.Tokens().AddTo(m, em => (MailAddress)em);
			return m;
		}
	}

	[Configuration.Section(Path = MailConfiguration.ConfigPath)]
	public class MailConfiguration : Configuration.Section {

		public const string ConfigPath = ConfigRoot + "/Homesell.config";

		[ConfigurationProperty("smtpServer", IsRequired=false)]
		public string SmtpServer { get { return (string)this["smtpServer"]; } set { this["smtpServer"] = value; } }

		[ConfigurationProperty("smtpPort", IsRequired=false, DefaultValue=25)]
		public int SmtpPort { get { return (int)(this["smtpPort"] ?? 110); } set { this["smtpPort"] = value; } }

		[ConfigurationProperty("smtpUsername", IsRequired=false)]
		public string SmtpUsername { get { return (string)this["smtpUsername"]; } set { this["smtpUsername"] = value; } }

		[ConfigurationProperty("smtpPassword", IsRequired=false)]
		public string SmtpPassword { get { return (string)this["smtpPassword"]; } set { this["smtpPassword"] = value; } }

		[ConfigurationProperty("mailboxServer", IsRequired=false)]
		public string MailboxServer { get { return (string)this["mailboxServer"]; } set { this["mailboxServer"] = value; } }

		[ConfigurationProperty("mailboxPort", IsRequired=false, DefaultValue=-1)]
		public int MailboxPort { get { return (int)(this["mailboxPort"] ?? -1); } set { this["mailboxPort"] = value; } }

		[ConfigurationProperty("mailboxUsername", IsRequired=false)]
		public string MailboxUsername { get { return (string)this["mailboxUsername"]; } set { this["mailboxUsername"] = value; } }

		[ConfigurationProperty("mailboxPassword", IsRequired=false)]
		public string MailboxPassword { get { return (string)this["mailboxPassword"]; } set { this["mailboxPassword"] = value; } }

		[ConfigurationProperty("mailboxProtocol", IsRequired=false, DefaultValue=Mailbox.ProtocolClass.Default)]
		public Mailbox.ProtocolClass MailboxProtocol { get { return (Mailbox.ProtocolClass)(this["mailboxProtocol"] ?? Mailbox.ProtocolClass.Default); } set { this["mailboxProtocol"] = value; } }

		[ConfigurationProperty("defaultSender", IsRequired=false)]
		public string DefaultSender { get { return (string)this["defaultSender"]; } set { this["defaultSender"] = value; } }

		[ConfigurationProperty("owner", IsRequired=false)]
		public string Owner { get { return (string)this["owner"]; } set { this["owner"] = value; } }

		[ConfigurationProperty("admin", IsRequired=false)]
		public string Admin { get { return (string)this["admin"]; } set { this["admin"] = value; } }

		[ConfigurationProperty("maxMailsPerHour", IsRequired = false, DefaultValue = 100)]
		public int MaxMailsPerHour { get { var max = (int)(this["maxMailsHour"] ?? 100); return max <= 0 ? int.MaxValue : max; } set { this["maxMailsPerHour"] = value; } }

		[ConfigurationProperty("maxMailsPerDay", IsRequired = false, DefaultValue = int.MaxValue)]
		public int MaxMailsPerDay { get { var max = (int)(this["maxMailsDay"] ?? 0); return max <= 0 ? int.MaxValue : max; } set { this["maxMailsPerDay"] = value; } }

		[ConfigurationProperty("dontSendTokens", IsRequired=false, DefaultValue="#nomail")]
		public string DontSendTokens { get { return (string)this["dontSendTokens"] ?? "#nomail"; } set { this["dontSendTokens"] = value; } }

		[ConfigurationProperty("daysToArchive", IsRequired = false, DefaultValue=180)]
		public int DaysToArchive { get { return (int)(this["daysToArchive"] ?? 180); } set { this["daysToArchive"] = value; } }

		[ConfigurationProperty("defaultCC", IsRequired = false, DefaultValue=null)]
		public string DefaultCC { get { return (string)this["defaultCC"]; } set { this["defaultCC"] = value; } }

	}

	public enum MailPriority { High = 1, Low = 0 }

	public class Mail : Mail<Mail> { }

	public class Mail<Self> : MailMessage, IAutostart where Self: Mail<Self> {
		public static readonly MailConfiguration Configuration = new MailConfiguration();
		public Mail() : base() { var now = DateTime.Now; Replace("#date", now.ToShortDateString(), "#time", now.ToShortTimeString()); Success = false; }

		public virtual Self PlainText(string text) { base.PlainText = text; return (Self)this; }
		public virtual Self Html(string html) { base.Body = html; base.IsBodyHtml = !string.IsNullOrEmpty(html); return (Self)this; }
		public virtual Self Body(string html) { base.Body = html; return (Self)this; }
		public virtual Self Subject(string text) { base.Subject = text; return (Self)this; }
		public virtual Self Template(string path, params object[] args) { base.Template = string.Format(path, args); return (Self)this; }
		public virtual Self Language(string languages) { base.Languages = languages; return (Self)this; }
		public virtual Self To(string emails) { base.To = emails; return (Self)this; }
		public virtual Self To(MailAddress email) { base.To.Add(email); return (Self)this; }
		public virtual Self To(params Person[] persons) { if (persons.Length > 0) Personal(persons[0]); base.To.AddRange(persons.Select(p => p.MailAddress)); return (Self)this; }
		public virtual Self To(IEnumerable<Person> persons) { return To(persons.ToArray()); }
		public virtual Self CC(string emails) { base.CC = emails; return (Self)this; }
		public virtual Self CC(MailAddress email) { base.CC.Add(email); return (Self)this; }
		public virtual Self CC(params Person[] persons) { base.CC.AddRange(persons.Select(p => p.MailAddress)); return (Self)this; }
		public virtual Self CC(IEnumerable<Person> persons) { base.CC.AddRange(persons.Select(p => p.MailAddress)); return (Self)this; }
		public virtual Self Personal(Person person) { base.Person = person; return (Self)this; }
		public virtual Self Priority(MailPriority priority) { base.Priority = priority; return (Self)this; }
		public virtual Self From(string email) { base.From = email; return (Self)this; }
		public virtual Self Replace(string placeholder, object text) { base[placeholder] = text.ToString(); return (Self)this; }
		public virtual Self Replace(params object[] replacements) { int i = 0; while (i < replacements.Length-1) Replace((string)replacements[i++], replacements[i++]); return (Self)this; }
		public virtual Self Resource(LinkedResource res) { Resources.Add(res); return (Self)this; }
		int nid = 0;
		public virtual Self Resource(Stream stream, string mimetype, out string id) { var res = new LinkedResource(stream, mimetype); res.ContentId = id = mimetype.UpTo('/') + ++nid; Resources.Add(res); return (Self)this; }
		public virtual Self Attachment(Attachment a) { Attachments.Add(a); return (Self)this; }
		public virtual Self Attachment(string file) { Attachments.Add(new Attachment(Paths.Map(file), MimeType.OfFile(file))); return (Self)this; }
		public virtual Self Attachment(Stream stream, string name) { Attachments.Add(new Attachment(stream, name, MimeType.OfFile(stream, name))); return (Self)this; }
		public Exception Exception { get; set; }
		public virtual Self SendTo(string emails) { if (emails != null) To(emails); return Send(); }
		public virtual Self SendTo(params Person[] persons) { To(persons); return Send(); }
		public new virtual Self Send() { Success = base.Send(); return (Self)this; }

		public bool Success { get; protected set; }

		internal static void Cleanup() {
			var days = Configuration.DaysToArchive;
			if (days > 0) {
				var old = DateTime.Now - TimeSpan.FromDays(days);
				using (var db = new Context()) {
					var oldmails = db.SentMails.Where(m => m.Sent < old);
					foreach (var mail in oldmails) mail.Delete();			
					db.SentMails.Remove(m => m.Sent < old);
					db.SaveChanges();
				}
			}
		}
		
		internal static AutoResetEvent WaitForSchedule= new AutoResetEvent(true);
		static object SendLock = new object();
		static Task SendTask = null;

		void SendScheduled() {
		
			var maxperhour = Configuration.MaxMailsPerHour;
			var maxperday = Configuration.MaxMailsPerDay;

			if (maxperhour <= 0) maxperhour = int.MaxValue;
			if (maxperday <= 0) maxperday = int.MaxValue;

			int senttoday = 0;
			int sentlasthour = 0;

			var now = DateTime.Now;

			while (true) {

				using (var db = new Context()) {
					
					if (maxperday < int.MaxValue) {
						senttoday = db.SentMails
							.Where(m => m.Sent.Date == now.Date)
							.Count();
					}
					if (maxperhour < int.MaxValue) {
						var lasthour = now - TimeSpan.FromHours(1);
						sentlasthour = db.SentMails
							.Where(m => m.Sent >= lasthour)
							.Count();
					}

					var n = Math.Min(maxperhour -  sentlasthour, maxperday - senttoday);

					var tosend = db.ScheduledMails
						.Include(m => m.HtmlText)
						.Include(m => m.PlainText)
						.Include(m => m.Resources)
						.Where(m => m.Priority == MailPriority.High)
						.OrderByDescending(m => m.Sent)
						.Take(n)
						.ToList();

					var nlow = (n - tosend.Count) / 2;

					tosend.AddRange(db.ScheduledMails
						.Include(m => m.HtmlText)
						.Include(m => m.PlainText)
						.Include(m => m.Resources)
						.Where(m => m.Priority == MailPriority.Low)
						.OrderByDescending(m => m.Sent)
						.Take(nlow)
					);

					lock (SendLock) {
						SendTask = Tasks.Do(() => {
							foreach (var m in tosend) m.Mail.SendNow(db, m);
						});
					}
					SendTask.Wait();
					lock (SendLock) SendTask = null;
				}

				WaitForSchedule.WaitOne();
			}
		}

		public void Startup() { Tasks.DoLater(25000, SendScheduled); }
		public void Shutdown() {
			lock (SendLock) {
				if (SendTask != null) {
					SendTask.Wait();
					Cleanup();
				}
			}
		}
	}

	public class SmtpClient : System.Net.Mail.SmtpClient, IDisposable {

		public static readonly MailConfiguration Configuration = Mail.Configuration;

		public SmtpClient(): this(Configuration.SmtpServer, Configuration.SmtpPort, Configuration.SmtpUsername, Configuration.SmtpPassword) { }

		public SmtpClient(string host, int port, string user, string password) {
			try {
				Exception = null;
				Credentials = new NetworkCredential(user, password);
				DeliveryMethod = SmtpDeliveryMethod.Network;
				Host = host;
				Port = port;
			} catch (Exception ex) {
				Exception = ex;
				Log.Error("Error connectiong to the SMTP Server {0}.", ex, this.Host);
			}
		}


		// public bool Test { get; set; }

		public Exception Exception { get; set; }

		public class MailsPerHourLimitReachedException : Exception {
			public MailsPerHourLimitReachedException(string msg) : base(msg) { }
		}

		public new void Send(MailMessage msg) {
			try {
				Exception = null;
				msg.Schedule(this);
				if (!string.IsNullOrEmpty(Configuration.DefaultCC)) {
					var oldto = msg.To;
					msg.To = Configuration.DefaultCC;
					msg.Schedule(this);
					msg.To = oldto;
				}
			} catch (Exception ex) {
				Exception = ex;
				Log.Error("Error sending mail.", ex);
				throw ex;
			}
		}

		internal void SendNow(MailMessage msg) {
			try {
				Exception = null;
				base.Send(msg);
			} catch (Exception ex) {
				Exception = ex;
				Log.Error("Error sending mail.", ex);
				throw ex;
			}
		}
	}

	public class MailMessage : System.Net.Mail.MailMessage {

		public static readonly MailConfiguration Configuration = new MailConfiguration();

		public MailMessage(): base() {
			BodyEncoding = Encoding.UTF8;
			SubjectEncoding = Encoding.UTF8;
			IsBodyHtml = true;
			Resources = new List<LinkedResource>();
			PlainText = null;
			Body = "";
			Languages = "*";
			Subject = null;
			if (!string.IsNullOrEmpty(Mail.Configuration.DefaultSender)) From = Mail.Configuration.DefaultSender;
			Priority = MailPriority.High;
			Substitutions = new List<string>();
		}

		/// <summary>
		/// Creates a new mail.
		/// </summary>
		public MailMessage(string subject, string body): base(subject, body) {
			Subject = subject;
			Body = body;
			BodyEncoding = Encoding.UTF8;
			SubjectEncoding = Encoding.UTF8;
			if (!string.IsNullOrEmpty(Mail.Configuration.DefaultSender)) From = Mail.Configuration.DefaultSender;
			Languages = "*";
			Priority = MailPriority.High;
			Resources = new List<LinkedResource>();
			PlainText = null;
			IsBodyHtml = true;
			Substitutions = new List<string>();
		}
		/// <summary>
		/// Creates a new mail.
		/// </summary>
		public MailMessage(string subject, string body, MailAddress to): this(subject, body) {
			To.Add(to);
		}

		public MailMessage(string template): this() {
			Template = template;
		}

		//public string BodyHtml { get; set; }
		public string Template { get; set; }
		public string PlainText { get; set; }
		public List<LinkedResource> Resources { get; set; }
		public List<string> Substitutions { get; set; }
		public string Languages { get; set; }
		public Person Person { get; set; }
		public Html.Document Document;
		public MailPriority Priority { get; set; }
		bool DownloadResources = true;

		public void Replace(string placeholder, object text) { this[placeholder] = (text ?? "").ToString(); }
		public void Replace(params object[] replacements) { int i = 0; while (i < replacements.Length-1) Replace((string)replacements[i++], replacements[i++]); }
		Dictionary<string, string> ResourceFiles = new Dictionary<string, string>();
		List<Task> Downloads = new List<Task>();

		internal string CreateResource(string template, string url) {
			string contentId, cid;

			url = Paths.Relative(template, url);

			lock (ResourceFiles) {
				if (ResourceFiles.ContainsKey(url)) return ResourceFiles[url];
				contentId = "Resource" + (ResourceFiles.Count+1).ToString();
				cid = "cid:" + contentId;
				ResourceFiles[url] = cid;
			}

			if (DownloadResources) {

				var type = MimeType.OfExtension(url);
				if (type != null) {
					System.IO.Stream file;
					if (url.Contains(":")) {
						if (url.IndexOf(':') == 1) file = Files.Open(url.UpTo('?'));
						else file = Files.Download(new Uri(url));
					} else if (url.StartsWith("~")) file = Files.OpenVirtual(url.UpTo('?'));
					else file = Files.Download(new Uri(new Uri(Paths.Url(template)), url));
					if (file != null) {
						var image = new LinkedResource(file, type);
						image.ContentId = contentId;
						Resources.Add(image);
					} else {
						Log.Error("Unable to open email resource {0}", url);
						cid = "#";
					}
				} else {
					throw new NotSupportedException(string.Format("Mime type for file {0} is unknown.", url));
				}
			}

			return cid;
		}

		bool transformed = false;
		public void Transform() {
			if (transformed) return;

			Personalize();
			Substitute();
			Inline();
			Secure();
			ExtractLanguage();
			var title = Document["title"];
			if (Subject == null) {
				if (title != null) Subject = Document["title"].InnerText;
				else Subject = "";
			}

			transformed = true;
		}

		internal void ReadTemplate() {
			try {
				if (Template == null) return;
				var languages = Languages ?? "";
				var langs = languages.Tokens().ToList();
				int i = 1;
				var langsstr = langs
					.Select(s => s + ";q=" + ((0.9 / langs.Count)*i++).ToString())
					.StringList(",");
				Body = Files.Html(new Uri(Paths.Url(Template)), new Action<AdvancedWebClient>((AdvancedWebClient web) => { web.Headers[HttpRequestHeader.AcceptLanguage] = langsstr; }));

				var title = Document["title"];
				if (title != null) Subject = Document["title"].InnerText;
				else Subject = "";

				IsBodyHtml = true;
			} catch (Exception ex) {
				Log.Error("Error opening email template {0}:", ex, Template);
				Document = null;
				Body = Configuration.DontSendTokens;
			}
		}

		bool inlined = false;
		private void Inline() {
			if (inlined) return;
			try {

				var doc = Document;
				if (doc == null) doc = Document = new Html.Document(Body);

				var fileurls = new Regex(@"url\((.*)\)");

				foreach (var script in doc.FindAll("script")) script.Remove(); // purge scripts
				foreach (var img in doc.FindAll("img"))
					img.Src = CreateResource(Template, img.Src); // inline images
				foreach (var link in doc.FindAll("link[rel=stylesheet][type=text/css]")) { // inline css files
					var css = link.DownloadText();
					var href = link.Href;
					if (!href.Contains(':')) href = Paths.Combine(Template, href);
					css = fileurls.Replace(css, new MatchEvaluator(match => "url(" + CreateResource(href, match.Groups[1].Value) + ")")); // inline url(...) files of css
					var style = new Html.Element("<style type='text/css'></style>");
					style.InnerText = css;
					link.Replace(style);
				}
				foreach (var img in doc.FindAll("[style*=background]")) { // inline css file references in style attribute
					img.Style = fileurls.Replace(img.Style, new MatchEvaluator(match => "url(" + CreateResource(Template, match.Groups[2].Value) + ")"));
				}
				foreach (var style in doc.FindAll("style")) { // inline file references in header css
					style.InnerText = fileurls.Replace(style.InnerText, new MatchEvaluator(match => "url(" + CreateResource(Template, match.Groups[2].Value) + ")"));
				}
				foreach (var link in doc.FindAll("a")) {
					if (!(link.Href.Contains(':') || link.Href.StartsWith("#"))) {
						var href = link.Href;
						var path = Paths.Directory(Template);
						while (href.StartsWith("../")) {
							href = href.After("../");
							path = Paths.Directory(path);
						}
						link.Href = Services.Paths.Url(Paths.Combine(path, href));
					}
				}
				//Body = doc.Text;
				inlined = true;
			} catch (Exception ex) {
				Log.Error("Error inlining mail contents {0}:", ex, Template);
				Document = null;
				Body = Configuration.DontSendTokens;
			}
		}

		bool langExtracted = false;
		internal void ExtractLanguage() {
			if (!langExtracted && !string.IsNullOrEmpty(Languages) && Languages != "*") {
				if (Document != null) {
					Document = (Html.Document)Document.ExtractLanguages(Languages);
					Body = Document.Text;
					PlainText = Document.PlainText;
					Subject = Document.Find("title").InnerText;
					IsBodyHtml = true;
				} else {
					if (!string.IsNullOrEmpty(Body)) {
						Body = new Html.Document(Body).ExtractLanguages(Languages).Text;
					}
					if (!string.IsNullOrEmpty(PlainText)) {
						PlainText = Html.Element.ExtractLanguagesFromText(PlainText, Languages);
					}
					if (!string.IsNullOrEmpty(Subject)) {
						Subject = Html.Element.ExtractLanguagesFromText(Subject, Languages);
					}
				}
				langExtracted = true;
				//Languages =  null;
			}
		}

		static Regex langtags = new Regex(@"((#title):([a-zA-Z0-9\-]+))|((#unsubscribe):([a-zA-Z0-9\-]+))|((#country):([a-zA-Z0-9\-]+))|((#salutation):([a-zA-Z0-9\-]+))|(#[A-Z]{3}[0-9'´`,.]*?[0-9])");

		bool personalized = false;
		internal void Personalize() {
			if (personalized || Person == null) return;
			Languages = Person.Culture.Name;

			//ExtractLanguage();

			if (Document != null) { Body = Document.Text; Document = null; }

			foreach (Match match in langtags.Matches(base.Body)) {
				if (match.Groups[1].Success) Replace(match.Groups[1].Value, Lang.Title(Person, new CultureInfo(match.Groups[3].Value))); // title
				else if (match.Groups[4].Success) Replace(match.Groups[4].Value, Lang.Unsubscribe(new CultureInfo(match.Groups[6].Value), Person.Email)); // unsubscribe
				else if (match.Groups[7].Success) Replace(match.Groups[7].Value, Lang.Country(new CultureInfo(match.Groups[9].Value), Person.Country)); // country
				else if (match.Groups[10].Success) Replace(match.Groups[10].Value, Lang.Salutation(Person, new CultureInfo(match.Groups[12].Value))); // salutation
				//TODO exchange currency
				// else {
				// }
			}
			var user = Person.User;
			var password = "";
			if (Membership.Provider.PasswordFormat == MembershipPasswordFormat.Clear || Membership.Provider.PasswordFormat == MembershipPasswordFormat.Encrypted) {
				password = user.GetPassword();
			}
			Replace("#title", Lang.Title(Person), "#salutation", Lang.Salutation(Person), "#address", Person.Address, "#city", Person.City, "#zip", Person.Zip, "#state", Person.State,
				"#country.iso", Person.Country, "#phone.link;#phoneto", "<a href=\"tel:" + Person.Phone + "\">" + Person.Phone + "</a>", "#phone", Person.Phone,
				"#email.link;#mailto", "<a href=\"mailto:" + Person.Email + "\">" + Person.Email + "</a>",
				"#email", Person.Email, "#company", Person.Company, "#username", Person.UserName, "#country.native", Lang.Country(Person.Culture, Person.Country),
				"#unsubscribe", Lang.Unsubscribe(Person.Culture, Person.Email), "#$", Person.Culture.NumberFormat.CurrencySymbol,
				"#password", password, "#name", Person.LastName, "#firstname", Person.FirstName, "#lastname", Person.LastName);
			personalized = true;
		}

		static Regex securetags = new Regex("<.+(('(([^'?&\\n\\r]+([?&]|&amp;))*#secure(\\(([^'\\n\\r\\)]*)\\))?(&[^'\\n\\r]*)?)')|(\"(([^\"?&\\n\\r]+([?&]|&amp;))*#secure(\\(([^\"\\n\\r\\)]*)\\))?(&[^\"\\n\\r]*)?)\"))[^>]*>");
		static Regex timeparams = new Regex("([0-9]+([.,][0-9])?)(days|minutes|seconds|weeks|months|years|d|h|m|s|w|M|y)");
		static Regex partags = new Regex("#secure\\(([^'\\n\\r\\)]*)\\)");

		bool secured = false;
		public void Secure() {
			if (secured) return;

			var doc = Document;
			if (doc == null) doc = Document = new Html.Document(Body);

			foreach (var e in doc.All("a[href*='#secure'],form[action*='#secure']")) {
				string url;
				if (e.Is("a")) url = e.Href;
				else url = e.Attribute["action"];
				var pars = partags.Match(url);
				var partext = "";
				var securetext = "#secure";
				var keys = new List<string>();
				if (pars.Success) {
					partext = pars.Groups[1].Value;
					securetext += "(" + partext + ")";
					keys = partext.Tokens(',');
				}
				var rawurl = url
					.Replace(securetext, "")
					.Replace("&amp;", "&")
					.Replace("&&", "&");
				if (rawurl.EndsWith("?") || rawurl.EndsWith("&")) rawurl = rawurl.Substring(0, rawurl.Length-1);

				var maxAgeMatch = timeparams.Match(partext);
				var maxAge = default(TimeSpan);
				if (maxAgeMatch.Success) {
					keys.Remove(maxAgeMatch.Value);
					double t = 0;
					if (double.TryParse(maxAgeMatch.Groups[1].Value, out t)) {
						switch (maxAgeMatch.Groups[3].Value) {
							case "seconds":
							case "s": maxAge = t.Seconds(); break;
							case "minutes":
							case "m": maxAge = t.Minutes(); break;
							case "hours":
							case "h": maxAge = t.Hours(); break;
							case "days":
							case "d": maxAge = t.Days(); break;
							case "weeks":
							case "w": maxAge = t.Weeks(); break;
							case "months":
							case "M": maxAge = t.Months(); break;
							case "years":
							case "y": maxAge = t.Years(); break;
						}
					}
				}

				var newurl = Security.SecureUrl(rawurl, maxAge, keys.ToArray())
					.Replace("&", "&amp;");
				if (e.Is("a")) e.Href = newurl;
				else e.Attribute["action"] = newurl;
			}
			secured = true;
		}

		public static bool SecureRequest(params string[] keys) { return Security.SecureRequest(keys); }

		bool substituted = false;
		internal void Substitute() {
			if (substituted) return;

			if (Document != null) { Body = Document.Text; Document = null; }

			var t = new StringBuilder(Body);
			var p = new StringBuilder(PlainText);

			int i = 0;
			while (i < Substitutions.Count-1) {
				var token = Substitutions[i++];
				var text = Substitutions[i++];
				var tokens = token.Tokens(';');
				foreach (var tk in tokens) {
					t = t.Replace(tk, text);
					p = p.Replace(tk, text);
					Subject = Subject.Replace(tk, text);
				}
			}

			Body = t.ToString(); PlainText = p.ToString();
		}

		public new MailAddress From { get { return new MailAddress(base.From); } set { base.From = value; } }
		public new MailAddressCollection To { get { return new MailAddressCollection(base.To); } set { base.To.Clear(); base.To.AddRange(value); } }
		public new MailAddressCollection CC { get { return new MailAddressCollection(base.CC); } set { base.CC.Clear(); base.CC.AddRange(value); } }

		public virtual object this[string placeholder] { set { Substitutions.Add(placeholder); Substitutions.Add((value ?? "").ToString()); } }

		public virtual bool DontSend {
			get {

				if (Document != null) { Body = Document.Text; Document = null; }

				var tokens = Configuration.DontSendTokens.SplitList(',', ';').ToArray();
				return PlainText.ContainsAny(tokens) || Body.ContainsAny(tokens) || Subject.ContainsAny(tokens);
			}
		}

		bool Prepare() {

			ReadTemplate();

			Transform();

			if (DontSend) return false;

			if (IsBodyHtml) {
				if (string.IsNullOrEmpty(PlainText)) {
					if (Document != null) PlainText = Document.PlainText;
					else PlainText = new Html.Document(Body).PlainText;
				}
			}

			if (Document != null) { Body = Document.Text; Document = null; }

			Resources.ForEach(res => { if (res.ContentStream.CanSeek) res.ContentStream.Seek(0, SeekOrigin.Begin); });
			Attachments.ForEach(a => { if (a.ContentStream.CanSeek) a.ContentStream.Seek(0, SeekOrigin.Begin); });

			var html = Body;
			Body = null;

			AlternateViews.Add(AlternateView.CreateAlternateViewFromString(PlainText, Encoding.UTF8, "text/plain"));
			var htmlView = AlternateView.CreateAlternateViewFromString(html, Encoding.UTF8, "text/html");
			htmlView.LinkedResources.AddRange(Resources);
			AlternateViews.Add(htmlView);
			return true;
		}

		public Exception Exception = null;

		internal void Close() {
			Resources.ForEach(res => { if (res.ContentStream.CanSeek) res.ContentStream.Close(); });
			Attachments.ForEach(a => { if (a.ContentStream.CanSeek) a.ContentStream.Close(); });
		}

		public virtual void Schedule(System.Net.Mail.SmtpClient smtp) {

			ReadTemplate();

			using (var db = new Context()) {
				var scheduledMail = new Data.ScheduledMail() {
					CC = CC.StringList(';'),
					From = From.ToString(),
					Personal = Person,
					Priority = Priority,
					Sent = DateTime.Now,
					Subject = Subject,
					To = To.StringList(';')
				};

				scheduledMail.HtmlText = Data.MailText.New(db, Body);
				scheduledMail.PlainText = Data.MailText.New(db, PlainText);

				Transform();

				if (DontSend) return;

				foreach (var lres in Resources) {
					var res = Data.MailResource.New(db, Data.MailResourceClass.Resource, lres.ContentId, lres.ContentType.MediaType, lres.ContentStream.ToArray());
					scheduledMail.Resources.Add(res);
				}
				foreach (var lres in Attachments) {
					var res = Data.MailResource.New(db, Data.MailResourceClass.Attachment, lres.ContentId, lres.ContentType.MediaType, lres.ContentStream.ToArray());
					scheduledMail.Resources.Add(res);
				}

				for (int i = 0; i < Substitutions.Count-1; i++) {
					scheduledMail.Substitutions.Add(Data.MailSubstitution.New(db, Substitutions[i++], Substitutions[i]));
				}
				db.ScheduledMails.Add(scheduledMail);
				db.SaveChanges();
			}
		}

		internal void Load(Data.ScheduledMail mail) {

			Template = null;

			CC = mail.CC;
			From = mail.From;
			Person = mail.Personal;
			Priority = mail.Priority;
			Subject = mail.Subject;
			To = mail.To;
			Body = mail.HtmlText.Text;
			PlainText = mail.PlainText.Text;

			foreach (var res in mail.Resources) {
				res.Load();
				if (res.Class == Data.MailResourceClass.Resource) {
					var lres = new LinkedResource(new MemoryStream(res.Data), res.MimeType);
					lres.ContentId = res.Name;
					Resources.Add(lres);
				} else {
					var at = new Attachment(new MemoryStream(res.Data), res.Name, res.MimeType);
					Attachments.Add(at);
				}
			}

			foreach (var subs in mail.Substitutions) Replace(subs.Token, subs.Text);

			DownloadResources = false;
			Transform();
			DownloadResources = true;
		}

		public virtual bool Send(System.Net.Mail.SmtpClient smtp) {
			try {
				((SmtpClient)smtp).Exception = null;
				Schedule(smtp);
				return (Exception = ((SmtpClient)smtp).Exception) == null;
			} catch (Exception ex) {
				Log.Error("Error sending mail.", ex);
				return false;
			}
		}

		internal bool SendNow(Context db, Data.ScheduledMail mail) {
			try {

				Load(mail);

				if (DontSend) return true;

				if (!Prepare()) return true;

				var smtp = mail.SmtpClient;

				smtp.SendNow(this);

				db.ScheduledMails.Remove(mail);

				if (Configuration.DaysToArchive > 0) Data.SentMail.New(db, mail);

				db.SaveChanges();
				return true;
			} catch (Exception ex) {
				Log.Error("Error sending mail.", ex);
				return false;
			}
		}

		public virtual bool Send() { using (var smtp = new SmtpClient()) return Send(smtp); }


		public PipeStream Eml() {

			Prepare();

			var assembly = typeof(System.Net.Mail.SmtpClient).Assembly;
			Type mailWriterType = assembly.GetType("System.Net.Mail.MailWriter");

			PipeStream pipe = new PipeStream();
			Stream writer = pipe;
			Tasks.Do<Stream>(() => {
				using (writer) {
					// Get reflection info for MailWriter contructor
					ConstructorInfo mailWriterContructor =
					mailWriterType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(Stream) }, null);

					// Construct MailWriter object with our FileStream
					object mailWriter = mailWriterContructor.Invoke(new object[] { writer });

					// Get reflection info for Send() method on MailMessage
					MethodInfo sendMethod = typeof(System.Net.Mail.MailMessage).GetMethod("Send", BindingFlags.Instance | BindingFlags.NonPublic);

					// Call method passing in MailWriter
					sendMethod.Invoke(this, BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { mailWriter, true, true }, null);

					// Finally get reflection info for Close() method on our MailWriter
					MethodInfo closeMethod = mailWriter.GetType().GetMethod("Close", BindingFlags.Instance | BindingFlags.NonPublic);

					// Call close method
					closeMethod.Invoke(mailWriter, BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { }, null);

					return writer;
				}
			});
			return pipe;
		}

		public void Save(string filename) { Files.Save(Eml(), filename); }
		public void Response(string filename) { Files.Response(Eml(), filename, "message/rfc822"); }

	}

}
