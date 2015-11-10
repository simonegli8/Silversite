using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Configuration;
using Silversite;

namespace Silversite.Services {

	public class AutoLogin: Web.IHttpAutoModule {

		public AutoLogin() : base() { }

		static string Login; 

		static AutoLogin() {
			Login = null;
			Login = new DevelopConfiguration().AutoLogin;
		}

		public void CheckLogin(object sender, EventArgs e) {
			if (TestServer.IsTestServer && !string.IsNullOrEmpty(Login)) FormsAuthentication.SetAuthCookie(Login, true);
		}

		public void Init(HttpApplication app) {
			//app.BeginRequest += CheckLogin;
			Modules.Remove(this);
		}

		public void Dispose() { }
	}
}
