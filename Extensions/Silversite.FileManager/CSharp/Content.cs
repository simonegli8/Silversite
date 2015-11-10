using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using Silversite.Web.UI;

namespace Silversite.Providers.FileManager {

	public class Content: ContentPresenter<FileSystemInfo, Web.UI.FileManager> {
		public Content() { }
		public Content(FileSystemInfo path, Web.UI.FileManager filemanager) { 
			Item = path;
			Container = filemanager;
			var content = (UserControls.Content)LoadControl("~/Silversite/Extensions/Silversite.FileManager/Content.ascx");
			content.Item = Item;
			content.Container = Container;
			Controls.Add(content);
		}

	}
}