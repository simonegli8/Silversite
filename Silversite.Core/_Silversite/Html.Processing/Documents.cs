using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Reflection;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;

namespace Silversite.Html {

	public enum EditorRight { None, NoAsp, SafeAspControls, AllAspControls, AllAspControlsAndCode };
	public enum ServerTagClass { Code, Declaration, Expression, AspNetExpression, HtmlEncodedExpression, Binding, Comment, PersistentObject, PersistentObjectName };
	public enum DocumentClass { Page, ContentPage, MasterPage, UserControl, Html };

	public class Literal : ElementBase {
		public virtual Token LiteralToken { get; protected set; }
		public virtual Token CDataStartToken { get; protected set; }
		public virtual Token CDataEndToken { get; protected set; }

		public Literal() : base() { LiteralToken = New<Token>(TokenClass.Literal); CDataStartToken = CDataEndToken = null; }
		public Literal(string html) : this() { Text = html; }

		public override void Clear() { base.Clear(LiteralToken, CDataStartToken, CDataEndToken); }

		public override string Value { get { return LiteralToken.Value; } set { LiteralToken.Value = value; } }
		public bool ContainsServerCode { get { return Text.Contains("<%=") || Text.Contains("<%$"); } }
		public bool IsCData {
			get { return CDataStartToken != null; }
			set {
				if (value != IsCData) {
					if (value) {
						Clear();
						CDataStartToken = New<Token>(TokenClass.CDataStart);
						LiteralToken = New<Token>(TokenClass.Literal);
						CDataEndToken = New<Token>(TokenClass.CDataEnd);
					} else {
						CDataStartToken = CDataEndToken = null;
					}
				}
			}
		}

		public override bool HasRestrictedServerCode(EditorRight rights) {
			return rights != EditorRight.AllAspControlsAndCode && ContainsServerCode;
		}
		public override string InnerText { get { return Value; } set { Value = value; } }
	}

	public class Tag : ElementBase {
		public virtual Token StartToken { get; protected set; }
		public virtual Token NameToken { get; protected set; }
		public virtual Token SpaceBeforeEndToken { get; protected set; }
		public virtual Token EndToken { get; protected set; }
		public virtual Token NewLineAfter { get; protected set; }
		AttributeCollection attributes;
		public override AttributeCollection Attributes { get { return attributes; } set { if (value != attributes) { attributes.Clear(); foreach (var a in value) attributes.Add(a.Clone()); } } }
		public override void Clear() { base.Clear(StartToken, NameToken, SpaceBeforeEndToken, EndToken, Attributes, NewLineAfter); }

		public Tag()
			: base() {
			StartToken = New<Token>(TokenClass.TagStart);
			NameToken = New<Token>(TokenClass.Identifier);
			attributes = New<AttributeCollection>();
			SpaceBeforeEndToken = New<Token>(TokenClass.Whitespace); SpaceBeforeEndToken.Value = " ";
			EndToken = New<Token>(TokenClass.SingleTagEnd);
			NewLineAfter = New<Token>(TokenClass.Whitespace); NewLineAfter.Value = Environment.NewLine;
		}
		public Tag(string html) : this() { Text = html; }


		Document doc = null;
		Type type = null;
		public override DocumentNode ParentNode {
			get { return base.ParentNode; }
			set {
				base.ParentNode = value;
				if (type != null && doc != Document) {
					Type = type;
				}
			}
		}

		public override string FullName { get { return NameToken.Value; } set { NameToken.Value = value; type = null; } }
		public override string Name { get { return base.Name; } set { base.Name = value; type = null; } }
		public override string Prefix { get { return base.Prefix; } set { base.Prefix = value; type = null; } }
		public virtual bool IsSingleTag {
			get {
				return EndToken.Class == TokenClass.SingleTagEnd || Parser.SingleTags.Contains(FullName, StringComparer.OrdinalIgnoreCase) || String.Equals(FullName, "lang", StringComparison.OrdinalIgnoreCase);
			}
			set { if (IsSingleTag != value) if (value) EndToken.Class = TokenClass.SingleTagEnd; else EndToken.Class = TokenClass.TagEnd; }
		}
		public virtual bool IsEndTag {
			get { return StartToken.Class == TokenClass.EndTagStart; }
			set {
				if (value != IsEndTag) {
					if (value) StartToken.Class = TokenClass.EndTagStart;
					else StartToken.Class = TokenClass.TagStart;
				}
			}
		}

		public void RegisterType() {
			if (Document != null && type != null) Type = type;
		}

