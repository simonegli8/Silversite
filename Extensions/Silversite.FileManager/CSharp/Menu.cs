using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using Silversite.Web.UI;

namespace Silversite.Providers.FileManager {

	internal class Menu: MenuPresenter<FileSystemInfo, Web.UI.FileManager> {
		public Menu() { }
		public Menu(FileSystemInfo path, Web.UI.FileManager filemanager) {
			Item = path;
			Container = filemanager;
			var menu = (UserControls.Menu)LoadControl("~/Silversite/Extensions/Silversite.FileManager/Menu.ascx");
			menu.Item = Item;
			menu.Container = Container;
			Controls.Add(menu);
		}
	}

}