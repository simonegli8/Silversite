using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sys = System.Web.Security;
using System.Text.RegularExpressions;
using System.Text;
using Silversite.Web.Providers;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Security;

namespace Silversite.Web.Providers {

	public class User {

		public User() { CreationDate = PasswordChangedDate = LastActivityDate = DateTime.Now; }

		[Key]
		public Guid UserId { get; set; }
		[Required, MaxLength(128)]
		public virtual string UserName { get; set; }
		[Required]
		public virtual Application Application { get; set; }
		[MaxLength(128), DataType(DataType.EmailAddress)]
		public virtual string Email { get; set; }
		[MaxLength(128), DataType(DataType.Password)]
		public virtual string ClearPassword { get; set; }
		[MaxLength(128), DataType(DataType.Password)]
		public virtual string HashedPassword { get; set; }
		[MaxLength(255)]
		public byte[] EncryptedPassword { get; set; }
		[Required]
		public virtual bool IsConfirmed { get; set; }
		[Required]
		public virtual int PasswordFailuresSinceLastSuccess { get; set; }
		[Required]
		public virtual int AnswerFailuresSinceLastSuccess { get; set; }
		public virtual DateTime? LastPasswordFailureDate { get; set; }
		public virtual DateTime? LastAnswerFailureDate { get; set; }
		[MaxLength(64)]
		public virtual string ConfirmationToken { get; set; }
		[Required]
		public virtual DateTime CreationDate { get; set; }
		public virtual DateTime PasswordChangedDate { get; set; }
		[MaxLength(64)]
		public virtual string PasswordVerificationToken { get; set; }
		public virtual DateTime? PasswordVerificationTokenExpirationDate { get; set; }
		public virtual DateTime? LastLockoutDate { get; set; }
		public virtual DateTime? LastLoginDate { get; set; }
		[Required]
		public virtual DateTime LastActivityDate { get; set; }
		[Required]
		public bool IsApproved { get; set; }
		[Required]
		public bool IsDeleted { get; set; }
		[MaxLength(255)]
		public string Comment { get; set; }
		[MaxLength(255)]
		public string PasswordQuestion { get; set; }
		[MaxLength(255)]
		public string PasswordAnswer { get; set; }

		public virtual ICollection<Role> Roles { get; set; }

		private bool isLockedOut;

		public virtual bool IsLockedOut {
			get { return isLockedOut; }
			set {
				LastActivityDate = DateTime.UtcNow;
				isLockedOut = value;
				if (isLockedOut) LastLockoutDate = DateTime.UtcNow;
			}
		}

		public virtual bool CheckPassword(string password) {
			var same = false;
			if (!string.IsNullOrEmpty(ClearPassword)) same = password == ClearPassword;
			else if (EncryptedPassword != null) same = password == Services.Crypto.DecryptPassword(EncryptedPassword);
			else same = Services.Crypto.VerifyHashedPassword(HashedPassword, password);

			if (same) PasswordFailuresSinceLastSuccess = 0;
			else {
				int failures = PasswordFailuresSinceLastSuccess;
				if (failures != -1) {
					if (DateTime.UtcNow - LastPasswordFailureDate > Sys.Membership.PasswordAttemptWindow.Minutes()) {
						PasswordFailuresSinceLastSuccess = 0;
						LastPasswordFailureDate = DateTime.UtcNow;
					}
					PasswordFailuresSinceLastSuccess++;

					if (PasswordFailuresSinceLastSuccess > Sys.Membership.MaxInvalidPasswordAttempts) {
						IsLockedOut = true;
					}
				}
			}

			return same && !IsLockedOut && IsApproved && IsConfirmed && !IsDeleted;
		}

