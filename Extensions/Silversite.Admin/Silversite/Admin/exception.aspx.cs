using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Silversite.Html;
using Silversite.Reflection;

namespace Silversite.Admin.silversite {
	public partial class exception: System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {

			var ex = Session["Silversite.Exception"] as Exception; 
			if (ex == null && Request.QueryString["log"] != null) {
				var key = int.Parse(Request.QueryString["log"]);
				using (var db = new Silversite.Context()) {
					var log = db.LogMessages.Find(key);
					if (log != null) {
						ex = log.Exception;
						Session["Silversite.Exception"] = ex;

						info.Property("Exception").Value = ex;
						info.Property("LogMessage").Value = log;
					}
				}
			}
			//if (Request.QueryString["raise"] != null) { // if query contains "raise" raise an exception found in the Session.


			/*} else {
				// If query does not contain raise, append raise an call the page again from the localhost, so the asp.net error page will show up for localhost (when customErrors is set to RemoteOnly in web.config).
				// Parse the result in a Html.Document and print the body.
				var url = Request.Url.ToString();
				if (url.Contains('?')) url += "&raise";
				else url += "?raise";
				var doc = Document.Open(new Uri(url));
				extext.Text = doc.Body.Text;
			} */
		}
	}
}