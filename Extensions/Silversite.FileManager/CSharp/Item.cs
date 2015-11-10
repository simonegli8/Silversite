using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using Silversite.Web.UI;

namespace Silversite.Providers.FileManager {

	internal class Item: ItemPresenter<FileSystemInfo, Web.UI.FileManager> {
		public Item() { }
		public Item(FileSystemInfo info, Web.UI.FileManager filemanager) {
			Item = info;
			Container = filemanager;
			var item = (UserControls.Item)LoadControl("~/Silversite/Extensions/Silversite.FileManager/Item.ascx");
			item.Item = Item;
			item.Container = Container;
			Controls.Add(item);
		}
	}

}