using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Silversite.Services {

	public interface IJavaScriptTextEditable {

		/// <summary>
		/// The container control to edit.
		/// </summary>
		Control Content { get; }
		/// <summary>The hidden control containing the content to replace the edit container with upon edit or null for no replacement.</summary>
		HiddenField EditableText { get; }
		HiddenField OriginalText { get; }

		string ClientOpenEditorScript { get; }
		string ClientSaveEditorScript { get; }
		string ClientCancelEditorScript { get; }
		string ContentType { get; }
	}

	public class JavaScriptTextEditor : Service<JavaScriptTextEditorProvider> {

		public JavaScriptTextEditor() { Menu = EditorMenu.Basic; ContentType = "text/html"; }

		/// <summary>Editor modes for different types of text.</summary>
		public enum EditorMenu { Full, Basic, BasicNoFiles, Source }

		/// <summary>The editor's mode.</summary>
		public EditorMenu Menu { get; set; }

		/// <summary>The name of the editor package, like Silversite.CKEditor or Silversite.TinyMCE</summary>
		public string Name { get { return Provider.Name; } }

		public string ContentType { get; set; }

		/// <summary>Javascript that open's the editor.</summary>
		public string ClientOpenEditorScript(Control container) {
			if (container is IJavaScriptTextEditable) ContentType = ((IJavaScriptTextEditable)container).ContentType;
			return Provider.ClientOpenEditorScript(this, container);
		}

		/// <summary>A html converter that converts back and forth to the editor.</summary>
		public Html.Converter Converter { get { return Provider.Converter; } }
	}

	[DefaultProvider]
	public class JavaScriptTextEditorProvider : Provider<JavaScriptTextEditor> {
		public virtual string ClientOpenEditorScript(JavaScriptTextEditor service, Control container) { return ";"; }
		public virtual string Name { get { return ""; } }
		public virtual Html.Converter Converter { get { return new Html.Converter(); } }
	}

}