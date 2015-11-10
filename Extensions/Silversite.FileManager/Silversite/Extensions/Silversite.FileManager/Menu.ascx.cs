using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

namespace Silversite.Providers.FileManager.UserControls {

	public partial class Menu: Silversite.Web.UI.MenuPresenter<FileSystemInfo, Web.UI.FileManager> {
		
		public Menu(): base() { }
		public Menu(FileSystemInfo path, Web.UI.FileManager filemanager): this() { Item = path; Container = filemanager; }

		protected void Page_Load(object sender, EventArgs e) {
			path.Text = Services.Paths.Unmap(Item.FullName);
		}
	}
}