		public virtual bool CheckAnswer(string answer) {
			var same = PasswordAnswer == answer;

			if (same) AnswerFailuresSinceLastSuccess = 0;
			else {
				int failures = AnswerFailuresSinceLastSuccess;
				if (failures != -1) {
					if (DateTime.UtcNow - LastAnswerFailureDate > Sys.Membership.PasswordAttemptWindow.Minutes()) {
						AnswerFailuresSinceLastSuccess = 0;
						LastAnswerFailureDate = DateTime.UtcNow;
					}
					AnswerFailuresSinceLastSuccess++;

					if (AnswerFailuresSinceLastSuccess > Sys.Membership.MaxInvalidPasswordAttempts) {
						IsLockedOut = true;
					}
				}
			}

			return same && !IsLockedOut && IsApproved && IsConfirmed;
		}


		public virtual string GetPassword() {
			if (!string.IsNullOrEmpty(ClearPassword)) return ClearPassword;
			else if (EncryptedPassword != null) return Services.Crypto.DecryptPassword(EncryptedPassword);
			else return null;
		}

		public virtual void SetPassword(MembershipProvider provider, string password) {
			LastActivityDate = DateTime.UtcNow;

			switch (provider.PasswordFormat) {
				case MembershipPasswordFormat.Clear:
					ClearPassword = password;
					HashedPassword = null;
					EncryptedPassword = null;
					break;
				case MembershipPasswordFormat.Encrypted:
					EncryptedPassword = Services.Crypto.EncryptPassword(password);
					ClearPassword = null;
					HashedPassword = null;
					break;
				case MembershipPasswordFormat.Hashed:
					HashedPassword = Services.Crypto.HashPassword(password);
					ClearPassword = null;
					EncryptedPassword = null;
					break;
			}
			PasswordChangedDate = DateTime.UtcNow;
		}

		public MembershipUser MembershipUser {
			get {
				return new MembershipUser(System.Web.Security.Membership.Provider.Name, UserName, UserId, Email, PasswordQuestion, Comment, IsApproved, IsLockedOut, CreationDate,
					LastLoginDate ?? DateTime.MinValue, LastActivityDate, PasswordChangedDate, LastLockoutDate ?? DateTime.MinValue);
			}
		}
	}

	public class MembershipProvider: ExtendedMembershipProvider {

		public override string ApplicationName { get { return Application.Current<MembershipProvider>().Name; } set { Application.Set<MembershipProvider>(value); } }

		bool enablePasswordReset = true;
		bool enablePasswordRetrieval = false;
		int maxInvalidPasswordAttempts = int.MaxValue;
		int minRequiredNonAlphanumericCharacters = 0;
		int minRequiredPasswordLength = 6;
		int passwordAttemptWindow = 4;
		System.Web.Security.MembershipPasswordFormat passwordFormat = Sys.MembershipPasswordFormat.Hashed;
		string passwordStrengthRegularExpression = null;
		bool requiresQuestionAndAnswer = false;
		bool requiresUniqueEmail = true;
		public override bool EnablePasswordReset { get { return enablePasswordReset; } }
		public override bool EnablePasswordRetrieval { get { return enablePasswordRetrieval && PasswordFormat != Sys.MembershipPasswordFormat.Hashed; } }
		public override int MaxInvalidPasswordAttempts { get { return maxInvalidPasswordAttempts; } }
		public override int MinRequiredNonAlphanumericCharacters { get { return minRequiredNonAlphanumericCharacters; } }
		public override int MinRequiredPasswordLength { get { return minRequiredPasswordLength; } }
		public override int PasswordAttemptWindow { get { return passwordAttemptWindow; } }
		public override System.Web.Security.MembershipPasswordFormat PasswordFormat { get { return passwordFormat; } }

		public override string PasswordStrengthRegularExpression { get { return passwordStrengthRegularExpression; } }
		public override bool RequiresQuestionAndAnswer { get { return requiresQuestionAndAnswer; } }
		public override bool RequiresUniqueEmail { get { return requiresUniqueEmail; } }


