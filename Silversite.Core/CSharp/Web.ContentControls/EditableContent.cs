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
using System.Configuration;

using Silversite.Services;

// TODO Move Html Parser library to Silversite.Common to speed up Silversite.Core loading
// The EditableContent is the only class in Silversite.Core using the Html Parser, so implement a Service for EditableContent with a Html Parser Provider

namespace Silversite.Web.UI {


	// global configuration of the EditableContent
	[Configuration.Section(Path = EditableConfiguration.Path, Name = "editableContent")]
	public class EditableConfiguration: Configuration.Section {

		public new const string Path = ConfigRoot + "/silversite.config";

		[ConfigurationProperty("effects", IsRequired = false, DefaultValue = false)]
		public bool Effects { get { return (bool)(this["effects"] ?? false); } set { this["effects"] = value; } }
	}

	[Flags]
	public enum Position { None = 0, Top = 1, Bottom = 2, Left = 4, Right = 8 }; // menu position

	[ToolboxData("<{0}:EditableContent runat=\"server\" Path=\"\"/>")]
	/* [ParseChildren(ChildrenAsProperties = false, "Controls")]
	[PersistChildren(true)] */
	///<summary>
	/// The base class for Controls who's content can be edited by the user.
	///</summary>
	public class EditableContent : System.Web.UI.WebControls.Panel, ICallbackEventHandler, IJavaScriptTextEditable, Html.IDocument, INamingContainer {

		class script1 { }
		class script2 { }

		public class PersistentStateCollection {

			EditableContent Editable;
			Dictionary<string, object> dict = null;

			public PersistentStateCollection(EditableContent editable) { Editable = editable; }

			private Html.Container Element { get { return Editable.PreviewElement != null ? Editable.PreviewElement : Editable.Element; } }

			public void Load() {
				if (dict == null) {
					dict = new Dictionary<string, object>();
					foreach (var po in Element.Children.All<Html.PersistentObject>()) dict.Add(po.Name, po.Value);
				}
			}

			public void Save() {
				foreach (var po in Element.Children.All<Html.PersistentObject>()) Editable.Element.Children.Remove(po);
				foreach (var name in dict.Keys) Element.Children.Add(new Html.PersistentObject() { Name = name, Value = dict[name] });
			}

			public object this[string name] {
				get {
					Load();
					object x = null;
					dict.TryGetValue(name, out x);
					return x;
				}
				set { dict[name] = value; }
			}
		}

		public EditableConfiguration Configuration = new EditableConfiguration();

		#region Style
		[Category("Behavior")]
		[Browsable(true)]
		public string Path { get { return (string)ViewState["Path"]; } set { ViewState["Path"] = value; } }

		[Category("Behavior")]
		[Browsable(true)]
		public bool PlaceHolder { get { return (bool)(ViewState["PlaceHolder"] ?? false); } set { ViewState["PlaceHolder"] = value; } }

		[Category("Appearance")]
		[DefaultValue(typeof(Color), "")]
		[TypeConverter(typeof(WebColorConverter))]
		public virtual Color BackColor { get { return Container.BackColor; } set { Container.BackColor = value; } }

		[Category("Appearance")]
		[DefaultValue(typeof(Color), "")]
		[TypeConverter(typeof(WebColorConverter))]
		public virtual Color BorderColor { get { return Container.BorderColor; } set { Container.BorderColor = value; } }

		[Category("Appearance")]
		public virtual BorderStyle BorderStyle { get { return Container.BorderStyle; } set { Container.BorderStyle = value; } }

		[Category("Appearance")]
		[DefaultValue(typeof(Unit), "")]
		public virtual Unit BorderWidth { get { return Container.BorderWidth; } set { Container.BorderWidth = value; } }

		[Category("Appearance")]
		[DefaultValue(typeof(Unit), "")]
		public virtual Unit BorderRadius { get; set; }

		[Category("Appearance")]
		[DefaultValue("")]
		[CssClassProperty]
		public virtual string CssClass { get { return Container.CssClass; } set { Container.CssClass = value; } }

		[Category("Appearance")]
		[DefaultValue(null)]
		public virtual string Buttons { get { return (string)ViewState["Buttons"]; } set { ViewState["Buttons"] = value; } }

