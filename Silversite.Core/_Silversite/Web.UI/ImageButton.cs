using System;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Drawing.Design;
using System.Security.Permissions;
using System.ComponentModel;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI;


namespace Silversite.Web.UI {

	[DefaultProperty("ImageUrl"), ToolboxData("<{0}:ImageButton runat=\"server\" ImageUrl=\"\" />")]
	public class ImageButton: System.Web.UI.WebControls.ImageButton {

		[Bindable(true),
		Category("Appearance"),
		DefaultValue(""),
		Editor("System.Web.UI.Design.ImageUrlEditor, SystemDesign", typeof(UITypeEditor)),
		UrlProperty()]
		public virtual string HoverImageUrl {
			get {
				return (string)ViewState["HoverImageUrl"] ?? string.Empty;
			}
			set {
				ViewState["HoverImageUrl"] = value;
			}
		}

		[Bindable(true),
		Category("Behavior"),
		DefaultValue(""),
		Editor("System.Web.UI.Design.ImageUrlEditor, SystemDesign", typeof(UITypeEditor)),
		UrlProperty()]
		public virtual string ActiveImageUrl {
			get {
				return (string)ViewState["ActivePageImageUrl"] ?? string.Empty;
			}
			set {
				ViewState["ActivePageImageUrl"] = value;
			}
		}

		bool js = false;
		bool JavaScript { get { return js = (js || Scripts.Register(Page)); } }

		protected override void OnPreRender(EventArgs e) {
			base.OnPreRender(e);
			if (JavaScript && !string.IsNullOrEmpty(HoverImageUrl) || !string.IsNullOrEmpty(ActiveImageUrl) ) {
				string onload = "Silversite.ImageButton$Init($get(\"" + ClientID + "\"), ";

				if (!string.IsNullOrEmpty(HoverImageUrl)) {
					onload += "\"" + ResolveUrl(HoverImageUrl) + "\", ";
				} else onload += "null, ";
				if (!string.IsNullOrEmpty(ActiveImageUrl)) {
					onload += "\"" + ResolveUrl(ActiveImageUrl) + "\");\n";
				} else onload += "null);\n";

				Page.ClientScript.RegisterStartupScript(GetType(), "Silversite.ImageButton$" + ClientID, onload, true);
				Scripts.Register(Page, "~/silversite/js/Silversite.ImageButton.js");
			}
		}

		protected override void AddAttributesToRender(HtmlTextWriter writer) {
			/*
			this.Attributes["onload"] = "this.hoverimg = new Image; this.hoverimg.src = '" + ResolveUrl(this.HoverImageUrl) + "'; this.normalimg = new Image; " +
				"this.normalimg.src = '" + ResolveUrl(this.ImageUrl) + "';";
			this.Attributes["onmouseover"] = "this.src = this.hoverimg.src;";
			this.Attributes["onmouseout"] = "this.src = this.normalimg.src;";
			*/
			if (JavaScript) {
				string onload = "Silversite.ImageButton$Init(this, ";
				if (!string.IsNullOrEmpty(HoverImageUrl)) {
					onload += "\"" + ResolveUrl(HoverImageUrl) + "\", ";
					Attributes["onmouseover"] = "Silversite.ImageButton$Set(this, 'hover')";
					Attributes["onmouseout"] = "Silversite.ImageButton$Set(this, 'normal')";
				} else onload += "null, ";
				if (!string.IsNullOrEmpty(ActiveImageUrl)) {
					onload += "\"" + ResolveUrl(ActiveImageUrl) + "\")";
					Attributes["onmousedown"] = "Silversite.ImageButton$SetImage(this, 'active')";
					Attributes["onmouseup"] = "Silversite.ImageButton$SetImage(this, 'hover')";
				} else onload += "null)";
			}
			//if (!string.IsNullOrEmpty(onload)) Attributes["onload"] = onload;

			base.AddAttributesToRender(writer);
		}
	}

	[DefaultProperty("NavigateUrl"), ToolboxData("<{0}:NavigateButton NavigateUrl=\"\" ImageUrl=\"\" HoverImageUrl=\"\" runat=\"server\"/>")]
	public class NavigateButton: ImageButton {

		[Bindable(true),
		Category("Behavior"),
		DefaultValue(""),
		Editor("System.Web.UI.Design.UrlEditor, SystemDesign", typeof(UITypeEditor)),
		UrlProperty()]
		public virtual string NavigateUrl {
			get {
				return (string)ViewState["NavigateUrl"] ?? string.Empty;
			}
			set {
				ViewState["NavigateUrl"] = value;
			}
		}

		[Bindable(true),
		Category("Behavior"),
		DefaultValue(""),
		Editor("System.Web.UI.Design.ImageUrlEditor, SystemDesign", typeof(UITypeEditor)),
		UrlProperty()]
		public virtual string ActivePageImageUrl {
			get {
				return (string)ViewState["ActivePageImageUrl"] ?? string.Empty;
			}
			set {
				ViewState["ActivePageImageUrl"] = value;
			}
		}

		protected override void OnClick(ImageClickEventArgs e) {
			if (!string.IsNullOrEmpty(NavigateUrl)) Page.Response.Redirect(NavigateUrl);
			else base.OnClick(e);
		}

		protected override void Render(HtmlTextWriter writer) {
			string hover, active, normal, page;
			hover = HoverImageUrl;
			active = ActiveImageUrl;
			page = ActivePageImageUrl;
			normal = ImageUrl;


			if (!string.IsNullOrEmpty(NavigateUrl) && 
				(!DesignMode && Page.ResolveClientUrl(Page.AppRelativeVirtualPath) == Page.ResolveClientUrl(NavigateUrl)) ||
				(DesignMode && Page.AppRelativeVirtualPath == NavigateUrl)) {
				HoverImageUrl = null;
				ActiveImageUrl = null;
				if (!string.IsNullOrEmpty(page)) {
					ImageUrl = page;
				} else if (!string.IsNullOrEmpty(hover)) {
					ImageUrl = hover;
				} else if (!string.IsNullOrEmpty(active)) {
					ImageUrl = active;
				}
				base.Render(writer);
				ImageUrl = normal;
			} else {
				base.Render(writer);
			}
		}
	}
}
