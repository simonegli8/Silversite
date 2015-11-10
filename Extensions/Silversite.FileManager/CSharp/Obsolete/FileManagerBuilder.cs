using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Silversite.Web.UI;

namespace Silversite.FileManager {

	public class FileManagerBuilder {

		public static readonly string[] Images = new string[] {"png","jpg","jpeg","tif","tiff","bmp","gif","svg" };

		public Silversite.Web.UI.FileManager M { get; private set; }

		public FileManagerBuilder(Silversite.Web.UI.FileManager m) { M = m; }

		public virtual void CreateFileManager() {
			M.Controls.Clear();

			M.Controls.Add(M.Menu);
			M.Controls.Add(M.TreeView);
			M.Controls.Add(M.DetailsView);
			
			/*
			var separator = new Panel();
			separator.Style.Add("float", "clear");
			M.Controls.Add(separator);
			*/
			
			new MenuBuilder(M).CreateMenu();
			new TreeBuilder(M).CreateTree();
			new DetailsBuilder(M).CreateDetails();
		}

	}
}