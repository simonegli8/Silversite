using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace Silversite.Html {

	public class CKEditorConverter: Html.Converter {

		public CKEditorConverter() { Reader = new CKEditorReader(); Parser = new CKEditorParser(); Parser.Reader = Reader; Writer = new CKEditorWriter(); }

		public override void Import(DocumentNode node) {
			if (node is ChildCollection) {
				var col = (ChildCollection)node;
				if (col.Count > 0 && col[0] is Element && ((Element)col[0]).Name.ToLower() == "html") {
					var html = (Element)col[0];
					if (html.Children.Count > 1 && html.Children[1] is Element && ((Element)html.Children[1]).Name.ToLower() == "body") {
						var body = (Element)html.Children[1];
						var txt = body.Children.Text;

						// remove identation
						var builder = new StringBuilder();
						var lines = txt.Split('\n').ToList();
						lines.RemoveAt(0);
						var mint = lines.Select(l => l.TakeWhile(ch => ch == '\t').Count()).Min(); // compute minimal tab indentation
						if (mint > 0) {
							while (mint-- > 0) builder.Append('\t');
							var indent = builder.ToString();
							lines = lines.Select(l => l.Replace(indent, "")).ToList();
						}
						builder.Clear(); builder.AppendLine();
						foreach (var l in lines) { builder.Append(l); builder.Append('\n'); }
						col.Text = builder.ToString();
					}
				}
			}
		}

		public override void ImportElement(Element e) {
			base.ImportElement(e);
			if (e.Attributes["class"].Contains("silversite_ckeditor_display_html_wrapper")) e.Children.Unwrap();
			if (e.Attributes["class"].Contains("silversite_ckeditor_display_html")) e.Parent.Remove(e);
		}

		public void ExportServerControlToken(Token t) {
			t.Value = t.Value.Replace("<", "[&lt;").Replace(">", "&gt;]");
		}
		public void ExportServerControlTag(Tag t) {
			ExportServerControlToken(t.StartToken); ExportServerControlToken(t.EndToken);
		}
		public void ExportServerControl(Element e) {
			ExportServerControlTag(e.StartTag); if (e.EndTag != null) ExportServerControlTag(e.EndTag);
			var ospan = new Element();
			ospan.FullName = "span";
			ospan.Attributes["style"] = "color:blue;font-family:Syntax LT, Arial, Helvetica, Sans-Serif;";
			ospan.Attributes["class"] = "silversite_ckeditor_display_html_wrapper";
			e.Wrap(ospan);
			var ispan = new Element();
			ispan.FullName = "span";
			ispan.Attributes["style"] = "color:black;font-family:Syntax LT, Arial, Helvetica, Sans-Serif;";
			ispan.Attributes["class"] = "silversite_ckeditor_display_html_wrapper";
			e.Children.Wrap(ispan);
		}

		public override void ExportElement(Element e) {
			bool single = e.Children.Count == 0;
			e.StartTag.IsSingleTag &= single;
			if (!single && e.EndTag == null) { e.EndTag = e.New<Tag>(); e.EndTag.IsEndTag = true; e.EndTag.FullName = e.FullName; }
			base.ExportElement(e);
			if (e.IsServerControl) ExportServerControl(e);
		}

	}

}