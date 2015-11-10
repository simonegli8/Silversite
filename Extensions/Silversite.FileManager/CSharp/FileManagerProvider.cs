using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.IO;
using Silversite;
using Silversite.Web.UI;

namespace Silversite.Providers.FileManager {

	public class Provider: FileManagerProvider {

		public override void CreateChildControls(Silversite.Web.UI.Presenter p) {
			if (p is Web.UI.FileManager) {
				var m = (Web.UI.FileManager)p;
				UserControls.FileManager uc = new UserControls.FileManager();
				uc = (UserControls.FileManager)uc.LoadControl("~/Silversite/Extensions/Silversite.FileManager/FileManager.ascx");
				m.Controls.Add(uc);
				uc.Container = m;
				if (Services.Files.FileExists(m.Path)) uc.Item = new FileInfo(m.Path);
				else uc.Item = new DirectoryInfo(m.Path);

			} else throw new NotSupportedException("Unknown Presenter. Only Silversite.Web.UI.FileManager is supported by this Provider.");
		}

	}
}
