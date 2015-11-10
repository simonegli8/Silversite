using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace Silversite.Web.UI {

	public class Css: HtmlLink {

		public string File { get { return Href; } set { Href = value; } }

		public Css() : base() { EnableViewState = false; Attributes["rel"] = "stylesheet"; Attributes["type"] = "text/css"; }
		public Css(string file): this() { File = file; EnableViewState=false; EnableTheming=false; }

		protected override void Render(HtmlTextWriter writer) {
			if (Visible) base.Render(writer);
		}

		public Css Register(Page page) {
			var headers = page.Header.Controls;
			if (!headers.OfType<Css>().Any(css => css.File == File)) headers.AddAt(0, this);
			return this;
		}

		public static Css Register(Page page, string file) {
			var css = new Css(file);
			return css.Register(page);
		}

		public static Css Register(Page page, Type type, string resource) {
			return Register(page, page.ClientScript.GetWebResourceUrl(type, resource));
		}

		protected override void OnInit(EventArgs e) {
			base.OnInit(e);
			Page.PreRender += PreRender;
		}

		protected void PreRender(object sender, EventArgs e) {
			if (Parent != Page.Header) {
				var header = Page.Header;
				Parent.Controls.Remove(this);
				header.Controls.AddAt(header.Controls.Count, this);
				base.OnPreRender(e);
			}
		}
	}

}