		public Type Type {
			get {
				if (Prefix.IsNullOrEmpty()) {
					switch (Name.ToLower()) {
						default:
						case "html": return typeof(HtmlGenericControl);
						case "a": return typeof(HtmlAnchor);
						case "button": return typeof(HtmlButton);
						case "form": return typeof(HtmlForm);
						case "head": return typeof(HtmlHead);
						case "img": return typeof(HtmlImage);
						case "input":
							switch (Attributes["type"]) {
								case "button": return typeof(HtmlInputButton);
								case "checkbox": return typeof(HtmlInputCheckBox);
								case "file": return typeof(HtmlInputFile);
								case "hidden": return typeof(HtmlInputHidden);
								case "image": return typeof(HtmlInputImage);
								case "password": return typeof(HtmlInputPassword);
								case "radiobutton": return typeof(HtmlInputRadioButton);
								case "reset": return typeof(HtmlInputReset);
								case "submit": return typeof(HtmlInputSubmit);
								case "text": return typeof(HtmlInputText);
								default: return typeof(HtmlGenericControl);
							}
						case "link": return typeof(HtmlLink);
						case "meta": return typeof(HtmlMeta);
						case "select": return typeof(HtmlSelect);
						case "table": return typeof(HtmlTable);
						case "td": return typeof(HtmlTableCell);
						case "tr": return typeof(HtmlTableRow);
						case "textarea": return typeof(HtmlTextArea);
						case "title": return typeof(HtmlTitle);
					}
				} else {
					foreach (var d in Document.Registrations) {
						if (d.TagPrefix == Prefix) {
							if (!d.TagName.IsNullOrEmpty() && d.TagName == Name && !d.Source.IsNullOrEmpty()) return Type.GetType("System.Web.UI.UserControl");
							else if (!d.Namespace.IsNullOrEmpty()) {
								var typeName = d.Namespace + "." + Name;
								if (!d.Assembly.IsNullOrEmpty()) typeName += ", " + d.Assembly;
								var type = Type.GetType(typeName);
								if (type != null) return type;
							}
						}
					}
				}
				return null;
			}
			set {
				ParentChanged += (sender, args) => RegisterType();
				doc = Document;
				var name = value.Name;
				var namespc = value.Namespace;
				if (namespc.StartsWith("System.Web.UI.HtmlControls")) {
					switch (name) {
						case "HtmlAnchor": FullName = "a"; break;
						case "HtmlButton": FullName = "button"; break;
						case "HtmlForm": FullName = "form"; break;
						case "HtmlHead": FullName = "head"; break;
						case "HtmlImage": FullName = "img"; break;
						case "HtmlInputButton": FullName = "input"; Attributes["type"] = "button"; break;
						case "HtmlInputCheckBox": FullName = "input"; Attributes["type"] = "checkbox"; break;
						case "HtmlInputFile": FullName = "input"; Attributes["type"] = "file"; break;
						case "HtmlInputHidden": FullName = "input"; Attributes["type"] = "hidden"; break;
						case "HtmlInputImage": FullName = "input"; Attributes["type"] = "image"; break;
						case "HtmlInputPassword": FullName = "input"; Attributes["type"] = "password"; break;
						case "HtmlInputRadioButton": FullName = "input"; Attributes["type"] = "radiobutton"; break;
						case "HtmlInputReset": FullName = "input"; Attributes["type"] = "reset"; break;
						case "HtmlInputSubmit": FullName = "input"; Attributes["type"] = "submit"; break;
						case "HtmlInputText": FullName = "input"; Attributes["type"] = "text"; break;
						case "HtmlLink": FullName = "link"; break;
						case "HtmlMeta": FullName = "meta"; break;
						case "HtmlSelect": FullName = "select"; break;
						case "HtmlTable": FullName = "table"; break;
						case "HtmlTableCell": FullName = "td"; break;
						case "HtmlTableRow": FullName = "tr"; break;
						case "HtmlTextArea": FullName = "textarea"; break;
						case "HtmlTitle": FullName = "title"; break;
						default: break;
					}
				} else {
					if (!(value.IsSubclassOf(typeof(System.Web.UI.Control)) || value == typeof(System.Web.UI.Control))) throw new NotSupportedException("Only types derived from System.Web.UI.Control can be server side elements.");
					var assembly = value.Assembly;
					if (Document == null) {
						FullName = "user:" + name;
						type = value;
						return;
					}
					foreach (var d in Document.Registrations) {
						if (!d.Namespace.IsNullOrEmpty() && d.Namespace == namespc) {
							var declAssemblyName = d.Assembly;
							if (!declAssemblyName.IsNullOrEmpty()) {
								if (assembly.FullName.StartsWith(declAssemblyName)) {
									FullName = d.TagPrefix + ":" + name;
									type = value;
									return;
								}
							} else {
								FullName = d.TagPrefix + ":" + name;
								type = value;
								return;
							}
						}
					}
					var prefix = "user" + Document.Registrations.Count().ToString();
					Document.AddRegistration(new RegistrationInfo { Assembly = assembly.FullName, Namespace = namespc, TagPrefix = prefix });
					FullName = prefix + ":" + name;
				}
				type = value;
			}
		}

		public bool IsScript { get { return Is("script"); } }
		public bool IsStyle { get { return Is("style"); } }
		public bool IsSpecialElement { get { return IsScript || IsStyle; } }

		public override IEnumerable<DocumentNode> AllChildren { get { return Attributes.AllChildren; } }

		static readonly string[] fileexts = new string[] { ".cmd", ".exe", ".pdf", ".gif", ".jpg", ".jpeg", ".png", ".xps", ".msi", ".wav", ".avi", ".mp3", ".mp4", ".bmp", ".svg", ".svgz", ".css", ".js",
			".zip", ".7z", ".rar", ".bz2", ".gz", ".tar", ".iso", ".xml", ".config", ".sitemap", ".xaml" };

