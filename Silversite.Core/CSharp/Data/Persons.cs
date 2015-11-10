// davidegli

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;
using Security = System.Web.Security;

[assembly: Silversite.Services.DependsOn(typeof(Silversite.Services.Persons))]

namespace Silversite.Services {

	[Flags]
	public enum FileSystemRights { None = 0, Home = 1, RootRead = 2, RootWrite = 4, Full = 7 }

	/// <summary>
	/// A persistent class that describes person data.
	/// </summary>
	[Table("Silversite_Services_Person", Schema="dbo")]
	public class Person {

		public enum Genders { None = 0, Male = 1, Female = 2, Miss = 3, Group = 4, Company = 5 }

		/// <summary>
		/// A persisten class with editor settings for a person
		/// </summary>
		[ComplexType]
		public class EditorSettingsClass {

			public EditorSettingsClass() {
				Rights = Html.EditorRight.SafeAspControls;
				Menu = JavaScriptTextEditor.EditorMenu.Basic;
				//History = true;
				EditableDocuments = null;
			}
			/// <summary>
			/// Editor rights for that person.
			/// </summary>
			public virtual Html.EditorRight Rights { get; set; }
			/// <summary>
			/// The default editor mode for that person.
			/// </summary>
			public virtual JavaScriptTextEditor.EditorMenu Menu { get; set; }
			//public virtual bool History { get; set; }
			internal List<EditRight> EditableDocuments { get; set; }

		}

		/// <summary>
		/// A persisten class with editor settings for a person
		/// </summary>
		[ComplexType]
		public class FileSettingsClass {

			public const long Unlimited = long.MaxValue;

			public FileSystemRights Rights { get; set; }
		
			public long Quota { get; set; }
		
		}

		public class CustomDataClass: System.Collections.Specialized.NameValueCollection {
			
			public Person Person { get; private set; }

			public CustomDataClass(Person person) { Person = person; Update(); }

			public void Update() {
				if (Count == 0) Person.customData = null;
				else {
					using (var m = new System.IO.MemoryStream()) {
						var f = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
						f.Serialize(m, this);
						Person.customData = m.ToArray();
					}
				}
			}

			public override void Add(string name, string value) { base.Add(name, value); Update(); }
			public override void Clear() { base.Clear(); Update(); }
			public override void Remove(string name) { base.Remove(name); Update(); }
			public override void Set(string name, string value) { base.Set(name, value); Update(); }

			internal static void Load(Person person) {
				if (person.customData == null) person.Custom.Clear();
				else {
					using (var m = new System.IO.MemoryStream(person.customData)) {
						var f = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
						var data = f.Deserialize(m) as CustomDataClass;
						data.Person = person;
						person.Custom = data;
					}
				}
			}
	}

		public Person() { Documents = new HashSet<Document>(); EditorSettings = new Person.EditorSettingsClass(); RegistrationDate = DateTime.Now; Custom = new CustomDataClass(this); }

		/// <summary>
		/// The database key for a person.
		/// </summary>
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Key { get; set; }
		/// <summary>
		/// The persons title.
		/// </summary>
		[MaxLength(64)]
		public string Title { get; set; }
		public Genders Gender { get; set; }
		[MaxLength(128)]
		public string FirstName { get; set; }
		[MaxLength(128)]
		public string LastName { get; set; }
		[MaxLength(128)]
		public string Company { get; set; }
		[MaxLength(128)]
		public string Address { get; set; }
		[MaxLength(32)]
		public string Zip { get; set; }
		[MaxLength(128)]
		public string City { get; set; }
		[MaxLength(64)]
		public string Country { get; set; }
		[MaxLength(64)]
		public string State { get; set; }
		[MaxLength(32)]
		public string TimeZone { get; set; }
		[MaxLength(16)]
		public string Culture { get; set; }
		[MaxLength(128)]
		public string Phone { get; set; }
		[MaxLength(128)]
		public string Email { get; set; }

		public MailAddress MailAddress {
			get {
				string name = ((string.IsNullOrEmpty(LastName) || string.IsNullOrEmpty(FirstName)) && !string.IsNullOrEmpty(Company)) ? Company : FirstName + " " + LastName;
				return new MailAddress(Email, name);
			}
		}

