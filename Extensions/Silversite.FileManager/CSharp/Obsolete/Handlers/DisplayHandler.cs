using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace Silversite.FileManager {

	public class DisplayHandler: FileHandler {

		public override string Files { get { return "*.html;*.htm;*.php;*.asp;*.aspx;*.ashx;*.ascx;*.png;*.gif;*.jpg;*.jpeg;*.tif;*.tiff;*.bmp;*.svg;*.svgz"; } }

		public override void Open(Silversite.Web.UI.FileManager m, string path) {
			var ext = Services.Paths.Extension(path);
			switch (ext) {
				case "html":
				case "php":
				case "htm":
				case "asp":
				case "aspx":
				case "ashx":
					HttpContext.Current.Server.Transfer(path, false); break;
				case "ascx":
					var uc = new UserControl().LoadControl(path);
					m.DetailsView.Controls.Clear();
					m.DetailsView.Controls.Add(uc);
					break;
				case "jpg":
				case "jpeg":
				case "gif":
				case "tif":
				case "tiff":
				case "png":
				case "bmp":
				case "svg":
				case "svgz":
					var div = new Panel();
					var img = new Image();
					img.ImageUrl = path;
					div.Style.Add("margin","auto");
					div.Controls.Add(img);
					m.DetailsView.Controls.Add(div);
					break;
			}
		}
	}
}