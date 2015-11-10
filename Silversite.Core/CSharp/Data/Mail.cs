using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.IO;
using System.IO.Compression;


namespace Silversite.Data {

	/// <summary>
	/// Types of mail resources.
	/// </summary>
	public enum MailResourceClass { Attachment, Resource }

	/// <summary>
	/// Stores a mail resource in the database.
	/// </summary>
	[Serializable]
	public class MailResource {

		public MailResource() { Key = new Guid(); }

		[Key]
		public Guid Key { get; set; }
		[Required]
		public int Hash { get; set; }

		public const string Path = "~/Silversite/data/mails";
		[NotMapped]
		public MailResourceClass Class { get; set; }
		[NotMapped]
		public string Name { get; set; }
		[NotMapped]
		public string MimeType { get; set; }
		private byte[] data;
		[NotMapped]
		public byte[] Data {
			get { return data; }
			set { data = value; Hash = Services.Hash.Compute(data); }
		}
		[NotMapped]
		public int References { get; set; }
		bool loaded = false;
	
		public static MailResource New(Silversite.Context db, MailResourceClass mrclass, string name, string mimeType, byte[] data) {
			var r = new MailResource();
			r.Data = data;
			var path = Silversite.Services.Paths.Combine(Path, r.Hash.ToString("x") + ".MailResource.bin");
			var list = new List<MailResource>();
			if (Silversite.Services.Files.Exists(path)) {
				list = Silversite.Services.Files.LoadSerializable(path) as List<MailResource>;
				var res = list.FirstOrDefault(x => Enumerable.SequenceEqual(x.Data, data));
				if (res != null) {
					res.References++;
					Silversite.Services.Files.Save(list, path);
					return res;
				}
			}
			r.MimeType = mimeType;
			r.Name = name;
			r.Class = mrclass;
			r.References = 1;
			db.MailResources.Add(r);
			list.Add(r);
			Silversite.Services.Files.SaveWithPath(list, path);
			return r;
		}

		public void Delete() {
			Load();
			var path = Silversite.Services.Paths.Combine(Path, Hash.ToString("x") + ".MailResource.bin");
			var list = new List<MailResource>();
			if (Silversite.Services.Files.Exists(path)) {
				list = Silversite.Services.Files.LoadSerializable(path) as List<MailResource>;
			}
			var res = list.FirstOrDefault(x => Enumerable.SequenceEqual(x.Data, data));
			if (res != null) {
				res.References--;
				if (res.References <= 0) list.Remove(res);
				Silversite.Services.Files.Save(list, path);
			}
		}

		public void Load() {
			if (loaded) return;
			var path = Silversite.Services.Paths.Combine(Path, Hash.ToString("x") + ".MailResource.bin");
			var list = new List<MailResource>();
			if (Silversite.Services.Files.Exists(path)) {
				list = Silversite.Services.Files.LoadSerializable(path) as List<MailResource>;
			}
			loaded = true;
			var res = list.FirstOrDefault(x => x.Key == Key);
			if (res != null) {
				Data = res.Data;
				Name = res.Name;
				MimeType = res.MimeType;
			}
		}
	}

	public class SmtpServer {
		[Key]
		public int Key { get; set; }
		[Required]
		public int Hash { get; set; }

		[Required, MaxLength(128)]
		public string Host { get; set; }
		[Required]
		public int Port { get; set; }
		[Required, MaxLength(128)]
		public string User { get; set; }
		[Required, MaxLength(128)]
		public string Password { get; set; }

		public static SmtpServer New(Silversite.Context db, string host, int port, string user, string password) {
			var hash = Services.Hash.Compute(string.Format("{0}, {1}, {2}, {3}", host, port, user, password));
			var smtp = db.SmtpServers.FirstOrDefault(s => s.Hash == hash && s.Host == host && s.Port == port && s.User == user && s.Password == password);
			if (smtp == null) {
				smtp = new SmtpServer() { Hash = hash, Host = host, Port = port, User = user, Password = password };
				db.SmtpServers.Add(smtp);
				db.SaveChanges();
			}
			return smtp;
		}
	}

	public class MailText {
		[Key]
		public int Key { get; set; }
		[Required]
		public int Hash { get; set; }
		[Required, MaxLength]
		public string Text { get; set; }

		public static MailText New(Silversite.Context db, string text) {
			var hash = Services.Hash.Compute(text);
			var mt = db.MailTexts.FirstOrDefault(t => t.Hash == hash && t.Text == text);
			if (mt == null) {
				mt = new MailText() { Hash = hash, Text = text };
				db.MailTexts.Add(mt);
				db.SaveChanges();
			}
			return mt;
		}
	}

	public class MailSubstitution {
		[Key]
		public int Key { get; set; }
		[Required]
		public int Hash { get; set; }
		[Required, MaxLength]
		public string Token { get; set; }
		[Required, MaxLength]
		public string Text { get; set; }

		public static MailSubstitution New(Silversite.Context db, string token, string text) {
			var hash = Services.Hash.Compute(token + text);
			var mt = db.MailSubstitutions.FirstOrDefault(t => t.Hash == hash && t.Text == text && t.Token == token);
			if (mt == null) {
				mt = new MailSubstitution() { Hash = hash, Token = token, Text = text };
				db.MailSubstitutions.Add(mt);
				db.SaveChanges();
			}
			return mt;
		}

	}
#if EF4
	public class MailPriorityEnum : Enum<Services.MailPriority> { }
#endif