		public override Document Follow() {
			if (!Href.IsNullOrEmpty()) {
				if (Href.StartsWith("#")) return Document;
				var url = Document.Url;
				if (Href.Contains(':')) url = new Uri(Href);
				else url = new Uri(url, Href);
				if (fileexts.Any(ext => url.PathAndQuery.UpTo('?').EndsWith(ext))) return null;
				return Document.OpenLink(url, null, Document.Encoding, Document.ExecuteJavaScript);
			} else if (Attributes["NavigateUrl"] != null) {
				var nav = Attributes["NavigateUrl"];
				if (fileexts.Any(ext => nav.UpTo('?').EndsWith(ext))) return null;
				if (nav.StartsWith("#")) return Document;
				if (nav.Contains(':')) return Document.Open(new Uri(nav), null, Document.Encoding, Document.ExecuteJavaScript);
				if (nav.Contains('~')) return Document.Open(nav);
				if (Document.Page != null) return Document.Open(Services.Paths.Combine(Document.Page.AppRelativeVirtualPath, nav));
				if (Document.Url != null) return Document.Open(new Uri(Document.Url, nav), null, Document.Encoding, Document.ExecuteJavaScript);
			}
			return null;
		}

		public override Stream Download() {
			string src = null;
			if (!Href.IsNullOrEmpty()) src = Href;
			else if (!Src.IsNullOrEmpty()) src = Src;
			else if (Attributes.Any(a => a.Name.EndsWith("Url"))) src = Services.Paths.Url(Attributes.First(a => a.Name.EndsWith("Url")).Value);

			var url = Document.Url;
			if (src.StartsWith("#")) return new MemoryStream(Encoding.UTF8.GetBytes(Document.Text));
			if (src.Contains(':')) url = new Uri(src);
			else url = new Uri(url, src);

			return Services.Files.Download(url);
		}

		public override string DownloadText() { return new StreamReader(Download(), true).ReadToEnd(); }

		public override string PlainText { get { return ""; } }
	}

	public class Element : Container {
		public virtual Tag StartTag { get; protected set; }
		Tag endTag;
		public virtual Tag EndTag { get { return endTag; } set { ReplaceNode(endTag, value); endTag = value; } }

		public Element()
			: base() {
			StartTag = New<Tag>();
			Children = New<ChildCollection>();
			EndTag = New<Tag>(); EndTag.IsEndTag = true; EndTag.EndToken.Class = TokenClass.TagEnd;
		}
		public Element(string html) : this() { Text = html; }

		public override void Clear() { base.Clear(StartTag, Children, EndTag); }

		public override AttributeCollection Attributes { get { return StartTag.Attributes; } }
		//public string this[string attribute] { get { return Attributes[attribute]; } set { Attributes[attribute] = value; } } 

		public override string Name { get { return StartTag.Name; } set { StartTag.Name = value; EndTag.Name = value; } }
		public override string Prefix { get { return StartTag.Prefix; } set { StartTag.Prefix = value; EndTag.Prefix = value; } }
		public override string FullName { get { return StartTag.FullName; } set { StartTag.FullName = value; EndTag.FullName = value; } }

		// public override string ContentKey { get { if (Attributes.Contains("ContentKey")) return Attributes["ContentKey"]; return null; } set { Attributes["ContentKey"] = value; } }
		public override bool IsServerControl { get { return StartTag.IsServerControl; } set { StartTag.IsServerControl = value; } }
		public override Type Type {
			get { return StartTag.Type; }
			set {
				StartTag.Type = value;
				ParentChanged += (sender, args) => StartTag.RegisterType();
			}
		}
		public override IEnumerable<DocumentNode> AllChildren { get { return Children.OfType<DocumentNode>().Concat(Attributes); } }
		public override Document Follow() { return StartTag.Follow(); }
		public override Stream Download() { return StartTag.Download(); }
		public override string DownloadText() { return StartTag.DownloadText(); }

		public override string InnerText {
			get {
				return Children.Text.Trim().Replace("\n" + Tabs, "\n");
			}
			set {
				value = Environment.NewLine + Tabs + "\t" + value.Trim().Replace("\n", '\n' + Tabs + '\t') + Environment.NewLine + Tabs;
				Children.Text = value;
			}
		}

		public override bool HasRestrictedServerCode(EditorRight rights) {
			switch (rights) {
				case EditorRight.NoAsp:
					return IsServerControl || Children.Any(ch => ch.HasRestrictedServerCode(rights));
				default:
				case EditorRight.SafeAspControls:
					return Attributes.ConatinsServerExpression || IsServerControl && (Name.Equals("script", StringComparison.OrdinalIgnoreCase) || Name.EndsWith("DataSource")) || Children.Any(ch => ch.HasRestrictedServerCode(rights));
				case EditorRight.AllAspControls:
					return Attributes.ConatinsServerExpression || Name.Equals("script", StringComparison.OrdinalIgnoreCase) && IsServerControl || Children.Any(ch => ch.HasRestrictedServerCode(rights));
				case EditorRight.AllAspControlsAndCode:
					return false;
			}
		}

