using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Silversite.Web.UI;

namespace Silversite.FileManager {

	public class MenuBuilder {

		public Silversite.Web.UI.FileManager M { get; set; }
		public string Path { get { return M.Path; } set { M.Path = value; M.Apply(); } }
		public MenuBuilder(Silversite.Web.UI.FileManager m) { M = m; }

		public virtual void CreateMenu() {

			var m = M.Menu;

			m.Controls.Clear();

			m.Width = new Unit("100%");
			m.Height = 20;


			var home = Silversite.Web.UI.FileManager.Paths(M.Root);
			var path = Path.FromOn(home);
			var segments = path.Tokens('/');
			
			var pathbox = new Panel();
			pathbox.Style.Add("float", "left");
			pathbox.Style.Add("border", "1px solid gray");
			pathbox.Height = 12;
			pathbox.Width = new Unit("70%");
			pathbox.Style.Add("padding", "4px");
			//var root = new Image();
			//root.ImageUrl = 
			for (int n = 0; n < segments.Count; n++) {
				if (n > 0) pathbox.Controls.Add(new Literal() { Text = " / " }); // TODO arrow icon.
				var lnk = new LinkButton();
				int ln = n;
				lnk.Click += (sender, args) => {
					Path = segments.Take(ln + 1).StringList('/');
				};
				lnk.Text = segments[n];
				pathbox.Controls.Add(lnk);
			}
			m.Controls.Add(pathbox);

			var view = new DropDownList();
			view.Items.Add(new ListItem() { Text = "Extra large icons" });
			view.Items.Add(new ListItem() { Text = "Large icons" });
			view.Items.Add(new ListItem() { Text="Medium icons" });
			view.Items.Add(new ListItem() { Text="Small icons" });
			view.Items.Add(new ListItem() { Text="List" });
			view.Items.Add(new ListItem() { Text="Details" });
			view.SelectedIndex = (int)M.View;
			view.SelectedIndexChanged += (sender, args) => {
				M.View = (Silversite.Web.UI.FileManager.Views)(view.SelectedIndex);
				M.Apply();
			};
			view.AutoPostBack = true;
			view.Width = 120;
			view.Style.Add("float", "right");
			view.ID = "view";
			m.Controls.Add(view);
		}
	}
}