	public class StoredMail {

		public StoredMail() { Sent = System.DateTime.Now; Key = new Guid(); }

		[Key]
		public Guid Key { get; set; }
		[Required]
		public System.DateTime Sent { get; set; }
		[Required]
#if EF4
		public MailPriorityEnum Priority { get; set; }
#else
		public Services.MailPriority Priority { get; set; }
#endif
		[MaxLength]
		public string To { get; set; }
		[MaxLength(128)]
		public string From { get; set; }
		[MaxLength]
		public string CC { get; set; }

		[MaxLength(128)]
		public string Subject { get; set; }

		public MailText PlainText { get; set; }
		public MailText HtmlText { get; set; }
		[Required]
		public ICollection<MailResource> Resources { get; set; }

		[Required]
		public ICollection<MailSubstitution> Substitutions { get; set; }

		public Services.Person Personal { get; set; }

		public SmtpServer SmtpServer { get; set; }

		public Services.SmtpClient SmtpClient {
			get { return new Services.SmtpClient(SmtpServer.Host, SmtpServer.Port, SmtpServer.User, SmtpServer.Password); }
		}

		public Services.Mail Mail {
			get {
				var mail = new Services.Mail()
					.To(To)
					.From(From)
					.Subject(Subject);
				if (PlainText != null) mail.PlainText(PlainText.Text);
				if (HtmlText != null) mail.Html(HtmlText.Text);
				foreach (var res in Resources) {
					switch (res.Class) {
						case MailResourceClass.Resource:
							var r = new System.Net.Mail.LinkedResource(res.Data.ToStream(), res.MimeType);
							r.ContentId = res.Name;
							mail.Resource(r);
							break;
						case MailResourceClass.Attachment:
							mail.Attachment(new System.Net.Mail.Attachment(res.Data.ToStream(), res.Name, res.MimeType));
							break;
						default: break;
					}
				}
				foreach (var s in Substitutions) mail.Replace(s.Token, s.Text);
				return mail;
			}
		}

		public static void New<T>(Silversite.Context db, DbSet<T> set, Services.MailMessage msg, System.Net.Mail.SmtpClient smtp) where T: StoredMail, new() {
			var mail = new T();
			set.Add(mail);
			mail.To = msg.To
				.Select(ma => ma.DisplayName + "<" + ma.Address + ">")
				.StringList(";");
			mail.From = msg.From.DisplayName + "<" + msg.From.Address + ">";
			mail.CC = msg.CC
				.Select(ma => ma.DisplayName + "<" + ma.Address + ">")
				.StringList(";");
			mail.Subject = msg.Subject;
			mail.Priority = msg.Priority;
			db.SaveChanges();

			MailText plaintext = null, body = null;

			if (!string.IsNullOrEmpty(msg.PlainText)) plaintext = MailText.New(db, msg.PlainText);

			if (msg.IsBodyHtml && string.IsNullOrEmpty(msg.Body)) body = MailText.New(db, msg.Body);

			mail.PlainText = plaintext;
			mail.HtmlText = body;
			mail.Resources = new List<MailResource>();

			foreach (var a in msg.Attachments) {
				var at = Data.MailResource.New(db, Data.MailResourceClass.Attachment, a.ContentId, a.ContentType.Name, a.ContentStream.ToArray());
				mail.Resources.Add(at);
			}
			foreach (var r in msg.Resources) {
				var res = Data.MailResource.New(db, Data.MailResourceClass.Attachment, r.ContentId, r.ContentType.Name, r.ContentStream.ToArray());
				mail.Resources.Add(res);
			}
			int i = 0;
			while (i < msg.Substitutions.Count) {
				var s = new Data.MailSubstitution() { Token = msg.Substitutions[i++], Text = msg.Substitutions[i++] };
				mail.Substitutions.Add(s);
			}

			var credentials = smtp.Credentials as System.Net.NetworkCredential;
			mail.SmtpServer = Data.SmtpServer.New(db, smtp.Host, smtp.Port, credentials.UserName, credentials.Password);
			db.SaveChanges();
		}

		public void Delete() {
			foreach (var res in Resources) res.Delete();
		}
	}

	public class ScheduledMail: StoredMail {
		public static void New(Silversite.Context db, Services.MailMessage msg, System.Net.Mail.SmtpClient smtp) { New<ScheduledMail>(db, db.ScheduledMails, msg, smtp); }
	}

	public class SentMail : StoredMail {
		public static void New(Silversite.Context db, Services.MailMessage msg, System.Net.Mail.SmtpClient smtp) { New<SentMail>(db, db.SentMails, msg, smtp); }
		public static void New(Silversite.Context db, ScheduledMail mail) {
			var sent = new SentMail() {
				CC = mail.CC, From = mail.From, HtmlText = mail.HtmlText, Personal = mail.Personal, PlainText = mail.PlainText, Priority = mail.Priority, Sent = DateTime.Now, Subject = mail.Subject,
				Resources = mail.Resources.ToList(), Substitutions = mail.Substitutions.ToList(), SmtpServer = mail.SmtpServer, To = mail.To
			};
			db.SentMails.Add(sent);
		}
	}


}