		[Category("Appearance")]
		[DefaultValue(null)]
		public virtual Position MenuPosition { get { var p = Position.Top | Position.Right; Enum.TryParse<Position>((string)ViewState["MenuPosition"], out p); return p; } set { ViewState["MenuPosition"] = value.ToString(); } }

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public CssStyleCollection Style { get { return Container.Style; } }
		#endregion

		public virtual JavaScriptTextEditor Editor { get; set; }

		// default settings
		public EditableContent()
			: base() {
			Container = new Panel();
			Menu = new Panel();
			Content = new Panel();
			ButtonsMenu = new Panel();
			PopupMenu = new Panel();
			Path = string.Empty;
			//BorderRadius = new Unit("5px");
			//BorderColor = Color.FromArgb(0xAA, 0xAA, 0xAA);
			//BorderWidth = new Unit("1px");
			//BorderStyle = BorderStyle.Dashed;
			//CssClass = "Silversite.EditableContent";
			Editor = new JavaScriptTextEditor();
			EditorConverter = new Html.Converter();
			if (EditorSettings.Rights == Html.EditorRight.NoAsp) ContentClass = TextContentClass.Html;
			else ContentClass = TextContentClass.Aspx;
			//container.Style.Add(HtmlTextWriterStyle.Padding, "5px");
		}

		// ??
		public string TypeName {
			get {
				var path = HttpUtility.UrlEncode(Path);
				return path;
			}
		}

		[PersistenceMode(PersistenceMode.InnerDefaultProperty)]
		public override ControlCollection Controls {
			get {
				return base.Controls;
			}
		}

		// if path is not empty load a UserControl from path and display its contents instead of Controls.
		Control LoadedControl = null;
		bool UserControl { get { return !string.IsNullOrEmpty(Path); } } // we must load a UserControl

		protected override void OnInit(EventArgs e) {
			try {
				if (UserControl) {
					var userControl = new UserControl();
					LoadedControl = userControl.LoadControl(Path);
				}
			} catch { }
			base.OnInit(e);
		}

		public virtual string PagePath { get { if (!UserControl) return Paths.Normalize(Page.AppRelativeVirtualPath); return Path; } } // the path to the aspx file.

		protected Html.Document
		// deal with the preview Html.Element
		Html.Container element = null;
		protected Html.Container Element { // the Html.Element in the aspx DOM corresponding to this EditableContent.
			get {
				if (element == null) {
					if (Info.ContentKey != Services.Document.None) {
						element = Html.Container.Open(this) as Html.Container;
					} else {
						Html.Document.CreateContentKeys(Page);
						Refresh();
					}
				}
				return element;
			}
		}
		protected Html.Element PreviewElement { get { return Element.Control<Preview>(); } } // the Html.Element corresponding to the preview element.

		protected Html.Element CreatePreviewElement() { // creates a preview element if there is none.
			var preview = PreviewElement;
			if (preview == null) {
				preview = new Html.Element();
				preview.Identation = Element.Identation + 1;
				preview.Type = typeof(Preview);
				Element.Children.AddAt(0, preview);
				if (Element is Html.Element) ((Html.Element)Element).StartTag.NewLineAfter.Value = "\r\n";
			}
			preview.IsServerControl = true;
			return preview;
		}
		// deal with the Preview Control.
		protected Preview PreviewControl { get { return Controls.OfType<Preview>().FirstOrDefault(); } } // returns the Preview Control or null if there is no preview control.
		protected bool HasPreview { get { return PreviewControl != null; } } // this EditableContent has a preview.
		DocumentInfo info = null;
		public DocumentInfo Info {
			get {
				if (info == null) {
					var p = PreviewControl;
					if (p != null) info = p.Controls.OfType<DocumentInfo>().FirstOrDefault();
					else info = Controls.OfType<DocumentInfo>().FirstOrDefault();
					if (info == null) info = new DocumentInfo();
				}
				return info;
			}
		}

		DocumentInfo pubinfo = null;
		public DocumentInfo PublishedInfo {
			get {
				if (pubinfo == null) {
					pubinfo = Controls.OfType<DocumentInfo>().FirstOrDefault();
					if (pubinfo == null) pubinfo = new DocumentInfo();
				}
				return pubinfo;
			}
		}

		int? Html.IDocument.ContentKey { get { return Info.ContentKey; } }

		PersistentStateCollection persistentState = null;
		public PersistentStateCollection PersistentState { get { return persistentState ?? (persistentState = new PersistentStateCollection(this)); } }

