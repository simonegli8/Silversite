using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Diagnostics.Contracts;

namespace Silversite.Html {
	
	public class Reader {

		public char[] Whitespace = new char[4] { ' ', '\r', '\n', '\t' };
		public char[] Reserved = new char[8] { ' ', '\r', '\n', '\t', '/', '<', '>', '=' };

		public enum Modes { Html, Css, Script, Literal };

		// public string[] SingleTags = new string[3] { "img", "hr", "br" };

		string text;
		public virtual string Text {
			get { return text; } 
			set {
				text = value ?? string.Empty;
				Line = 1; LastNewline = 0; Position = 0;
				CurrentToken = NewToken(); CurrentToken.Class = TokenClass.Literal;
				EndOfDocument.Start = text.Length; EndOfDocument.End = text.Length;
				Errors = new List<string>();
				Mode = Modes.Html;
				NextToken();
			}
		}

		public int Position { get; protected set; }
		public int Line { get; protected set; }
		public int LastNewline { get; protected set; }
		public Modes Mode { get; set; }

		public Token CurrentToken { get; protected set; }

		public string CurrentIdentation { get; set; }

		public Token EndOfDocument { get; set; }
		public List<string> Errors { get; private set; }
		public bool SuppressErrors { get; set; }
		public void Error(string message, params object[] args) { if(!SuppressErrors) Errors.Add(" Line " + Line.ToString() + ": " + string.Format(message, args)); }
		public string ErrorReport { get { var sb = new StringBuilder("<span style='color:red;'>"); foreach (var s in Errors) { sb.Append("<br/>"); sb.Append(s); } sb.Append("</span"); return sb.ToString(); } }

		public Parser Parser { get; set; }

		public Token NewToken() { var t = new Token(); t.Start = Position; t.Length = 0; t.Line = Line; t.Column = t.Start - LastNewline + 1; t.Parser = Parser; return t; }
		public Queue<Token> WhitespaceTokens { get; set; }
		public int Column { get { return Position - LastNewline + 1; } }
		// for debugging purposes.
		internal string LookAhead { get { return Text.SafeSubstring(Position, 64); } }
		internal string LookBack { get { return Text.SafeSubstring(Position - 64, 64); } }

		public Reader() {
			EndOfDocument = new Token(TokenClass.EndOfDocument);
			WhitespaceTokens = new Queue<Token>();
			Text = string.Empty;
		}
		public Reader(string text) : this() { Text = text; }

		public virtual void Skip(Token t) {
			if (t.Length < 0) t.Length = 0;
			while (Position < Text.Length && Position < t.End) {
				if (Text[Position] == '\n') { Line++; LastNewline = Position+1; }
				Position++;
			}
		}

		public virtual void ReadToReserved(Token t) { ReadTo(t, Reserved); }
		public virtual void ReadTo(Token t, params char[] characters) {
			Skip(t);
			int w;
			if (characters.Length > 1) {
				w = Text.IndexOfAny(characters, Position);
			} else {
				w = Text.IndexOf(characters[0], Position);
			}
			if (w > 0) {
				t.End = w;
				Skip(t);
			} else {
				//Start = End = Html.Length;
				t.End = Text.Length;
				Skip(t);
			}
		}
		public virtual void ReadTo(Token t, string s) {
			while(true) {
				ReadTo(t, s[0]);
				if (Position == Text.Length || Peek(s, null)) return; 
				t.Length++;
			}
		}
		public virtual void ReadOver(Token t, string s) {
			ReadTo(t, s);
			if (Position < Text.Length) t.Length += s.Length;
			else t.End = Text.Length;
			Skip(t);
		}

		public virtual char Peek(int i) { return (Position + i >= 0 && Position + i < Text.Length) ? Text[Position + i] : Whitespace[0]; }
		public virtual char Peek() { return Peek(0); }
		public virtual bool Peek(int i, string s) {
			if (Position + i + s.Length > Text.Length) return false;
			for (int n = 0; n < s.Length; n++) if (char.ToLowerInvariant(Peek(i+n)) != char.ToLowerInvariant(s[n])) return false;
			return true;
		}
		public virtual bool Peek(string s, Token t) { return Peek(0, s); }
		public string Preview { get { return Text.Substring(Position, Math.Min(80, Text.Length - Position)); } }

