using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

namespace Silversite.Providers.FileManager.UserControls {
	public partial class FileManager : System.Web.UI.UserControl {

		Silversite.Web.UI.FileManager container;
		public Silversite.Web.UI.FileManager Container {
			get {
				return container;
			}
			set {
				menu.Container =
				hierarchy.Container =
				content.Container =
				container = value;
			}
		}

		FileSystemInfo item;
		public FileSystemInfo Item {
			get { return item; }
			set {
				menu.Item =
				hierarchy.Item =
				content.Item =
				item = value;
			}
		}

		protected void Page_Load(object sender, EventArgs e) {	
		}
	}
}