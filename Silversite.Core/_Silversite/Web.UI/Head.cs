using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Silversite.Web.UI {

	[ToolboxData("<{0}:Head runat=\"server\"></Head>")]
	[ParseChildren(false, "Controls")]
	public class Head: UserControl {



		protected override void OnInit(EventArgs e) {
			base.OnInit(e);
			Page.Load += Load;
		}

		protected void Load(object sender, EventArgs e) {
			InsertHead();
		}

		void InsertHead() {
			if (Page.Header == null) throw new NotSupportedException("head tag must have runat=\"server\" attribute.");

			while (Controls.Count > 0) {
				var c = Controls[0];
				Controls.RemoveAt(0);
				Page.Header.Controls.AddAt(Page.Header.Controls.Count, c);
			}
		}
		protected override void OnPreRender(EventArgs e) {
			base.OnPreRender(e);
			InsertHead();
		}
		protected override void Render(System.Web.UI.HtmlTextWriter writer) {
			//base.Render(writer);
		}

	}
}