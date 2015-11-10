using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using Silversite.Web.UI;

namespace Silversite.Providers.FileManager {

	internal class Hierarchy: HierarchyPresenter<FileSystemInfo, Web.UI.FileManager> {
		public Hierarchy() { }
		public Hierarchy(FileSystemInfo path, Web.UI.FileManager filemanager) { 
			Item = path;
			Container = filemanager;
			var hierarchy = (UserControls.Hierarchy)LoadControl("~/Silversite/Extensions/Silversite.FileManager/Hierarchy.ascx");
			hierarchy.Item = Item;
			hierarchy.Container = Container;
			Controls.Add(hierarchy);
		}
	}
}