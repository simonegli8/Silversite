using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Silversite.Web.UI;

namespace Silversite.Web.UI {

	public class Splitter: Panel {

	public enum Orientations { Horizontal, Vertical }
	public enum Docking { None, Top, Left, Bottom, Right }

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);
			this.Requires("jQuery", "~/Silversite/Extensions/Silversite.Core/Splitter/jquery.cookie.min.js", "~/Silversite/Extensions/Silversite.Core/Splitter/splitter.min.js");

			if (!DefaultStyle) {
				var style = new System.Web.UI.HtmlControls.HtmlGenericControl("style");
				style.InnerText = "\n.splitter-bar-standard { background: #ccc; background-position: center; background-repeat:no-repeat; }\n" +
					".splitter-bar-vertical-standard { width: 4px; background-image: url(" + ResolveClientUrl("~/Silversite/Extensions/Silversite.Core/Splitter/vgrabber.gif") + "); border-left: 1px outset #ccc; border-right: 1px outset #ccc; }\n" +
					".splitter-bar-horizontal-standard { height: 4px; background-image: url(" + ResolveClientUrl("~/Silversite/Extensions/Silversite.Core/Splitter/hgrabber.gif") + "); border-top: 1px outset #ccc; border-bottom: 1px outset #ccc; }\n" +
					".ui-state-highlight-standard { opacity: 0.7; }\n.ui-state-hover-standard { opacity: 0.85; }\n.ui-state-default-standard { opacity: 1; }\n.ui-state-error-standard { opacity: 0.4; }\n" +
					".splitter-bar-vertical-docked-standard { width: 8px; background-image: url(" + ResolveClientUrl("~/Silversite/Extensions/Silversite.Core/Splitter/vdockbar-trans.gif") + "); border-left: 1px outset #ccc; border-right: 1px outset #ccc; }\n" +
					".splitter-bar-horizontal-docked-standard { height: 8px; background-image: url(" + ResolveClientUrl("~/Silversite/Extensions/Silversite.Core/Splitter/hdockbar-trans.gif") + "); border-top: 1px outset #ccc; border-bottom: 1px outset #ccc; }\n" +
					".splitter-iframe-hide-standard { visibility: hidden; }\n.splitter-pane-standard { overflow: auto; }\n";
				Page.Header.Controls.Add(style);
				DefaultStyle = true;
			}
		}

		public bool DefaultStyle { get { return (bool)(Page.Items["Splitter.DefaultStyle"] ?? false); } set { Page.Items["Splitter.DefaultStyle"] = value; } }
		public int? Position { get { return (int?)ViewState["Position"]; } set { ViewState["Position"] = value; } }
		public int? MinLeft { get { return (int?)ViewState["MinLeft"]; } set { ViewState["MinLeft"] = value; } }
		public int? MinRight { get { return (int?)ViewState["MinRight"]; } set { ViewState["MinRight"] = value; } }
		public int? MinTop { get { return (int?)ViewState["MinTop"]; } set { ViewState["MinTop"] = value; } }
		public int? MinBottom { get { return (int?)ViewState["MinBottom"]; } set { ViewState["MinBottom"] = value; } }
		public int? MaxLeft { get { return (int?)ViewState["MaxLeft"]; } set { ViewState["MaxLeft"] = value; } }
		public int? MaxRight { get { return (int?)ViewState["MaxRight"]; } set { ViewState["MaxRight"] = value; } }
		public int? MaxTop { get { return (int?)ViewState["MaxTop"]; } set { ViewState["MaxTop"] = value; } }
		public int? MaxBottom { get { return (int?)ViewState["MaxBottom"]; } set { ViewState["MaxBottom"] = value; } }
		public string AttachTo { get { return (string)ViewState["AttachedTo"]; } set { ViewState["AttachedTo"] = value; } }
		public Orientations Orientation { get { return (Orientations)(ViewState["Orientation"] ?? Orientations.Vertical); } set { ViewState["Orientation"] = value; } }
		public Docking Dock { get { return (Docking)(ViewState["Dock"] ?? Docking.None); } set { ViewState["Dock"] = value; } }
		public bool Fixed { get { return (bool)(ViewState["Fixed"] ?? false); } set { ViewState["Fixed"] = value; } }
		public bool Animated { get { return (bool)(ViewState["Animated"] ?? true); } set { ViewState["Animated"] = value; } }
		public bool AttachToWidth { get { return (bool)(ViewState["AttachToWidth"] ?? false); } set { ViewState["AttachToWidth"] = value; } }

		string id {
			get {
 				if (ID.IsNullOrEmpty()) {
					var n = (int)(Page.Items["Silversite.Core.Splitter"] ?? 0);
					ID = "Silversite-Core-Splitter-" + n;
					Page.Items["Silversite.Core.Splitter"] = n+1;
				}
				return ClientID;
			}
		}

		public string JSToggleDock { get { return "var e = $(\"#" + id + "\"); e.trigger(\"toggleDock\"+e._opts.eventNamespace);"; } }
		public string JSDock { get { return "var e = $(\"#" + id + "\"); e.trigger(\"dock\"+e._opts.eventNamespace);"; } }
		public string JSUndock { get { return "var e = $(\"#" + id + "\"); e.trigger(\"undock\"+e._opts.eventNamespace);"; } }

		string FindClientID(Control parent, string id) {
			if (parent == null) return null;
			var child = parent.FindControl(id);
			if (child != null) return child.ClientID;
			return FindClientID(parent.Parent, id);
		}

		public override void RenderControl(HtmlTextWriter w) {
			var myid = id;
			base.RenderControl(w);
			var vert = Orientation == Orientations.Vertical;

			w.AddAttribute("type", "text/javascript");
			w.RenderBeginTag(HtmlTextWriterTag.Script);
			w.WriteLine("//<![CDATA[");
			w.Write("var slider_func = function(){$(\"#");
			w.Write(myid);
			w.Write("\").splitter({outline: ");
			w.Write((!Animated).ToString().ToLower());
			w.Write(", dockSpeed: 200, dockKey: 'Z', accessKey: 'I', type: \"");
			w.Write(vert ? "v" : "h");
			w.Write("\"");
			if (Position.HasValue) {
				w.Write(vert ? ", sizeLeft: " : ", sizeTop: ");
				w.Write(Position);
			}
			if (Fixed) {
				w.Write(", fixed: ");
				w.Write(Fixed.ToString().ToLower());
			}
			if (MinLeft.HasValue) {
				w.Write(", minLeft: ");
				w.Write(MinLeft);
			}
			if (MinRight.HasValue) {
				w.Write(", minRight: ");
				w.Write(MinRight);
			}
			if (MinTop.HasValue) {
				w.Write(", minTop: ");
				w.Write(MinTop);
			}
			if (MinBottom.HasValue) {
				w.Write(", minBottom: ");
				w.Write(MinBottom);
			}
			if (MaxLeft.HasValue) {
				w.Write(", maxLeft: ");
				w.Write(MaxLeft);
			}
			if (MaxRight.HasValue) {
				w.Write(", maxRight: ");
				w.Write(MaxRight);
			}
			if (MaxTop.HasValue) {
				w.Write(", maxTop: ");
				w.Write(MaxTop);
			}
			if (MaxBottom.HasValue) {
				w.Write(", maxBottom: ");
				w.Write(MaxBottom);
			}

			if (AttachToWidth) w.Write(", resizeToWidth: true");
			if (!AttachTo.IsNullOrEmpty()) {
				w.Write(", resizeTo: ");
				w.Write(FindClientID(this, AttachTo));
			}
			w.Write(", cookie: \"Silversite-Splitter-");
			w.Write(myid);
			w.Write("\", dock: \"");
			w.Write(Dock.ToString().ToLower());
			w.WriteLine("\"});};");

			w.Write("$().ready(function(){slider_func();var prm = Sys.WebForms.PageRequestManager.getInstance();prm.add_endRequest(slider_func);});");
			w.WriteLine("//]]>");		
			w.RenderEndTag();
		}
	}

}