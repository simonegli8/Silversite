using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

using Silversite.Web.UI;

namespace Silversite.Providers.FileManager.UserControls {

	public partial class Content: ContentPresenter<FileSystemInfo, Web.UI.FileManager> {

		public Content(): base() { }
		public Content(FileSystemInfo path, Web.UI.FileManager filemanager): this() { Item = path; Container = filemanager; }

		protected void Page_Load(object sender, EventArgs e) {

		}

		protected override void CreateChildControls() {
			base.CreateChildControls();

			if (Item is FileInfo) {
				icons.Visible = false;
				details.Visible = false;
				//TODO file view of FileManager Content
			} else {

				FileSystemInfo[] infos;
				if (((DirectoryInfo)Item).Exists) {
					infos = ((DirectoryInfo)Item).GetFileSystemInfos();
				} else {
					infos = new FileSystemInfo[0];
				}

				if (Container.View == Web.UI.FileManager.Views.List) {
					icons.Visible = false;
					details.Visible = true;
					foreach (var info in infos) {
						var item = new Silversite.Providers.FileManager.Item(info, Container);
						iconsContent.Controls.Add(item);
					}
				} else {
					details.Visible = false;
					icons.Visible = true;
					foreach (var info in infos) {
						var item = new Silversite.Providers.FileManager.Item(info, Container);
						detailsContent.Controls.Add(item);
					}
				}
			}

		}
	}
}