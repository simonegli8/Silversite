using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Silversite;
using Silversite.Services;
using Silversite.Web.UI;

namespace Silversite.FileManager {

	public class DetailsBuilder {

		public Silversite.Web.UI.FileManager M { get; set; }
		public DetailsBuilder(Silversite.Web.UI.FileManager m) { M = m; }

		public virtual void CreateDetails() {
			var d = M.DetailsView;

			d.Controls.Clear();

			d.Style.Add("overflow", "auto");
			d.Style.Add("border", "1px solid gray");
			d.Style.Add("margin", "0px");
			d.Height = new Unit("300px");
			d.Width = new Unit("100%");
			d.Style.Add("background", "white");
			
			var path = M.Path;
			var ext = Paths.Extension(path);

			var handler = Handlers.Get(path);
			if (handler != null) handler.Open(M, path);
			else { // path does not exist or there is no handler
				/*var div = new Panel();
				div.Style.Add("margin", "auto");
				var icon = new Image();
				icon.ImageUrl = "~/Silversite/Extensions/Silversite.FileManager/images/fileicons/_Close.png";
				div.Controls.Add(icon);
				d.Controls.Add(div);*/
			}
		}

	}

}