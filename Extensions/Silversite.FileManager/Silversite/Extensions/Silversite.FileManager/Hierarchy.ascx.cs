using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

namespace Silversite.Providers.FileManager.UserControls {

	public partial class Hierarchy : Silversite.Web.UI.HierarchyPresenter<FileSystemInfo, Web.UI.FileManager> {
		
		public Hierarchy(): base() { }
		public Hierarchy(FileSystemInfo path, Web.UI.FileManager filemanager): this() { Item = path; Container = filemanager; }

		protected void Page_Load(object sender, EventArgs e) {

		}
	}
}