		public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config) {
			if (config == null) {
				throw new ArgumentNullException("config");
			}
			if (string.IsNullOrEmpty(name)) {
				name = "SilversiteMembershipProvider";
			}
			if (string.IsNullOrEmpty(config["description"])) {
				config.Remove("description");
				config.Add("description", "Silversite Membership Provider");
			}

			base.Initialize(name, config);

			ApplicationName = config["applicationName"];

			bool.TryParse(config["enablePasswordReset"], out enablePasswordReset);
			bool.TryParse(config["enablePasswordRetrieval"], out enablePasswordRetrieval);
			int.TryParse(config["maxInvalidPasswordAttempts"], out maxInvalidPasswordAttempts);
			int.TryParse(config["minRequiredNonAlphanumericCharacters"], out minRequiredNonAlphanumericCharacters);
			int.TryParse(config["minRequiredPasswordLength"], out minRequiredPasswordLength);

			int.TryParse(config["passwordAttemptWindow"], out passwordAttemptWindow);
			Enum.TryParse<Sys.MembershipPasswordFormat>(config["passwordFormat"], out passwordFormat);
			passwordStrengthRegularExpression = config["passwordStrengthRegularExpression"];
			bool.TryParse(config["requiresQuestionAndAnswer"], out requiresQuestionAndAnswer);
			bool.TryParse(config["requiresUniqueEmail"], out requiresUniqueEmail);
		}

		#region "Main Functions"

		public override string CreateAccount(string userName, string password, string email, bool requireConfirmationToken) {

			if (string.IsNullOrEmpty(userName)) {
				throw new Sys.MembershipCreateUserException(Sys.MembershipCreateStatus.InvalidUserName);
			}
			/*if (string.IsNullOrEmpty(email)) {
				throw new Sys.MembershipCreateUserException(Sys.MembershipCreateStatus.InvalidEmail);
			}*/
			if (!ValidPassword(password)) throw new Sys.MembershipCreateUserException(Sys.MembershipCreateStatus.InvalidPassword);

			using (var db = new Context()) {
				try {
					var user = db.AppActiveUsers<MembershipProvider>().FirstOrDefault(u => u.UserName == userName);

					if (user != null && user.IsDeleted) {
						db.Users.Remove(user);
						user = null;
					}

					User emailuser = null;
					if (!string.IsNullOrEmpty(email)) {
						emailuser = db.AppActiveUsers<MembershipProvider>().FirstOrDefault(u => u.Email == email);
					}
					if (user != null) {
						throw new Sys.MembershipCreateUserException(Sys.MembershipCreateStatus.DuplicateUserName);
					}
					if (emailuser != null && RequiresUniqueEmail) {
						throw new Sys.MembershipCreateUserException(Sys.MembershipCreateStatus.DuplicateEmail);
					}
					string token = null;
					if (requireConfirmationToken) {
						token = Services.Crypto.GenerateToken();
					}

					User NewUser = new User {
						UserId = Guid.NewGuid(),
						UserName = userName,
						Application = Application.Current<MembershipProvider>(db),
						IsConfirmed = !requireConfirmationToken,
						IsApproved = true,
						Email = email,
						ConfirmationToken = token,
						CreationDate = DateTime.UtcNow,
						PasswordChangedDate = DateTime.UtcNow,
						PasswordFailuresSinceLastSuccess = 0,
						AnswerFailuresSinceLastSuccess = 0,
						LastPasswordFailureDate = null,
						LastAnswerFailureDate = null,
						LastActivityDate = DateTime.UtcNow,
						LastLoginDate = null,
						LastLockoutDate = null,
						Comment = null
					};

					NewUser.SetPassword(this, password);

					db.Users.Add(NewUser);
					db.SaveChanges();
					return token;
				} catch {
					throw new Sys.MembershipCreateUserException(Sys.MembershipCreateStatus.DuplicateUserName);
				}
			}
		}

