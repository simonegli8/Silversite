using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Silversite.Html {

	/// <summary>
	/// A class that converts Html.
	/// </summary>
	public class Converter {
		/// <summary>
		/// The Reader used by this converter.
		/// </summary>
		public virtual Reader Reader { get; protected set; }
		/// <summary>
		/// The parser used by this converter.
		/// </summary>
		public virtual Parser Parser { get; protected set; }
		/// <summary>
		/// The Writer used by this converter.
		/// </summary>
		public virtual Writer Writer { get; protected set; }
		/// <summary>
		/// 
		/// </summary>
		public Converter() { Reader = new Reader(); Parser = new Parser(Reader); Writer = new Writer(); }
		/// <summary>
		/// Imports html text into a html DOM document.
		/// </summary>
		/// <param name="node">The DOM element to import to.</param>
		/// <param name="html">The html source to import.</param>
		public virtual void Import(DocumentNode node, string html) {
			Parser.Reader = Reader;
			var p = node.Parser;
			node.Parser = Parser;
			node.Text = html;
			node.Parser = p;
			Import(node);
		}
		/// <summary>
		/// Exports a DOM element into a html string.
		/// </summary>
		/// <param name="node">The DOM element to export.</param>
		/// <param name="html">The exported html string.</param>
		public virtual void Export(DocumentNode node, out string html) {
			Export(node);
			var w = node.Writer;
			node.Writer = Writer;
			html = node.Text;
			node.Writer = w;
		}
		/// <summary>
		/// Number of spaces to replace a tab with when importing.
		/// </summary>
		public int? ImportReplaceTabsWithNSpaces { get { if (importReplaceTabs == null) return null; return importReplaceTabs.Length; } set { if (value.HasValue) importReplaceTabs = " ".Repeat(value.Value); else importReplaceTabs = null; } }
		/// <summary>
		/// Number of spaces that will be replaced with a tab when importing.
		/// </summary>
		public int? ImportReplaceNSpacesWithTabs { get { if (importReplaceSpaces == null) return null; return importReplaceSpaces.Length; } set { if (value.HasValue) importReplaceSpaces = " ".Repeat(value.Value); else importReplaceSpaces = null; } }
		string importReplaceTabs = null;
		string importReplaceSpaces = null;
		/// <summary>
		/// Import a whitespace or comment token.
		/// </summary>
		/// <param name="token">The token to import.</param>
		public virtual void ImportWhitespaceOrComment(Token token) {
			if (importReplaceTabs != null) token.Value = token.Value.Replace("\t", importReplaceTabs);
			if (importReplaceSpaces != null) token.Value = token.Value.Replace(importReplaceSpaces, "\t");
		}
		/// <summary>
		/// Imports leading whitespace for a token.
		/// </summary>
		/// <param name="token">The token to import.</param>
		public virtual void ImportLeadingWhitespace(Token token) {
			token = token.Previous as Token;
			if (token != null && token.IsWhitespaceOrComment) {
				ImportLeadingWhitespace(token);
				ImportWhitespaceOrComment(token);
			}
		}
		/// <summary>
		/// Imports trailing whitespace for a token.
		/// </summary>
		/// <param name="token">The token to import.</param>
		public virtual void ImportTrailingWhitespace(Token token) {
			var t2 = token.Next as Token;
			while (t2 != null && t2.IsWhitespaceOrComment) t2 = t2.Next as Token;
			if (t2 == null) {
				token = token.Next as Token;
				if (token != null && token.IsWhitespaceOrComment) {
					ImportWhitespaceOrComment(token);
					ImportTrailingWhitespace(token);
				}
			}
		}
		public virtual void ImportTokenValue(Token t) { }
		public virtual void ImportToken(Token token) {
			if (token.IsWhitespaceOrComment) ImportWhitespaceOrComment(token);
			else {
				ImportLeadingWhitespace(token);
				ImportTokenValue(token);
				ImportTrailingWhitespace(token);
			}
		}
		public virtual void ImportNewLineAfter(Token token) { if (!token.IsWhitespaceOrComment) ImportToken(token); }
		public virtual void ImportAttribute(Attribute a) { ImportToken(a.Spacer); ImportToken(a.NameToken); if (a.HasValue) { ImportToken(a.EqualsToken); ImportToken(a.ValueToken); } }
		public virtual void ImportAttributes(AttributeCollection atrs) { foreach (var a in atrs.ToList()) ImportAttribute(a); }
		public virtual void ImportChildren(ChildCollection children) { foreach (var child in children.ToList()) Import(child); }
		public virtual void ImportLiteral(Literal literal) {
			if (literal.IsCData) ImportToken(literal.CDataStartToken);
			ImportToken(literal.LiteralToken);
			if (literal.IsCData) ImportToken(literal.CDataEndToken);
		}
		public virtual void ImportServerTag(ServerTag tag) {
			ImportToken(tag.StartToken);
			if (tag.StartToken.ServerTagClass == ServerTagClass.Declaration) { ImportToken(tag.NameToken); ImportAttributes(tag.Attributes); } else ImportToken(tag.ValueToken);
			ImportToken(tag.EndToken);
			ImportNewLineAfter(tag.NewLineAfter);
		}
		public virtual void ImportDoctype(Doctype doc) {
			ImportToken(doc.StartToken);
			ImportChildren(doc.Children);
			ImportToken(doc.EndToken);
			ImportNewLineAfter(doc.NewLineAfter);
		}
		public virtual void ImportTag(Tag tag) {
			if (tag is ServerTag) ImportServerTag((ServerTag)tag);
			else {
				ImportToken(tag.StartToken);
				ImportToken(tag.NameToken);
				ImportAttributes(tag.Attributes);
				ImportToken(tag.SpaceBeforeEndToken);
				ImportToken(tag.EndToken);
				ImportNewLineAfter(tag.NewLineAfter);
			}
		}
		public virtual void ImportXmlDocHeader(XmlDocHeader xmldoc) { ImportTag(xmldoc); }
		public virtual void ImportSpecialElement(SpecialElement s) { ImportTag(s.StartTag); ImportToken(s.ValueToken); if (s.EndTag != null) ImportTag(s.EndTag); }
		public virtual void ImportScript(Script s) { ImportSpecialElement(s); }
		public virtual void ImportStyle(Style s) { ImportSpecialElement(s); }
		public virtual void ImportElement(Element e) {
			bool single = e.Children.Count == 0;
			e.StartTag.IsSingleTag &= single;
			if (!single && e.EndTag == null) { e.EndTag = e.New<Tag>(); e.EndTag.IsEndTag = true; e.EndTag.FullName = e.FullName; }
			ImportTag(e.StartTag); ImportChildren(e.Children); if (e.EndTag != null) ImportTag(e.EndTag);
		}
		public virtual void ImportDocument(Document doc) { ImportChildren(doc.Children); }

		public virtual void Import(DocumentNode node) {
			if (node is Literal) { ImportLiteral((Literal)node); return; }
			if (node is Attribute) { ImportAttribute((Attribute)node); return; }
			if (node is Doctype) { ImportDoctype((Doctype)node); return; }
			if (node is ServerTag) { ImportServerTag((ServerTag)node); return; }
			if (node is Tag) { ImportTag((Tag)node); return; }
			if (node is XmlDocHeader) { ImportXmlDocHeader((XmlDocHeader)node); return; }
			if (node is Style) { ImportStyle((Style)node); return; }
			if (node is Script) { ImportScript((Script)node); return; }
			if (node is SpecialElement) { ImportSpecialElement((SpecialElement)node); return; }
			if (node is AttributeCollection) { ImportAttributes((AttributeCollection)node); return; }
			if (node is ChildCollection) { ImportChildren((ChildCollection)node); return; }
			if (node is Element) { ImportElement((Element)node); return; }
			if (node is Document) { ImportDocument((Document)node); return; }
		}

		public virtual void Import(TextNode t) {
			if (t is DocumentNode) { Import((DocumentNode)t); return; }
			if (t is Token) { ImportToken((Token)t); return; }
		}

		public int? ExportReplaceTabsWithNSpaces { get { if (exportReplaceTabs == null) return null; return exportReplaceTabs.Length; } set { if (value.HasValue) exportReplaceTabs = " ".Repeat(value.Value); else exportReplaceTabs = null; } }
		public int? ExportReplaceNSpacesWithTabs { get { if (exportReplaceSpaces == null) return null; return exportReplaceSpaces.Length; } set { if (value.HasValue) exportReplaceSpaces = " ".Repeat(value.Value); else exportReplaceSpaces = null; } }
		string exportReplaceTabs = null;
		string exportReplaceSpaces = null;

		public virtual void ExportWhitespaceOrComment(Token token) {
			if (exportReplaceTabs != null) token.Value = token.Value.Replace("\t", exportReplaceTabs);
			if (exportReplaceSpaces != null) token.Value = token.Value.Replace(exportReplaceSpaces, "\t");
		}
		public virtual void ExportLeadingWhitespace(Token token) {
			token = token.Previous as Token;
			if (token != null && token.IsWhitespaceOrComment) {
				ExportLeadingWhitespace(token);
				ExportWhitespaceOrComment(token);
			}
		}
		public virtual void ExportTrailingWhitespace(Token token) {
			var t2 = token.Next as Token;
			while (t2 != null && t2.IsWhitespaceOrComment) t2 = t2.Next as Token;
			if (t2 == null) {
				token = token.Next as Token;
				if (token != null && token.IsWhitespaceOrComment) {
					ExportWhitespaceOrComment(token);
					ExportTrailingWhitespace(token);
				}
			}
		}
		public virtual void ExportTokenValue(Token t) { }
		public virtual void ExportToken(Token token) {
			if (token.IsWhitespaceOrComment) ExportWhitespaceOrComment(token);
			else {
				ExportLeadingWhitespace(token);
				ExportTokenValue(token);
				ExportTrailingWhitespace(token);
			}
		}
		public virtual void ExportNewLineAfter(Token token) { if (!token.IsWhitespaceOrComment) ExportToken(token); }
		public virtual void ExportAttribute(Attribute a) { ExportToken(a.Spacer); ExportToken(a.NameToken); if (a.HasValue) { ExportToken(a.EqualsToken); ExportToken(a.ValueToken); } }
		public virtual void ExportAttributes(AttributeCollection atrs) { foreach (var a in atrs.ToList()) ExportAttribute(a); }
		public virtual void ExportChildren(ChildCollection children) { foreach (var child in children.ToList()) Export(child); }
		public virtual void ExportLiteral(Literal literal) {
			if (literal.IsCData) ExportToken(literal.CDataStartToken);
			ExportToken(literal.LiteralToken);
			if (literal.IsCData) ExportToken(literal.CDataEndToken);
		}
		public virtual void ExportServerTag(ServerTag tag) {
			ExportToken(tag.StartToken);
			if (tag.StartToken.ServerTagClass == ServerTagClass.Declaration) { ExportToken(tag.NameToken); ExportAttributes(tag.Attributes); } else ExportToken(tag.ValueToken);
			ExportToken(tag.EndToken);
			ExportNewLineAfter(tag.NewLineAfter);
		}
		public virtual void ExportDoctype(Doctype doc) {
			ExportToken(doc.StartToken);
			ExportChildren(doc.Children);
			ExportToken(doc.EndToken);
			ExportNewLineAfter(doc.NewLineAfter);
		}
		public virtual void ExportTag(Tag tag) {
			if (tag is ServerTag) ExportServerTag((ServerTag)tag);
			else {
				ExportToken(tag.StartToken);
				ExportToken(tag.NameToken);
				ExportAttributes(tag.Attributes);
				ExportToken(tag.SpaceBeforeEndToken);
				ExportToken(tag.EndToken);
				ExportNewLineAfter(tag.NewLineAfter);
			}
		}
		public virtual void ExportXmlDocHeader(XmlDocHeader xmldoc) { ExportTag(xmldoc); }

		public virtual void ExportSpecialElement(SpecialElement s) { ExportTag(s.StartTag); ExportToken(s.ValueToken); if (s.EndTag != null) ExportTag(s.EndTag); }
		public virtual void ExportScript(Script s) { ExportSpecialElement(s); }
		public virtual void ExportStyle(Style s) { ExportSpecialElement(s); }
		public virtual void ExportElement(Element e) {
			bool single = e.Children.Count == 0;
			e.StartTag.IsSingleTag &= single;
			if (!single && e.EndTag == null) { e.EndTag = e.New<Tag>(); e.EndTag.IsEndTag = true; e.EndTag.FullName = e.FullName; }
			ExportTag(e.StartTag); ExportChildren(e.Children); if (e.EndTag != null) ExportTag(e.EndTag);
		}
		public virtual void ExportDocument(Document doc) { ExportChildren(doc.Children); }

		public virtual void Export(DocumentNode node) {
			if (node is Literal) { ExportLiteral((Literal)node); return; }
			if (node is Attribute) { ExportAttribute((Attribute)node); return; }
			if (node is Doctype) { ExportDoctype((Doctype)node); return; }
			if (node is ServerTag) { ExportServerTag((ServerTag)node); return; }
			if (node is Tag) { ExportTag((Tag)node); return; }
			if (node is XmlDocHeader) { ExportXmlDocHeader((XmlDocHeader)node); return; }
			if (node is Style) { ExportStyle((Style)node); return; }
			if (node is Script) { ExportScript((Script)node); return; }
			if (node is SpecialElement) { ExportSpecialElement((SpecialElement)node); return; }
			if (node is AttributeCollection) { ExportAttributes((AttributeCollection)node); return; }
			if (node is ChildCollection) { ExportChildren((ChildCollection)node); return; }
			if (node is Element) { ExportElement((Element)node); return; }
			if (node is Document) { ExportDocument((Document)node); return; }
		}

		public virtual void Export(TextNode t) {
			if (t is DocumentNode) { Export((DocumentNode)t); return; }
			if (t is Token) { ExportToken((Token)t); return; }
		}
	}

}