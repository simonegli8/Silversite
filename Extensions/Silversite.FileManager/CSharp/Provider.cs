using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Silversite;
using Silversite.Web.UI;

namespace Silversite.Providers.FileManager {

	public class Provider: FileManagerProvider {

		public override void CreateChildControls(Silversite.Web.UI.Presenter p) {
			if (p is Web.UI.FileManager) {
				var m = (Web.UI.FileManager)p;
				m.Content = new Content();
				m.Menu = new Menu();
				m.Hierarchy = new Hierarchy();
				m.Controls.Add(m.Menu);

				/* var splitter = new Silversite.Web.UI.Splitter();
				splitter.LeftPane = m.Hierarchy;
				splitter.RightPane = m.Content;
				splitter.LeftPanePixelWidth = 160;
				splitter.LeftPaneMinPixelWidth = 0;
				splitter.CssClass = "Silversite-SplitterPanel";
				*/

				//m.Controls.Add(splitter);
			} else throw new NotSupportedException("Unknown Presenter. Only Silversite.Web.UI.FileManager is supported by this Provider.");
		}

	}
}
