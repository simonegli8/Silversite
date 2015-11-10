using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Silversite;
using Silversite.Services;

namespace Silversite.FileManager {

	public class Icon: Image {

		public static readonly string[] Images = new string[] { "png", "gif", "bmp", "jpg",  "jpeg", "tif", "tiff" };

		public Icon(string path, int width, int height) {
			var ext = Paths.Extension(path);
			var iconpath = Paths.Directory(path) + "/_" + Paths.File(path);
			Width = width; Height = height;

			if (Files.FileExistsVirtual(iconpath + ".png")) ImageUrl = iconpath + ".png";
			else if (Files.FileExistsVirtual(iconpath + ".gif")) ImageUrl = iconpath + ".gif";
			else if (Files.DirectoryExists(path)) {
				ImageUrl = "~/Silversite/Extensions/Silversite.FileManager/images/fileicons/_Open.png";
			} else {
				if (ext == "svg" || ext == "svgz") {
					ImageUrl = path;
				} else if (Images.Any(e => e == ext)) {
					ImageUrl = path + ".thumbnail?width=" + width + "&height=" + height;
				} else {
					iconpath = "~/Silversite/Extensions/Silversite.FileManager/images/fileicons/" + ext + ".png";
					if (Files.ExistsVirtual(iconpath)) ImageUrl = iconpath;
					else ImageUrl = "~/Silversite/Extensions/Silversite.FileManager/images/fileicons/_Documents.png";
				}
			}


		}
	}
}