		protected virtual void CheckParseErrors() {
			if (Element.Parser.Errors.Count > 0) throw new ArgumentException("There are errors in your document.<br/>" + Element.Parser.ErrorReport);
			if (Element.HasRestrictedServerCode(EditorSettings.Rights)) throw new NotSupportedException("You have insufficient rights to edit ASP.NET contents.");
		}

		protected void Refresh() { // reload the page.
			var path = Page.Request.AppRelativeCurrentExecutionFilePath;
			var dompath = Domains.Path(path);
			if (Files.FileExists(dompath)) Page.Server.Execute(dompath);
			else Page.Server.Execute(path);
			Page.Response.End();
		}

		protected virtual bool CanUndo { get { return (HasPreview && Info.Revision > 0) || PublishedInfo.Revision > 0; } }
		protected virtual bool CanRedo { get { return HasPreview && Info.Revision <= PublishedInfo.Revision; } }
		protected virtual bool CanPublish { get { return PreviewControl != null; } }
		public virtual bool CanDelete { get; set; }
		public virtual bool CanEdit { get { return true; } }

		protected virtual void Preview() { // set EditableContent to preview mode.
			var p = CreatePreviewElement();
			p.Children = Element.Children; // clone.
			Info.Revision = PublishedInfo.Revision + 1;
			p.Info = Info;
			PersistentState.Save();
			Element.Preview();
		}
		protected virtual void Publish() { // publish the preview content
			Info.Published = DateTime.Now;
			var p = PreviewElement;
			if (p != null) {
				Element.Children = p.Children;
				Element.Info = Info;
				PersistentState.Save();
				Element.Publish();
			}
		}

		protected virtual void Revert(int revision) { // revert the content to revision
			if (revision >= 0 && (revision <= PublishedInfo.Revision || HasPreview && (revision == PublishedInfo.Revision + 1))) {
				var p = CreatePreviewElement();
				//p.Revert(revision);
				Element.Save();
				Refresh();
			}
		}

		public virtual void Delete() {
			var doc = Element.Parent.DocumentElement;
			if (doc != null) {
				Element.Parent.Remove(Element);
				doc.Publish();
			}
		}

		protected virtual void OnMenuCommand(CommandEventArgs c) {
			switch (c.CommandName.ToLower()) {
				case "edit": break; // Edit is handled on the client side
				case "undo": Revert(Info.Revision - 1); break;
				case "redo": Revert(Info.Revision + 1); break;
				case "publish": Publish(); break;
				case "delete": Delete(); break;
				default: break;
			}
		}
		void HandleMenuCommand(object sender, CommandEventArgs c) { OnMenuCommand(c); }

		public enum Modes { View, Edit };
		Nullable<Modes> mode = null;

		public virtual Modes Mode { // the edit mode the EditableContent is in.
			get {
				var edit = false;
				if (!mode.HasValue) {
					string sessionmode = null;
					try {
						if (HttpContext.Current.Session != null) sessionmode = (string)Page.Session["silversite.pagemode"];
					} catch { }
					if ((Page.Request.QueryString["silversite.pagemode"] == "edit" || sessionmode == "edit") && Editor.IsAvailable) {
						edit = EditRights.IsEditable(Info, Persons.Current);
						edit &= Element != null && !Element.HasRestrictedServerCode(EditorSettings.Rights);
						edit &= Web.UI.Scripts.jQuery.Register(Page) && Web.UI.Scripts.Register(Page, "~/Silversite/JavaScript/Silversite.Overlays.js");
					}
				}
				return edit ? Modes.Edit : Modes.View;
			}
		}

		// user settings for the editor
		protected Person.EditorSettingsClass EditorSettings { get { return Persons.Current != null ? Persons.Current.EditorSettings : Persons.Default.EditorSettings; } }
		public TextContentClass ContentClass { get { return (TextContentClass)ViewState["ContentClass"]; } set { ViewState["ContentClass"] = value; } }

		// rendered controls
		public UpdatePanel Update = new UpdatePanel();
		public Icon EditButton = new EditIcon();
		public Icon UndoButton = new UndoIcon();
		public Icon RedoButton = new RevertIcon();
		public Icon PublishButton = new PublishIcon();
		public Icon DeleteButton = new DeleteIcon();
		//public Icon PropertiesButton = new PropertiesIcon();
		public Panel Menu { get; private set; }
		public Panel ButtonsMenu { get; private set; }
		public Panel PopupMenu { get; private set; }
		public Panel Content { get; private set; }
		public Panel Container { get; private set; }
		public HiddenField EditableText { get; private set; }
		public HiddenField OriginalText { get; private set; }

