using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Silversite.Services {
	
	public class Mailbox : StaticService<Mailbox, MailboxProvider>, IDisposable {

		public enum ProtocolClass { Default = 0, Pop3 = 110, Imap4 = 143, Imap4Ssl = 585, ImapS = 993, SslPop = 995 };
		[Flags]
		public enum StatusClass { None = 0, New = 1, Downloaded = 2, Unread = 4, Read = 8 };

		public class Mail : Services.Mail {
			public StatusClass Status { get; set; }
			public object MailId { get; set; }
		}

		public static readonly MailConfiguration Configuration = Mail.Configuration;

		public ProtocolClass Protocol { get; set; }
		public string Server { get; set; }
		public int Port { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }

		public int DefaultPort { get { return (int)Protocol; } }

		public Mailbox(): this(
				Configuration.MailboxProtocol != ProtocolClass.Default ? Configuration.MailboxProtocol : ProtocolClass.Pop3,
				Configuration.MailboxServer, Configuration.MailboxUsername, Configuration.MailboxPassword, Configuration.MailboxPort
				) { }

		public Mailbox(ProtocolClass protocol, string server, string username, string password, int port = -1) {
			Protocol = protocol; Server = server; Username = username; Password = password;
			if (port > 0) Port = port;
			else Port = DefaultPort;
			MailboxClient = Provider.Open(this);
		}

		IDisposable MailboxClient { get; set; }

		public IEnumerable<Mail> New() { return Provider.New(this); }
		public IEnumerable<Mail> All() { return Provider.All(this); }
		public void Remove(Mail mail) { Provider.Remove(this, mail); }
		public void Remove(IEnumerable<Mail> mails) { Provider.Remove(this, mails); }

		public void SelectInbox() { Provider.SelectInbox(this); }
		public bool SelectFolder(string folder) { return Provider.SelectFolder(this, folder); }
		public IEnumerable<string> ListFolders() { return Provider.ListFolders(this); }

		void IDisposable.Dispose() { if (MailboxClient != null) { MailboxClient.Dispose(); MailboxClient = null; } }
	}

	public abstract class MailboxProvider : Provider<Mailbox> {
		public abstract IDisposable Open(Mailbox mailbox);
		public abstract IEnumerable<Mailbox.Mail> New(Mailbox mailbox);
		public abstract IEnumerable<Mailbox.Mail> All(Mailbox mailbox);
		public abstract void Remove(Mailbox mailbox, Mailbox.Mail mail);
		public abstract void Remove(Mailbox mailbox, IEnumerable<Mailbox.Mail> mails);
		public abstract void SelectInbox(Mailbox mailbox);
		public abstract bool SelectFolder(Mailbox mailbox, string folder);
		public abstract IEnumerable<string> ListFolders(Mailbox mailbox);
	}

}