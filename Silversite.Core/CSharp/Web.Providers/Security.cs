using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Runtime.CompilerServices;
using System.Net;

namespace Silversite.Web.Providers {

	public static class Security {

		public static Guid CurrentUserId {
			get { return GetUserId(CurrentUserName); }
		}

		public static string CurrentUserName {
			get { return Context.User.Identity.Name; }
		}

		public static bool HasUserId {
			get { return !(CurrentUserId == Guid.Empty); }
		}

		public static bool IsAuthenticated {
			get { return Request.IsAuthenticated; }
		}

		private static HttpContextBase Context {
			get { try { return new HttpContextWrapper(HttpContext.Current); } catch { return null; } }
		}

		private static HttpRequestBase Request {
			get { return Context.Request; }
		}

		private static HttpResponseBase Response {
			get { return Context.Response; }
		}

		private static ExtendedMembershipProvider VerifyProvider() {
			ExtendedMembershipProvider provider = System.Web.Security.Membership.Provider as ExtendedMembershipProvider;
			if (provider == null) {
				throw new InvalidOperationException("Provider Is Not ExtendedMembershipProvider");
			}
			return provider;
		}

		public static bool Login(string userNameOrEmail, string password, bool persistCookie = false) {
			ExtendedMembershipProvider provider = VerifyProvider();
			var success = provider.ExtendedValidateUser(userNameOrEmail, password);
			if (!(string.IsNullOrEmpty(success))) {
				FormsAuthentication.SetAuthCookie(success, persistCookie);
				return true;
			} else {
				return false;
			}
		}

		public static void Logout() {
			VerifyProvider();
			FormsAuthentication.SignOut();
		}

		public static bool ChangePassword(string userName, string currentPassword, string newPassword) {
			VerifyProvider();
			bool success = false;
			try {
				var currentUser = System.Web.Security.Membership.GetUser(userName, true);
				success = currentUser.ChangePassword(currentPassword, newPassword);
			} catch (ArgumentException) {

			}
			return success;
		}

		public static bool ConfirmAccount(string accountConfirmationToken) {
			ExtendedMembershipProvider provider = VerifyProvider();
			return provider.ConfirmAccount(accountConfirmationToken);
		}

		public static string CreateAccount(string userName, string password, string email, bool requireConfirmationToken = false) {
			ExtendedMembershipProvider provider = VerifyProvider();
			return provider.CreateAccount(userName, password, email, requireConfirmationToken);
		}

		public static string GeneratePasswordResetToken(string userName, int tokenExpirationInMinutesFromNow = 1440) {
			ExtendedMembershipProvider provider = VerifyProvider();
			return provider.GeneratePasswordResetToken(userName, tokenExpirationInMinutesFromNow);
		}

		public static bool UserExists(string userName) {
			VerifyProvider();
			return System.Web.Security.Membership.GetUser(userName) != null;
		}

		public static Guid GetUserId(string userName) {
			VerifyProvider();
			MembershipUser user = System.Web.Security.Membership.GetUser(userName);
			if (user == null) {
				return Guid.Empty;
			}
			return Guid.Parse(user.ProviderUserKey.ToString());
		}

		public static Guid GetUserIdFromPasswordResetToken(string token) {
			ExtendedMembershipProvider provider = VerifyProvider();
			return provider.GetUserIdFromPasswordResetToken(token);
		}

		public static bool IsCurrentUser(string userName) {
			VerifyProvider();
			return string.Equals(CurrentUserName, userName, StringComparison.OrdinalIgnoreCase);
		}

		public static bool IsConfirmed(string userName) {
			ExtendedMembershipProvider provider = VerifyProvider();
			return provider.IsConfirmed(userName);
		}

		private static bool IsUserLoggedOn(Guid userId) {
			VerifyProvider();
			return CurrentUserId == userId;
		}

		public static void RequireAuthenticatedUser() {
			VerifyProvider();
			var user = Context.User;
			if (user == null || !user.Identity.IsAuthenticated) {
				Response.SetStatus(HttpStatusCode.Unauthorized);
			}
		}

		public static void RequireUser(Guid userId) {
			VerifyProvider();
			if (!IsUserLoggedOn(userId)) {
				Response.SetStatus(HttpStatusCode.Unauthorized);
			}
		}

		public static void RequireUser(string userName) {
			VerifyProvider();
			if (!string.Equals(CurrentUserName, userName, StringComparison.OrdinalIgnoreCase)) {
				Response.SetStatus(HttpStatusCode.Unauthorized);
			}
		}

		public static void RequireRoles(params string[] ArrayOfRoles) {
			VerifyProvider();
			foreach (string role in ArrayOfRoles) {
				if (!Roles.IsUserInRole(CurrentUserName, role)) {
					Response.SetStatus(HttpStatusCode.Unauthorized);
					return;
				}
			}
		}

		public static bool ResetPassword(string passwordResetToken, string newPassword) {
			ExtendedMembershipProvider provider = VerifyProvider();
			return provider.ResetPasswordWithToken(passwordResetToken, newPassword);
		}

		public static bool IsAccountLockedOut(string userName, int allowedPasswordAttempts, int intervalInSeconds) {
			VerifyProvider();
			return IsAccountLockedOut(userName, allowedPasswordAttempts, TimeSpan.FromSeconds(intervalInSeconds));
		}

		public static bool IsAccountLockedOut(string userName, int allowedPasswordAttempts, TimeSpan interval) {
			ExtendedMembershipProvider provider = VerifyProvider();
			return IsAccountLockedOutInternal(provider, userName, allowedPasswordAttempts, interval);
		}

		private static bool IsAccountLockedOutInternal(ExtendedMembershipProvider provider, string userName, int allowedPasswordAttempts, TimeSpan interval) {
			return (provider.GetUser(userName, false) != null && provider.GetPasswordFailuresSinceLastSuccess(userName) > allowedPasswordAttempts && provider.GetLastPasswordFailureDate(userName).Add(interval) > DateTime.UtcNow);
		}

		public static int GetPasswordFailuresSinceLastSuccess(string userName) {
			ExtendedMembershipProvider provider = VerifyProvider();
			return provider.GetPasswordFailuresSinceLastSuccess(userName);
		}

		public static DateTime GetCreateDate(string userName) {
			ExtendedMembershipProvider provider = VerifyProvider();
			return provider.GetCreateDate(userName);
		}

		public static DateTime GetPasswordChangedDate(string userName) {
			ExtendedMembershipProvider provider = VerifyProvider();
			return provider.GetPasswordChangedDate(userName);
		}

		public static DateTime GetLastPasswordFailureDate(string userName) {
			ExtendedMembershipProvider provider = VerifyProvider();
			return provider.GetLastPasswordFailureDate(userName);
		}

		public static void SetStatus(this HttpResponseBase response, HttpStatusCode httpStatusCode) {
			SetStatus(response, (int)httpStatusCode);
		}

		public static void SetStatus(this HttpResponseBase response, int httpStatusCode) {
			response.StatusCode = httpStatusCode;
			response.End();
		}
		public static bool IsEmpty(string value) {
			return string.IsNullOrEmpty(value);
		}
	}
}