		protected override void CreateChildControls() {
			base.CreateChildControls();

			if (LoadedControl != null) {
				Controls.Clear();
				Controls.Add(LoadedControl);
			}

			if (Mode == Modes.Edit) { // control is in edit mode

				EditableText = new HiddenField();
				Export();
				string html;
				Editor.Converter.Export(Element.Children, out html);
				EditableText.Value = html;
				EditableText.ID = "EditableText";
				OriginalText = new HiddenField();
				OriginalText.ID = "OriginalText";

				ButtonsMenu.Controls.Clear();
				ButtonsMenu.ID = "Buttons";
				ButtonsMenu.Controls.Add(DeleteButton);
				ButtonsMenu.Controls.Add(PublishButton);
				ButtonsMenu.Controls.Add(UndoButton);
				ButtonsMenu.Controls.Add(RedoButton);
				ButtonsMenu.Controls.Add(EditButton);
				ButtonsMenu.Style.Add("border-bottom", "1px dotted #AAAAAA");
				ButtonsMenu.Style.Add("border-left", "1px dotted #AAAAAA");
				ButtonsMenu.Style.Add("border-radius", "2px");

				PopupMenu.Style[HtmlTextWriterStyle.Visibility] = "hidden";
				PopupMenu.ID = "Popup";

				var v = MenuPosition & (Position.Top | Position.Bottom);
				var h = MenuPosition & (Position.Left | Position.Right);
				if (v == Position.None) MenuPosition |= Position.Top;
				if (v == (Position.Top | Position.Bottom)) MenuPosition &= Position.Top;
				if (h == Position.None) MenuPosition |= Position.Right;
				if (h == (Position.Left | Position.Right)) MenuPosition &= Position.Right;

				Menu.Controls.Clear();
				Menu.ID = "Menu";
				Menu.Controls.Add(PopupMenu);
				if ((MenuPosition & Position.Bottom) != Position.None) Menu.Controls.AddAt(0, ButtonsMenu);
				else Menu.Controls.Add(ButtonsMenu);

				Menu.Style.Add(HtmlTextWriterStyle.Position, "absolute");
				if ((MenuPosition & Position.Top) != Position.None) Menu.Style.Add("top", "5px");
				if ((MenuPosition & Position.Bottom) != Position.None) Menu.Style.Add("bottom", "5px");
				if ((MenuPosition & Position.Left) != Position.None) Menu.Style.Add("left", "5px");
				if ((MenuPosition & Position.Right) != Position.None) Menu.Style.Add("right", "5px");

				Content.Controls.Clear();
				Content.ID = "Content";

				var preview = Controls.OfType<Preview>().FirstOrDefault();
				List<Control> controls = null;
				if (preview != null) controls = preview.Controls.OfType<Control>().ToList();
				else controls = Controls.OfType<Control>().ToList();
				Controls.Clear();
				foreach (Control ct in controls) Content.Controls.Add(ct);

				Container.Controls.Clear();
				Container.ID = "Container";
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

				Update.ID = "Update";
				Update.ContentTemplateContainer.Controls.Add(Menu);
				Update.ContentTemplateContainer.Controls.Add(Content);
				Update.ContentTemplateContainer.Controls.Add(OriginalText);
				Update.ContentTemplateContainer.Controls.Add(EditableText);

				var script = new Script();
				Container.Controls.Add(Update);
				Container.Controls.Add(script);

				Controls.Add(Container);

				DeleteButton.ConfirmText = "Wollen sie diesen Eintrag wirklich löschen?";
				PublishButton.OnClientClick += "Silversite.Overlays.Wait($('#" + Container.ClientID + "')); return true;";
				UndoButton.OnClientClick += "Silversite.Overlays.Wait($('#" + Container.ClientID + "')); return true;";
				RedoButton.OnClientClick += "Silversite.Overlays.Wait($('#" + Container.ClientID + "')); return true;";
				Editor.Menu = EditorSettings.Menu;
				EditButton.OnClientClick = Editor.ClientOpenEditorScript(this) + "; return false;";

				if (Configuration.Effects) {
					script.Text = "$(document).ready(function() { Silversite.Overlays.InitPopup('" + Container.ClientID + "'); });";
				}
			}
		}