		/// <summary>
		/// The username used to login.
		/// </summary>
		[MaxLength(128)]
		public string UserName { get; set; }
		/// <summary>
		/// The encrypted password.
		/// </summary>
		[MaxLength(128)]
		public string Password { get; set; }
		/// <summary>
		/// The date when the person was registered.
		/// </summary>
		public DateTime RegistrationDate { get; set; }
		/// <summary>
		/// The disk space used by this person.
		/// </summary>
		public long DiskSpaceUsed { get; set; }
		/// <summary>
		/// All documents this person is author of.
		/// </summary>
		public virtual ICollection<Document> Documents { get; protected set; }
		public virtual Html.EditorRight EditorRights { get; set; }
		public virtual JavaScriptTextEditor.EditorMenu EditorMenu { get; set; }
		public virtual string EditableDocuments { get; set; }
		
		public virtual EditorSettingsClass EditorSettings { get; protected set; }

		public virtual FileSettingsClass FileSettings { get; set; }

		public string Providers { get; set; }

		internal byte[] customData;

		[Column("CustomData")]
		internal byte[] customDataInternal { get { return customData; } set { customData = value; CustomDataClass.Load(this); } }

		[NotMapped]
		public CustomDataClass Custom { get; internal set; }

		// Membership

		/// <summary>
		/// Create a MembershipUser for this person.
		/// </summary>
		/// <returns>A new MembershipUser.</returns>
		public virtual MembershipUser CreateUser(string password) { return System.Web.Security.Membership.CreateUser(UserName, password, Email); }
		/// <summary>
		/// Deletes the corresponding MembershipUser.
		/// </summary>
		/// <returns>True if the user was deleted.</returns>
		public virtual bool DeleteUser() {
			if (System.Web.Security.Membership.DeleteUser(UserName)) {
				UserName = null;
				return true;
			}
			Log.Write("Administration", "Failed to delete user " + UserName + ".");
			return false;
		}
		/// <summary>
		/// Gets the MembershipUser for this person.
		/// </summary>
		[NotMapped]
		public virtual MembershipUser User { get { return System.Web.Security.Membership.GetUser(UserName); } }
		/// <summary>
		/// Updates the MembershipUser that corresponds to this person.
		/// </summary>
		/// <returns></returns>
		public virtual bool UpdateUser() {
			var u = User;
			bool res = true;
			u.Email = Email;
			System.Web.Security.Membership.UpdateUser(u);
			return res;
		}
		/// <summary>
		/// Unlock a locked out user.
		/// </summary>
		public void Unlock() {
			Persons.Unlock(User);
		}

		// Roles

		/// <summary>
		/// Returns the user roles for this person.
		/// </summary>
		[NotMapped]
		public virtual string[] Roles { get { return System.Web.Security.Roles.GetRolesForUser(UserName); } }
		/// <summary>
		/// Adds this person to the supplied comma or semicolon delimited list of roles.
		/// </summary>
		/// <param name="roleNames"></param>
		public virtual void AddToRoles(string roleNames) { System.Web.Security.Roles.AddUserToRoles(UserName, roleNames.SplitList(',', ';').ToArray()); }
		/// <summary>
		/// Removes this person from the supplied comma or semicolon delimited list of roles.
		/// </summary>
		/// <param name="roleNames"></param>
		public virtual void RemoveFromRoles(string roleNames) { System.Web.Security.Roles.RemoveUserFromRoles(UserName, roleNames.SplitList(',', ';').ToArray()); }
		/// <summary>
		/// True if this person is in the supplied role.
		/// </summary>
		/// <param name="role">The role to check for.</param>
		/// <returns> True if this person is in the supplied role.</returns>
		public virtual bool IsInRole(string role) { return System.Web.Security.Roles.IsUserInRole(UserName, role); }


		// User files

		/// <summary>
		/// The persons home directory, used by the filemanager.
		/// </summary>
		public virtual string HomePath { get { return "~/silversite/users/" + FirstName + " " + LastName; } }
		/// <summary>
		/// Creates the persons home folder.
		/// </summary>
		public void CreateHomeFolder() { if (!Files.DirectoryExists(HomePath)) Files.CreateDirectory(HomePath); }
		/// <summary>
		/// Deletes the persons home folder.
		/// </summary>
		public void DeleteHomeFolder() { Files.Delete(HomePath); }
		/// <summary>
		/// Returns an absolute path. When the person has full file system acesss it returns the supplied path, otherwise a path relative to the home folder.
		/// </summary>
		/// <param name="relativePath">A relative path</param>
		/// <returns>Returns an absolute path. When the person has full file system acesss it returns the supplied path, otherwise a path relative to the home folder.</returns>
		public string AbsolutePath(string relativePath) {
			if (IsInRole(Files.FullFileSystemAccessRole)) {
				if (relativePath.StartsWith("~")) return relativePath;
				if (relativePath.StartsWith("/")) return "~" + relativePath;
				else return "~/" + relativePath;
			} else return Paths.Combine(HomePath, relativePath);
		}
	}