		public override string PlainText {
			get {
				string NewLine = Environment.NewLine;
				switch (FullName.ToLower()) {
					case "h1":
					case "h2":
					case "h3":
					case "h4":
					case "h5":
					case "h6":
					case "div":
					case "p":
					case "blockquote":
					case "pre":
						return base.PlainText + NewLine + NewLine;
					case "br": return NewLine;
					case "a":
					case "span":
					case "dt":
					case "address":
					case "em":
					case "strong":
					case "code":
					case "samp":
					case "kbd":
					case "var":
					case "cite":
					case "dfn":
					case "abbr":
					case "acronym":
					case "q":
					case "ins":
					case "b":
					case "i":
					case "tt":
					case "u":
					case "strike":
					case "big":
					case "small":
					case "sup":
					case "sub":
					case "center":
					case "body":
					case "html":
					case "form": return base.PlainText;
					case "ul": return NewLine + Children.Where(child => child.Is("li")).Select(child => "* " + child.PlainText.Replace("\n", "\n  ")).StringList(NewLine) + NewLine;
					case "ol": int n = 1; return NewLine + Children.Where(child => child.Is("li")).Select(child => n.ToString() + ". " + child.PlainText.Replace("\n", "\n    ")).StringList(NewLine) + NewLine;
					case "dl": return NewLine + Children.Where(child => child.Is("dt") || child.Is("dd")).Select(child => child.PlainText).StringList(NewLine) + NewLine;
					case "dd": return "   " + base.PlainText.Replace("\n", "\n   ");
					case "table":
					case "tr":
					case "td":
					case "th":
					default: return "";
				}
			}
		}
	}

	public class DocumentInfo : Element, Services.IDocumentInfo {

		public DocumentInfo() : base() { Type = typeof(Web.UI.DocumentInfo); IsServerControl = true; }
		public DocumentInfo(string html) : this() { Text = html; }

		public override int ContentKey { get { int key; if (int.TryParse(Attributes["ContentKey"] ?? string.Empty, out key)) return key; return Services.Document.None; } set { Attributes["ContentKey"] = value.ToString(); } }
		public int Revision { get { var n = 0; int.TryParse(Attributes["Revision"] ?? string.Empty, out n); return n; } set { if (value != null) Attributes["Revision"] = value.ToString(); } }
		public int CurrentRevision { get { var n = 0; int.TryParse(Attributes["CurrentRevision"] ?? string.Empty, out n); return n; } set { Attributes["CurrentRevision"] = value.ToString(); } }
		public Services.Person Author { get { return Services.Persons.Find(Attributes["Author"]) ?? Services.Persons.Current; } set { Attributes["Author"] = value != null ? value.UserName : null; } }
		public string Title { get { return Attributes["Title"]; } set { Attributes["Title"] = value; } }
		public string Notes { get { return Attributes["Notes"]; } set { Attributes["Notes"] = value; } }
		public string Tags { get { return Attributes["Tags"]; } set { Attributes["Tags"] = value; } }
		public string Categories { get { return Attributes["Categories"]; } set { Attributes["Categories"] = value; } }
		public DateTime Published { get { var d = DateTime.Now; DateTime.TryParse(Attributes["Published"] ?? string.Empty, out d); return d; } set { Attributes["Published"] = value != default(DateTime) ? value.ToString("g") : null; } }
		public override string PlainText { get { return ""; } }
	}

	public class SpecialElement : ElementBase {
		public Tag StartTag { get; protected set; }
		Tag endTag;
		public Tag EndTag { get { return endTag; } set { ReplaceNode(endTag, value); endTag = value; } }
		public Token ValueToken { get; protected set; }

		public SpecialElement() : base() { StartTag = New<Tag>(); ValueToken = New<Token>(TokenClass.Script); EndTag = New<Tag>(); EndTag.IsEndTag = true; }
		public SpecialElement(string html) : this() { Text = html; }
		public override void Clear() { base.Clear(StartTag, EndTag, ValueToken); }

		public override string Value {
			get { return ValueToken.Value.Trim().Replace("\n" + Tabs, "\n"); }
			set {
				value = Environment.NewLine + Tabs + "\t" + value.Trim().Replace("\n", '\n' + Tabs + '\t') + Environment.NewLine + Tabs;
				ValueToken.Value = value;
			}
		}
		public override string Name { get { return StartTag.Name; } set { StartTag.Name = value; EndTag.Name = value; } }
		public override string Prefix { get { return StartTag.Prefix; } set { StartTag.Prefix = value; EndTag.Prefix = value; } }
		public override string FullName { get { return StartTag.FullName; } set { StartTag.FullName = value; EndTag.FullName = value; } }
		public bool IsServerControl { get { return StartTag.IsServerControl; } set { StartTag.IsServerControl = value; } }
		public override AttributeCollection Attributes { get { return StartTag.Attributes; } }

		public string Type { get { return Attribute["type"] ?? ""; } set { Attribute["type"] = value; } }
		public override string InnerText { get { return Value; } set { Value = value; } }

		public override IEnumerable<DocumentNode> AllChildren { get { return StartTag.AllChildren; } }
		public override string PlainText { get { return ""; } }

		public override Stream Download() { return StartTag.Download(); }
		public override string DownloadText() { return StartTag.DownloadText(); }
	}

	public class Script : SpecialElement {

		public Script() : base() { StartTag.FullName = "script"; EndTag.FullName = "script"; Type = "text/javascript"; }
		public Script(string html) : this() { Text = html; }

		public override bool HasRestrictedServerCode(EditorRight rights) {
			switch (rights) {
				default:
				case EditorRight.SafeAspControls:
				case EditorRight.AllAspControls:
				case EditorRight.NoAsp:
					return IsServerControl;
				case EditorRight.AllAspControlsAndCode:
					return false;
			}
		}
	}

	public class Style : SpecialElement {
		public Style() { StartTag.FullName = "style"; EndTag.FullName = "style"; Type = "text/css"; }
		public Style(string html) : this() { Text = html; }
	}

