using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Silversite;

namespace Silversite.Web.UI {

	public partial class ExceptionControlBase : System.Web.UI.UserControl {

		public Exception Exception { get { return ViewState["Exception"] as Exception; } set { ViewState["Exception"] = value; } }
		public Services.LogMessage LogMessage { get { return ViewState["LogMessage"] as Services.LogMessage; } set { ViewState["LogMessage"] = value; } }
		public int Frame { get { return (int)(ViewState["Frame"] ?? 0); } set { ViewState["Frame"] = value; } }

		protected override void OnInit(EventArgs e) {
			base.OnInit(e);
		}

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);
			CreateControls();
		}

		Exception rex = null;
		Services.LogMessage rmsg = null;
		int rframe = -1;

		protected override void CreateChildControls() {
			base.CreateChildControls();
			CreateControls();
		}

		protected override object SaveViewState() {
			CreateControls();
			return base.SaveControlState();
		}

		protected void CreateControls() {

			if (rex == Exception && rmsg == LogMessage && rframe == Frame) return;

			Exception = Exception ?? Session["Silversite.Exception"] as Exception;
			if (Exception == null && LogMessage != null) Exception = LogMessage.Exception;

			var ex = rex = Exception;
			var log = rmsg = LogMessage;

			string sourcefile, sourcetext, sourcemethod;
			int sourceline;

			if (ex != null) {
				ex.Info(Frame, out sourcemethod, out sourcefile, out sourceline, out sourcetext);
				rframe = Frame;
			} else if (log != null) {
				sourcefile = log.SourceFile;
				sourceline = log.SourceLine;
				sourcetext = log.SourceText;
				rframe = 0;
			} else {
				sourcetext = sourcefile = null;
				sourceline = 0;
				rframe = 0;
			}

			file.Text = sourcefile ?? "";
			line.Text = sourceline.ToString();
			if (sourcetext != null) {
				var sourcelines = HttpUtility.HtmlEncode(sourcetext).Replace("\t", "&nbsp;&nbsp;&nbsp;").SplitList('\n').ToList();

				for (int i = 0; i < sourcelines.Count; i++) {
					var l = i + 1;
					if (l != 5) sourcelines[i] = "<span style=\"color:darkgreen\">Line " + (l + sourceline - 5).ToString() + ":</span> &nbsp; &nbsp; &nbsp; &nbsp; " + sourcelines[i] + "<br/>";
					else sourcelines[i] = "<span style=\"color:red;\">Line " + (l + sourceline - 5).ToString() + ": &nbsp; &nbsp; &nbsp; &nbsp; " + sourcelines[i] + "</span><br/>";
				}

				source.Text = sourcelines.String();
			}
			if (ex != null) {

				stacktitle.Text = ex.GetType().FullName + ": " + ex.Message;

				//var framestext = ex.RelativeStackTrace().SplitList('\n');
				frames.Controls.Clear();
				for (int n = 0; n < ex.FrameCount(); n++) {
					int frame = n;
					ex.Info(n, out sourcemethod, out sourcefile, out sourceline, out sourcetext);
					var button = new LinkButton();
					button.ID = "framebutton" + frame;
					button.Text = " at <span style=\"color:darkgreen\">" + HttpUtility.HtmlEncode(sourcemethod) + "</span>";
					if (sourcefile != null) button.Text += "<br/><span style=\"color:darkblue\">" + HttpUtility.HtmlEncode(sourcefile) + ":" + sourceline.ToString() + "</span>";
					button.Click += new EventHandler((sender, args) => {
						Frame = frame; CreateControls();
					});
					var br = new Literal();
					br.Text = "<br/>";
					frames.Controls.AddAt(frames.Controls.Count, button);
					frames.Controls.AddAt(frames.Controls.Count, br);
				}

				if (ex is System.Data.Entity.Validation.DbEntityValidationException) {
					var lines = new List<string>();
					foreach (var errs in ((System.Data.Entity.Validation.DbEntityValidationException)ex).EntityValidationErrors) {
						foreach (var err in errs.ValidationErrors) {
							lines.Add(HttpUtility.HtmlEncode(err.PropertyName + ": " + err.ErrorMessage));
						}
					}
					info.Text = lines.StringList("<br/>");
					infopanel.Visible = true;
				} else {
					infopanel.Visible = false;
				}
			}
		}
	}
}