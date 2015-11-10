using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Silversite.Web.UI;

namespace Silversite.FileManager {

	public class DefaultDirectoryHandler: DirectoryHandler {

		public override void Open(Silversite.Web.UI.FileManager m, string path) {
			var info = Services.Files.DirectoryInfoVirtual(path);
			var files = info.Children
				.OfType<System.Web.Hosting.VirtualFileBase>()
				.Select(f => Services.Paths.Normalize(f.VirtualPath));

			switch (m.View) {
				case Silversite.Web.UI.FileManager.Views.HugeIcons:
				case Silversite.Web.UI.FileManager.Views.BigIcons:
				case Silversite.Web.UI.FileManager.Views.MediumIcons:
				case Silversite.Web.UI.FileManager.Views.SmallIcons:
					foreach (var name in files) File.Add(m, this, name);
					break;
				case Silversite.Web.UI.FileManager.Views.List:
					foreach (var name in files) File.Add(m, this, name);
					break;
				case Silversite.Web.UI.FileManager.Views.Details:
					var head = new Literal() { Text = "<table style=\"background:white;width:100%;height:100%;\">" };
					m.DetailsView.Controls.Add(head);
					foreach (var name in files) File.Add(m, this, name);
					var foot = new Literal() { Text = "</table>" };
					m.DetailsView.Controls.Add(foot);
					break;
			}
		}
	}
}