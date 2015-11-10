using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Silversite.Web;

namespace Silversite.silversite.test {
	public partial class LogTest: System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
		}

		void RaiseException() {
			throw new InvalidOperationException("This is an Exception.");
		}

		protected void WriteClick(object sender, EventArgs a) {
			Services.Log.Enabled = true;
			Exception e = null;
			if (ex.Checked) {
				try {
					RaiseException();
				} catch (Exception e2) {
					e = e2;
				}
				Services.Log.Error(msg.Text, e);
			} else {
				Services.Log.Write(cat.Text, msg.Text);
			}
			if (im.Checked) Services.Log.Flush();
			
		}

		protected void LogClick(object sender, EventArgs e) {
			Services.Log.Enabled = true;
			Response.Redirect("~/Silversite/Admin/log.aspx");
		}
	}
}
