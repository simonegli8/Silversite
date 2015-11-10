using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Text;
using System.Configuration;

namespace Silversite.Web.UI {


	[Configuration.Section(Path = ConfigPath, Name = "jQuery")]
	public class jQueryConfiguration : Configuration.Section {

		public const string ConfigPath = ConfigRoot + "/Silversite.config";

		[ConfigurationProperty("version", IsRequired = false, DefaultValue = "1.8.3")]
		public string Version { get { return this["version"] as string ?? "1.8.3"; } set { this["version"] = value; } }

		[ConfigurationProperty("uiVersion", IsRequired = false, DefaultValue = "1.9.2")]
		public string UIVersion { get { return this["uiVersion"] as string ?? ""; } set { this["uiVersion"] = value; } }

		[ConfigurationProperty("Theme", IsRequired = false, DefaultValue = "Smoothness")]
		public string Theme { get { return this["Theme"] as string ?? ""; } set { this["Theme"] = value; } }
	}

	public static class Scripts {

		class SilversiteScript { }

		public static bool Register(Page page, string Source, bool composite) {
			if (page.Request.Browser.EcmaScriptVersion.Major >= 1) {
				if (Source != null) {
					var scriptManager = ScriptManager.GetCurrent(page);
					if (scriptManager != null) {
#if DEBUG
						composite = false;
#endif
						if (!scriptManager.Scripts.Any(s => s.Path == "~/Silversite/JavaScript/Silversite.js.aspx")) scriptManager.Scripts.Add(new ScriptReference("~/Silversite/JavaScript/Silversite.js.aspx"));

#if Microsoft
						if (!composite || Services.Runtime.IsMono) {
							if (!scriptManager.Scripts.Any( s => s.Path == Source)) scriptManager.Scripts.Add(new ScriptReference(Source));
						} else {
							if (!scriptManager.CompositeScript.Scripts.Any(s => s.Path == Source)) scriptManager.CompositeScript.Scripts.Add(new ScriptReference(Source));
						}
#else
						scriptManager.Scripts.Add(new ScriptReference(Source));
#endif
					} else {
						if (!page.ClientScript.IsClientScriptIncludeRegistered("Silversite.js")) page.ClientScript.RegisterClientScriptInclude("Silversite.js", "~/Silversite/JavaScript/Silversite.js.aspx");
						page.ClientScript.RegisterClientScriptInclude(Source.GetHashCode().ToString(), page.ResolveClientUrl(Source));
					}
					return true;
				}
			}
			return false;
		}
		public static bool Register(Page page, string Source) { return Register(page, Source, true); }

		public static class jQuery {

			public static jQueryConfiguration Configuration = new jQueryConfiguration();

			public static bool Register(Page page) {
#if DEBUG
				return Scripts.Register(page, "http://ajax.aspnetcdn.com/ajax/jQuery/jquery-" + Configuration.Version + ".js", false);
#else
				return Scripts.Register(page, "http://ajax.aspnetcdn.com/ajax/jQuery/jquery-" + Configuration.Version + ".min.js");
#endif
			}
		}

		public static class jQueryUI {

			public enum Themes { Default, UILigthness, UIDarkness, Smoothness, Start, Redmond, Sunny, Overcast, LeFrog, Flick, PepperGrinder, Eggplant, DarkHive, Cupertino, SouthStreet, Blitzer, Humanity, HotSneaks, ExciteBike,
				Vader, DotLuv, MintChoc, BlackTie, Trontastic, SwankyPurse, Other }

			public static bool Register(Page page, string theme) {
				Css.Register(page, "http://ajax.aspnetcdn.com/ajax/jquery.ui/" + jQuery.Configuration.Version + "/themes/" + theme.ToLower() + "/jquery-ui.css");
#if DEBUG
				return Scripts.Register(page, "http://ajax.aspnetcdn.com/ajax/jquery.ui/" + jQuery.Configuration.Version + "/jquery-ui.js");
#else
				return Scripts.Register(page, "http://ajax.aspnetcdn.com/ajax/jquery.ui/" + jQuery.Configuration.Version + "/jquery-ui.min.js");
#endif
			}
			public static bool Register(Page page, Themes theme) { return Register(page, theme == Themes.Default ? jQuery.Configuration.Theme : theme.ToString()); }
			public static bool Register(Page page) { return Register(page, jQuery.Configuration.Theme); }
		}

