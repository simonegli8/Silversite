using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Reflection;
using System.ComponentModel;
using Silversite;
using Silversite.Services;
using Silversite.Reflection;

namespace Silversite.Web.UI {

	public class Icon: System.Web.UI.WebControls.ImageButton {
		public Icon(): base() { EnableViewState = false; }
		public string Label { get { return AlternateText; } set { AlternateText = ToolTip = value; } }
		public string ConfirmText { get { return (string)ViewState["ConfirmText"]; } set { ViewState["ConfirmText"] = value; } }

		protected override void Render(HtmlTextWriter writer) {
			if (string.IsNullOrEmpty(CssClass)) Style[HtmlTextWriterStyle.Margin] = "0px 4px 0px 4px";
			if (!string.IsNullOrEmpty(ConfirmText)) OnClientClick = "return confirm(\"" + ConfirmText.Replace("\"", "\\\"") + "\");";
			/* ImageUrl = ResolveClientUrl(ImageUrl);
			if (ImageUrl.StartsWith("~")) {
				ImageUrl = this.ResolvePageUrl(ImageUrl);
			}
			Debug.Assert(!ImageUrl.StartsWith("~"));
			*/
			base.Render(writer);
		}
		public string TypeName { get { return Icons.Name(this); } }
	}

	public class DeleteIcon: Icon {
		public DeleteIcon() {
			Label = "Löschen"; CommandName = "Delete";
			ConfirmText = "Wollen Sie dieses Element wirklich löschen?";
			ImageUrl = "~/silversite/images/del.gif";
		}
	}

	public class EditIcon: Icon {
		public EditIcon() {
			Label="Bearbeiten"; CommandName="Edit"; ImageUrl = "~/silversite/images/lupe.png";
		}
	}

	public class TextEditIcon: Icon {
		public TextEditIcon() {
			Label="Bearbeiten"; CommandName="Edit"; ImageUrl="~/silversite/images/edit.png";
		}
	}

	public class AliasIcon: Icon {
		public AliasIcon() {
			Label="Verweisen"; CommandName="Alias"; ImageUrl = "~/silversite/images/aliasarrow.png";
		}
	}

	public class CancelIcon: Icon {
		public CancelIcon() {
			Label="Abbrechen"; CommandName="Cancel"; ImageUrl = "~/silversite/images/cancel.gif";
		}
	}

	public class UndoIcon: Icon {
		public UndoIcon() {
			Label = "Rückgängig"; CommandName = "Undo"; ImageUrl = "~/silversite/images/undo.png";
		}
	}

	public class RedoIcon: Icon {
		public RedoIcon() {
			Label = "Wiederherstellen"; CommandName = "Redo"; ImageUrl = "~/silversite/images/redo.png";
		}
	}

	public class DiscardIcon: Icon {
		public DiscardIcon() {
			Label = "Verwerfen"; CommandName = "Discard"; ImageUrl = "~/silversite/images/undo.png";
		}
	}

	public class RevertIcon: Icon {
		public RevertIcon() {
			Label = "Alte Version"; CommandName = "Revert"; ImageUrl = "~/silversite/images/undo.png";
		}
	}

	public class AddIcon: Icon {
		public AddIcon() {
			Label = "Neu..."; CommandName = "Add"; ImageUrl = "~/silversite/images/Add16.png";
		}
	}

	public class RemoveIcon: Icon {
		public RemoveIcon() {
			Label = "Entfernen..."; CommandName = "Remove"; ImageUrl = "~/silversite/images/Delete16.png";
		}
	}

	public class SaveIcon: Icon {
		public SaveIcon() {
			Label="Speichern"; CommandName="Save"; ImageUrl = "~/silversite/images/save.png";
		}
	}

	public class PublishIcon: Icon {
		public PublishIcon() {
			Label = "Publizieren"; CommandName = "Publish"; ImageUrl = "~/silversite/images/save.png";
		}
	}

	public class AcceptIcon: Icon {
		public AcceptIcon() {
			Label = "Akzeptieren"; CommandName = "Accept"; ImageUrl = "~/silversite/images/ok.gif";
		}
	}

	public class PrintIcon: Icon {
		public PrintIcon() {
			Label="Drucken"; CommandName="Print"; ImageUrl = "~/silversite/images/print.gif";
		}
	}

	public class PdfIcon: Icon {
		public PdfIcon() {
			Label="Pdf anschauen"; CommandName="PdfView"; ImageUrl = "~/silversite/images/pdf.gif";
		}
	}

	public class PdfUploadIcon: Icon {
		public PdfUploadIcon() {
			Label="Pdf hochladen"; CommandName="UploadPdf"; ImageUrl = "~/silversite/images/pdfupload.png";
		}
	}

	public class PdfLargeIcon: Icon {
		public PdfLargeIcon() {
			Label="Pdf anschauen"; CommandName="PdfView"; ImageUrl = "~/silversite/images/pdflarge.png";
		}
	}

	public class ExcelLargeIcon: Icon {
		public ExcelLargeIcon() {
			Label="Nach Excel exportieren"; CommandName="ExcelView"; ImageUrl = "~/silversite/images/excellarge.png";
		}
	}

	public class ExcelIcon: Icon {
		public ExcelIcon() {
			Label="Nach Excel exportieren"; CommandName="ExcelView"; ImageUrl = "~/silversite/images/excel.png";
		}
	}

