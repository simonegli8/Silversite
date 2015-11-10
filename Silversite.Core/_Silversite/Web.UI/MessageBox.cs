using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Threading;

namespace Silversite.Web.UI {

	public class MessageBoxWaiter : System.Web.UI.Control {

		MessageBox Box { get; set; }

		public MessageBoxWaiter(MessageBox box) { Box = box; }

		protected override void Render(System.Web.UI.HtmlTextWriter writer) {
			base.Render(writer);
			Monitor.Exit(Page.Items[MessageBox.LockID]);
			Page.Response.Flush();
			Box.Signal.Wait();
		}
	}

	public class MessageBox: Panel, IDisposable {

		public ManualResetEventSlim Signal { get; set; }
		public const string LockID = "MessageLock";

		public MessageBox() {
			Signal = new ManualResetEventSlim(true);
		}

		protected override void OnInit(EventArgs e) {
			base.OnInit(e);
			Page.Items[LockID] = new object();
			Monitor.Enter(Page.Items[LockID]);
		}

		/*
		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);
			Page.PreRender += (sender, args) => {
				Page.Controls.Add(new MessageBoxWaiter(this));
			};
		}
		*/

		protected override object SaveViewState() {
			var f = Page.Form;
			var p = f.Parent;
			var i = p.Controls.IndexOf(f);
			p.Controls.AddAt(i+1, new MessageBoxWaiter(this));
			return base.SaveViewState();
		}

		public void Show() { Signal.Reset(); }
		public void Write(string text) {
			Show();
			lock (Page.Items[LockID]) {
				Page.Response.Write("<script type=\"text/javascript\">var box = document.getElementById(\"" + ClientID + "\"); box.innerHTML = box.innerHTML + \"" + text + "\";</script>"+Environment.NewLine);
				Page.Response.Flush();
			}
		}
		public void Write(string text, params object[] args) { Write(string.Format(text, args)); }
		public void WriteLine(string text) { Write(text + "<br/>"); }
		public void WriteLine(string text, params object[] args) { WriteLine(string.Format(text, args)); }
		public void Message(string text) { Write(HttpUtility.HtmlEncode(text)); }
		public void Message(string text, params object[] args) { Message(string.Format(text, args)); }
		public void MessageLine(string text) { Message(text + "<br/>"); }
		public void MessageLine(string text, params object[] args) { MessageLine(string.Format(text, args)); }
		public void End() { Signal.Set(); }
		public void Close() { End(); }
		public void Dispose() { End(); }
	}
}