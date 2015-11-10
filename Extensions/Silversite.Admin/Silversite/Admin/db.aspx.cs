using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Silversite;

namespace Silversite.silversite.admin {

	public partial class DbVersion: System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) { }

		/*
		protected void VersionChanged(IAsyncResult result) {
			Providers.Providers.Finished(this);
 		}
		*/
		protected void RowCommand(object sender, CommandEventArgs e) { }
		/*	var row = grid.Rows[int.Parse((string)e.CommandArgument)];
			var assembly = row.Cells[0].Text;
			var version = row.Cells[1].Controls.OfType<Web.UI.VersionsDropDownList>().FirstOrDefault().Value;
			if (e.CommandName == "Save") {
				if (Services.ProviderVersions.StoreVersion(assembly) != version) {
					monitor.Start(this);
					Services.ProviderVersions.BeginChangeVersion(assembly, version, VersionChanged, null);
				}
			} else if (e.CommandName == "Cancel") {
				row.Cells[1].Controls.OfType<Web.UI.VersionsDropDownList>().FirstOrDefault().SelectedValue = Services.ProviderVersions.StoreVersion(assembly).ToString();
			}
		} */
	}
}