using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Security;
using Sys = System.Web.Security;

namespace Silversite.Web.Providers {

	public class User {

		public User() { CreationDate = PasswordChangedDate = LastActivityDate = DateTime.Now; }

		[Key]
		public virtual Guid UserId { get; set; }
		[Required,	 MaxLength(128)]
		public virtual string UserName { get; set; }

		public int? ApplicationKey { get; set; }

		[ForeignKey("ApplicationKey")]
		public virtual Application Application { get; set; }
		[MaxLength(128)]
		public virtual string Domain { get; set; }
		[MaxLength(128), DataType(DataType.EmailAddress)]
		public virtual string Email { get; set; }
		[MaxLength(128), DataType(DataType.Password)]
		public virtual string ClearPassword { get; set; }
		[MaxLength(128), DataType(DataType.Password)]
		public virtual string HashedPassword { get; set; }
		[MaxLength(255)]
		public byte[] EncryptedPassword { get; set; }
		public virtual bool IsConfirmed { get; set; }
		public virtual int PasswordFailuresSinceLastSuccess { get; set; }
		public virtual int AnswerFailuresSinceLastSuccess { get; set; }
		public virtual Nullable<DateTime> LastPasswordFailureDate { get; set; }
		public virtual Nullable<DateTime> LastAnswerFailureDate { get; set; }
		[MaxLength(64)]
		public virtual string ConfirmationToken { get; set; }
		public virtual DateTime CreationDate { get; set; }
		public virtual DateTime PasswordChangedDate { get; set; }
		[MaxLength(64)]
		public virtual string PasswordVerificationToken { get; set; }
		public virtual Nullable<DateTime> PasswordVerificationTokenExpirationDate { get; set; }
		public virtual Nullable<DateTime> LastLockoutDate { get; set; }
		public virtual Nullable<DateTime> LastLoginDate { get; set; }
		public virtual DateTime LastActivityDate { get; set; }

		public bool IsApproved { get; set; }
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
}