		public override bool ConfirmAccount(string accountConfirmationToken) {
			if (string.IsNullOrEmpty(accountConfirmationToken)) {
				throw CreateArgumentNullOrEmptyException("accountConfirmationToken");
			}
			using (var db = new Context()) {
				try {
					var row = db.AppActiveUsers<MembershipProvider>().FirstOrDefault(u => u.ConfirmationToken == accountConfirmationToken);
					if (row != null) {
						row.IsConfirmed = true;
						// row.LastActivityDate = DateTime.UtcNow;
						db.SaveChanges();
						return true;
					}
				} catch { }
				return false;
			}
		}

		public override bool ChangePassword(string userName, string oldPassword, string newPassword) {
			if (string.IsNullOrEmpty(userName)) {
				throw CreateArgumentNullOrEmptyException("userName");
			}
			if (string.IsNullOrEmpty(oldPassword)) {
				throw CreateArgumentNullOrEmptyException("oldPassword");
			}
			if (string.IsNullOrEmpty(newPassword)) {
				throw CreateArgumentNullOrEmptyException("newPassword");
			}
			if (!ValidPassword(newPassword)) return false;

			using (var db = new Context()) {
				try {
					var user = db.AppActiveUsers<MembershipProvider>().FirstOrDefault(u => u.UserName == userName);
					if (user == null) {
						return false;
					}


					bool validPassword = user.CheckPassword(oldPassword);
					if (validPassword) user.SetPassword(this, newPassword);

					// user.LastActivityDate = DateTime.UtcNow;

					db.SaveChanges();
					return validPassword;
				} catch {
					return false;
				}
			}
		}

		public override bool DeleteAccount(string userName) {
			if (string.IsNullOrEmpty(userName)) {
				throw CreateArgumentNullOrEmptyException("userName");
			}
			using (var db = new Context()) {
				try {
					var user = db.AppUsers<MembershipProvider>().FirstOrDefault(u => u.UserName == userName);
					if (user == null) {
						return false;
					}
					db.Users.Remove(user);
					db.SaveChanges();
				} catch { }
				return true;
			}
		}

		public override bool IsConfirmed(string userName) {
			if (string.IsNullOrEmpty(userName)) {
				throw CreateArgumentNullOrEmptyException("userName");
			}
			using (var db = new Context()) {
				var user = db.AppActiveUsers<MembershipProvider>().FirstOrDefault(u => u.UserName == userName);
				if (user == null) {
					return false;
				}

				// user.LastActivityDate = DateTime.UtcNow;

				if (user.IsConfirmed) {
					return true;
				} else {
					return false;
				}
			}
		}

		public override string GeneratePasswordResetToken(string userName, int tokenExpirationInMinutesFromNow) {
			if (string.IsNullOrEmpty(userName)) {
				throw CreateArgumentNullOrEmptyException("userName");
			}
			using (var db = new Context()) {
				try {
					var user = db.AppActiveUsers<MembershipProvider>().FirstOrDefault(u => u.UserName == userName);
					if (user == null) {
						throw new InvalidOperationException(string.Format("User not found: {0}", userName));
					}
					if (!user.IsConfirmed) {
						throw new InvalidOperationException(string.Format("User not found: {0}", userName));
					}
					string token = null;
					if (user.PasswordVerificationTokenExpirationDate > DateTime.UtcNow) {
						token = user.PasswordVerificationToken;
					} else {
						token = Services.Crypto.GenerateToken();
					}
					user.PasswordVerificationToken = token;
					user.PasswordVerificationTokenExpirationDate = DateTime.UtcNow.AddMinutes(tokenExpirationInMinutesFromNow);

					// user.LastActivityDate = DateTime.UtcNow;
					db.SaveChanges();
					return token;
				} catch {
					throw new InvalidOperationException(string.Format("User not found: {0}", userName));
				}
			}
		}