	public class ServerTag : Tag {
		public Token ValueToken { get; protected set; }

		public ServerTag()
			: base() {
			ValueToken = New<Token>(TokenClass.Literal);
			Attributes.After(ValueToken); // place the ValueToken after the Attributes
		}
		public ServerTag(string html) : this() { Text = html; }

		public override void Clear() { base.Clear(StartToken, NameToken, Attributes, EndToken, ValueToken, NewLineAfter); }

		public override string Value { get { return ValueToken.Value; } set { ValueToken.Value = value; } }
		public ServerTagClass Class { get { return StartToken.ServerTagClass; } set { StartToken.ServerTagClass = value; } }

		public override bool IsSingleTag { get { return true; } set { } }

		public string Source {
			get { if (Class == ServerTagClass.Declaration && Name == "Register") return Attributes["src"]; return string.Empty; }
			set { if (Class == ServerTagClass.Declaration && Name == "Register") Attributes["src"] = value; }
		}
		public string TagName {
			get { if (Class == ServerTagClass.Declaration && Name == "Register") return Attributes["tagname"]; return string.Empty; }
			set { if (Class == ServerTagClass.Declaration && Name == "Register") Attributes["tagname"] = value; }
		}
		public string TagPrefix {
			get { if (Class == ServerTagClass.Declaration && Name == "Register") return Attributes["tagprefix"]; return string.Empty; }
			set { if (Class == ServerTagClass.Declaration && Name == "Register") Attributes["tagprefix"] = value; }
		}
		public string Namespace {
			get { if (Class == ServerTagClass.Declaration && Name == "Register") return Attributes["namespace"]; return string.Empty; }
			set { if (Class == ServerTagClass.Declaration && Name == "Register") Attributes["namespace"] = value; }
		}
		public string Assembly {
			get { if (Class == ServerTagClass.Declaration && Name == "Register") return Attributes["assembly"]; return string.Empty; }
			set { if (Class == ServerTagClass.Declaration && Name == "Register") Attributes["assembly"] = value; }
		}

		public override bool HasRestrictedServerCode(EditorRight rights) {
			switch (rights) {
				case EditorRight.NoAsp:
					return true;
				default:
				case EditorRight.SafeAspControls:
				case EditorRight.AllAspControls:
					return Class == ServerTagClass.Code || Class == ServerTagClass.Expression;
				case EditorRight.AllAspControlsAndCode:
					return false;
			}
		}
		public override string PlainText { get { return ""; } }
	}

	public class PersistentObject : DocumentNode {
		public virtual Token StartToken { get; protected set; }
		public virtual Token EndToken { get; protected set; }
		public virtual Token NameToken { get; protected set; }
		public virtual Token NameEndToken { get; protected set; }
		public Token TextToken { get; protected set; }
		public virtual Token NewLineAfter { get; protected set; }

		public PersistentObject()
			: base() {
			StartToken = New<Token>(TokenClass.TagStart);
			NameToken = New<Token>(TokenClass.Literal);
			NameEndToken = New<Token>(TokenClass.TagEnd);
			TextToken = New<Token>(TokenClass.Literal);
			EndToken = New<Token>(TokenClass.SingleTagEnd);
			NewLineAfter = New<Token>(TokenClass.Whitespace); NewLineAfter.Value = Environment.NewLine;
		}
		public PersistentObject(string html) : this() { Text = html; }

		public override void Clear() { base.Clear(StartToken, TextToken, EndToken, NewLineAfter); }

		public new string Name { get { return NameToken.Value; } set { NameToken.Value = value; } }
		public string Text { get { return TextToken.Value; } set { TextToken.Value = value; } }

		public object Value {
			get {
				var trimmed = Text.Trim();
				if (trimmed.StartsWith("!object:")) {
					using (var s = new MemoryStream(Convert.FromBase64String(trimmed.Substring("Base64Object:".Length)))) {
						var f = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
						return f.Deserialize(s);
					}
				} else if (trimmed.StartsWith("!char:")) return trimmed[Text.IndexOf('\'') + 1];
				else if (trimmed.StartsWith("!byte:")) return (byte)int.Parse("0x" + trimmed.Substring("!byte:".Length).Trim());
				else if (trimmed.StartsWith("!bool:")) return bool.Parse(trimmed.Substring("!bool:".Length).Trim());
				else if (trimmed.StartsWith("!short:")) return short.Parse(trimmed.Substring("!short:".Length).Trim());
				else if (trimmed.StartsWith("!int:")) return int.Parse(trimmed.Substring("!int:".Length).Trim());
				else if (trimmed.StartsWith("!long:")) return long.Parse(trimmed.Substring("!long:".Length).Trim());
				else if (trimmed.StartsWith("!ushort:")) return ushort.Parse(trimmed.Substring("!ushort:".Length).Trim());
				else if (trimmed.StartsWith("!uint:")) return uint.Parse(trimmed.Substring("!uint:".Length).Trim());
				else if (trimmed.StartsWith("!ulong:")) return ulong.Parse(trimmed.Substring("!ulong:".Length).Trim());
				else if (trimmed.StartsWith("!float:")) return float.Parse(trimmed.Substring("!float:".Length).Trim());
				else if (trimmed.StartsWith("!double:")) return double.Parse(trimmed.Substring("!double:".Length).Trim());
				else return Text;
			}
			set {
				if (value is string && !((string)value).Contains("%>")) Text = (string)value;
				else if (value is char) Text = " !char: '" + value + "'";
				else if (value is byte) Text = " !byte: " + ((int)value).ToString("X2");
				else if (value is bool) Text = " !bool: " + value.ToString();
				else if (value is short) Text = " !short: " + value.ToString();
				else if (value is int) Text = " !int: " + value.ToString();
				else if (value is long) Text = " !long: " + value.ToString();
				else if (value is ushort) Text = " !ushort: " + value.ToString();
				else if (value is uint) Text = " !uint: " + value.ToString();
				else if (value is ulong) Text = " !ulong: " + value.ToString();
				else if (value is float) Text = " !float: " + ((float)value).ToString("R");
				else if (value is double) Text = " !double: " + ((double)value).ToString("R");
				else {
					using (var s = new MemoryStream()) {
						var f = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
						f.Serialize(s, value);
						Text = " !object: " + Convert.ToBase64String(s.ToArray(), Base64FormattingOptions.InsertLineBreaks);
					}
				}
			}
		}
		public override string PlainText { get { return ""; } }
	}


