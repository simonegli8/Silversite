
//Copyright 1997-2009 Syrinx Development, Inc.
//This file is part of the Syrinx Web Application Framework (SWAF).
// == BEGIN LICENSE ==
//
// Licensed under the terms of any of the following licenses at your
// choice:
//
//  - GNU General Public License Version 3 or later (the "GPL")
//    http://www.gnu.org/licenses/gpl.html
//
//  - GNU Lesser General Public License Version 3 or later (the "LGPL")
//    http://www.gnu.org/licenses/lgpl.html
//
//  - Mozilla Public License Version 1.1 or later (the "MPL")
//    http://www.mozilla.org/MPL/MPL-1.1.html
//
// == END LICENSE ==

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Security.Permissions;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Silversite.Web;

// [assembly: WebResource("Silversite.Web.css.Menu.css", "text/css")]
namespace Silversite.Web.UI {

	[Flags()]
	public enum MenuDirection { Left = 1, Up = 2, Right = 4, Down = 8, Default = 0 };

	public enum MenuSkin { Default }

	public interface IMenuItemContainer {
		List<MenuItem> Items { get; }
	}

	[AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.Minimal)]
	[Themeable(true)]
	[ParseChildren(true, "Items")]
	public class Menu: Control, IMenuItemContainer, IPostBackEventHandler {
		public enum SubMenuShowEffect { display, fade, slide, toggle };

		const string DefaultMenuClass = "SilversiteMenu", DefaultMenuItemClass = "SilversiteMenuItem",
			DefaultSubMenuClass="SilversiteSubMenu", DefaultSubMenuItemClass = "SilversiteSubMenuItem",
			DefaultImageClass = "SilversiteMenuImage";

		protected Orientation m_orient = Orientation.Horizontal;
		protected SubMenuShowEffect m_subMenuShowEffect = SubMenuShowEffect.display;
		protected int m_numLayers, m_itemNumLayers, m_subMenuNumLayers, m_subMenuItemNumLayers;
		protected List<MenuItem> m_items = new List<MenuItem>();
		protected string m_externalLinkDefaultTarget;
		protected string m_currentPageBaseUrl;
		protected MenuDirection m_direction = MenuDirection.Default;

		protected string m_className = DefaultMenuClass, m_itemClassName = DefaultMenuItemClass,
			m_subMenuClassName = DefaultSubMenuClass, m_subMenuItemClassName = DefaultSubMenuItemClass,
			m_menuImageClassName = DefaultImageClass;

		bool scripts;
		Css css;

		protected override void OnInit(EventArgs e) {
			base.OnInit(e);
			css = Css.Register(Page,"~/Silversite/Css/Silversite.Menu.css");
			Scripts.jQuery.Register(Page);
			Scripts.Register(Page, "~/Silversite/JavaScript/Silversite.Menu.js");
		}

		protected override void OnPreRender(EventArgs e) {
			base.OnPreRender(e);
			scripts = Scripts.Register(Page);
			css.Visible = (m_className == DefaultMenuClass || m_itemClassName == DefaultMenuItemClass ||
				m_subMenuClassName == DefaultSubMenuClass || m_subMenuItemClassName == DefaultSubMenuItemClass ||
				m_menuImageClassName == DefaultImageClass);
		}

		[Category("Appearance")]
		public Orientation Orientation { get { return m_orient; } set { m_orient = value; } }

		[Category("Appearance")]
		public MenuSkin Skin { get; set; }

		[Category("Appearance")]
		public MenuDirection Direction { get { return m_direction; } set { m_direction = value; } }

		[Category("Appearance")]
		public SubMenuShowEffect MenuShowEffect { get { return m_subMenuShowEffect; } set { m_subMenuShowEffect = value; } }

		public List<MenuItem> Items { get { return m_items; } }

		[Category("Appearance")]
		public int NumLayers { get { return m_numLayers; } set { m_numLayers = value; } }

		[Category("Appearance")]
		public int ItemNumLayers { get { return m_itemNumLayers; } set { m_itemNumLayers = value; } }

		[Category("Appearance")]
		public int SubMenuNumLayers { get { return m_subMenuNumLayers; } set { m_subMenuNumLayers = value; } }

		[Category("Appearance")]
		public int SubMenuItemNumLayers { get { return m_subMenuItemNumLayers; } set { m_subMenuItemNumLayers = value; } }

		[Category("Appearance")]
		public string CssClass { get { return m_className; } set { m_className = value; } }

		[Category("Appearance")]
		public string ItemCssClass { get { return m_itemClassName; } set { m_itemClassName = value; } }

		[Category("Appearance")]
		public string ImageCssClass { get; set; }

		[Category("Appearance")]
		public string SubMenuCssClass { get { return m_subMenuClassName; } set { m_subMenuClassName = value; } }

		[Category("Appearance")]
		public string SubMenuItemCssClass { get { return m_subMenuItemClassName; } set { m_subMenuItemClassName = value; } }

		string SkinName { get { return Skin != MenuSkin.Default ? Skin.ToString() : string.Empty; } }
		string SkinnedCssClass { get { return CssClass == DefaultMenuClass ? DefaultMenuClass + SkinName : CssClass; } }
		string SkinnedItemCssClass { get { return ItemCssClass == DefaultMenuItemClass ? DefaultMenuItemClass + SkinName : CssClass; } }
		string SkinnedImageCssClass { get { return ImageCssClass == DefaultImageClass ? DefaultImageClass + SkinName : ImageCssClass; } }
		string SkinnedSubMenuCssClass { get { return SubMenuCssClass == DefaultSubMenuClass ? DefaultSubMenuClass + SkinName : SubMenuCssClass; } }
		string SkinnedSubMenuItemCssClass { get { return SubMenuItemCssClass == DefaultSubMenuItemClass ? DefaultSubMenuItemClass + SkinName : SubMenuItemCssClass; } }

		public string ExternalLinkDefaultTarget { get { return m_externalLinkDefaultTarget; } set { m_externalLinkDefaultTarget = value; } }

		protected override void Render(HtmlTextWriter writer) {
			renderMenuArea(writer, Items, Orientation, Direction, NumLayers, SkinnedCssClass, ItemNumLayers, SkinnedItemCssClass, ClientID, string.Empty);
		}
		
		public void RaisePostBackEvent(string argument) {
			string[] indexes = argument.Split(',');
			IMenuItemContainer c = this;
			foreach (var i in indexes) {
				try {
					c = c.Items[int.Parse(i)];
				} catch { }
			}
			if (c != null && c is MenuItem) ((MenuItem)c).OnClick(EventArgs.Empty);
		}

		protected virtual void renderMenuArea(HtmlTextWriter writer, List<MenuItem> items, Orientation orient, MenuDirection direction, int numLayers, string cssClass, int itemNumLayers, string itemCssClass, string firstDivId, string menupath) {
			renderStartLayers(writer, numLayers, cssClass, null, firstDivId);
			writer.WriteLine("<table border='0' cellspacing='0' cellpadding='0'>");
			writer.Indent++;
			if (orient == Orientation.Horizontal) { writer.WriteLine("<tr>"); writer.Indent++; }
			var includeImgSpan = shouldIncludeImageSpanInSubmenu(Items);
			for (int pos = 0; pos < items.Count; pos++) {
				MenuItem mi = items[pos];
				mi.setupMenuItem();
				if (mi.Visible) {
					if (orient == Orientation.Vertical) {writer.WriteLine("<tr>"); writer.Indent++; }
					writer.WriteLine("<td>"); writer.Indent++;

					//string width100 = orient == Orientation.Horizontal ? string.Empty : "width:100%;";
					string width100 = string.Empty;

					string menudir = string.Empty;
					for (MenuDirection m = MenuDirection.Left; m <= MenuDirection.Down; m = (MenuDirection)((int)m<<1)) {
						if ((m & direction) != 0) menudir += " MenuDirection" + m.ToString();
					}

					if (mi.Items.Count != 0) { writer.WriteLine("<div class='{0} Silversite_Menu_SubMenuParent' style='{1}'>", orient == Orientation.Horizontal ? "VertMenu" : "HorzMenu", width100); writer.Indent++; }
					string extraClass = string.Empty;

					if (pos == 0)
						if (items.Count == 1) extraClass = "FirstChild LastChild";
						else extraClass = "FirstChild";
					else if (pos == items.Count - 1) extraClass = "LastChild";
					
					if (isMenuItemSelectedPage(mi)) extraClass += " Selected";
					renderStartLayers(writer, itemNumLayers, itemCssClass, extraClass, null);
					string target = calcTargetName(mi);
					if (target.Length != 0)
						target = string.Format(" target='{0}'", target);

					var menuitempath = menupath + "," + pos.ToString();

					string href;
					if (mi.HandlesClick) {
						href = Page.ClientScript.GetPostBackClientHyperlink(this, menuitempath.TrimStart(','), false);
						target = string.Empty;
					} else {
						href = getMenuItemNavUrl(mi);
						href = string.IsNullOrEmpty(href) ? "javascript:;" : ResolveUrl(href);
					}

					writer.WriteLine("<a href=\"{0}\"{2} style='display:block;white-space:nowrap;'><div style='white-space:nowrap;cursor:pointer;{3}'>{1}</div></a>",
						href, getMenuItemHtml(includeImgSpan, mi), target, width100);

					renderEndLayers(writer, itemNumLayers);
					if (mi.Items.Count != 0) {
						string extraSubMenuClass = string.Empty;
						if (MenuShowEffect != SubMenuShowEffect.display)
							extraSubMenuClass = MenuShowEffect.ToString() + "Menu" + menudir;
						writer.WriteLine("</div>"); writer.Indent--;

						if (scripts) {
							writer.Write("<div class='");
							writer.Write(SkinnedSubMenuCssClass);
							writer.WriteLine(" {0}' style='z-index:1000;position:absolute;display:none;'>", extraSubMenuClass); writer.Indent++;
							renderMenuArea(writer, mi.Items, Orientation.Vertical, direction, SubMenuNumLayers, SkinnedSubMenuCssClass, SubMenuItemNumLayers, SkinnedSubMenuItemCssClass, null, menuitempath);
							writer.WriteLine("</div>"); writer.Indent--;
						}
					}
					writer.WriteLine("</td>"); writer.Indent--;
					if (orient == Orientation.Vertical) { writer.WriteLine("</tr>"); writer.Indent--; }
				}
			}
			if (orient == Orientation.Horizontal) { writer.WriteLine("</tr>"); writer.Indent--; }
			writer.WriteLine("</table>"); writer.Indent--;
			renderEndLayers(writer, numLayers);
		}

		protected virtual bool isMenuItemSelectedPage(MenuItem mi) {
			bool isIt = false;
			string nav = getMenuItemNavUrl(mi);
			if (nav != null)
				isIt = nav.EndsWith(CurrentPageBaseUrl, StringComparison.CurrentCultureIgnoreCase);

			return isIt;
		}

		protected virtual string CurrentPageBaseUrl {
			get {
				if (m_currentPageBaseUrl == null) {
					m_currentPageBaseUrl = Page.Request.RawUrl;
					int i = m_currentPageBaseUrl.IndexOf('?');
					if (i > 0)
						m_currentPageBaseUrl = m_currentPageBaseUrl.Substring(0, i);
				}
				return m_currentPageBaseUrl;
			}
		}

		protected virtual bool shouldIncludeImageSpanInSubmenu(List<MenuItem> menu) {
			bool should = false;
			foreach (MenuItem mi in menu)
				if (should = mi.ImageUrl != null)
					break;
			return should;
		}

		protected virtual string getMenuItemHtml(bool includeImgSpan, MenuItem mi) {
			return mi.getMenuItemHtml(this, includeImgSpan, SkinnedImageCssClass ?? string.Empty);
		}

		protected virtual string getMenuItemNavUrl(MenuItem mi) {
			return mi.getCurrentNavigateUrl();
		}

		protected virtual string calcTargetName(MenuItem mi) {
			string target = mi.Target;
			if ((target == null || target.Length == 0) &&
                 mi.NavigateUrl != null && mi.NavigateUrl.IndexOf(':') != -1 && ExternalLinkDefaultTarget != null)
				target = ExternalLinkDefaultTarget;
			return target == null ? string.Empty : target;
		}

		protected static void renderStartLayers(HtmlTextWriter writer, int numLayers, string cssClass, string extraCssClass, string firstDivId) {
			string firstClass = cssClass;
			if (extraCssClass != null)
				firstClass += " " + extraCssClass;

			if (firstDivId != null) {
				writer.WriteLine("<div id='{1}' class='{0}'>", firstClass, firstDivId); writer.Indent++;
			} else {
				writer.WriteLine("<div class='{0}'>", firstClass); writer.Indent++;
			}
			char p = 'a';
			for (int pos = 0; pos < numLayers - 1; pos++) {
				writer.WriteLine("<div class='{0}{1}'>", cssClass, (char)(p + pos)); writer.Indent++;
			}
			if (numLayers > 0) {
				writer.WriteLine("<div class='{0}-c' >", cssClass); writer.Indent++;
				writer.WriteLine("<div class='{0}-w'></div>", cssClass); writer.Indent++;
			}
		}

		protected static void renderEndLayers(HtmlTextWriter writer, int numLayers) {
			writer.WriteLine("</div>"); writer.Indent--;
			for (int pos = 0; pos < numLayers; pos++)
				writer.Write("</div>"); writer.Indent--;
		}
	}

	[Serializable]
	[ParseChildren(true, "Items")]
	public class MenuItem: IMenuItemContainer {
		protected string m_text, m_navigateUrl, m_target, m_imageUrl;
		protected List<MenuItem> m_items = new List<MenuItem>();
		protected MenuDirection m_direction = MenuDirection.Default;

		public MenuItem() {
		}
		public MenuItem(string text, string navigateUrl) {
			m_text = text;
			m_navigateUrl = navigateUrl;
		}
		public MenuItem(string text, string navigateUrl, string target) {
			m_text = text;
			m_navigateUrl = navigateUrl;
			m_target = target;
		}

		public virtual void OnClick(EventArgs e) {
			if (Click != null) Click(this, e);
		}
		public event EventHandler Click;
		public bool HandlesClick { get { return Click != null; } }

		public string Text { get { return m_text; } set { m_text = value; } }

		public string Target { get { return m_target; } set { m_target = value; } }

		[UrlProperty()]
		public string NavigateUrl { get { return m_navigateUrl; } set { m_navigateUrl = value; } }

		[UrlProperty()]
		public string ImageUrl { get { return m_imageUrl; } set { m_imageUrl = value; } }

		public MenuDirection Direction { get { return m_direction; } set { m_direction = value; } }

		public List<MenuItem> Items { get { return m_items; } }

		public virtual bool Visible { get { return true; } }

		public virtual string getCurrentNavigateUrl() { return NavigateUrl; }

		public virtual string getMenuItemHtml(Menu menu, bool includeImgSpan, string ImageCssClass) {
			if (ImageUrl == null)
				return includeImgSpan ? "<span class='nimg'></span>" + Text : Text;

			return string.Format("<img class='{0}' style='border:0;vertical-align:middle;' src='{1}' alt=''  />{2}", ImageCssClass, menu.ResolveClientUrl(ImageUrl), Text);
		}

		public virtual void setupMenuItem() {
			//Intentionally left blank - Base menu item has not setup.
		}
	}
}
