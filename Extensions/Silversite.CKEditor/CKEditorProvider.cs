using System;
using System.Web.UI;
using Silversite;
using System.IO;
using System.Collections.Generic;

namespace Silversite.Web.UI {

	public class CKEditorProvider: Services.JavaScriptTextEditorProvider {

		class script1 { }
		class script2 { }
		class script3 { }
		class script4 { }

		// IJavaScriptHtmlEditable methods
		private string CallbackCommandScript(Control e) {
			return "function(command, text, callback, error) {" + e.Page.ClientScript.GetCallbackEventReference(e, "command + ':' + text", "callback", "{ }", "error", true) + " }";
		}

		public override string OpenEditorScript(Services.JavaScriptTextEditor editor, Control e) {
			string openScript = "";
			string closeScript = "";
			Services.IJavaScriptTextEditable editable = null;
			if (e is Services.IJavaScriptTextEditable) {
				editable = (Services.IJavaScriptTextEditable)e;
				e = editable.Content;
				openScript = editable.OnClientOpenEditor + ";";
				closeScript = editable.OnClientCloseEditor;
			}
			if (Web.UI.Scripts.jQuery.Register(e.Page) &&
#if DEBUG
				Web.UI.Scripts.Register(e.Page, "~/Silversite/Extensions/Silversite.CKEditor/ckeditor/ckeditor.js") &&
				// Web.Scripts.Register(e.Page, "~/Silversite/Extensions/Silversite.CKEditor/ckeditor/config.js") && 
				// Web.UI.Scripts.Register(e.Page, "~/Silversite/Extensions/Silversite.CKEditor/ckeditor/adapters/jquery.js") &&
				Web.UI.Scripts.Register(e.Page, "~/Silversite/Extensions/Silversite.CKEditor/js/Silversite.CKEditor.js")) {
#else
				Web.Scripts.Register(page, "~/Silversite/Extensions/Silversite.CKEditor/ckeditor/ckeditor.js") &&
				// Web.Scripts.Register(page, "~/Silversite/Extensions/Silversite.CKEditor/ckeditor/config.js") && 
				// Web.Scripts.Register(page, "~/Silversite/Extensions/Silversite.CKEditor/ckeditor/adapters/jquery.js") &&
				Web.Scripts.Register(page, "~/Silversite/Extensions/Silversite.CKEditor/js/Silversite.CKEditor.js")) {
#endif
				if (string.IsNullOrEmpty(e.ClientID)) throw new ArgumentException("Content must have valid ID.");

				return openScript + "Silversite.CKEditor.Open('" + e.ClientID + "', " + CallbackCommandScript(e) + ", '" + editor.ContentType + "', '" + editor.Menu.ToString() + "', " + closeScript + "');";
			}
			return string.Empty;
		}

		Html.Converter converter = null;
		public override Html.Converter Converter { get { if (converter == null) converter = new Html.CKEditorConverter(); return converter; } }

	}
}
