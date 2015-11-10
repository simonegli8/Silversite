using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Silversite.SilversiteDevelop.Web.UI.Pages {

	public partial class db: System.Web.UI.Page {

		protected void Page_Load(object sender, EventArgs e) {
			//Response.Redirect("~/Silversite/Admin/db.aspx");
		}

		protected void CreateDb(object sender, EventArgs e) {
			Data.Database.Default.CreateIfNotExists();
		}

		protected void DropDb(object sender, EventArgs e) {
			Data.Database.Default.Delete();
		}


	}
}