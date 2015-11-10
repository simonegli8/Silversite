using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.ComponentModel;

namespace Silversite.Web.UI {

	public class ProviderMessagesMonitor: Panel {

		public ProviderMessagesMonitor() { Watch = false; }
		
		[Category("Behavior")]
		[Browsable(true)]
		public Type Sender { get { return (Type)ViewState["Sender"]; } set { ViewState["Sender"] = value; } }

		[Category("Behavior")]
		[Browsable(true)]
		public bool Verbose { get { return (bool)(ViewState["Verbose"] ?? false); } set { ViewState["Verbose"] = value; } }

		[Category("Behavior")]
		[Browsable(true)]
		public string Redirect { get { return (string)ViewState["Redirect"]; } set { ViewState["Redirect"] = value; } }

		[Category("Behavior")]
		[Browsable(true)]
		public int Timeout { get { return (int)(ViewState["Redirect"] ?? 60000); } set { ViewState["Redirect"] = value; } }

		public bool Watch { get; set; }

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);
			Watch = false;
		}

		public void Clear() {
			while (Services.Providers.Messages.Count > 0) {
				var msg = Services.Providers.Messages.Dequeue();
			}
		}

		public void Start(object sender) {
			Clear();
			Sender = sender != null ? sender.GetType() : null;
			Watch = true;
		}

		[Category("Behavior")]
		[Browsable(true)]
		public event EventHandler Finished; 

		protected override void RenderContents(System.Web.UI.HtmlTextWriter writer) {
			base.RenderContents(writer);
			if (Watch) {
				Watch = false;
				var msg = Services.Providers.Messages.DequeueOrBlock(Timeout);
				while (!(msg.Finished && (Sender == null || Sender.IsInstanceOfType(msg.Sender)))) {
					if (Verbose && msg.Exception != null) {
						writer.Write("<p style='padding: 0 0 0 0; margin: 0 0 0 10px; color:red;'>");
						writer.Write(msg.Text);
						writer.Write("<br/>");
						writer.Write(HttpUtility.HtmlEncode(msg.Exception.InnerException.Message).Replace("\n", "<br/>"));
						writer.Write("</p>");
					} else {
						writer.Write(msg.Text + "</br>");
					}
					writer.Flush();
					Page.Response.Flush();
					msg = Services.Providers.Messages.DequeueOrBlock();
					//HttpContext.Current.Response.Flush();
				}
				if (Finished != null) Finished(this, EventArgs.Empty);
				if (!string.IsNullOrEmpty(Redirect)) {
					System.Threading.Thread.Sleep(3000);
					writer.Write("<script language= \"JavaScript\">window.location = \"" + ResolveClientUrl(Redirect) + "\";</script>");
				}
			}
		}

	}
}