	public class SendMailIcon: Icon {
		public SendMailIcon() {
			Label="Mail versenden"; CommandName="SendMail"; ImageUrl = "~/silversite/images/send.png";
		}
	}

	public class SendMailPreviewIcon: Icon {
		public SendMailPreviewIcon() {
			Label="Newsletter Vorschau versenden"; CommandName="SendMailPreview"; ImageUrl = "~/silversite/images/sendpreview.png";
		}
	}

	public class SendMailTestIcon: Icon {
		public SendMailTestIcon() {
			Label="Newsletter Test versenden"; CommandName="SendMailTest"; ImageUrl = "~/silversite/images/sendtest.png";
		}
	}

	public static class Icons {

		static Dictionary<string, Type> icons = null;

		public static void Init() {
			if (icons == null) {
				icons = new Dictionary<string,Type>();
				foreach (var t in Types.GetAllCustom<Icon>()) {
					icons.Add(Name(t), t);
				}
			}
		}

		static string Trim(string name, string postfix) {
			if (name.EndsWith(postfix)) return name.Remove(name.Length - postfix.Length);
			return name;
		}

		public static Type Type(string name) {
			Init();
			Type t;
			if (icons.TryGetValue(Trim(name, "Icon"), out t)) return t;
			return null;
		}

		public static void GetPar(ref string name, out string p) {
			p = null;
			if (name != null) {
				int i = name.IndexOf(':');
				if (i != -1) {
					p = name.Substring(i+1);
					name = name.Substring(0, i);
				}
			}
		}

		public static void Set(Icon icon, string p) {
			string p2;
			GetPar(ref p, out p2);
			if (p2 != null) p = p2;

			if (p != null) {
				var tokens = p.SplitList(';');
				foreach (var token in tokens) {
					try {
						var expr = token.Split('=');
						var prop = expr[0];
						var val = expr[1];
						icon.Property(prop).Value = val;
					} catch { }
				}
			}
		}

		public static Icon New(string name) {
			string p ;
			GetPar(ref name, out p);
		
			Type t = Type(name);
			if (t == null) return null;

			Icon icon = Silversite.New.Object(t) as Icon;
			Set(icon, p);
			return icon;
		}

		public static string Name(Type t) { return Trim(t.Name, "Icon"); }
		public static string Name(Icon icon) { return Name(icon.GetType()); }
	}

	[ParseChildren(false, "TemplateControls")]
	public class IconBar: Panel, IBindableTemplate {

		public IconBar() { TemplateControls = new List<Control>(); }
		public IconBar(string set) : this() { Set = set; }
		public IconBar(string set, string iconClass) : this(set) { IconCssClass = iconClass; }
		public IconBar(string set, string iconClass, string CommandArgument) : this(set, iconClass) { this.CommandArgument = CommandArgument; }

		Control instantiatedContainer = null;

		[Category("Behavior")]
		[Browsable(true)]
		public string Set { get { return (string)ViewState["Set"] ?? string.Empty; } set { ViewState["Set"] = value; Update(); } }

		[Category("Appearance")]
		[Browsable(true)]
		public string IconCssClass { get { return (string)ViewState["IconCssClass"]; } set { ViewState["IconCssClass"] = value; } }

		[Category("Behavior")]
		[Browsable(true)]
		public string CommandArgument { get { return (string)ViewState["CommandArgument"]; } set { ViewState["CommandArgument"] = value; Update(); } }

		public List<Control> TemplateControls { get; private set; } 

		void Add(Control container, Control c) {
			if (c is Icon) ((Icon)c).CssClass = IconCssClass;
			container.Controls.Add(c);
		}

		void Update() {
			if (instantiatedContainer != null) {
				var names = Set.Split('|');
				int i = 0, j = 0, n;
				foreach (var name in names) {
					if (name == "*") {
						while (i < TemplateControls.Count) { j++; i++; }
					} else if (int.TryParse(name, out n)) {
						while (i < TemplateControls.Count && n > 0) { j++; i++; }
					} else {
						Icon icon = null;
						if (j < instantiatedContainer.Controls.Count) icon = instantiatedContainer.Controls[j++] as Icon;
						if (icon != null) {
							Icons.Set(icon, name);
							if (!string.IsNullOrEmpty(CommandArgument)) icon.CommandArgument =  CommandArgument;
						}
					}
				}
			}
		}

		public void InstantiateIn(Control container) {
			var names = Set.Split('|');
			int i = 0, n;
			foreach (var name in names) {
				if (name == "*") {
					while (i < TemplateControls.Count) Add(container, TemplateControls[i++]);
				} else if (int.TryParse(name, out n)) {
					while (i < TemplateControls.Count && n > 0) Add(container, TemplateControls[i++]);
				} else {
					var icon = Icons.New(name);
					if (icon != null) {
						if (!string.IsNullOrEmpty(CommandArgument)) icon.CommandArgument =  CommandArgument;
						Add(container, icon);
					}
				}
			}
			instantiatedContainer = container;
			container.DataBind();
		}

		public System.Collections.Specialized.IOrderedDictionary ExtractValues(Control container) {
			var d = new System.Collections.Specialized.OrderedDictionary();
			d["Set"] = Set;
			d["CommandArgument"] = CommandArgument;
			d["IconCssClass"] = IconCssClass;
			return d;
			// throw new NotImplementedException();
		}


		protected override void CreateChildControls() {
			base.CreateChildControls();
			Controls.Clear();
			InstantiateIn(this);
		}

	}
}
