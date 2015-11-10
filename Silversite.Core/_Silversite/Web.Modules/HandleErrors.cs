using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.IO;

namespace Silversite.Web {

	public class Exceptions: IHttpAutoModule {

		public event EventHandler Handle;
		public void Init(HttpApplication app) { app.Error += Error; }

		static List<DateTime> Errors = new List<DateTime>();

		public static Exception LastError { get; set; }

		public void Error(object sender, EventArgs e) {
			var server = ((HttpApplication)sender).Server;

			// record error timestamps to suppress to many errors, causing application pool to be suspended
			var now = DateTime.Now;
			lock (Errors) {
				DateTime tenminutesago = now.AddMinutes(-10);
				Errors.Insert(0, now);
				while (Errors.Count > 0 && Errors[Errors.Count - 1] < tenminutesago) Errors.RemoveAt(Errors.Count - 1);
			}
			// log error
			var ex = server.GetLastError();
			Services.Log.Error("Application Error", ex);
			LastError = ex;

			if (Errors.Count >= 4 && Errors[3] > now.AddMinutes(-6)) { // suppress error
				server.ClearError();
			}
		}

		public void Dispose() { }
	}
}
