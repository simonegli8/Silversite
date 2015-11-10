using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.UI;

namespace Silversite.Web.UI {

	public class Zoom: Panel {

		public const double DefaultZoomFactor = 3;
		public const string DefaultSpeed = "200";

		public double ZoomFactor { get; set; }
		public string Speed { get; set; }

		/*
		public double DefaultZoomFactor { get; set; }
		public string DefaultSpeed { get; set; }
		
		public Zoom() { ZoomFactor = 2; DefaultZoomFactor = double.NaN; }
		*/

		bool jQuery = false;
		protected override void OnPreRender(EventArgs e) {
			base.OnPreRender(e);
			jQuery = Scripts.Register(Page);
			Scripts.Register(Page, "~/Silversite/JavaScript/Silversite.Zoom.js");
			/*
			if (ZoomFactor != 0 && ZoomFactor != 1 && Scripts.Register(Page)) {
				Page.ClientScript.RegisterClientScriptBlock(typeof(Zoom), "Silversite.Zoom.Factor", "Silversite_Zoom_Factor=" + DefaultZoomFactor.ToString() + ";", true);
				Page.ClientScript.RegisterClientScriptBlock(typeof(Zoom), "Silversite.Zoom.Speed", "Silversite_Zoom_Speed=" + DefaultSpeed + ";", true);
			}
			*/
		}

		public override void RenderControl(HtmlTextWriter writer) {
			if (Controls.Count != 3 && !(Controls[1] is WebControl)) throw new NotSupportedException("ZoomPanel must contain exactly one child element.");
			var control = (WebControl)Controls[1];
			if (jQuery) {
				//control.CssClass = "Silversite_Zoom";
				var d = new Dictionary<string, object>();
				if (ZoomFactor != 2) d["factor"] = ZoomFactor;
				if (!string.IsNullOrEmpty(Speed)) d["speed"] = Speed;
				
				Attributes["onload"] = Scripts.InitScript("Silversite.Zoom$Set(this, {0})", d);
			}
			control.RenderControl(writer);
		}
	
	}

	public class ZoomImage: Image {

		public const double DefaultZoomFactor = 3;
		public const string DefaultSpeed = "200";

		public double ZoomFactor { get; set; }
		public string Speed { get; set; }

		/*
		public double DefaultZoomFactor { get; set; }
		public string DefaultSpeed { get; set; }
		
		public ZoomImage() { ZoomFactor = 2; DefaultZoomFactor = double.NaN; }
		*/

		bool jQuery = false;
		protected override void OnPreRender(EventArgs e) {
			base.OnPreRender(e);
			jQuery = Scripts.Register(Page);
			/*
			if (ZoomFactor != 0 && ZoomFactor != 1 && Scripts.Register(Page)) {
				if (!string.IsNullOrEmpty(DefaultSpeed)) Page.ClientScript.RegisterClientScriptBlock(typeof(ZoomImage), "Silversite.Zoom.Speed", "Silversite_Zoom_Speed=" + DefaultSpeed + ";", true);
				if (!double.IsNaN(DefaultZoomFactor)) Page.ClientScript.RegisterClientScriptBlock(typeof(ZoomImage), "Silversite.Zoom.Factor", "Silversite_Zoom_Factor=" + DefaultZoomFactor.ToString() + ";", true);
			}
			*/
		}

		public override void RenderControl(HtmlTextWriter writer) {
			if (jQuery) {
				//CssClass = "Silversite_Zoom";
				var d = new Dictionary<string, object>();
				if (ZoomFactor != 2) d["factor"] = ZoomFactor;
				if (!string.IsNullOrEmpty(Speed)) d["speed"] = Speed;

				Attributes["onload"] = Scripts.InitScript("Silversite.Zoom$Set(this, {0})", d);
			}
			base.RenderControl(writer);
		}

	}
}
