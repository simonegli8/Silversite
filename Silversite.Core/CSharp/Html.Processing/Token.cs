using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics.Contracts;

namespace Silversite.Html {

	public enum TokenClass { Whitespace, TagStart, TagEnd, SingleTagEnd, EndTagStart, ServerTagStart, ServerTagEnd, Identifier, Equals, String, Literal, CDataStart, CData, CDataEnd,
		Script, DoctypeStart, HtmlCommentStart, HtmlComment, HtmlCommentEnd, EndOfDocument, XmlDocTagStart, XmlDocTagEnd  }
	public enum StringClass { SingleQuote, DoubleQuote }

	public class Token: TextNode {

		TokenClass tokenClass;
		StringClass stringClass;
		ServerTagClass serverTagClass;
		public string Identation;
		public int Line, Column, Start, End;

		string tvalue;
		public string Value {
			get { return tvalue; }
			set {
				if (Parent != null && Parent is Attribute && ((Attribute)Parent).NameToken == this && Parent.Parent != null && Parent.Parent is AttributeCollection) {
					var pp = Parent.Parent;
					pp.Remove(Parent);
					tvalue = value;
					pp.Add(Parent);
				} else {
					// if (!string.IsNullOrEmpty(tvalue) && string.IsNullOrEmpty(value) && IsWhitespace) Debug.Break();
					tvalue = value;
				}
			}
		}

		public Token() : base() {
			Identation = string.Empty; tokenClass = TokenClass.Whitespace; stringClass = Html.StringClass.DoubleQuote; serverTagClass = Html.ServerTagClass.Expression; Value = string.Empty;
			Line = 1; Column = 1; Start = 0; End = 0;
		}
		public Token(TokenClass tokenClass) : this() { Class = tokenClass; }

		public virtual TokenClass Class {
			get { return tokenClass; }
			set {
				// if (value == TokenClass.Whitespace && tokenClass == TokenClass.Whitespace && Length > 0) Debug.Break(); 
				tokenClass = value; Writer.WriteTokenValue(this); Length = Value.Length; }
		}
		public virtual StringClass StringClass { get { return stringClass; } set { stringClass = value; Writer.WriteTokenValue(this); Length = Value.Length; } }
		public virtual ServerTagClass ServerTagClass {
			get { return serverTagClass; }
			set { serverTagClass = value; Writer.WriteTokenValue(this); Length = Value.Length; }
		}

		public Token LeadingWhitespace { get { if (Previous != null && (Previous is Token && ((Token)Previous).Class == TokenClass.Whitespace)) return (Token)Previous; return null; } }
		public int Length { get { return End - Start; } set { End = Start + value; } }

		public void AddIdentation(string identation) {
			if (Class == TokenClass.Whitespace && Value.Contains('\n') || Start == 0) Value += identation;
		}
		public void RemoveIdentation(string identation) {
			if (Class == TokenClass.Whitespace && Value.Contains('\n') || Start == 0 && Value.EndsWith(identation)) Value = Value.Substring(0, Value.Length - identation.Length);
		}

		public bool IsWhitespace { get { return Class == TokenClass.Whitespace; } }
		public bool IsComment { get { return IsServerComment || IsHtmlComment; } }
		public bool IsServerComment {
			get {
				return (Class == TokenClass.ServerTagStart || Class == TokenClass.ServerTagEnd || Class == TokenClass.Literal) &&
					(ServerTagClass == Html.ServerTagClass.Comment || ServerTagClass == Html.ServerTagClass.PersistentObject || ServerTagClass == Html.ServerTagClass.PersistentObjectName);
			}
		}
		public bool IsHtmlComment { get { return Class == TokenClass.HtmlCommentStart || Class == TokenClass.HtmlCommentEnd || Class == TokenClass.HtmlComment; } }
		public bool IsWhitespaceOrComment { get { return IsWhitespace || IsComment; } }
		public bool IsNewLineAfter { get { return Parent is Tag && this == ((Tag)Parent).NewLineAfter || Parent is Doctype && this == ((Doctype)Parent).NewLineAfter; } }

		public void CopyFrom(Token t) {
			Start = t.Start; End = t.End; Line = t.Line; Column = t.Column; Identation = t.Identation; tokenClass = t.tokenClass; stringClass = t.stringClass; serverTagClass = t.serverTagClass; Value = t.Value;
		}

		// for debugging purposes.
		string LookAhead { get { return Reader.Text.SafeSubstring(this.Start, 64); } }
		string LookBack { get { return Reader.Text.SafeSubstring(this.End - 64, 64); } }

		public void ReadWhiteSpace(bool leading) {
			var index = Parent.Nodes.IndexOf(this);
			Debug.Assert(index >= 0);
			while (Parser.Reader.WhitespaceTokens.Count > 0) {
				var ws = Parser.Reader.WhitespaceTokens.Dequeue();
				if (!leading) Parent.Insert(++index, ws);
				else Parent.Insert(index++, ws);
			}
		}

		public void Read() {
			CopyFrom(Parser.Reader.CurrentToken);
			ReAdd();
			ReadWhiteSpace(true);
			Parser.Reader.NextToken();
			if (Parser.Reader.CurrentToken == Parser.Reader.EndOfDocument) ReadWhiteSpace(false);
		}

		public void ReadNewLineAfter() {
			if (Parser.Reader.WhitespaceTokens.Count > 0) {
				var ws = Parser.Reader.WhitespaceTokens.Peek();
				int nl = ws.Value.IndexOf('\n');
				if (nl >= 0) {
					CopyFrom(ws);
					// steal all text up to '\n' from ws
					Value = ws.Value.SafeSubstring(0, nl + 1);
					End = Start + Value.Length;
					ws.Value = ws.Value.SafeSubstring(nl + 1);
					ws.Start += nl+1; ws.End -= nl+1; ws.Line++; ws.Column = 0;
				} else {
					Value = string.Empty;
				}
			} else {
				Value = string.Empty;
			}
		}
	}

}