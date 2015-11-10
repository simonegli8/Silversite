// davidegli

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Migrations;

namespace Silversite {

	/// <summary>
	/// The database context for the Silversite.dll assembly, with all DbSet's needed by that assembly.
	/// </summary>
	public class Context: Data.Context<Context> {
		public Context() : base() { Database.Open<Context>(this); }
		public Context(Data.Database db) : base(db) { db.Open<Context>(this); }

		/// <summary>
		/// The Persons stored in the database.
		/// </summary>
		public DbSet<Services.Person> Persons { get; set; }
		/// <summary>
		/// The Documents stored in the database.
		/// </summary>
		public DbSet<Services.Document> Documents { get; set; }
		/// <summary>
		/// The edit rights of users for documents.
		/// </summary>
		public DbSet<Services.EditRights> EditRights { get; set; }
		/// <summary>
		/// The users stored in the database. This set is used by the Silversite Memberhsip Provider.
		/// </summary>
		public DbSet<Web.Providers.User> Users { get; set; }
		/// <summary>
		/// The roles stored in the database. This set is used by the Silversite Role Provider.
		/// </summary>
		public DbSet<Web.Providers.Role> Roles { get; set; }
		/// <summary>
		/// Applications for the Web.Providers like Membership, Role, Session and Profile provider.
		/// </summary>
		public DbSet<Web.Providers.Application> Applications { get; set; }
		/// <summary>
		/// All deleted users.
		/// </summary>
		public IQueryable<Web.Providers.User> DeletedUsers { get { return Users.Where(u => u.IsDeleted == true); } }
		/// <summary>
		/// All active users.
		/// </summary>
		public IQueryable<Web.Providers.User> ActiveUsers { get { return Users.Where(u => u.IsDeleted == false); } }
		/// <summary>
		/// All users of the current application.
		/// </summary>
		public IQueryable<Web.Providers.User> AppUsers<T>() {
			var appkey = Web.Providers.Application.Current<T>().Key;
			return Users
				.Include(u => u.Application)
				.Where(u => u.Application.Key == null || u.Application.Key == appkey);
		}
		/// <summary>
		/// All deleted users for the current application.
		/// </summary>
		public IQueryable<Web.Providers.User> AppDeletedUsers<T>() { return AppUsers<T>().Where(u => u.IsDeleted == true); }
		/// <summary>
		/// All active users for the current application.
		/// </summary>
		public IQueryable<Web.Providers.User> AppActiveUsers<T>() { return AppUsers<T>().Where(u => u.IsDeleted == false); }
		public IQueryable<Web.Providers.Role> AppRoles<T>() {
			var appkey = Web.Providers.Application.Current<T>().Key;
			return Roles
				.Include(r => r.Application)
				.Where(r => r.Application.Key == null || r.Application.Key == appkey);
		}

		/*
		public DbSet<Web.Providers.Profile> Profiles { get; set; }
		public DbSet<Web.Providers.Session> Session { get; set; }
		public DbSet<Web.Providers.SessionCleanup> SessionCleanup { get; set; }

		public IQueryable<Web.Providers.Session> AppSession<T>() { var app = Web.Providers.Application.Current<T>(); return Session.Where(s => s.Application == null || s.Application == app); }
		*/

		/// <summary>
		/// The set of LogMessages stored in the database.
		/// </summary>
		public DbSet<Services.LogMessage> LogMessages { get; set; }
		/// <summary>
		/// The set of sent mails stored in the database.
		/// </summary>
		public DbSet<Data.SentMail> SentMails { get; set; }
		// mails scheduled for sending
		public DbSet<Data.ScheduledMail> ScheduledMails { get; set; }
		// the text bodies of the mails
		public DbSet<Data.MailText> MailTexts { get; set; }
		// resources attached to the mails
		public DbSet<Data.MailResource> MailResources { get; set; }
		// substitutions of tokens in mail templates
		public DbSet<Data.MailSubstitution> MailSubstitutions { get; set; }
		public DbSet<Data.SmtpServer> SmtpServers { get; set; }
		// application uptime records
		public DbSet<Services.UptimeRecord> Uptime { get; set; }
		// stopwatch records
		public DbSet<Services.StopwatchRecord> Stopwatch { get; set; }
		// configrured languages
		public DbSet<Data.Language> Languages { get; set; }
		// configured currencies
		public DbSet<Data.Currency> Currencies { get; set; }
		// The base currency
		public Data.Currency BaseCurrency { get { return Currencies.FirstOrDefault(c => c.IsBaseCurrency); } }

		protected override void OnModelCreating(DbModelBuilder modelBuilder) {
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Web.Providers.User>()
				.HasMany(p => p.Roles)
				.WithMany(r => r.Users)
				.Map(t => t.MapLeftKey("Role")
					.MapRightKey("User")
					.ToTable("Silversite_Web_Providers_UserRoles")
				);
		}

		/// <summary>
		/// Saves changes to the database.
		/// </summary>
		/// <returns></returns>
		public override int SaveChanges() {
			// update current user cache, if current user was modified.
			var cur = Services.Persons.Current; 
			if (cur != null) {
				var username = cur.UserName;
				var dbperson = Persons.Local.FirstOrDefault(p => p.UserName == username); // find modified current user
				if (dbperson != null) Services.Persons.Current = dbperson;
			}
			return base.SaveChanges();
		}

		/*
		public static Context Open() { return Data.Context.Open<Context>(); }
		public static Context Open(Data.Database db) { return Data.Context.Open<Context>(db); }
		public static Context OpenPage() { return Data.Context.OpenPage<Context>(); }
		public static Context OpenPage(Data.Database db) { return Data.Context.OpenPage<Context>(db); }
		public static Context OpenStack() { return Data.Context.OpenStack<Context>(); }
		public static Context OpenStack(Data.Database db) { return Data.Context.OpenStack<Context>(db); }
		*/

		public override void Seed(Data.DbVersion oldVersion) {
			base.Seed(oldVersion);

			if (Membership.GetAllUsers().Count == 0) {

				// create default admin user

				Services.Person admin = null;

				admin = new Services.Person() { UserName="admin" };
				Persons.AddOrUpdate(u => u.UserName, admin);
				SaveChanges();

				Membership.CreateUser("admin", "admin");

				System.Web.Security.Roles.CreateRole("Administrators");
				admin.AddToRoles("Administrators");
			}

		}
	}

}