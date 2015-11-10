using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Silversite.silversite.admin {
	public partial class InstallWizard: System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {

		}

		protected void NextClick(object sender, EventArgs e) {
			switch (wiz.ActiveStep.ID) {
			case "dbstep":
				var dbtype = wiz.FindControl("dbtype") as DropDownList;
				switch (dbtype.SelectedValue) {
				case "MSSql":
					break;
				case "Sql2005":
					break;
				case "MySql":
					break;
				}
				break;
			}
		}

		


	}
}