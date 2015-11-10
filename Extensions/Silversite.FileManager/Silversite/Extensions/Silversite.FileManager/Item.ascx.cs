using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

namespace Silversite.Providers.FileManager.UserControls {

	public partial class Item: Silversite.Web.UI.HierarchyPresenter<FileSystemInfo, Web.UI.FileManager> {

		public Item(): base() { }
		public Item(FileSystemInfo path, Web.UI.FileManager filemanager): this() { Item = path; Container = filemanager; }

		const int KB = 1024;
		const int MB = 1024*KB;
		const int GB = 1024 *MB;

		protected string Image {
			get {
				var ext = Services.Paths.Extension(Item.Name);
				if (ext == "jpg" || ext == "jpeg" || ext == "png" || ext == "gif" || ext == "svg" || ext == "svgz" || ext == "bmp" || ext == "tif" || ext == "tiff" ) return Services.Paths.Unmap(Item.FullName);
				if (Services.Files.FileExists(Item.FullName + "._icon.png")) return Services.Paths.Unmap(Item.FullName + "._icon.png");

				const string IconRoot = "~/Silversite/Extensions/Silversite.FileManager/images/fileicons/";
				var img = IconRoot + ext + ".png";
				if (Services.Files.FileExists(img)) return img;
				return IconRoot + "default.png";
			}
		}

		protected void Page_Load(object sender, EventArgs e) {
			if (Container.View == Web.UI.FileManager.Views.Details) {
				icons.Visible = false;
				list.Visible = true;
				detailimage.Width = 20;
				detailimage.Height = 20;
				detailimage.ImageUrl = Image;
				detailname.Text = Item.Name;
				if (Item is FileInfo) {
					var file = (FileInfo)Item;
					detailsize.Text = (file.Length > GB ? (file.Length / (double)GB).ToString("D:3") + " GB" :
						(file.Length > MB ? (file.Length / (double)MB).ToString("D:3") + " MB" :
						(file.Length > KB ? (file.Length / (double)KB).ToString("D:3") + " KB" :
						(file.Length.ToString() + " Byte"))));
					detaildate.Text = file.LastWriteTimeUtc.ToShortDateString() + " " + file.LastWriteTimeUtc.ToShortTimeString();
				} else {
					var dir = (DirectoryInfo)Item;
					detailsize.Text = "";
					detaildate.Text = dir.LastWriteTimeUtc.ToShortDateString() + " " + dir.LastWriteTimeUtc.ToShortTimeString();
				}
			} else {
				icons.Visible = true;
				list.Visible = false;
				switch (Container.View) {
					case Web.UI.FileManager.Views.HugeIcons:
						iconimage.Width = iconimage.Height = 100;
						break;
					case Web.UI.FileManager.Views.BigIcons:
						iconimage.Width = iconimage.Height = 64;
						break;
					case Web.UI.FileManager.Views.MediumIcons:
					default:
						iconimage.Width = iconimage.Height = 32;
						break;
					case Web.UI.FileManager.Views.SmallIcons:
					case Web.UI.FileManager.Views.List:
						iconimage.Width = iconimage.Height = 16;
						break;
				}
				iconimage.ImageUrl = Image;
				iconlabel.Text = Item.Name;
			}
		}
	}
}