		public virtual void SkipWhitespace(ref Token t) {
			var start = Position;
			while (Position < Text.Length && Whitespace.Contains(Text[Position])) {
				if (Text[Position] == '\n') { Line++; LastNewline = Position+1; }
				Position++;
			}
			if (start != Position) {
				t.Class = TokenClass.Whitespace; t.End = Position;
				WhitespaceTokens.Enqueue(t); Read(t);
				if (LastNewline >= t.Start) CurrentIdentation = Text.Substring(LastNewline, t.End - LastNewline);
				t = NewToken();
				CurrentToken = t;
			}
		}
		public void Read(Token t) {
			Contract.Ensures(t.Start <= t.End);
			Skip(t);
			if (t.Class == TokenClass.EndOfDocument) return;
			if (t.Class == TokenClass.String) t.Value = Text.Substring(t.Start + 1, t.Length - 2);
			else t.Value = Text.Substring(t.Start, t.Length);
			if (t.Class != TokenClass.Whitespace) {
				t.Identation = CurrentIdentation;
				CurrentIdentation = string.Empty;
			}
		}

		public virtual Token NextToken() {
			if (CurrentToken == EndOfDocument) return EndOfDocument;

			Skip(CurrentToken);

			var t = NewToken();
		
			if (t.Start >= Text.Length) {
				t = EndOfDocument;
			} else if (Mode == Modes.Css) {
				t.Class = TokenClass.Script; ReadTo(t, "</style>"); Read(t); Mode = Modes.Html;
			} else if (Mode == Modes.Script) {
				t.Class = TokenClass.Script; ReadTo(t, "</script>"); Read(t); Mode = Modes.Html;
			} else if (Mode == Modes.Literal) {
				t.Class = TokenClass.Literal; ReadTo(t, "<"); Read(t); Mode = Modes.Html;
			} else if (CurrentToken.Class == TokenClass.ServerTagStart && CurrentToken.ServerTagClass != ServerTagClass.Declaration) {
				t.Class = TokenClass.Literal; ReadTo(t, "%>");
			} else if (CurrentToken.Class == TokenClass.ServerTagStart && CurrentToken.ServerTagClass == ServerTagClass.PersistentObject) {
				t.Class = TokenClass.Literal; t.ServerTagClass = ServerTagClass.PersistentObject; ReadTo(t, "]");
			} else if (CurrentToken.Class == TokenClass.Literal && CurrentToken.ServerTagClass == ServerTagClass.PersistentObject) {
				t.Class = TokenClass.ServerTagEnd; t.ServerTagClass = ServerTagClass.PersistentObjectName; t.Length = 1;
			} else if (CurrentToken.Class == TokenClass.ServerTagEnd && CurrentToken.ServerTagClass == ServerTagClass.PersistentObjectName) {
				t.Class = TokenClass.Literal; t.ServerTagClass = ServerTagClass.PersistentObject; ReadTo(t, "--%>");
			} else if (CurrentToken.Class == TokenClass.CDataStart) {
				t.Class = TokenClass.CData; ReadTo(t, "]]>");
			} else if (CurrentToken.Class == TokenClass.CData) {
				t.Class = TokenClass.CDataEnd; Peek("]]>", t);
			} else {
				SkipWhitespace(ref t);

				if (t.Start >= Text.Length) t = EndOfDocument;
				else {
					switch (Peek()) {
					case '<':
						if (Peek(1) == '%') {
							t.Class = TokenClass.ServerTagStart; t.Length = 3;
							switch (Peek(2)) {
							case '$': t.ServerTagClass = ServerTagClass.AspNetExpression; break;
							case ':': t.ServerTagClass = ServerTagClass.HtmlEncodedExpression; break;
							case '#': t.ServerTagClass = ServerTagClass.Binding; break;
							case '=': t.ServerTagClass = ServerTagClass.Expression; break;
							case '@': t.ServerTagClass = ServerTagClass.Declaration; break;
							case '-':
								if (Peek(3) == '-') { // <%-- server comment
									if (Peek(4, "[SilversitePersistentObject:")) { t.ServerTagClass = ServerTagClass.PersistentObject; break;
									} else {
										t.ServerTagClass = ServerTagClass.Comment; t.Length = 4; Read(t); WhitespaceTokens.Enqueue(t);
										t = NewToken(); t.Class = TokenClass.Literal; t.ServerTagClass = ServerTagClass.Comment;
										ReadTo(t, "--%>");
										t.Length = Position - t.Start; Read(t); WhitespaceTokens.Enqueue(t);
										if (Position < Text.Length) { t = NewToken(); t.Class = TokenClass.ServerTagEnd; t.ServerTagClass = ServerTagClass.Comment; t.Length = 4; t.Line = Line; Read(t); WhitespaceTokens.Enqueue(t); }
										CurrentToken = t;
										return NextToken();
									}
								} else {
									t.ServerTagClass = ServerTagClass.Code; t.Length = 2; break;
								}
							default: t.ServerTagClass = ServerTagClass.Code; t.Length = 2; break;
							}
						} else if (Peek(1) == '/') {
							t.Class = TokenClass.EndTagStart;
						} else if (Peek(1, "![CDATA[")) { // <
							t.Class = TokenClass.CDataStart;
						} else if (Peek(1, "!DOCTYPE")) {
							t.Class = TokenClass.DoctypeStart;
						} else if (Peek(1, "!--")) {
							t.Class = TokenClass.HtmlCommentStart;
							//if (Peek(4, "[if ")) ReadOver(t, "]>"); // conditional html comment
							t.Length = 4; Read(t); WhitespaceTokens.Enqueue(t);
							t = NewToken(); t.Class = TokenClass.HtmlComment;
							ReadTo(t, "-->");
							t.Length = Position - t.Start; Read(t); WhitespaceTokens.Enqueue(t);
							if (Position < Text.Length) { t = NewToken(); t.Class = TokenClass.HtmlCommentEnd; t.Length = 3; t.Line = Line; Read(t); WhitespaceTokens.Enqueue(t); }
							CurrentToken = t;
							return NextToken();

							//Read(t); WhitespaceTokens.Enqueue(t); CurrentToken = t; return NextToken();
						/*} else if (Peek(1, "![endif]-->")) { // end of conditional html comment
							t.Class = TokenClass.HtmlCommentEnd; t.Length = "<![endif]-->".Length; Read(t); WhitespaceTokens.Enqueue(t); CurrentToken = t; return NextToken(); */
						} else if (Peek(1, "?")) { // xml header
							t.Class = TokenClass.XmlDocTagStart; t.Length = 2;
						} else {
							t.Class = TokenClass.TagStart; t.Length = 1;
						}
						break;
					case '-':
						if (Peek(1, "->")) {
							t.Class = TokenClass.HtmlCommentEnd; Read(t); WhitespaceTokens.Enqueue(t); CurrentToken = t; return NextToken();
						} else {
							t.Class = TokenClass.Identifier; t.Length = 1; ReadToReserved(t);
						}
						break;
					case '?':
						if (Peek(1, ">")) { // xml header end tag.
							t.Class = t.Class = TokenClass.XmlDocTagEnd; t.Length = 2;
						} else {
							t.Class = TokenClass.Identifier; t.Length = 1; ReadToReserved(t);
						}
						break;
					case '>': t.Class = TokenClass.TagEnd; t.Length = 1; break;
					case '%':
						if (Peek(1) == '>') {
							t.Class = TokenClass.ServerTagEnd; t.Length = 2;
						} else {
							t.Class = TokenClass.Identifier; t.Length = 1; ReadToReserved(t);
						}
						break;
					case '/':
						if (Peek(1) == '>') {
							t.Class = TokenClass.SingleTagEnd; t.Length = 2;
						} else {
							t.Class = TokenClass.Identifier; t.Length = 1; ReadToReserved(t);
						}
						break;
					case '=': t.Class = TokenClass.Equals; t.Length = 1; break;
					case '\'':
					case '\"':
						t.Class = TokenClass.String;
						var quote = Peek();
						if (quote == '"') t.StringClass = StringClass.DoubleQuote; else t.StringClass = StringClass.SingleQuote;
						t.Length++;
						ReadTo(t, quote);
						//if (Peek() == '\n' || Peek() == ' ') Error("string ends with newline.");
						t.Length++;
						break;
					default:
						t.Class = TokenClass.Identifier; t.Length = 1; ReadToReserved(t);
						break;
					}
				}
				if (t.Start > t.End) t.Length = 0;
				if (t.End > Text.Length) t.End = Text.Length;
			}
			Read(t);
			return CurrentToken = t;
		}

		public Token LiteralToken() {
			if (CurrentToken.Class == TokenClass.Identifier || CurrentToken.Class == TokenClass.String || CurrentToken.Class == TokenClass.Whitespace || CurrentToken.Class == TokenClass.Equals) {
				var t0 = CurrentToken;
				Mode = Modes.Literal;
				var t1 = NextToken();
				t1.Start = t0.Start;
				Read(t1);
				return CurrentToken = t1;
			}
			return null;
		}

		public virtual Token SkipToTokens(params TokenClass[] classes) {
			var list = classes.ToList();
			list.Add(TokenClass.EndOfDocument);
			while (list.All(c => CurrentToken.Class != c)) NextToken();
			return CurrentToken;
		}

		public virtual Token PeekToken() {
			var cur = CurrentToken;
			var line = Line;
			var position = Position;
			var nl = LastNewline;
	
			var next = NextToken();

			CurrentToken = cur;
			Line = line;
			Position = position;
			LastNewline = nl;

			return next;
		}

	}
}