using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace Silversite {

	public static class PageExtensions {

		public static void RequireUser(this Page m, Services.Person user) {
			if (HttpContext.Current.User.Identity.Name != user.UserName) {
				FormsAuthentication.RedirectToLoginPage();
			}
		}

		public static void RequireRole(this Page m, string role) {
			if (!Roles.IsUserInRole(role)) FormsAuthentication.RedirectToLoginPage();
		}

	}
}