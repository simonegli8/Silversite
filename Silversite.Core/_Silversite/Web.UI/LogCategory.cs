using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace Silversite.Web.UI {

	public class LogCategory: DropDownList {

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);
			if (!Page.IsPostBack) {
				using (var db = new Homesell.Data.Context()) {
					var list = db.LogMessages.Select(m => m.Category ?? "").Distinct().OrderBy(m => m).ToList();
					if (!list.Contains("")) list.Insert(0, "");
					DataSource = list;
					DataBind();
				}
			}
		}
	}

}