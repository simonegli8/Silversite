using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Text;
using System.Web.Security;
using Silversite.Services;
using System.IO;
using System.Drawing;
using System.Net;
using Silversite.Configuration;
using SysConf = System.Configuration;

namespace Silversite.Web.UI {

	[ToolboxData("<{0}:TextContent runat=\"server\" Path=\"\"/>")]
	[ParseChildren(false, "Controls")]
	///<summary>
	/// The base class for Controls who's content can be edited by the user.
	///</summary>
	public class TextContent: EditableContent {

		public TextContent(): base() {
			CssClass = "Silversite.TextContent Silversite.EditableContent";
		}

		protected virtual void SetButtons() { undoButton.Visible = Revision > 0; redoButton.Visible = Revision < 0; publishButton.Visible = PreviewElement != null; }

		protected int Revision { get { return (int)(Element.Attributes["Revision"] ?? 0); } set { Element.Attributes["Revision"] = value; } }

		protected virtual void Publish() {
			var p = PreviewElement;
			if (p != null) {
				Element.Children.Clear();
				Element.Children.AddRange(p.Children);
				Revision++;
				Element.Publish();
				SetButtons();
			}
		}
		protected virtual void SetRevision(int revision) {
			var p = CreatePreviewElement();
			p.Attributes["Revision"] = revision;
			p.Children.Load(revision);
			Element.Preview();
			SetButtons();
		}

		public virtual void Delete() {
			Element.Parent.Remove(Element);
			Element.Publish();
		}
		public void MenuCommand(object sender, CommandEventArgs c) {
			switch(c.CommandName) {
			case "Edit": break;
			case "Undo": SetRevision(Revision-1); break;
			case "Redo": SetRevision(Revision+1); break;
			case "Publish": Element.Preview(); break;
			case "Delete": Delete; break;
			}
		}

			
		Icon editButton = new EditIcon();
		Icon undoButton = new UndoIcon();
		Icon deleteButton = new DeleteIcon();
		Icon publishButton = new PublishIcon();

		protected override void CreateChildControls() {
			base.CreateChildControls();

			if (LoadedControl != null) {
				Controls.Clear();
				Controls.Add(LoadedControl);
			}

			if (IsInEditMode) {
		
				var buttons = (Buttons ?? Configuration.Menu).Split(';', ',', ' ', '|').Select(b => b.ToLower());

				deleteButton.ConfirmText = "Wollen sie diesen Eintrag wiklich löschen?"; 
				publishButton.Visible = HasPreview;
				editButton.OnClientClick = Services.JavaScriptTextEditor.OpenEditorScript(Page, this, EditorSettings.Mode) + "; return false;";
				publishButton.OnClientClick += "Silversite.Overlays.Wait($('#" + Container.ClientID + "')); return true;";
				undoButton.OnClientClick += "Silversite.Overlays.Wait($('#" + Container.ClientID + "')); return true;";

				Menu.Controls.Clear();
				foreach (var b in buttons) {
					switch(b) {
					case "edit":	Menu.Controls.Add(editButton); break;
					case "undo": Menu.Controls.Add(undoButton); break;
					case "publish": Menu.Controls.Add(publishButton); break;
					case "delete": Menu.Controls.Add(deleteButton); break;
					default: break;
					}
				}
			
				Menu.Style.Add(HtmlTextWriterStyle.Position, "absolute");
				Menu.Style.Add(HtmlTextWriterStyle.Top, "5px");
				Menu.Style.Add("right", "5px");
				Menu.ID = "EditMenu";

				Content.Controls.Clear();
				Content.ID = "EditContent";
				
				var preview = Controls.OfType<Preview>().FirstOrDefault();
				List<Control> controls = null;
				if (preview != null) controls = preview.Controls.OfType<Control>().ToList();
				else controls = Controls.OfType<Control>().ToList();
				Controls.Clear();
				foreach (Control ct in controls) Content.Controls.Add(ct);

				Container.Controls.Clear();
				Container.ID = "EditContainer";
				Container.Style.Add(HtmlTextWriterStyle.BorderWidth, "1px");
				Container.Style.Add(HtmlTextWriterStyle.BorderColor, "#AAAAAA");
				Container.Style.Add(HtmlTextWriterStyle.BorderStyle, "dashed");
				Container.Style.Add(HtmlTextWriterStyle.Position, "relative");
				Container.Style.Add(HtmlTextWriterStyle.Padding, "5px");
				Container.Style.Add("-moz-border-radius", BorderRadius.ToString());
				Container.Style.Add("border-radius", BorderRadius.ToString());

				foreach (var st in Style.Keys) {
					if (st is string) Container.Style[(string)st] = Style[(string)st];
					else if (st is HtmlTextWriterStyle) Container.Style[(HtmlTextWriterStyle)st] = Style[(HtmlTextWriterStyle)st];
				}
				Container.Style.Add(HtmlTextWriterStyle.Position, "relative");
		
				var script = new Script();
				Container.Controls.Add(script);
				Container.Controls.Add(Menu);
				Container.Controls.Add(Content);

				Controls.Add(Container);
	
				if (Configuration.Effects) {
					script.Text = "$(document).ready(function() { Silversite.Overlays.InitPopup('" + Container.ClientID + "'); });";
				}
			}
		}

