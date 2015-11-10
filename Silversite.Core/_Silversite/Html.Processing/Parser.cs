using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.Hosting;
using System.IO;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Silversite.Html {

	public class Parser {

		public Token CurrentToken { get { return Reader.CurrentToken; } }
		public Token EndOfDocument { get { return Reader.EndOfDocument; } set { Reader.EndOfDocument = value; } }
		public Token NextToken() { return Reader.NextToken(); }
		public Token PeekToken() { return Reader.PeekToken(); }
		public Token SkipTo(params TokenClass[] c) { return Reader.SkipToTokens(c); }

		public List<string> Errors { get { return Reader != null ? Reader.Errors : new List<string>(); } }
		public string ErrorReport { get { return Reader.ErrorReport; } }
		public void Error(string message, params object[] args) {
			Reader.Error(message, args);
		}
		public bool SuppressErrros { get { return Reader.SuppressErrors; } set { Reader.SuppressErrors = value; } }
		// for debugging purposes.
		internal string LookAhead { get { return Reader.LookAhead; } }
		internal string LookBack { get { return Reader.LookBack; } }

		Reader reader = null;
		public virtual Reader Reader { get { return reader ?? (reader = new Reader()); } set { reader = value; reader.Parser = this; } }

		public string[] SingleTags = new string[] { "img", "hr", "br", "meta", "input", "link" };

		public Parser(Reader reader) { Reader = reader; }
		public Parser() { Reader = new Reader(); }

		public virtual bool ParseLiteral(Literal lt, bool literalToken) {
			if (literalToken) Reader.LiteralToken();
			lt.Clear();
			lt.IsCData = false;
			if (CurrentToken.Class == TokenClass.Literal || CurrentToken.Class == TokenClass.Identifier || CurrentToken.Class == TokenClass.Equals || CurrentToken.Class == TokenClass.CData || CurrentToken.Class == TokenClass.String) {
				lt.LiteralToken.Read();
				return true;
			} else if (CurrentToken.Class == TokenClass.CDataStart) {
				lt.IsCData = true;
				lt.CDataStartToken.Read();
				lt.LiteralToken.Read();
				lt.CDataEndToken.Read();
				return true;
			} else Error("Literal: literal expected.");
			return false;
		}

		public virtual bool ParseAttribute(Attribute a) {
			a.Clear();
			string tag = string.Empty;
			if (a.ParentNode != null && a.ParentNode.ParentNode != null && a.ParentNode.ParentNode is Tag) tag = ((Tag)(a.ParentNode.ParentNode)).FullName;
			a.Spacer.Value = string.Empty;
			if (CurrentToken.Class == TokenClass.Identifier) {
				a.NameToken.Read();
				if (CurrentToken.Class == TokenClass.Equals) {
					a.HasValue = true;
					a.EqualsToken.Read();
					if (CurrentToken.Class == TokenClass.String || CurrentToken.Class == TokenClass.Identifier) {
						a.ValueToken.Read();
						return true;
					} else {
						Error("Attribute in {0}: string value or identifier expected.", tag);
						a.ValueToken.Class = TokenClass.String; a.ValueToken.Value = string.Empty;
					}
				} else {
					// Error("Attribute in {0}: equal sign expected.", tag);
					a.EqualsToken.Class = TokenClass.Equals;
					a.ValueToken.Class = TokenClass.String; a.ValueToken.Value = string.Empty;
					a.HasValue = false;
				}
			} else {
				Error("Attribute in {0}: attribute identifier expected.", tag);
				a.NameToken.Class = TokenClass.Identifier; a.NameToken.Value = "_error" + a.Index.ToString();
			}
			SkipTo(TokenClass.TagStart, TokenClass.Identifier, TokenClass.TagEnd, TokenClass.SingleTagEnd);
			return false;
		}

		public virtual bool ParseAttributes(AttributeCollection c) {
			c.Clear();
			bool res = true;
			while (CurrentToken != EndOfDocument && CurrentToken.Class == TokenClass.Identifier) {
				var a = c.New<Attribute>();
				if (!ParseAttribute(a)) res = false;
			}
			return res;
		}

		public virtual bool ParseSpecialElement(SpecialElement s) {
			s.Clear();

			if (ParseTag(s.StartTag) && (s.StartTag.IsSpecialElement || s.StartTag.FullName == "_error")) {
				if (!s.StartTag.IsSingleTag) {
					s.ValueToken.Read();
					if (CurrentToken.Class == TokenClass.EndTagStart) {
						if (ParseEndTag(s.EndTag) && s.EndTag.IsEndTag) {
							if (s.EndTag.FullName != s.StartTag.FullName) { Error("{0}: end tag {1} does not match script start tag.", s.GetType().Name, s.EndTag.FullName); s.EndTag.FullName = s.StartTag.FullName; }
							return true;
						} else {
							Error("{0}: script end tag expected.", s.GetType().Name, s.StartTag.FullName);
							s.EndTag.IsEndTag = true;
							s.EndTag.FullName = s.Name;
						}
					} else {
						Error("{0}: script end tag expected.", s.GetType().Name, s.StartTag.FullName);
						s.EndTag.IsEndTag = true;
						s.EndTag.FullName = s.Name;
					}
				} else {
					s.ValueToken.Start = s.ValueToken.End = s.StartTag.EndToken.End; s.ValueToken.Value = string.Empty;
					s.EndTag = null;
					return true;
				}
			}
			return false;
		}
		public virtual bool ParseScript(Script s) { s.Clear(); return ParseSpecialElement(s); }
		public virtual bool ParseStyle(Style s) { s.Clear();  return ParseSpecialElement(s); }

		public virtual bool ParseElement(Element e) {
			e.Clear();
			if (ParseTag(e.StartTag)) {
				if (!e.StartTag.IsSingleTag && !SingleTags.Contains(e.StartTag.FullName.ToLower()) && !(e.StartTag.Prefix == "lang")) {
					ParseChildren(e.Children);
					if (CurrentToken.Class != TokenClass.EndTagStart) Error("Element {0}: end tag expected.", e.StartTag.FullName);
					SkipTo(TokenClass.EndTagStart);
					var name = PeekToken();
					if (name.Class == TokenClass.Identifier && name.Value == e.StartTag.FullName) {
						if (ParseEndTag(e.EndTag) && e.EndTag.IsEndTag) return true;
						else {
							Error("Element {0}: end tag not well formed.", e.StartTag.FullName);
						}
					} else {
						Error("Element {0}: end tag {1} does not match start tag {2}.", e.StartTag.FullName, e.EndTag.FullName, e.StartTag.FullName);
					} 
				} else {
					e.Children.Clear();
					e.EndTag = null;
					return true;
				}
			}
			e.EndTag.IsEndTag = true;
			if (e.EndTag != null) e.EndTag.FullName = e.FullName; 
			return false;
		}

		public virtual bool ParseDoctype(Doctype d) {
			d.Clear();
			d.NewLineAfter.Value = string.Empty;
			var res = true;
			if (CurrentToken.Class == TokenClass.DoctypeStart) {
				d.StartToken.Read();
				var cl = CurrentToken.Class;
				while (cl != TokenClass.EndOfDocument &&
					(cl == TokenClass.Literal || cl == TokenClass.Identifier || cl == TokenClass.CDataStart || cl == TokenClass.String || cl == TokenClass.ServerTagStart)) {
					switch (cl) {
					case TokenClass.Identifier:
					case TokenClass.Literal:
					case TokenClass.CDataStart:
					case TokenClass.String:
						var literal = d.Children.New<Literal>();
						if (!ParseLiteral(literal, false)) res = false;
						break;
					case TokenClass.ServerTagStart:
						var serverTag = d.Children.New<ServerTag>();
						if (!ParseServerTag(serverTag)) res = false;
						break;
					}
					cl = CurrentToken.Class;
				}
				SkipTo(TokenClass.TagEnd, TokenClass.TagStart);
				if (CurrentToken.Class == TokenClass.TagEnd) {
					d.EndToken.Read();
					d.NewLineAfter.ReadNewLineAfter();
				} else {
					res = false;
					Error("Doctype: End tag expected.");
					d.EndToken.Class = TokenClass.TagEnd;
				}
			} else {
				res = false;
				Error("Doctype: Doctype start expected.");
				d.StartToken.Class = TokenClass.DoctypeStart;
				d.EndToken.Class = TokenClass.TagEnd;
			}
			return res;
		}

		public virtual bool ParsePersistentObject(PersistentObject t) {
			t.Clear();
			t.NewLineAfter.Value = string.Empty;
			if (CurrentToken.Class == TokenClass.ServerTagStart && CurrentToken.ServerTagClass == ServerTagClass.PersistentObject) {
				t.StartToken.Read();
				if (CurrentToken.Class == TokenClass.Literal && CurrentToken.ServerTagClass == ServerTagClass.PersistentObject) {
					t.NameToken.Read();
					if (CurrentToken.Class == TokenClass.ServerTagEnd && CurrentToken.ServerTagClass == ServerTagClass.PersistentObjectName) {
						t.NameEndToken.Read();
						if (CurrentToken.Class == TokenClass.Literal && CurrentToken.ServerTagClass == ServerTagClass.PersistentObject) {
							t.TextToken.Read();
							if (CurrentToken.Class == TokenClass.ServerTagEnd) {
								t.EndToken.Read();
								t.NewLineAfter.ReadNewLineAfter();
								return true;
							} else {
								Error("PersistentObject: End of server comment expected.");
								return false;
							}
						} else {
							Error("PersistenObejct: Name end tag expected.");
							return false;
						}
					} else {
						Error("PersistentObject: Literal expected.");
						return false;
					}
				} else {
					Error("PersistentObject: Literal expected.");
					return false;
				}
			} else {
				Error("PersistenObject: Start tag expected.");
				return false;
			}
		}

		public virtual bool ParseChildren(ChildCollection c) {
			Contract.Assume(c.Parser == this);
			c.Clear();
			var res = true;
			var cl = CurrentToken.Class;
			while (cl != TokenClass.EndOfDocument &&
				(cl == TokenClass.Literal || cl == TokenClass.Identifier || cl == TokenClass.Equals || cl == TokenClass.CDataStart || cl == TokenClass.String || cl == TokenClass.TagStart ||
					cl == TokenClass.ServerTagStart || cl == TokenClass.DoctypeStart || cl == TokenClass.XmlDocTagStart)) {
				switch (cl) {
				case TokenClass.Identifier:
				case TokenClass.Literal:
				case TokenClass.Equals:
				case TokenClass.CDataStart:
				case TokenClass.String:
					var literal = c.New<Literal>();
					if (!ParseLiteral(literal, true)) res = false;
					break;
				case TokenClass.TagStart:
					var p = PeekToken();
					if (p.Class == TokenClass.Identifier && p.Value.ToLower() == "script") {
						var script = c.New<Script>();
						if (!ParseScript(script)) res = false;
					} else if (p.Class == TokenClass.Identifier && p.Value.ToLower() == "style") {
						var style = c.New<Style>();
						if (!ParseStyle(style)) res = false;
					} else {
						var elem = c.New<Element>();
						if (!ParseElement(elem)) res = false;
					}
					break;
				case TokenClass.ServerTagStart:
					if (CurrentToken.ServerTagClass == ServerTagClass.PersistentObject) {
						var t = c.New<PersistentObject>();
						if (!ParsePersistentObject(t)) res = false;
					} else {
						var serverTag = c.New<ServerTag>();
						if (!ParseServerTag(serverTag)) res = false;
					}
					break;
				case TokenClass.DoctypeStart:
					var doctype = c.New<Doctype>();
					if (!ParseDoctype(doctype)) res = false;
					break;
				case TokenClass.XmlDocTagStart:
					var xmldoc = c.New<XmlDocHeader>();
					if (!ParseXmlDocHeader(xmldoc)) res = false;
					break;
				}
				cl = CurrentToken.Class;
			}
			return res;
		}

		public virtual bool ParseTag(Tag tag) {
			tag.Clear();
			tag.NewLineAfter.Value = string.Empty;
			if (CurrentToken.Class == TokenClass.TagStart) {
				tag.StartToken.Read();
				if (CurrentToken.Class == TokenClass.Identifier) {
					tag.NameToken.Read();
					ParseAttributes(tag.Attributes);
					if (CurrentToken.Class == TokenClass.TagEnd || CurrentToken.Class == TokenClass.SingleTagEnd) {
						if (tag.IsScript) Reader.Mode = Html.Reader.Modes.Script;
						else if (tag.IsStyle) Reader.Mode = Html.Reader.Modes.Css;
						tag.EndToken.Read();
						tag.NewLineAfter.ReadNewLineAfter();
						return true;
					} else {
						Error("Element {0}: tag's closing bracket expected.", tag.Name);
						tag.EndToken.Class = TokenClass.TagEnd;
						SkipTo(TokenClass.SingleTagEnd, TokenClass.TagEnd, TokenClass.TagStart);
					}
				} else {
					Error("Element: tag id expected.");
					tag.StartToken.Class = TokenClass.TagStart;
					tag.NameToken.Class = TokenClass.Identifier; tag.NameToken.Value = "_error";
					tag.Attributes.Clear();
					tag.EndToken.Class = TokenClass.TagEnd;
				}
			} else {
				Error("Element: tag expected.");
				tag.StartToken.Class = TokenClass.TagStart;
				tag.NameToken.Class = TokenClass.Identifier; tag.NameToken.Value = "_error";
				tag.Attributes.Clear();
				tag.EndToken.Class = TokenClass.TagEnd;
			}
			return false;
		}

		public virtual bool ParseXmlDocHeader(XmlDocHeader xmldoc) {
			xmldoc.Clear();
			xmldoc.NewLineAfter.Value = string.Empty;
			if (CurrentToken.Class == TokenClass.XmlDocTagStart) {
				xmldoc.StartToken.Read();
				if (CurrentToken.Class == TokenClass.Identifier && CurrentToken.Value == "xml") {
					xmldoc.NameToken.Read();
					ParseAttributes(xmldoc.Attributes);
					if (CurrentToken.Class == TokenClass.XmlDocTagEnd) {
						xmldoc.EndToken.Read();
						xmldoc.NewLineAfter.ReadNewLineAfter();
						return true;
					} else {
						Error("Element {0}: xml doc tag's closing bracket expected.", xmldoc.Name);
						xmldoc.EndToken.Class = TokenClass.XmlDocTagEnd;
						SkipTo(TokenClass.XmlDocTagEnd, TokenClass.SingleTagEnd, TokenClass.TagEnd, TokenClass.TagStart);
					}
				} else {
					Error("Element: xml doc tag id \"xml\" expected.");
					xmldoc.StartToken.Class = TokenClass.XmlDocTagStart;
					xmldoc.NameToken.Class = TokenClass.Identifier; xmldoc.NameToken.Value = "xml";
					xmldoc.Attributes.Clear();
					xmldoc.EndToken.Class = TokenClass.XmlDocTagEnd;
				}
			} else {
				Error("Element:xml doc tag expected.");
				xmldoc.StartToken.Class = TokenClass.XmlDocTagStart;
				xmldoc.NameToken.Class = TokenClass.Identifier; xmldoc.NameToken.Value = "xml";
				xmldoc.Attributes.Clear();
				xmldoc.EndToken.Class = TokenClass.XmlDocTagEnd;
			}
			return false;
		}


		public bool ParseEndTag(Tag tag) {
			tag.Clear();
			tag.NewLineAfter.Value = string.Empty;
			if (CurrentToken.Class == TokenClass.EndTagStart) {
				tag.StartToken.Read();
				if (CurrentToken.Class == TokenClass.Identifier) {
					tag.NameToken.Read();
					if (CurrentToken.Class == TokenClass.TagEnd) {
						tag.EndToken.Read();
						tag.NewLineAfter.ReadNewLineAfter();
						return true;
					}
					Error("EndTag {0}: tag's closing bracket expected.", tag.Name);
					tag.EndToken.Class = TokenClass.TagEnd;
					return false;
				} else {
					Error("EndTag: identifier expected.");
					tag.StartToken.Class = TokenClass.EndTagStart;
					tag.NameToken.Class = TokenClass.Identifier; tag.NameToken.Value = "_error";
					tag.EndToken.Class = TokenClass.TagEnd;
				}
			} else {
				Error("EndTag: end tag start expected.");
				tag.StartToken.Class = TokenClass.EndTagStart;
				tag.NameToken.Class = TokenClass.Identifier; tag.NameToken.Value = "_error";
				tag.EndToken.Class = TokenClass.TagEnd;
			}
			return false;
		}

		public bool ParseServerTag(ServerTag tag) {
			tag.Clear();
			tag.NewLineAfter.Value = string.Empty;
			if (CurrentToken.Class == TokenClass.ServerTagStart) {
				var sc = CurrentToken.ServerTagClass;
				tag.StartToken.Read();
				switch (sc) {
				case ServerTagClass.Declaration:
					if (CurrentToken.Class == TokenClass.Identifier) {
						tag.NameToken.Read();
						ParseAttributes(tag.Attributes);
						if (CurrentToken.Class == TokenClass.ServerTagEnd) {
							tag.EndToken.Read();
							tag.NewLineAfter.ReadNewLineAfter();
							return true;
						} else {
							Error("Server tag {0}: closing bracket expected.", tag.FullName);
							tag.EndToken.Class = TokenClass.ServerTagEnd; tag.EndToken.ServerTagClass = tag.StartToken.ServerTagClass;
							SkipTo(TokenClass.ServerTagEnd, TokenClass.TagStart);
						}
					} else {
						Error("Server tag {0}: identifier expected.", tag.FullName);
						tag.NameToken.Class = TokenClass.Identifier; tag.NameToken.Value = "_error";
						tag.EndToken.Class = TokenClass.ServerTagEnd; tag.EndToken.ServerTagClass = tag.StartToken.ServerTagClass;
						SkipTo(TokenClass.ServerTagEnd, TokenClass.TagStart);
					}
					break;
				case ServerTagClass.Binding:
				case ServerTagClass.Code:
				case ServerTagClass.Expression:
				case ServerTagClass.Comment:
				case ServerTagClass.AspNetExpression:
				case ServerTagClass.HtmlEncodedExpression:
					if (CurrentToken.Class == TokenClass.Literal) {
						tag.ValueToken.Read();
						if (CurrentToken.Class == TokenClass.ServerTagEnd) {
							tag.EndToken.Read();
						}
					}
					break;
				}
			} else Error("Server Tag: server tag expected.");
			return false;
		}

		public virtual bool ParseDocument(Document doc) {
			doc.Clear();
			var res = ParseChildren(doc.Children);
			return res;
		}

		public virtual bool Parse(TextNode node) {
			node.Clear();
			if (node is Literal) return ParseLiteral((Literal)node, true);
			if (node is Attribute) return ParseAttribute((Attribute)node);
			if (node is ServerTag) return ParseServerTag((ServerTag)node);
			if (node is Tag) return ParseTag((Tag)node);
			if (node is XmlDocHeader) return ParseXmlDocHeader((XmlDocHeader)node);
			if (node is AttributeCollection) return ParseAttributes((AttributeCollection)node);
			if (node is ChildCollection) return ParseChildren((ChildCollection)node);
			if (node is Element) return ParseElement((Element)node);
			if (node is Script) return ParseScript((Script)node);
			if (node is Style) return ParseStyle((Style)node);
			if (node is SpecialElement) return ParseSpecialElement((SpecialElement)node);
			if (node is Document) return ParseDocument((Document)node);
			if (node is Token) ((Token)node).Read();
			return true;
		}

	}
}
