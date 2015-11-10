using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Silversite.Web.UI {

	[DefaultProperty("Text"), ToolboxData("<{0}:IndexHyperLink runat=\"server\" NavigateUrl=\"\"></{0}:IndexHyperLink>")]
	public class IndexHyperLink: HyperLink {

		public IndexHyperLink() {
			CssClass = "indexlink";
			ActiveCssClass = "activeindexlink";
		}

		[Browsable(true)]
		public virtual string ActiveCssClass {
			get {
				if (ViewState != null) return (ViewState["ActiveCssClass"] as string) ?? string.Empty;
				else return string.Empty;
			}
			set {
				if (ViewState != null) ViewState["ActiveCssClass"] = value;
			}
		}

		protected override void AddAttributesToRender(System.Web.UI.HtmlTextWriter writer) {
			string oldCssClass = CssClass;
			if (!string.IsNullOrEmpty(ActiveCssClass) && ResolveUrl(NavigateUrl) == ResolveUrl(Page.AppRelativeVirtualPath)) {
				CssClass = ActiveCssClass;
			}
			base.AddAttributesToRender(writer);
			CssClass = oldCssClass;
		}

	}
}