	public class Doctype : ElementBase {
		public Token StartToken { get; protected set; }
		public ChildCollection Children { get; protected set; }
		public Token EndToken { get; protected set; }
		public Token NewLineAfter { get; protected set; }

		public Doctype() : base() { StartToken = New<Token>(TokenClass.DoctypeStart); Children = New<ChildCollection>(); EndToken = New<Token>(TokenClass.TagEnd); NewLineAfter = New<Token>(TokenClass.Whitespace); NewLineAfter.Value = Environment.NewLine; }
		public Doctype(string html) : this() { Text = html; }
		public override void Clear() { base.Clear(StartToken, Children, EndToken, NewLineAfter); }
		public override string PlainText { get { return ""; } }
	}

	public class XmlDocHeader : Tag {
		public XmlDocHeader()
			: base() {
			StartToken = New<Token>(TokenClass.XmlDocTagStart);
			NameToken = New<Token>(TokenClass.Identifier);
			Attributes = New<AttributeCollection>();
			EndToken = New<Token>(TokenClass.XmlDocTagEnd);
			NewLineAfter = New<Token>(TokenClass.Whitespace); NewLineAfter.Value = Environment.NewLine;
			Attributes["version"] = "1.0";
			Attributes["encoding"] = "UTF-8";
		}
		public XmlDocHeader(string html) : this() { Text = html; }
		public override void Clear() { base.Clear(StartToken, NameToken, Attributes, EndToken, NewLineAfter); }
		public override string PlainText { get { return ""; } }
	}

	public class RegistrationInfo {
		public string Source { get; set; }
		public string Namespace { get; set; }
		public string Assembly { get; set; }
		public string TagName { get; set; }
		public string TagPrefix { get; set; }
		public bool Global { get; set; }
	}

	public class LoginControl {
		public LoginControl(Document doc) { Page = doc; }
		Document Page;
		public string Username { get { return Page["form :text ~ :password"].Value; } set { Page["form :text ~ :password"].Value = value; } }
		public string Password { get { return Page["form :password"].Value; } set { Page["form :password"].Value = value; } }
	}

	public class JavaScriptBrowser : Services.StaticService<JavaScriptBrowser, JavaScriptBrowserProvider> {
		public static string Run(string html) { return Provider.Run(html); }
	}

	public class JavaScriptBrowserProvider : Services.Provider<JavaScriptBrowser> {
		public virtual string Run(string html) { return html; }
	}

	public class Document : Container {

		public string Path { get; set; }
		public Services.Domain Domain { get; set; }
		public Page Page { get { return Control as Page; } }
		public Uri Url { get; internal set; }
		public CookieCollection Cookies = new CookieCollection();
		public Encoding Encoding { get; set; }
		public bool ExecuteJavaScript { get; set; }

		//Document properties
		public virtual Element Head { get { return this.Find<Element>(e => string.Compare(e.FullName, "head", true) == 0); } set { } }
		public virtual Element Body { get { return this.Find<Element>(e => string.Compare(e.FullName, "body", true) == 0); } set { } }
		public virtual Element Form { get { return this.Find<Element>(e => string.Compare(e.FullName, "form", true) == 0); } set { } }
		public virtual ServerTag Declaration { get { return this.Find<ServerTag>(s => s.FullName == "Page" || s.FullName == "MasterPage" || s.FullName == "Control"); } set { } }
		public virtual string MasterPage { get { var d = Declaration; if (d != null && d.Attributes.Contains("MasterPageFile")) return d.Attributes["MasterPageFile"]; return null; } }
		public virtual LoginControl Login { get { return new LoginControl(this); } }