		protected override object SaveViewState() {
			if (Mode == Modes.Edit) {
				if (!CanPublish) PublishButton.Style.Add(HtmlTextWriterStyle.Display, "none");
				if (!CanDelete) DeleteButton.Style.Add(HtmlTextWriterStyle.Display, "none");
				if (!CanUndo) UndoButton.Style.Add(HtmlTextWriterStyle.Display, "none");
				if (!CanRedo) RedoButton.Style.Add(HtmlTextWriterStyle.Display, "none");
			}
			return base.SaveViewState();
		}

		public virtual string ClientOpenEditorScript {
			get {
				return "function() { $('#" + MenuPanel.ClientID + "').hide();" +
					"$('#" + OriginalText.ClientID + "').val($('#" + Content.ClientID + "').html());" +
					"$('#" + Content.ClientID + "').html($('#" + EditableText.ClientID + "').val()); }";
			}
		}

		public virtual string ClientSaveEditorScript {
			get {
				return "function() { $('#" + MenuPanel.ClientID + "').show();" +
					"var text = $('#" + Content.ClientID + "').html();" +
					Page.ClientScript.GetCallbackEventReference(this, "text",
						"function(args, context) { $('#" + Content.ClientID + "').html(args); }", // text saved
						"{}",
						"function(args, context) { $('#" + Content.ClientID + "').html($('#" + OriginalText.ClientID + "').val()); }", // error saving text
						true) +
					"$('#" + OriginalText.ClientID + "').val(''); }";
			}
		}

		public virtual string ClientCancelEditorScript {
			get {
				return "function() { $('#" + MenuPanel.ClientID + "').hide();" +
					"$('#" + Content.ClientID + "').html($('#" + OriginalText.ClientID + "').val());" +
					"$('#" + OriginalText.ClientID + "').val(''); }";
			}
		}

		public virtual string ContentType { get { return (string)ViewState["ContentType"]; } set { ViewState["ContentType"] = value; } }

		Panel MenuPanel { get { return (Panel)Update.FindControl("Menu"); } }
		Panel ContainerPanel { get { return (Panel)Update.FindControl("Container"); } }
		Panel ContentPanel { get { return (Panel)Update.FindControl("Content"); } }
		Control IJavaScriptTextEditable.Content { get { return ContentPanel; } }

		protected override void Render(HtmlTextWriter writer) {
			var oldcss = CssClass;
			CssClass += " Silversite.EditableContent." + Mode.ToString();
			base.Render(writer);
			CssClass = oldcss;
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Html.Converter EditorConverter { get; set; } // the converter that converts the content for the javascript editor.

		public virtual void Export() { // export the content for the javascript editor
			var e = (PreviewElement ?? Element);
			EditorConverter.Export(e.Children);
			Element.Children = e.Children;
			CheckParseErrors();
		}
		public virtual void Import() { // import the content from the javascript editor
			EditorConverter.Import(Element.Children);
			CheckParseErrors();
		}

		// ajax callback
		string Result = string.Empty;
		public string GetCallbackResult() { return Result; } // return callback result to ajax

		public void RaiseCallbackEvent(string arg) {  // raise a callback event from ajax
			if (string.IsNullOrEmpty(Container.ClientID)) CreateChildControls(); // this is needed so we know the Container.ClientID, because otherwise Container get's not instantiated in CallbackEvent.
			Result = null;
			try {
				var oldText = Element.Children.Text; // save old text

				// save new document
				Editor.Converter.Import(Element.Children, arg);
				CheckParseErrors();
				Import();

				try {
					Preview(); // save preview
					var re = Element.Rendered(); // get rendered html.
					if (re == null) throw new Exception("Invalid HTML.");
					Result = re.Children.Text; // return the element's rendered html

				} catch (Exception ex) { // exception viewing the new preview page
					Element.Children.Text = oldText; // reset text
					Element.Preview();
					throw ex;
				}
			} catch (Exception ex) {
				Log.Error("EditableContent callback exception:", ex);
				Result = "<img src='" + ResolveClientUrl("~/Silversite/Images/delete.png") + "'> Errors: " + Element.Parser.ErrorReport;
			}
		}

	}
}