		/*
		public static class Silversite {
			public static bool Register(Page page) {
				// return Scripts.Register(page, typeof(Silversite), "~/Silversite/JavaScript/Silversite.js");
			}
		}
		*/

		public static bool Register(Page page) {
			return jQuery.Register(page); // && Silversite.Register(page); ;
		}

		public static bool RegisterScripts(this Page page) {
			return Register(page); 
		}

		static int key = 0;
		public static void RegisterBlock(Page page, string command) {
			page.ClientScript.RegisterClientScriptBlock(typeof(Page), (key++).ToString(), command); 
		}
		public static void RegisterReady(Page page, string command) {
			command = "$(document).ready(function() {" + command + "});";
			RegisterBlock(page, command);
		}
		public static string InitScript(string command, Dictionary<string, object> parameters) {
			if (parameters.Count == 0) return string.Empty;
			var jsobj = new StringBuilder();
			jsobj.Append("{");
			int i = 0;
			foreach (var item in parameters) {
				if (i++ > 0) jsobj.Append(", ");
				jsobj.Append(item.Key);
				jsobj.Append(":");
				jsobj.Append(item.Value.ToString());
			}
			jsobj.Append("}");
			return string.Format(command, jsobj.ToString());
		}

		public static string InitScript(string command, params object[] parameters) {
			var d = new Dictionary<string, object>();
			for (int i = 0; i < parameters.Length-1; ) {
				d[parameters[i++].ToString()] = parameters[i++].ToString();
			}
			return InitScript(command, d);
		}

	}

	public class Script: System.Web.UI.Control  {
		public Script() : base() { Type = "text/javascript"; UseScriptManager = true; Text = null; }

		protected override void Render(HtmlTextWriter writer) {
			if (!UseScriptManager || Type != "text/javascript" || Text != null) {
				if (!string.IsNullOrEmpty(Type)) writer.AddAttribute(HtmlTextWriterAttribute.Type, Type);
				if (!string.IsNullOrEmpty(Src)) writer.AddAttribute(HtmlTextWriterAttribute.Src, ResolveClientUrl(Src));
				writer.RenderBeginTag(HtmlTextWriterTag.Script);
				RenderChildren(writer);
				writer.RenderEndTag();
			}
		}
		protected override void OnPreRender(EventArgs e) {
			base.OnPreRender(e);
			if (UseScriptManager && Type == "text/javascript" && Text == null) Scripts.Register(Page, Src);
		}
		public string Src { get; set; }
		public string Type { get; set; }
		public bool UseScriptManager { get; set; }
		public string Text { get; set; }

		protected override void CreateChildControls() {
			base.CreateChildControls();
			if (Text != null) {
				var l = new System.Web.UI.WebControls.Literal();
				if (!Text.StartsWith("//<![CDATA[")) l.Text = "//<![CDATA[\r\n" + Text + "\r\n//]]>"; 
				else l.Text = Text;
				Controls.Add(l);
			}
		}
	}

	public class jQuery: System.Web.UI.Control {
		public jQuery(): base() { }
		protected override void Render(HtmlTextWriter writer) { }
		protected override void OnLoad(EventArgs e) { Scripts.jQuery.Register(Page); }
		public static bool Register(Page page) { return Scripts.jQuery.Register(page); }
	}

		public class jQueryUI: System.Web.UI.Control {
		public jQueryUI(): base() { Theme = Scripts.jQueryUI.Themes.Default; }
		public string ThemeName { get { return (string)ViewState["Theme"]; } set { ViewState["Theme"] = value; } }
		public Scripts.jQueryUI.Themes Theme {
			get {
				Scripts.jQueryUI.Themes theme = Scripts.jQueryUI.Themes.Other;
				Enum.TryParse(ThemeName, out theme);
				return theme;
			}
			set {
				ThemeName = value.ToString();
			}
		}
		protected override void Render(HtmlTextWriter writer) { }
		protected override void OnLoad(EventArgs e) { Scripts.jQueryUI.Register(Page, Theme); }
		public static bool Register(Page page, Scripts.jQueryUI.Themes theme) { return Scripts.jQueryUI.Register(page, theme); }
		public static bool Register(Page page, string theme) { return Scripts.jQueryUI.Register(page, (string.IsNullOrEmpty(theme) || theme.ToLower() == "default") ?	Scripts.jQuery.Configuration.Theme : theme); }
		}
}