		IEnumerable<RegistrationInfo> WebConfigRegistrations {
			get {
				var pages = (System.Web.Configuration.PagesSection)System.Web.Configuration.WebConfigurationManager.GetSection("system.web/pages");
				return pages.Controls.OfType<System.Web.Configuration.TagPrefixInfo>().Select(c => new RegistrationInfo { Source = c.Source, Assembly = c.Assembly, Namespace = c.Namespace, TagName = c.TagName, TagPrefix = c.TagPrefix, Global = true }).Reverse();
			}
		}
		public virtual IEnumerable<RegistrationInfo> Registrations {
			get {
				return this.All<ServerTag>(s => s.FullName == "Register").Reverse()
					.Select(c => new RegistrationInfo { Source = c.Source, Assembly = c.Assembly, Namespace = c.Namespace, TagName = c.TagName, TagPrefix = c.TagPrefix, Global = false })
					.Union(WebConfigRegistrations);
			}
		}
		public virtual void AddRegistration(RegistrationInfo info) {
			var lastreg = this.All<ServerTag>(s => s.Parent == this && (s.FullName == "Page" || s.FullName == "MasterPage" || s.FullName == "Control" || s.FullName == "Register")).LastOrDefault();
			var decl = new ServerTag() { Class = ServerTagClass.Declaration, Source = info.Source, Namespace = info.Namespace, Assembly = info.Assembly, TagName = info.TagName, TagPrefix = info.TagPrefix };
			if (lastreg != null) Children.Insert(lastreg.Index + 1, decl);
			else Children.Insert(0, decl);
		}

		public override TextNode Clone() {
			var doc = (Document)base.Clone();
			doc.Path = Path; doc.Domain = Domain; doc.Url = Url; doc.Cookies = new CookieCollection(); Cookies.OfType<Cookie>().ForEach(c => doc.Cookies.Add(c));
			return doc;
		}

		public DocumentClass Class {
			get {
				var d = Declaration;
				if (d != null) {
					switch (d.FullName) {
						case "Page":
							if (d.Attributes.Contains("MasterPageFile")) return DocumentClass.ContentPage;
							else return DocumentClass.Page;
						case "MasterPage": return DocumentClass.MasterPage;
						case "Control": return DocumentClass.UserControl;
						default: throw new NotSupportedException();
					}
				} else {
					return DocumentClass.Html;
				}
			}
		}
		public override bool HasRestrictedServerCode(EditorRight rights) { return Children.Any(n => n.HasRestrictedServerCode(rights)); }

		// search in this document and all MasterPages and UserControls.
		public Element DeepControl<T>(Func<Element, bool> condition) where T : Control { return DeepFind<Element>(e => e.Type == typeof(T) && condition(e)); }
		public Element DeepControl<T>() where T : Control { return DeepControl<T>(n => true); }
		public IEnumerable<Element> DeepControls<T>(Func<Element, bool> condition) where T : Control { return DeepAll<Element>(e => e.Type == typeof(T) && condition(e)); }
		public IEnumerable<Element> DeepControls<T>() where T : Control { return DeepControls<T>(n => true); }
		public Container DeepFind(IDocument control) { return DeepFind<Container>(c => c.ContentKey == control.ContentKey); }
		public T DeepFind<T>(Func<T, bool> condition) where T : DocumentNode {
			var res = Find<T>(condition);
			if (res == null && MasterPage != null) res = Open(MasterPage).DeepFind<T>(condition); // search all master pages.
			if (res == null) {
				foreach (var reg in Registrations) { // search all UserControls.
					if (!reg.Source.IsNullOrEmpty()) {
						res = Open(reg.Source).DeepFind<T>(condition);
						if (res != null) return res;
					}
				}
			}
			return res;
		}
		public T DeepFind<T>() where T : DocumentNode { return DeepFind<T>(n => true); }
		public IEnumerable<T> DeepAll<T>(Func<T, bool> condition) where T : DocumentNode {
			var res = All<T>(condition);
			if (MasterPage != null) res = res.Concat(Open(MasterPage).DeepAll<T>(condition)); // search all master pages.
			foreach (var reg in Registrations) { // search all UserControls.
				if (!reg.Source.IsNullOrEmpty()) {
					res = res.Concat(Open(reg.Source).DeepAll<T>(condition));
				}
			}
			return res;
		}
		public IEnumerable<T> DeepAll<T>() where T : DocumentNode { return DeepAll<T>(n => true); }
		public IEnumerable<T> DeepAll<T>(string selector) where T: DocumentNode { return DeepAll<T>(Expression(null, ref selector)); }
		public T DeepFind<T>(string selector) where T: DocumentNode { return DeepAll<T>(selector).FirstOrDefault(); }

		public static bool CreateContentKeys(Html.Document doc) { // creates a ContentKey for all IDocuments in a Page that have no ContentKey.
			var refresh = false;
			foreach (var editable in doc.DeepAll<Html.Container>(c => c.Type.Implements(typeof(IDocument))).Reverse()) {
				var info = editable.Children.OfType<Element>().FirstOrDefault(child => child is DocumentInfo);
				if (info == null || info.ContentKey == Services.Document.None) {
					editable.Publish();
					refresh = true;
				}
			}
			return refresh;
		}
		public static bool CreateContentKeys(Page page) { return CreateContentKeys(Open(page)); }

		public Document() : base() { Domain = Services.Domains.Current; Path = "~/Silversite/temp/temp.aspx"; Parser = new Parser(); Writer = new Writer(); Reader = new Reader(); Encoding = Encoding.UTF8; }
		public Document(string html) : this() { Text = html; }
		public Document(string html, Parser parser, Reader reader, Writer writer) : base() { Parser = parser; Writer = writer; Reader = reader; Text = html; }

		static Regex IsHtmlExpr = new Regex("(<%@\\s+Page)|(<%@\\s+Control)|(<%@\\s+Master)|(</?[A-Za-z0-9.:;_\\-*]+(\\s*[A-Za-z_\\-.]=(\"[^\"\\n]*\")|('[^'\\n]*'))*/?>)");
		private static bool IsHtml(string text) {
			if (text == null) return false;
			var match = IsHtmlExpr.Match(text);
			return match != null && match.Success;
		}

