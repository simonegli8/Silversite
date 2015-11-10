using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace Silversite.Html {

	public class Writer {

		public TextWriter TextWriter { get; set; }

		public Writer() { }
		public Writer(TextWriter w) { TextWriter = w; }

		public virtual string PostProcess(string output) { return output; }

		public int? ReplaceTabsWithNSpaces { get { if (replaceTabs == null) return null; return replaceTabs.Length; } set { if (value.HasValue) replaceTabs = " ".Repeat(value.Value); else replaceTabs = null; } }
		public int? ReplaceNSpacesWithTabs { get { if (replaceSpaces == null) return null; return replaceSpaces.Length; } set { if (value.HasValue) replaceSpaces = " ".Repeat(value.Value); else replaceSpaces = null; } }
		string replaceTabs = null;
		string replaceSpaces = null;

		public virtual void WriteWhitespaceOrComment(Token token) {
			switch (token.Class) {
			case TokenClass.ServerTagStart:
			case TokenClass.ServerTagEnd:
			case TokenClass.Literal: if (token.ServerTagClass == ServerTagClass.Comment) TextWriter.Write(token.Value); break;
			case TokenClass.Whitespace:
				string value = token.Value;
				if (replaceTabs != null) value = value.Replace("\t", replaceTabs);
				if (replaceSpaces != null) value = value.Replace(replaceSpaces, "\t");
				TextWriter.Write(value);
				break;
			case TokenClass.HtmlComment:
			case TokenClass.HtmlCommentEnd:
			case TokenClass.HtmlCommentStart: TextWriter.Write(token.Value); break;
			default: break;
			}
		}

		public virtual void WriteLeadingWhitespace(Token token) {
			var prev = token.Previous as Token;
			if (prev != null && prev.IsWhitespaceOrComment) 	{
				WriteLeadingWhitespace(prev);
				WriteWhitespaceOrComment(prev);
			} 
		}

		public virtual void WriteTrailingWhitespace(Token token) {

			var nextNonWhitespace = token.Next as Token;
			while (nextNonWhitespace != null && nextNonWhitespace.IsWhitespaceOrComment) nextNonWhitespace = nextNonWhitespace.Next as Token;
			if (nextNonWhitespace == null) { // only write trailing whitespace if there is no more next token that will write leading whitespace.
				var next = token.Next as Token;
				if (next != null && next.IsWhitespaceOrComment) {
					WriteWhitespaceOrComment(next);
					WriteTrailingWhitespace(next);
				}
			}
		}

		public virtual void WriteTokenValue(Token t) {
			switch (t.Class) {
			case TokenClass.EndTagStart: t.Value = "</"; break;
			case TokenClass.Equals: t.Value = "="; break;
			case TokenClass.ServerTagEnd:
				if (t.ServerTagClass == ServerTagClass.Comment) t.Value = "--%>";
				else if (t.ServerTagClass == ServerTagClass.PersistentObjectName) t.Value = "]";
				else t.Value = "%>";
				break;
			case TokenClass.ServerTagStart:
				switch (t.ServerTagClass) {
				case ServerTagClass.AspNetExpression: t.Value = "<%$"; break;
				case ServerTagClass.HtmlEncodedExpression: t.Value = "<%:"; break;
				case ServerTagClass.Binding: t.Value = "<%#"; break;
				case ServerTagClass.Expression: t.Value = "<%="; break;
				case ServerTagClass.Declaration: t.Value = "<%@"; break;
				case ServerTagClass.Comment: t.Value = "<%--"; break;
				case ServerTagClass.Code: t.Value = "<%"; break;
				case ServerTagClass.PersistentObject: t.Value = "<%--[Silversite.PersistentObject:"; break;
				}
				break;	
			case TokenClass.SingleTagEnd: t.Value = "/>"; break;
			case TokenClass.TagEnd: t.Value = ">"; break;
			case TokenClass.TagStart: t.Value = "<"; break;
			case TokenClass.DoctypeStart: t.Value = "<!DOCTYPE"; break;
			case TokenClass.HtmlCommentStart: t.Value = "<!--"; break;
			case TokenClass.HtmlCommentEnd: t.Value = "-->"; break;
			case TokenClass.CDataEnd: t.Value = "]]>"; break;
			case TokenClass.CDataStart: t.Value = "<![CDATA["; break;
			case TokenClass.Literal:
			case TokenClass.Script:
			case TokenClass.Identifier:
			case TokenClass.String: t.Value = string.Empty; break;
			case TokenClass.Whitespace: t.Value = " "; break;
			case TokenClass.XmlDocTagStart: t.Value = "<?"; break;
			case TokenClass.XmlDocTagEnd: t.Value = "?>"; break;
			default: break;
			}
		}

		public virtual void WriteToken(Token token) {
			WriteLeadingWhitespace(token);
			if (token.IsWhitespaceOrComment) WriteWhitespaceOrComment(token);
			else {
				if (token.Class == TokenClass.String) {
					if (token.StringClass == StringClass.DoubleQuote) {
						TextWriter.Write('"');
						TextWriter.Write(token.Value);
						TextWriter.Write('"');
					} else {
						TextWriter.Write("'");
						TextWriter.Write(token.Value);
						TextWriter.Write("'");
					}
				} else if (token.Class != TokenClass.EndOfDocument) TextWriter.Write(token.Value);
			}
			WriteTrailingWhitespace(token);
		}
	
		public virtual void WriteNewLineAfter(Token token) {
			if (!token.IsWhitespaceOrComment) WriteToken(token);
		}

		public virtual void WriteAttribute(Attribute a) { /* WriteToken(a.Spacer); */ WriteToken(a.NameToken); if (a.HasValue) { WriteToken(a.EqualsToken); WriteToken(a.ValueToken); } }

		public virtual void WriteAttributes(AttributeCollection atrs) {
			var attributes = ((IEnumerable<Attribute>)atrs).OrderBy(a => a.Index);
			foreach (var a in attributes) WriteAttribute(a);
		}

		public virtual void WriteChildren(ChildCollection children) { foreach (var child in children) Write(child); }

		public virtual void WriteLiteral(Literal literal) {
			if (literal.IsCData) WriteToken(literal.CDataStartToken);
			WriteToken(literal.LiteralToken);
			if (literal.IsCData) WriteToken(literal.CDataEndToken);
		}

		public virtual void WriteServerTag(ServerTag tag) {
			WriteToken(tag.StartToken);
			if (tag.StartToken.ServerTagClass == ServerTagClass.Declaration) { WriteToken(tag.NameToken); WriteAttributes(tag.Attributes); } else WriteToken(tag.ValueToken);
			WriteToken(tag.EndToken);
			WriteNewLineAfter(tag.NewLineAfter);
		}

		public virtual void WriteDoctype(Doctype doc) {
			WriteToken(doc.StartToken);
			WriteChildren(doc.Children);
			WriteToken(doc.EndToken);
			WriteNewLineAfter(doc.NewLineAfter);
		}

		public virtual void WriteTag(Tag tag) {
			if (tag is ServerTag) WriteServerTag((ServerTag)tag);
			else {
				WriteToken(tag.StartToken);
				WriteToken(tag.NameToken);
				WriteAttributes(tag.Attributes);
				//WriteToken(tag.SpaceBeforeEndToken);
				WriteToken(tag.EndToken);
				WriteNewLineAfter(tag.NewLineAfter);
			}
		}

		public virtual void WriteXmlDocHeader(XmlDocHeader xmldoc) { WriteTag(xmldoc); }

		public virtual void WriteSpecialElement(SpecialElement s) { WriteTag(s.StartTag); WriteToken(s.ValueToken); if (s.EndTag != null) WriteTag(s.EndTag); }

		public virtual void WriteScript(Script s) { WriteSpecialElement(s); }

		public virtual void WriteStyle(Style s) { WriteSpecialElement(s); }

		public virtual void WriteElement(Element e) {
			bool single = e.Children.Count == 0;
			e.StartTag.IsSingleTag &= single;
			if (!single && e.EndTag == null) { e.EndTag = e.New<Tag>(); e.EndTag.IsEndTag = true; e.EndTag.FullName = e.FullName; }
			if (e.StartTag.IsSingleTag && e.EndTag != null) e.EndTag = null;
			WriteTag(e.StartTag); WriteChildren(e.Children); if (e.EndTag != null) WriteTag(e.EndTag);
		}

		public virtual void WriteDocument(Document doc) { WriteChildren(doc.Children); }

		public virtual void Write(DocumentNode node) {
			if (node is Literal) { WriteLiteral((Literal)node); return; }
			if (node is Attribute) { WriteAttribute((Attribute)node); return; }
			if (node is Doctype) { WriteDoctype((Doctype)node); return; }
			if (node is ServerTag) { WriteServerTag((ServerTag)node); return; }
			if (node is Tag) { WriteTag((Tag)node); return; }
			if (node is XmlDocHeader) { WriteXmlDocHeader((XmlDocHeader)node); return; }
			if (node is Style) { WriteStyle((Style)node); return; }
			if (node is Script) { WriteScript((Script)node); return; }
			if (node is SpecialElement) { WriteSpecialElement((SpecialElement)node); return; }
			if (node is AttributeCollection) { WriteAttributes((AttributeCollection)node); return; }
			if (node is ChildCollection) { WriteChildren((ChildCollection)node); return; }
			if (node is Element) { WriteElement((Element)node); return; }
			if (node is Document) { WriteDocument((Document)node); return; }
		}

		public virtual void Write(TextNode t) {
			if (t is DocumentNode) { Write((DocumentNode)t); return; }
			if (t is Token) { WriteToken((Token)t); return; }
		}
	}
}