	/// <summary>
	/// A class that serves as an interface to the database for persistent persons.
	/// </summary>
	public class Persons: Web.IAutostart {

	/*	public class Context: Documents.Context {
			public Context() : base() { }
			public Context(Data.Database db) : base(db) { }
		}
	*/
		/// <summary>
		/// The person for the current logged in user. This method is fast, it ususally does not access the database, because it does caching in the session object.
		/// </summary>
		public static Person Current {
			get {
				try {
					if (HttpContext.Current != null && HttpContext.Current.Session != null) {
						var sess = (Person)HttpContext.Current.Session["Persons.Current"];
						if (sess != null) return sess;
					}
					string username = null;
					if (HttpContext.Current != null && HttpContext.Current.User.Identity.IsAuthenticated) username = HttpContext.Current.User.Identity.Name;
					if (username == null) return null;
					Person p = null;
					using (var db = new Silversite.Context()) {
						//p = db.Persons.FirstOrDefault(u => u.UserName == username);
					}
					if (HttpContext.Current != null && HttpContext.Current.Session != null) {
						HttpContext.Current.Session["Persons.Current"] = p;
					}
					return p;
				} catch { return null; }
			}
			internal set {
				try {
					if (HttpContext.Current != null && HttpContext.Current.Session != null) {
						HttpContext.Current.Session["Persons.Current"] = value;
					}
				} catch { }
			}
		}
		/// <summary>
		/// Find the person that corresponds to the supplied username.
		/// </summary>
		/// <param name="username">The username.</param>
		/// <returns>The person that correspond to the username.</returns>
		public static Person Find(string username) { if (Current != null && Current.UserName == username) return Current; using (var db = new Silversite.Context()) { return null; } }
		/// <summary>
		/// Find the person that corresponds to the supplied username.
		/// </summary>
		/// <param name="db">A Database Context.</param>
		/// <param name="username">The username.</param>
		/// <returns>The person that correspond to the username.</returns>
		public static Person Find(Silversite.Context db, string username) { if (Current != null && Current.UserName == username) return Current; return null; }

		static Person defaultPerson = new Person();
		/// <summary>
		/// Returns a person with the default settings (for example editor settings.)
		/// </summary>
		public static Person Default { get { return defaultPerson; } set { defaultPerson = value; } }

		readonly static TimeSpan UnlockTime = TimeSpan.FromMinutes(4);
		readonly static TimeSpan minLockedOutTime = TimeSpan.FromMinutes(4);
		static Timer unlockTimer = null;
		static object TimerLock = new object();
		/// <summary>
		/// Unlocks all locked out users. This method gets called every 4 minutes.
		/// </summary>
		/// <param name="state"></param>
		public static void UnlockAll() {
			var users = System.Web.Security.Membership.GetAllUsers().OfType<MembershipUser>().Where(u => u.IsLockedOut && (u.LastLockoutDate < DateTime.Now - minLockedOutTime));
			foreach (var u in users) u.UnlockUser();
		}
		/// <summary>
		/// Unlock a specific user.
		/// </summary>
		/// <param name="UserName">The username.</param>
		public static void Unlock(string UserName) {
			var user = System.Web.Security.Membership.GetUser(UserName);
			Unlock(user);
		}
		/// <summary>
		/// Unlocks a MembershipUser
		/// </summary>
		/// <param name="user">The user.</param>
		public static void Unlock(MembershipUser user) {
			if (user.IsLockedOut) {
				user.UnlockUser();
				Log.Write("Administration", "Unlocked user " + user.UserName);
			}
		}
		/// <summary>
		/// Disposes the timer used to unlock users.
		/// </summary>
		public void Shutdown() {
			lock (TimerLock) {
				if (unlockTimer != null) {
					unlockTimer.Dispose();
					unlockTimer = null;
				}
			}
		}
		/// <summary>
		/// Creates a timer that unlocks users every 4 minutes.
		/// </summary>
		public void Startup() {
			lock (TimerLock) {
				//if (unlockTimer == null) unlockTimer = Tasks.Recurring(UnlockTime, UnlockAll);
			}
		}
	}
}