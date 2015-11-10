using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Silversite.silversite.test.master {

	public partial class TestMaster: System.Web.UI.MasterPage {

		public void Page_Load(object sender, EventArgs e) {
			if (!IsPostBack) {
				var info = Services.Files.DirectoryInfoVirtual("~/Silversite/Test");
				var files = info.Files.OfType<System.Web.Hosting.VirtualFile>().Select(vf => new { Name = vf.Name, Path = Services.Paths.Normalize(vf.VirtualPath) }).Where(vf => !vf.Name.EndsWith(".cs")).ToList();
				fileswitch.DataSource = files;
				fileswitch.DataTextField = "Name";
				fileswitch.DataValueField = "Path";
				fileswitch.DataBind();
				fileswitch.SelectedValue = Request.AppRelativeCurrentExecutionFilePath;

				if (testfilelist != null) {
					testfilelist.DataSource = files;
					testfilelist.DataBind();
				}
			}
		}

		public void FileSwitchChanged(object sender, EventArgs e) {
			if (fileswitch.SelectedValue != Request.AppRelativeCurrentExecutionFilePath) {
				Response.Redirect(fileswitch.SelectedValue);
			}
		}
	}
}