		string Result = string.Empty;

		static HashSet<HttpRequest> SaveRequests = new HashSet<HttpRequest>();
		static Dictionary<HttpRequest, string> OldText = new Dictionary<HttpRequest,string>();

		public class CookieAwareWebClient: WebClient {

        public CookieContainer Cookies = new CookieContainer();

        protected override WebRequest GetWebRequest(Uri address) {
            WebRequest request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                (request as HttpWebRequest).CookieContainer = Cookies;
            }
            return request;
			}
		}

		public void RaiseCallbackEvent(string arg) {
			Result = null;
			try {
				string cmd, par = string.Empty;
				int i = arg.IndexOf(':');
				if (i == -1) cmd = arg;
				else {
					cmd = arg.Substring(0, i);
					par = arg.Substring(i + 1);
				}

				switch (cmd) {
				case "load":
					Html.DocumentNode html;

					var preview = PreviewElement; // if Preview element exists load preview else HtmlElement.Children.
					if (preview != null) html = preview.Children;
					else html = Element.Children;
					EditorConverter.Export(html);
					Services.JavaScriptTextEditor.Converter.Export(html, out Result);
					break;
				case "save":
					var oldText = Element.Children.Text; // save old text
					
					preview = CreatePreviewElement();
					Services.JavaScriptTextEditor.Converter.Import(preview.Children, par);
					EditorConverter.Import(preview.Children);
					// TODO preview.Children.AddIdentation(preview.ChildIdentation);
					CheckParseErrors();
					Element.Preview();

					// page is now saved, now get the new html via a WebClient request to the saved page.
					try {
						var web = new CookieAwareWebClient(); // start a webrequest to the new preview page.
						web.UseDefaultCredentials = true;
						web.Encoding = Encoding.UTF8;
						web.Headers[HttpRequestHeader.UserAgent] = Request.UserAgent;
						//web.Headers[HttpRequestHeader.TransferEncoding] = "UTF8";
						//web.Headers[HttpRequestHeader.ContentEncoding] = "UTF8";
						foreach (var key in Request.Cookies.Keys.OfType<string>()) {
							var cookie = Request.Cookies[key];
							var wc = new Cookie(cookie.Name, cookie.Value, cookie.Path, Request.Url.Host);
							wc.Expires = cookie.Expires;
							web.Cookies.Add(wc);
						}

						Html.Element e = null;
						var dochtml = web.DownloadString(Request.Url.AbsoluteUri); // get the new preview page's html.
						if (dochtml != null) {

							// parse the received html page for the Container control.
							if (string.IsNullOrEmpty(Container.ClientID)) CreateChildControls(); // this is needed so we know the Container.ClientID, because otherwise Container get's not instantiated in CallbackEvent.
							var doc = new Html.Document(dochtml);
							e = doc.Find(Container.ClientID); // get our element out of the html.
						}
						if (e != null) {
							Result = e.Children.Text; // return the element's html
						} else {
							Element.Children.Text = oldText;
							Save();
							throw new Exception("Invalid HTML.");
						}
						
					} catch (Exception ex) { // exception viewing the new preview page
						Element.Children.Text = oldText; // reset text
						Save();
						Result = null;
						throw ex;
					}

					break;
				default: throw new NotSupportedException("EditableContent: Unknown callback command.");
				}
			} catch (Exception ex) {
				Services.Log.Error("EditableContent callback exception:", ex);
				throw ex;
			}
		}

		public string GetCallbackResult() { return Result; }

		// IJavaScriptHtmlEditable methods
		public string LoadScript {
			get { return "function(callback, error) {" + Page.ClientScript.GetCallbackEventReference(this, "'load'", "callback", "{ }", "error", true) + " }"; }
		}

		public string SaveScript {
			get { return "function(asp, callback, error) {" + Page.ClientScript.GetCallbackEventReference(this, "'save:' + asp", "callback", "{ }", "error", true) + " }"; }
		}

	}
}