		public override bool ResetPasswordWithToken(string token, string newPassword) {
			if (string.IsNullOrEmpty(newPassword)) {
				throw CreateArgumentNullOrEmptyException("newPassword");
			}
			using (var db = new Context()) {
				try {
					var user = db.AppActiveUsers<MembershipProvider>().FirstOrDefault(u => u.PasswordVerificationToken == token && u.PasswordVerificationTokenExpirationDate > DateTime.UtcNow);
					if (user != null) {
						user.SetPassword(this, newPassword);
						user.PasswordChangedDate = DateTime.UtcNow;
						user.PasswordVerificationToken = null;
						user.PasswordVerificationTokenExpirationDate = null;
						// user.LastActivityDate = DateTime.UtcNow;
						db.SaveChanges();
						return true;
					}
				} catch { }
				return false;
			}
		}

		public override string ExtendedValidateUser(string userNameOrEmail, string password) {
			if (string.IsNullOrEmpty(userNameOrEmail)) {
				throw CreateArgumentNullOrEmptyException("userNameOrEmail");
			}
			if (string.IsNullOrEmpty(password)) {
				throw CreateArgumentNullOrEmptyException("password");
			}
			using (var db = new Context()) {
				try {
					User user = null;
					user = db.AppActiveUsers<MembershipProvider>().FirstOrDefault(u => u.UserName == userNameOrEmail);
					if (user == null) {
						user = db.AppActiveUsers<MembershipProvider>().FirstOrDefault(u => u.Email == userNameOrEmail);
					}
					if (user == null) {
						return string.Empty;
					}
					if (!user.IsConfirmed || !user.IsApproved) {
						return string.Empty;
					}

					bool verificationSucceeded = user.CheckPassword(password);

					user.LastActivityDate = DateTime.UtcNow;
					db.SaveChanges();
					if (verificationSucceeded) {
						return user.UserName;
					}
				} catch { }
				return string.Empty;
			}
		}

		private ArgumentException CreateArgumentNullOrEmptyException(string paramName) {
			return new ArgumentException(string.Format("Argument cannot be null or empty: {0}", paramName));
		}

		#endregion

		#region "Get Functions"

		public override System.DateTime GetPasswordChangedDate(string userName) {
			if (string.IsNullOrEmpty(userName)) {
				throw CreateArgumentNullOrEmptyException("userName");
			}
			using (var db = new Context()) {
				var user = db.AppActiveUsers<MembershipProvider>().FirstOrDefault(u => u.UserName == userName);
				if (user == null) {
					throw new InvalidOperationException(string.Format("User not found: {0}", userName));
				}
				return user.PasswordChangedDate;
			}
		}

		public override System.DateTime GetCreateDate(string userName) {
			if (string.IsNullOrEmpty(userName)) {
				throw CreateArgumentNullOrEmptyException("userName");
			}
			using (var db = new Context()) {
				var user = db.AppActiveUsers<MembershipProvider>().FirstOrDefault(u => u.UserName == userName);
				if (user == null) {
					throw new InvalidOperationException(string.Format("User not found: {0}", userName));
				}
				return user.CreationDate;
			}
		}

		public override int GetPasswordFailuresSinceLastSuccess(string userName) {
			if (string.IsNullOrEmpty(userName)) {
				throw CreateArgumentNullOrEmptyException("userName");
			}
			using (var db = new Context()) {
				var user = db.AppActiveUsers<MembershipProvider>().FirstOrDefault(u => u.UserName == userName);
				if (user == null) {
					throw new InvalidOperationException(string.Format("User not found: {0}", userName));
				}
				return user.PasswordFailuresSinceLastSuccess;
			}
		}

		public override System.Web.Security.MembershipUser GetUser(string userName, bool userIsOnline) {
			if (string.IsNullOrEmpty(userName)) {
				// throw CreateArgumentNullOrEmptyException("userName");
				return null;
			}
			using (var db = new Context()) {
				var user = db.AppActiveUsers<MembershipProvider>().FirstOrDefault(u => u.UserName == userName);
				if (user == null) {
					return null;
				}
				if (userIsOnline) {
					user.LastActivityDate = DateTime.UtcNow;
					db.SaveChanges();
				}
				return user.MembershipUser;
			}
		}