		public static Document Open(Page page) { var doc = Open(page.AppRelativeVirtualPath); if (doc != null) doc.Control = page; return doc; }
		public static Document Open(string path, Action<Services.AdvancedWebClient> webClientSetup = null, Encoding encoding = null, bool executeJavaScript = false) {
			if (path.StartsWith("http:") || path.StartsWith("https:") || path.StartsWith("ftp:") || path.StartsWith("ftps:")) return Open(new Uri(path), webClientSetup, encoding, executeJavaScript);
			return Open(Services.Domains.Current, path);
		}

		//public static new Document Open(int ContentKey) { return Container.Open(ContentKey) as Document; }
		public static Document Open(Services.Domain domain, string path) {
			var doc = new Document();
			var html = Services.Domains.Files.Load(domain, path);
			if (html == null) html = Services.Files.LoadVirtual(path);
			if (html == null || !IsHtml(html)) return null;
			doc.Domain = domain; doc.Path = path;
			doc.Text = html;
			doc.Url = new Uri(Services.Paths.Url(path));
			doc.Encoding = Encoding.UTF8;
			return doc;
		}
		public static Document Open(Uri url, Action<Services.AdvancedWebClient> webClientSetup = null, Encoding encoding = null, bool executeJavaScript = false) {
			if (url.IsFile) return Open(url.AbsolutePath);
			else {
				var doc = new Document();
				return doc.OpenLink(url, webClientSetup, encoding, executeJavaScript);
			}
		}

		public Document OpenLink(Uri url, Action<Services.AdvancedWebClient> webClientSetup = null, Encoding encoding = null, bool executeJavaScript = false) {
			try {
				if (url.IsFile) return Open(url.AbsolutePath);
				else {
					encoding = encoding ?? Encoding;
					var doc = new Document();
					string html = null;
					if (url.Scheme == "http" || url.Scheme == "https") {
						if (Services.Paths.IsLocal(url)) {
							html = Services.Files.Execute(url.AbsoluteUri);
						} else {
							var web = new Services.AdvancedWebClient(Services.Paths.IsLocal(url));
							web.Encoding = encoding;
							web.Cookies = Cookies;
							if (webClientSetup != null) webClientSetup(web);
							html = web.DownloadString(url.ToString());
							doc.Cookies = web.Cookies;
						}
					} else {
						using (var s = Services.Files.Download(url)) {
							if (s != null) return null;
							using (var r = new StreamReader(s, Encoding.UTF8, true)) {
								html = r.ReadToEnd();
							}
						}
					}
					if (ExecuteJavaScript) html = JavaScriptBrowser.Run(html);
					if (!IsHtml(html)) return null;
					doc.Text = html;
					doc.Url = url;
					doc.Encoding = encoding;
					return doc;
				}
			} catch (Exception ex) {
				return new NoDocument(ex);
			}
		}

		public Document OpenLink(string url, Action<Services.AdvancedWebClient> webClientSetup = null, Encoding encoding = null) {
			if (!url.Contains("://")) url = Services.Paths.Url(url);
			return OpenLink(new Uri(url), webClientSetup, encoding);
		}


		public override Container Rendered() {
			if (Page == null) throw new NotSupportedException("No page is associated with this document so it coul load a rendered page.");
			return Open(Page.Request.Url);
		}

		private string MapUrl(string href) {
			if (!(href.Contains(':') || href.StartsWith("#"))) {
				var path = Services.Paths.Directory(Document.Url.ToString());
				while (href.StartsWith("../")) {
					href = href.After("../");
					path = Services.Paths.Directory(path);
				}
				return Services.Paths.Combine(path, href);
			}
			return href;
		}

		public void Serve() {
			var fileurls = new Regex(@"url\((.*)\)");
			foreach (var script in All("script")) script.Src = MapUrl(script.Src); // scripts
			foreach (var img in All("img")) img.Src = MapUrl(img.Src); // images
			foreach (var link in All("link[rel=stylesheet][type=text/css]")) link.Href = MapUrl(link.Href); // style sheets
			foreach (var img in All("[style*=background]")) { // css file references in style attribute
				img.Style = fileurls.Replace(img.Style, new MatchEvaluator(match => "url(" + MapUrl(match.Groups[1].Value) + ")"));
			}
			foreach (var style in All("style")) { // css styles
				style.InnerText = fileurls.Replace(style.InnerText, new MatchEvaluator(match => "url(" + MapUrl(match.Groups[1].Value) + ")"));
			}
			foreach (var link in All("a")) link.Href = MapUrl(link.Href);
			foreach (var form in All("form")) form.Attribute["action"] = MapUrl(form.Attribute["action"]);
			HttpContext.Current.Response.Clear();
			HttpContext.Current.Response.ContentType = "text/html";
			HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;
			HttpContext.Current.Response.Write(Text);
			HttpContext.Current.Response.End();
		}

	}

	public class NoDocument : Document {
		public NoDocument() : base() { Exception = null; }
		public NoDocument(string html) : this() { }
		public NoDocument(Exception ex) : base() { Exception = ex; }
		public Exception Exception { get; private set; }
		public override TextNode Clone() {
			var doc = (NoDocument)base.Clone();
			doc.Exception = Exception;
			return doc;
		}
	}
}
