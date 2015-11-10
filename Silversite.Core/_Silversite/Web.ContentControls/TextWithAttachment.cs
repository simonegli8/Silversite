using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.ComponentModel;

namespace Silversite.Web.UI {
	public class TextWithAttachment: EditableContent {

		public Icon UploadButton = new PdfUploadIcon();
		public FileUpload FileUpload = new FileUpload();

		protected override void  OnLoad(EventArgs e) {
			base.OnLoad(e);
			if (IsPostBack && FileUpload.HasFile && Services.Paths.Extension(FileUpload.FileName) == ".pdf") { // FileUpload has a file to upload

				// upload file
				Services.Persons.Current.CreateHomeFolder();
				var home = Services.Persons.Current.HomePath;
				home = Services.Paths.Combine(home, "Attachments");
				Services.Files.CreateDirectory(home);
				var file = Services.Paths.Combine(home, FileUpload.FileName);
				var nfile = file;
				int n = 0;
				while (Services.Files.FileExists(nfile)) { nfile = Services.Paths.ChangeExtension(file, n.ToString() + ".pdf"); n++; }
				FileUpload.SaveAs(Services.Paths.Map(nfile));

				// create attachment
				var at = Element.FindControl<Attachment>();
				if (at == null) { at = new Html.Element(); at.Type = typeof(Attachment); at.IsServerControl = true; Element.Children.Add(at); }
				at.Attributes["File"] = nfile;
				Preview();
			}
		}
		
		protected override void  OnMenuCommand(CommandEventArgs c) {
			base.OnMenuCommand(c);
			switch (c.CommandName) {
			case "Remove":
				var at = Element.FindControl<Attachment>();
				if (at != null) Element.Children.Remove(at);
				Preview();
				break;
			default: break;
			}
		}

		protected override void CreateChildControls() {
			base.CreateChildControls();

			UploadButton.OnClientClick = "$(#'" + PopupMenu.ClientID + "').toggle();";

			ButtonsMenu.Controls.AddAt(1, UploadButton);
			PopupMenu.Controls.Add(FileUpload);
		}
	}

	public class Attachment: UserControl {

		[Category("Behavior")]
		[Browsable(true)]
		public string File { get { return (string)ViewState["File"]; } set { ViewState["File"] = value; } }

		public Icon RemoveButton = new RemoveIcon();

		public EditableContent.Modes Mode {
			get {
				var p = Parent;
				while (p != null && !(p is EditableContent)) p = p.Parent;
				if (p != null) return ((EditableContent)p).Mode;
				return EditableContent.Modes.View;
			}
		}

		protected override void CreateChildControls() {
			base.CreateChildControls();

			if (!string.IsNullOrEmpty(File)) {
				var p = new HtmlGenericControl("p");
				Controls.Add(p);
				if (Mode == EditableContent.Modes.Edit) p.Controls.Add(RemoveButton);
				var link = new HyperLink();
				link.NavigateUrl = ResolveClientUrl(File);
				var icon = new PdfIcon();
				link.Controls.Add(icon);
				var literal = new Literal();
				literal.Text = Services.Paths.File(File);
				link.Controls.Add(icon);
				link.Controls.Add(literal);
				p.Controls.Add(link);
			}
		}
	}

}