		public override System.Guid GetUserIdFromPasswordResetToken(string token) {
			if (string.IsNullOrEmpty(token)) {
				throw CreateArgumentNullOrEmptyException("token");
			}
			using (var db = new Context()) {
				var result = db.AppActiveUsers<MembershipProvider>().FirstOrDefault(u => u.PasswordVerificationToken == token);
				if (result != null) {
					return result.UserId;
				}
				return Guid.Empty;
			}
		}

		public override System.DateTime GetLastPasswordFailureDate(string userName) {
			if (string.IsNullOrEmpty(userName)) {
				throw CreateArgumentNullOrEmptyException("userName");
			}
			using (var db = new Context()) {
				var user = db.AppActiveUsers<MembershipProvider>().FirstOrDefault(u => u.UserName == userName);
				if (user == null) {
					throw new InvalidOperationException(string.Format("User not found: {0}", userName));
				}
				return user.LastPasswordFailureDate ?? DateTime.MinValue;
			}
		}

		#endregion

		public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer) {
			using (var db = new Context()) {
				try {
					var user = db.AppActiveUsers<MembershipProvider>().FirstOrDefault(u => u.UserName == username);
					if (user == null) {
						throw new InvalidOperationException(string.Format("User not found: {0}", username));
					}
					var validPassword = user.CheckPassword(password);
					if (validPassword) {
						user.PasswordQuestion = newPasswordQuestion;
						user.PasswordAnswer = newPasswordAnswer;
					}

					db.SaveChanges();
					return validPassword;
				} catch {
					return false;
				}
			}
		}

		public override Sys.MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out Sys.MembershipCreateStatus status) {

			status = Sys.MembershipCreateStatus.Success;

			if (string.IsNullOrEmpty(username)) status = Sys.MembershipCreateStatus.InvalidUserName;
			//if (string.IsNullOrEmpty(email)) status = Sys.MembershipCreateStatus.InvalidEmail;
			if (!ValidPassword(password)) status = Sys.MembershipCreateStatus.InvalidPassword;
			if (RequiresQuestionAndAnswer) {
				if (string.IsNullOrEmpty(passwordQuestion)) status = Sys.MembershipCreateStatus.InvalidQuestion;
				if (string.IsNullOrEmpty(passwordAnswer)) status = Sys.MembershipCreateStatus.InvalidAnswer;
			}

			if (status != Sys.MembershipCreateStatus.Success) return null;

			using (var db = new Context()) {

				try {
					var user = db.AppUsers<MembershipProvider>().FirstOrDefault(u => u.UserName == username);

					if (user != null && user.IsDeleted) {
						db.Users.Remove(user);
						user = null;
					}

					User emailuser = null;
					if (!string.IsNullOrEmpty(email)) {
						emailuser = db.AppActiveUsers<MembershipProvider>().FirstOrDefault(u => u.Email == email);
					}
					if (user != null) status = Sys.MembershipCreateStatus.DuplicateUserName;
					if (emailuser != null && RequiresUniqueEmail) status = Sys.MembershipCreateStatus.DuplicateEmail;
					if (status != Sys.MembershipCreateStatus.Success) return null;

					User NewUser = new User {
						UserId = Guid.NewGuid(),
						UserName = username,
						IsConfirmed = true,
						Email = email,
						ConfirmationToken = null,
						CreationDate = DateTime.UtcNow,
						PasswordChangedDate = DateTime.UtcNow,
						PasswordFailuresSinceLastSuccess = 0,
						AnswerFailuresSinceLastSuccess = 0,
						LastPasswordFailureDate = null,
						LastActivityDate = DateTime.UtcNow,
						LastLoginDate = null,
						LastLockoutDate = null,
						PasswordQuestion = passwordQuestion,
						PasswordAnswer = passwordAnswer,
						IsApproved = isApproved,
						Application = Application.Current<MembershipProvider>(db)
					};
					if (providerUserKey is Guid) NewUser.UserId = (Guid)providerUserKey;
					else NewUser.UserId = Guid.NewGuid();

					NewUser.SetPassword(this, password);

					db.Users.Add(NewUser);
					db.SaveChanges();
					return NewUser.MembershipUser;
				} catch (Exception ex) {
					status = Sys.MembershipCreateStatus.DuplicateUserName; return null;
				}
			}
		}

		public override bool DeleteUser(string username, bool deleteAllRelatedData) {
			if (string.IsNullOrEmpty(username)) {
				return false;
			}
			using (var db = new Context()) {
				try {
					var user = db.AppUsers<MembershipProvider>().FirstOrDefault(u => u.UserName == username);
					if (user != null) {
						if (!deleteAllRelatedData) user.IsDeleted = true;
						else db.Users.Remove(user);
						db.SaveChanges();
						return true;
					}
				} catch {
					return true;
				}
				return false;
			}
		}

		public override Sys.MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords) {
			using (var db = new Context()) {
				var users = db.AppActiveUsers<MembershipProvider>().Where(u => u.Email == emailToMatch).Skip(pageIndex).Take(pageSize);
				totalRecords = users.Count();
				var collection = new Sys.MembershipUserCollection();
				foreach (var u in users) collection.Add(u.MembershipUser);
				return collection;
			}
		}

		public override Sys.MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords) {
			using (var db = new Context()) {
				IEnumerable<User> users;
				if (usernameToMatch.ContainsAny(',', ';', '*')) {
					users = db.AppActiveUsers<MembershipProvider>().AsEnumerable().Where(u => Services.Paths.Match(usernameToMatch, u.UserName));
				} else {
					users = db.AppActiveUsers<MembershipProvider>().Where(u => u.UserName == usernameToMatch);
				}
				users = users.Skip(pageIndex).Take(pageSize);
				totalRecords = users.Count();
				var collection = new Sys.MembershipUserCollection();
				foreach (var u in users) collection.Add(u.MembershipUser);
				return collection;
			}
		}

		public override Sys.MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords) {
			using (var db = new Context()) {
				var users = db.AppActiveUsers<MembershipProvider>().OrderBy(u => u.UserName).Skip(pageIndex).Take(pageSize);
				totalRecords = users.Count();
				var collection = new Sys.MembershipUserCollection();
				foreach (var u in users) collection.Add(u.MembershipUser);
				return collection;
			}
		}

		public override int GetNumberOfUsersOnline() {
			using (var db = new Context()) {
				var now = DateTime.UtcNow;
				var window = Sys.Membership.UserIsOnlineTimeWindow.Minutes();
				return db.AppActiveUsers<MembershipProvider>().Count(u => now - u.LastActivityDate > window);
			}
		}

		public override string GetPassword(string username, string answer) {
			if (string.IsNullOrEmpty(username)) throw CreateArgumentNullOrEmptyException("username");
			using (var db = new Context()) {
				var user = db.AppActiveUsers<MembershipProvider>().FirstOrDefault(u => u.UserName == username);
				if (user != null) {
					if (user.CheckAnswer(answer)) return user.GetPassword();
				}
			}
			return null;
		}

		public override Sys.MembershipUser GetUser(object providerUserKey, bool userIsOnline) {
			if (!(providerUserKey is Guid)) throw new InvalidOperationException("No Guid providerUserKey");
			var uid = (Guid)providerUserKey;
			using (var db = new Context()) {
				var user = db.AppActiveUsers<MembershipProvider>().FirstOrDefault(u => u.UserId == uid);
				if (user != null) {
					if (userIsOnline) {
						user.LastActivityDate = DateTime.UtcNow;
						db.SaveChanges();
					}
					return user.MembershipUser;
				}
			}
			return null;
		}

		public override string GetUserNameByEmail(string email) {
			if (string.IsNullOrEmpty(email)) throw CreateArgumentNullOrEmptyException("email");
			using (var db = new Context()) {
				var user = db.AppActiveUsers<MembershipProvider>().FirstOrDefault(u => u.Email == email);
				if (user != null) return user.UserName;
			}
			return null;
		}

		public override string ResetPassword(string username, string answer) {
			if (string.IsNullOrEmpty(username)) throw CreateArgumentNullOrEmptyException("username");
			using (var db = new Context()) {
				var user = db.AppActiveUsers<MembershipProvider>().FirstOrDefault(u => u.UserName == username);
				if (user != null) {
					if (user.CheckAnswer(answer)) {
						var pwd = GeneratePassword();
						user.SetPassword(this, pwd);
						return pwd;
					}
				}
			}
			return null;
		}

		public override bool UnlockUser(string username) {
			if (string.IsNullOrEmpty(username)) throw CreateArgumentNullOrEmptyException("username");
			using (var db = new Context()) {
				var user = db.AppActiveUsers<MembershipProvider>().FirstOrDefault(u => u.UserName == username);
				if (user != null && user.IsLockedOut) {
					user.IsLockedOut = false;
					db.SaveChanges();
					return true;
				}
			}
			return false;
		}

		public override void UpdateUser(Sys.MembershipUser muser) {
			using (var db = new Context()) {
				User user = null;
				if (muser.ProviderUserKey is Guid) {
					var uid = (Guid)muser.ProviderUserKey;
					user = db.AppActiveUsers<MembershipProvider>().FirstOrDefault(u => u.UserId == uid);
				}
				if (user == null) user = db.AppActiveUsers<MembershipProvider>().FirstOrDefault(u => u.UserName == muser.UserName);
				if (user != null) {
					user.Comment = muser.Comment;
					user.CreationDate = muser.CreationDate;
					user.Email = muser.Email;
					user.IsApproved = muser.IsApproved;
					user.LastActivityDate = muser.LastLoginDate;
					user.LastLoginDate = muser.LastLoginDate;
					db.SaveChanges();
				}
			}
		}

		public override bool ValidateUser(string username, string password) {
			if (string.IsNullOrEmpty(username)) throw CreateArgumentNullOrEmptyException("username");
			using (var db = new Context()) {
				var user = db.AppActiveUsers<MembershipProvider>().FirstOrDefault(u => u.UserName == username);
				if (user != null) {
					user.LastActivityDate = DateTime.UtcNow;
					db.SaveChanges();
					return user.CheckPassword(password);
				}
			}
			return false;
		}

		#region "helper functions"
		public bool ValidPassword(string password) {
			bool valid = true;
			valid = !string.IsNullOrEmpty(password);
			valid &= password.Length >= MinRequiredPasswordLength;
			valid &= password.Count(ch => !char.IsLetterOrDigit(ch)) >= MinRequiredNonAlphanumericCharacters;
			valid &= string.IsNullOrEmpty(PasswordStrengthRegularExpression) || Regex.IsMatch(password, PasswordStrengthRegularExpression, RegexOptions.None);

			if (PasswordFormat == Sys.MembershipPasswordFormat.Hashed) {
				var hashedPassword = Services.Crypto.HashPassword(password);
				valid &= hashedPassword.Length <= 128;
			} else if (PasswordFormat == Sys.MembershipPasswordFormat.Encrypted) {
				var encryptedPassword = Services.Crypto.EncryptPassword(password);
				valid &= encryptedPassword.Length <= 255;
			} else if (PasswordFormat == Sys.MembershipPasswordFormat.Clear) {
				valid &= password.Length <= 128;
			}

			return valid;
		}

		public string GeneratePassword() {
			return Sys.Membership.GeneratePassword(MinRequiredPasswordLength + 2, MinRequiredNonAlphanumericCharacters);
		}
		#endregion
	}
}