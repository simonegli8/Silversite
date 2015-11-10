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
using System.Net.Mime;
using System.IO;
using System.Reflection;
using System.Web.Configuration;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace Silversite.Services {

	public class MailAddress : System.Net.Mail.MailAddress {
		public MailAddress(string email) : base(email) { }
		public MailAddress(string email, string name) : base(email, name) { }
		public MailAddress(string email, string name, Encoding encoding) : base(email, name, encoding) { }
		public MailAddress(System.Net.Mail.MailAddress adr): this(adr.Address, adr.DisplayName, Encoding.UTF8) { }
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

	public class SentMail {
		
		public SentMail() { Sent = DateTime.Now; }

		[Key]
		public int Key { get; set; }
		public DateTime Sent { get; set; }
		[MaxLength(64)]
		public string Name { get; set; }
		[MaxLength(64)]
		public string Email { get; set; }
		[MaxLength(128)]
		public string Subject { get; set; }
		[MaxLength]
		public string Text { get; set; }
		[MaxLength]
		public byte[] EmlCompressed { get; set; }
		[Required]
		public int Size { get; set; }

		public Stream Eml {
			get {
				var m = new MemoryStream(EmlCompressed);
				return new System.IO.Compression.DeflateStream(m, System.IO.Compression.CompressionMode.Decompress);
			}
		}

		public void Save(string filename) { Files.Save(Eml, filename); }
		public void Download(string filename) { Files.Serve(Eml, filename); }
	}

	[Configuration.Section(Path = MailConfiguration.ConfigPath)]
	public class MailConfiguration: Configuration.Section {

		public const string ConfigPath = ConfigRoot + "/Silversite.config";

		[ConfigurationProperty("smtpServer", IsRequired=false)]
		public string SmtpServer { get { return (string)this["smtpServer"]; } set { this["smtpServer"] = value; } }

		[ConfigurationProperty("smtpPort", IsRequired=false, DefaultValue=25)]
		public int SmtpPort { get { return (int)(this["smtpPort"] ?? 110); } set { this["smtpPort"] = value; } }

		[ConfigurationProperty("smtpUsername", IsRequired=false)]
		public string SmtpUsername { get { return (string)this["smtpUsername"]; } set { this["smtpUsername"] = value; } }

		[ConfigurationProperty("smtpPassword", IsRequired=false)]
		public string SmtpPassword { get { return (string)this["smtpPassword"]; } set { this["smtpPassword"] = value; } }

		[ConfigurationProperty("defaultSender", IsRequired=false)]
		public string DefaultSender { get { return (string)this["defaultSender"]; } set { this["defaultSender"] = value; } }

		[ConfigurationProperty("owner", IsRequired=false)]
		public string Owner { get { return (string)this["owner"]; } set { this["owner"] = value; } }

		[ConfigurationProperty("admin", IsRequired=false)]
		public string Admin { get { return (string)this["admin"]; } set { this["admin"] = value; } }

		[ConfigurationProperty("maxMailsPerHour", IsRequired = false, DefaultValue = 100)]
		public int MaxMailsPerHour { get { var max = (int)(this["maxMailsPerHour"] ?? 100); return max <= 0 ? int.MaxValue : max; } set { this["maxMailsPerHour"] = value; } }

		[ConfigurationProperty("dontSendTokens", IsRequired=false, DefaultValue="#nomail")]
		public string DontSendTokens { get { return (string)this["dontSendTokens"] ?? "#nomail"; } set { this["dontSendTokens"] = value; } }

		[ConfigurationProperty("archiveSize", IsRequired = false, DefaultValue=100)]
		public int ArchiveSize { get { return (int)(this["archiveSize"] ?? 100); } set { this["archiveSize"] = value; } }

		[ConfigurationProperty("defaultCC", IsRequired = false, DefaultValue=null)]
		public string DefaultCC { get { return (string)this["defaultCC"]; } set { this["defaultCC"] = value; } }

		[ConfigurationProperty("mailboxServer", IsRequired = false)]
		public string MailboxServer { get { return (string)this["mailboxServer"]; } set { this["mailboxServer"] = value; } }

		[ConfigurationProperty("mailboxPort", IsRequired = false, DefaultValue = -1)]
		public int MailboxPort { get { return (int)(this["mailboxPort"] ?? -1); } set { this["mailboxPort"] = value; } }

		[ConfigurationProperty("mailboxUsername", IsRequired = false)]
		public string MailboxUsername { get { return (string)this["mailboxUsername"]; } set { this["mailboxUsername"] = value; } }

		[ConfigurationProperty("mailboxPassword", IsRequired = false)]
		public string MailboxPassword { get { return (string)this["mailboxPassword"]; } set { this["mailboxPassword"] = value; } }

		[ConfigurationProperty("mailboxProtocol", IsRequired = false, DefaultValue = Mailbox.ProtocolClass.Default)]
		public Mailbox.ProtocolClass MailboxProtocol { get { return (Mailbox.ProtocolClass)(this["mailboxProtocol"] ?? Mailbox.ProtocolClass.Default); } set { this["mailboxProtocol"] = value; } }

	}

	public enum MailPriority { High = 1, Low = 0 }

	public class Mail : Mail<Mail> { }

	public class Mail<Self> : MailMessage, IAutostart where Self : Mail<Self> {
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
		public virtual Self Replace(string placeholder, object text) { base[placeholder] = (text ?? "").ToString(); return (Self)this; }
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
		}

		public void Startup() { /* Tasks.DoLater(25000, SendScheduled); */ }
		public void Shutdown() { Cleanup(); }

	}

	public class MailsPerHourLimitReachedException : Exception {
		public MailsPerHourLimitReachedException(string msg) : base(msg) { }
	}

	/// <summary>
	/// A class that implements sending of mails.
	/// </summary>

	public class SmtpClient: System.Net.Mail.SmtpClient, IDisposable {

		public static readonly MailConfiguration Configuration = Mail.Configuration;

		public SmtpClient() {
			try {
				Exception = null;
				Credentials = new NetworkCredential(Configuration.SmtpUsername, Configuration.SmtpPassword);
				DeliveryMethod = SmtpDeliveryMethod.Network;
				Host = Configuration.SmtpServer;
			} catch (Exception ex) {
				Exception = ex;
				Log.Error("Error connectiong to the SMTP Server {0}.", ex, this.Host);
			}
		}

		// public bool Test { get; set; }
	
		public Exception Exception { get; set; }

		public new void Send(System.Net.Mail.MailMessage msg) {
			try {
				if (msg is MailMessage) {
					if (((MailMessage)msg).Prepare()) base.Send(msg);
				} else base.Send(msg);
				if (!string.IsNullOrEmpty(Configuration.DefaultCC)) {
					var oldto = msg.To.ToList();
					msg.To.Clear();
					msg.To.AddRange(
						Configuration.DefaultCC
							.Tokens()
							.Select(t => new MailAddress(t))
					);
					base.Send(msg);
					msg.To.Clear();
					msg.To.AddRange(oldto);
				}
			} catch (Exception ex) {
				Exception = ex;
				Log.Error("Error sending email.", ex);
				//return false;
			}
			//return true;
		}
	}


	public class MailMessage: System.Net.Mail.MailMessage {

		public static readonly MailConfiguration Configuration = Mail.Configuration;

		public MailMessage()
			: base() {
			BodyEncoding = Encoding.UTF8;
			SubjectEncoding = Encoding.UTF8;
			IsBodyHtml = true;
			Resources = new List<LinkedResource>();
			PlainText = "";
			Body = "";
			Subject = "";
			Languages = "*";
			if (!string.IsNullOrEmpty(Mail.Configuration.DefaultSender)) From = Mail.Configuration.DefaultSender;
			Priority = MailPriority.High;
			Substitutions = new List<string>();
		}

		/// <summary>
		/// Creates a new mail.
		/// </summary>
		public MailMessage(string subject, string body)
			: base(subject, body) {
			Subject = subject;
			Body = body;
			BodyEncoding = Encoding.UTF8;
			SubjectEncoding = Encoding.UTF8;
			if (!string.IsNullOrEmpty(Mail.Configuration.DefaultSender)) From = Mail.Configuration.DefaultSender;
			Languages = "*";
			Priority = MailPriority.High;
			Resources = new List<LinkedResource>();
			PlainText = "";
			IsBodyHtml = true;
			Substitutions = new List<string>();
		}

		/// <summary>
		/// Creates a new mail.
		/// </summary>
		public MailMessage(string subject, string body, MailAddress to)
			: this(subject, body) {
			To.Add(to);
		}

		public MailMessage(string template)
			: this() {
			Template = template;
			Body = "";
			Subject = "";
		}

		public string Template { get; set; }
		public string PlainText { get; set; }
		public List<LinkedResource> Resources { get; set; }
		public List<string> Substitutions { get; set; }
		public string Languages { get; set; }
		public Person Person { get; set; }
		public Html.Document Document;
		public MailPriority Priority { get; set; }

		public void Replace(string placeholder, object text) { this[placeholder] = (text ?? "").ToString(); }
		public void Replace(params object[] replacements) { int i = 0; while (i < replacements.Length-1) Replace((string)replacements[i++], replacements[i++]); }
		Dictionary<string, string> ResourceFiles = new Dictionary<string, string>();
		List<Task> Downloads = new List<Task>();

		static HashSet<string> images = new HashSet<string>(new string[] { "image/jpeg", "image/bmp", "image/png", "image/svg+xml", "image/tiff", "image/gif" });

		internal string CreateResource(string template, string url, Html.Document document = null) {
			string contentId, cid;

			if (url.StartsWith("cid:") || url.StartsWith("data:")) return url;

			url = Paths.Relative(template, url);

			lock (ResourceFiles) {
				if (ResourceFiles.ContainsKey(url)) return ResourceFiles[url];
				contentId = "Resource" + (ResourceFiles.Count+1).ToString();
				cid = "cid:" + contentId;
				ResourceFiles[url] = cid;
			}

			var type = MimeType.OfExtension(url);
			if (type != null) {
				System.IO.Stream file;
				if (url.Contains(":")) {
					if (url.IndexOf(':') == 1) file = Files.Read(url.UpTo('?'));
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

			if (document != null && images.Contains(type)) { // insert hidden image to avoid attachment listing in windows live mail
				var body = document["body"] as Html.Element;
				if (body["img[src='" + cid + "']"] == null) body.Children.Add(new Html.Element("<img src='" + cid + "' style='display:none;' />"));
			}

			return cid;
		}

		/*
		internal string CreateResource(string template, string url) {
			string contentId, cid;
			
			lock (ResourceFiles) {
				if (ResourceFiles.ContainsKey(url)) return ResourceFiles[url];
				contentId = "Resource" + (ResourceFiles.Count+1).ToString();
				cid = "cid:" + contentId;
				ResourceFiles[url] = cid;
			}

			var type = MimeType.OfExtension(url);
			if (type != null) {
				// start parallel downloading of resources but avoid deadlock due to running out of threadpool threads for local ASP.NET requests.
				int max, cmax;
				ThreadPool.GetMaxThreads(out max, out cmax);
				var runningDownloads = Downloads.Where(d => !d.IsCompleted).ToArray();
				if (runningDownloads.Length > max/2) Task.WaitAny(runningDownloads);
				
				Downloads.Add(Tasks.Do(() => {
					System.IO.Stream file;
					if (url.Contains(":")) file = Files.Download(new Uri(url));
					else if (url.StartsWith("~")) file = Files.OpenVirtual(url);
					else file = Files.Download(new Uri(new Uri(Paths.Url(template)), url));
					var image = new LinkedResource(file, type);
					image.ContentId = contentId;
					Resources.Add(image);
				}));
			} else {
				throw new NotSupportedException(string.Format("Mime type for file {0} is unknown.", url));
			}

			return cid;
		} */

		bool transformed = false;
		public void Transform() {
			if (transformed) return;

			Personalize();
			Substitute();
			Inline();
			Secure();
			ExtractLanguage();

			transformed = true;
		}

		internal void ReadTemplate() {
			try {
				if (Template == null) return;
				string txt;
				var languages = Languages ?? "";
				var langs = languages.Tokens().ToList();
				int i = 1;
				var langsstr = langs
					.Select(s => s + ";q=" + ((0.9 / langs.Count)*i++).ToString())
					.StringList(",");
				Body = Files.Html(new Uri(Paths.Url(Template)), new Action<AdvancedWebClient>((AdvancedWebClient web) => { web.Headers[HttpRequestHeader.AcceptLanguage] = langsstr; }));

				Transform();

				var title = Document["title"];
				if (title != null) Subject = Document["title"].InnerText;
				else Subject = "";

				Document.Url = new Uri(Paths.Url(Template));
				IsBodyHtml = true;

			} catch (Exception ex) {
				Debug.Break();
				Log.Error("Error opening email template {0}; url: {1}", ex, Template, Paths.Url(Template));
				Document = null;
				Body = Configuration.DontSendTokens;
			}
		}

		Regex fileurls = new Regex(@"url\((.*)\)");
		public string InlineUrlFiles(string text) {
			return fileurls.Replace(text, new MatchEvaluator(match => "url(" + CreateResource(Template, match.Groups[1].Value, Document) + ")")); // inline url(...) files
		}

		bool inlined = false;
		private void Inline() {
			if (inlined) return;
			try {

				var doc = Document;
				if (doc == null) doc = Document = new Html.Document(Body);
				doc.Url = Document.Url = new Uri(Paths.Url(Template));

				foreach (var script in doc.All("script")) script.Remove(); // purge scripts
				foreach (var img in doc.All("img")) 
					img.Src = CreateResource(Template, img.Src); // inline images
				foreach (var link in doc.All("link[rel=stylesheet][type=text/css]")) { // inline css files
					var css = link.DownloadText();
					css = InlineUrlFiles(css); // inline url(...) files of css
					var style = new Html.Element("<style type='text/css'></style>");
					style.InnerText = css;
					link.Replace(style);
				}
				foreach (var img in doc.All("[style*=background]")) img.Style = InlineUrlFiles(img.Style); // inline css file references in style attribute
				foreach (var style in doc.All("style")) style.InnerText = InlineUrlFiles(style.InnerText); // inline file references in header css
				foreach (var link in doc.All("a")) { // make links global 
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
				inlined = true;
			} catch (Exception ex) {
				Debug.Break();
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
			Languages = Person.Culture;
			
			ExtractLanguage();

			if (Document != null) { Body = Document.Text; Document = null; }

			foreach (Match match in langtags.Matches(base.Body)) {
				if (match.Groups[1].Success) Replace(match.Groups[1].Value, Lang.Title(Person, match.Groups[3].Value)); // title
				else if (match.Groups[4].Success) Replace(match.Groups[4].Value, Lang.Unsubscribe(match.Groups[6].Value, Person.Email)); // unsubscribe
				else if (match.Groups[7].Success) Replace(match.Groups[7].Value, Lang.Country(match.Groups[9].Value, Person.Country)); // country
				else if (match.Groups[10].Success) Replace(match.Groups[10].Value, Lang.Salutation(Person, match.Groups[12].Value)); // salutation
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
				"#unsubscribe", Lang.Unsubscribe(Person.Culture, Person.Email), "#$", new System.Globalization.CultureInfo(Person.Culture ?? "de-CH").NumberFormat.CurrencySymbol,
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
							case "seconds": case "s": maxAge = t.Seconds(); break;
							case "minutes": case "m": maxAge = t.Minutes(); break;
							case "hours": case "h": maxAge = t.Hours(); break;
							case "days": case "d": maxAge = t.Days(); break;
							case "weeks": case "w": maxAge = t.Weeks(); break;
							case "months": case "M": maxAge = t.Months(); break;
							case "years": case "y": maxAge = t.Years(); break;
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
		public new MailAddressCollection To { get { return new MailAddressCollection(base.To); } set { base.To.Clear();  base.To.AddRange(value); } }
		public new MailAddressCollection CC { get { return new MailAddressCollection(base.CC); } set { base.CC.Clear();  base.CC.AddRange(value); } }

		public virtual object this[string placeholder] { set { Substitutions.Add(placeholder); Substitutions.Add((value ?? "").ToString()); } }

		public virtual bool DontSend {
			get {

				if (Document != null) { Body = Document.Text; Document = null; }

				var tokens = Configuration.DontSendTokens.SplitList(',', ';').ToArray();
				return PlainText.ContainsAny(tokens) || Body.ContainsAny(tokens) || Subject.ContainsAny(tokens);
			}
		}

		internal bool Prepare() {

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
			Attachments.ForEach(a => {if (a.ContentStream.CanSeek) a.ContentStream.Seek(0, SeekOrigin.Begin); });

			if (string.IsNullOrWhiteSpace(Body)) {
				Log.Error("Mail with empty body to {0}.", To.String());
				Debug.Break();
			}

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

		public virtual bool Send(System.Net.Mail.SmtpClient smtp) {
			try {
				if (smtp is SmtpClient) ((SmtpClient)smtp).Exception = null;
				if (DontSend) return true;
				if (!Prepare()) return true;

				if (string.IsNullOrEmpty(Subject)) {
					Log.Error("Mail with emtpy subject.");
					Debug.Break();
				}

				/*if (string.IsNullOrWhiteSpace(Body)) {
					Log.Error("Mail with empty body.");
					Debug.Break();
				}*/

				smtp.Send(this);

				Close();

				return (!(smtp is SmtpClient) || (Exception = ((SmtpClient)smtp).Exception) == null);
			} catch (Exception ex) {
				Log.Error("Error sending mail.", ex);
				return false;
			}
		}


		public virtual bool Send() { using (var smtp = new SmtpClient()) return Send(smtp); }


		public PipeStream Eml() {
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
		public void Response(string filename) { Files.Serve(Eml(), filename, "message/rfc822"); }

	}
}
