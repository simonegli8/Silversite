using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Reflection;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using Silversite;

namespace Silversite.Html {

	public interface IDocument {
		Nullable<int> ContentKey { get; }
	}

	public enum DocumentClasses { Html, WebForms, RazorCS, RazorVB }

	public class DocumentNode : TextNode {

		public DocumentNode() : base() { }
		public DocumentNode(string html) : base(html) { }

		DocumentClasses? docClass = null;
		public DocumentClasses DocClass { get { return Document != null ? Document.DocClass : docClass ?? DocumentClasses.WebForms; } set { docClass = value; } }

		public virtual int Index {
			get {
				var p = ParentNode;
				if (p is AttributeCollection) return ((AttributeCollection)p).IndexOf(this);
				else if (p is ChildCollection) return ((ChildCollection)p).IndexOf(this);
				else throw new NotSupportedException();
			}
		}

		public Document Document { get { if (this is Document) return (Document)this; if (ParentNode == null) return null; return ParentNode.Document; } }
		public event EventHandler ParentChanged;
		public new virtual DocumentNode ParentNode {
			get { return ((TextNode)this).Parent as DocumentNode; }
			set {
				((TextNode)this).Parent = value;
				if (ParentChanged != null) ParentChanged(this, EventArgs.Empty);
			}
		}
		public new Container Parent {
			get {
				if (ParentNode != null) {
					if (ParentNode is Container) return (Container)ParentNode;
					if (ParentNode.ParentNode != null && ParentNode.ParentNode is Container) return (Container)ParentNode.ParentNode;
				}
				return null;
			}
		}
		public virtual bool HasRestrictedServerCode(EditorRight rights) { return AllChildren.Any(ch => ch != this && ch.HasRestrictedServerCode(rights)); }
		public IEnumerable<Container> Parents { get { var p = Parent; while (p != null) { yield return p; p = p.Parent; } } }

		public virtual IEnumerable<DocumentNode> AllChildren { get { return Nodes.OfType<DocumentNode>(); } }

		public DocumentNode LastNode {
			get {
				if (AllChildren.Count() == 0) return this;
				return AllChildren.Last().LastNode;
			}
		}

		public DocumentNode FirstNode {
			get {
				if (AllChildren.Count() == 0) return this;
				return AllChildren.First().FirstNode;
			}
		}

		public DocumentNode NextNode {
			get {
				var p = ParentNode;
				if (p == null) return null;
				int i = p.Nodes.IndexOf(this);
				while (++i < p.Nodes.Count && !(p.Nodes[i] is DocumentNode)) ;
				if (i < p.Nodes.Count) return ((DocumentNode)p.Nodes[i]).FirstNode;
				else {
					p = p.NextNode;
					if (p == null) return null;
					return p.FirstNode;
				}
			}
		}

		public DocumentNode PreviousNode {
			get {
				var p = ParentNode;
				if (p == null) return null;
				int i = p.Nodes.IndexOf(this);
				while (--i >= 0 && !(p.Nodes[i] is DocumentNode)) ;
				if (i >= 0) return ((DocumentNode)p.Nodes[i]).LastNode;
				else {
					p = p.PreviousNode;
					if (p == null) return null;
					return p.LastNode;
				}
			}
		}

		// form postbacks
		public virtual Document Submit() {
			var f = this;
			while (f != null && !(f is Element && ((Element)f).Is("form"))) f = f.Parent;
			if (f == null) f = this.Find<Element>(e => e.Is("form"));
			if (f == null) throw new NotSupportedException("Html Document has no form to postback");
			var form = (Element)f;
			var url = Document.Url;
			if (url == null) throw new NotSupportedException("Document must have an url");
			var action = form.Attributes["action"];
			if (!action.IsNullOrEmpty()) {
				if (action.Contains(':')) url = new Uri(action);
				else url = new Uri(url, action);
			}
			var query = new StringBuilder();
			var inputs = form.All<Element>(e => e.Is("input"));
			var formValues = new NameValueCollection();
			bool clickbutton = true;
			if (this is Element && ((Element)this).Is("input") && ((Element)this).Attributes["type"] == "submit") {
				((Element)this).Attributes["value"] = "1";
				clickbutton = false;
			}
			foreach (var input in inputs) {
				if (clickbutton && input.Attributes["type"] == "submit") {
					input.Attributes["value"] = "1";
					clickbutton = false;
				}
				if (input.Attributes["name"] != null) {
					formValues.Add(input.Attributes["name"], input.Attributes["value"] ?? "");

					if (query.Length > 0) query.Append('&');
					query.Append(input.Attributes["name"]);
					query.Append('=');
					query.Append(input.Attributes["value"] ?? "");
				}
			}

			HttpWebRequest web = null;
			foreach (Cookie cookie in Document.Cookies) web.CookieContainer.Add(cookie);

			if (form.Attributes["method"] == null || form.Attributes["method"].ToLower() == "get") {
				web = (HttpWebRequest)WebRequest.Create(new Uri(Services.Paths.RemoveSlash(url.OriginalString.UpTo('?')) + "?" + query.ToString()));
			} else {
				web = (HttpWebRequest)WebRequest.Create(url);
				var files = inputs
					.Where(e => e.Attributes["type"] == "file")
					.Select(e => new UploadFile(e.Attributes["value"], e.Attributes["name"], Services.MimeType.OfExtension(e.Attributes["value"])))
					.ToArray();
				HttpUploadHelper.Upload(web, files, formValues);
			}

			web.Accept = "text/html, application/xhtml+xml, */*";
			web.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)";
			web.Host = url.Host;

			var resp = web.GetResponse();
			if (resp == null) return null;

			using (var r = new StreamReader(resp.GetResponseStream())) {
				var html = r.ReadToEnd();
				if (Document.ExecuteJavaScript) html = JavaScriptBrowser.Run(html);
				var doc = new Document(html);
				doc.ExecuteJavaScript = Document.ExecuteJavaScript;
				doc.Url = web.RequestUri;
				doc.Cookies = (resp as HttpWebResponse).Cookies;
				return doc;
			}

			return null;
		}

		public Element Form {
			get {
				if (this is Element && ((Element)this).Is("form")) return (Element)this;
				if (Parent == null) return null;
				return Parent.Form;
			}
		}

		// search documents
		public T Find<T>(Func<T, bool> condition) where T : DocumentNode {
			if (this is T && condition((T)this)) return (T)this;
			foreach (var ch in AllChildren) {
				var x = ch.Find<T>(condition);
				if (x != null) return x;
			}
			return null;
		}
		public T Find<T>() where T : DocumentNode { return Find<T>(n => true); }
		public IEnumerable<T> All<T>(Func<T, bool> condition) where T : DocumentNode {
			if (this is T && condition((T)this)) yield return (T)this;
			foreach (var ch in AllChildren.ToList()) {
				foreach (var x in ch.All<T>(condition)) yield return x;
			}
		}
		public IEnumerable<T> All<T>() where T : DocumentNode { return All<T>(n => true); }
		/*public Element FindElement(string type) { return Find<Element>(e => e.Is(type)); }
		public IEnumerable<Element> FindElements(string type) { return FindAll<Element>(e => e.Is(type)); }
		public Element FindElement(string type, Func<Element, bool> condition) { return Find<Element>(e => e.Is(type) && condition(e)); }
		public IEnumerable<Element> FindElements(string type, Func<Element, bool> condition) { return FindAll<Element>(e => e.Is(type) && condition(e)); } */
		public Element Control<T>(Func<Element, bool> condition) where T : Control { return Find<Element>(e => e.Type == typeof(T) && condition(e)); }
		public Element Control<T>() where T : Control { return Control<T>(n => true); }
		public IEnumerable<Element> Controls<T>(Func<Element, bool> condition) where T : Control { return All<Element>(e => e.Type == typeof(T) && condition(e)); }
		public IEnumerable<Element> Controls<T>() where T : Control { return Controls<T>(n => true); }
		public Element Control<T>(string selector) { return All<Element>(selector).FirstOrDefault(e => e.Type == typeof(T)); }
		public IEnumerable<Element> Controls<T>(string selector) { return All<Element>(selector).Where(e => e.Type == typeof(T)); }
		public Container Find(IDocument control) { return Find<Container>(c => c.ContentKey == control.ContentKey); }

		/* JQuery selectors for Find

		All Selector (“*”)																		Basic
		Selects all elements.

		:animated Selector																	Basic Filter, jQuery Extensions
		Select all elements that are in the progress of an animation at the time the selector is run.

		Attribute Contains Prefix Selector [name|="value"]				Attribute
		Selects elements that have the specified attribute with a value either equal to a given string or starting with that string followed by a hyphen (-).

		Attribute Contains Selector [name*="value"]						Attribute
		Selects elements that have the specified attribute with a value containing the a given substring.

		Attribute Contains Word Selector [name~="value"]				Attribute
		Selects elements that have the specified attribute with a value containing a given word, delimited by spaces.

		Attribute Ends With Selector [name$="value"]						Attribute
		Selects elements that have the specified attribute with a value ending exactly with a given string. The comparison is case sensitive.

		Attribute Equals Selector [name="value"]								Attribute
		Selects elements that have the specified attribute with a value exactly equal to a certain value.

		Attribute Not Equal Selector [name!="value"]						Attribute, jQuery Extensions
		Select elements that either don't have the specified attribute, or do have the specified attribute but not with a certain value.

		Attribute Starts With Selector [name^="value"]					Attribute
		Selects elements that have the specified attribute with a value beginning exactly with a given string.

		:button Selector																		Deprecated, Form, jQuery Extensions
		Selects all button elements and elements of type button.

		:checkbox Selector																	Deprecated, Form, jQuery Extensions
		Selects all elements of type checkbox.

		 :checked Selector																	Form
		Matches all elements that are checked.

		Child Selector (“parent > child”)												Hierarchy
		Selects all direct child elements specified by "child" of elements specified by "parent".

		Class Selector (“.class”)															Basic
		Selects all elements with the given class.

		:contains() Selector																	Content Filter
		Select all elements that contain the specified text.

		Descendant Selector (“ancestor descendant”)						Hierarchy
		Selects all elements that are descendants of a given ancestor.

		:disabled Selector																	Form
		Selects all elements that are disabled.

		Element Selector (“element”)													Basic
		Selects all elements with the given tag name.

		:empty Selector																		Content Filter
		Select all elements that have no children (including text nodes).

		:enabled Selector																		Form
		Selects all elements that are enabled.

		:eq() Selector																			Basic Filter, jQuery Extensions
		Select the element at index n within the matched set.

		:even Selector																			Basic Filter, jQuery Extensions
		Selects even elements, zero-indexed. See also odd.

		:file Selector																				Deprecated, Form, jQuery Extensions
		Selects all elements of type file.

		:first-child Selector																	Child Filter
		Selects all elements that are the first child of their parent.

		:first Selector																			Basic Filter, jQuery Extensions
		Selects the first matched element.

		:focus selector																			Basic Filter, Form
		Selects element if it is currently focused.

		:gt() Selector																			Basic Filter, jQuery Extensions
		Select all elements at an index greater than index within the matched set.

		Has Attribute Selector [name]													Attribute
		Selects elements that have the specified attribute, with any value.

		:has() Selector																			Content Filter, jQuery Extensions
		Selects elements which contain at least one element that matches the specified selector.

		:header Selector																		Basic Filter, jQuery Extensions
		Selects all elements that are headers, like h1, h2, h3 and so on.

		:hidden Selector																		jQuery Extensions, Visibility Filter
		Selects all elements that are hidden.

		ID Selector (“#id”)																	Basic
		Selects a single element with the given id attribute.

		:image Selector																		Deprecated, Form, jQuery Extensions
		Selects all elements of type image.

		:input Selector																			Deprecated, Form, jQuery Extensions
		Selects all input, textarea, select and button elements.

		:last-child Selector																	Child Filter
		Selects all elements that are the last child of their parent.

		:last Selector																			Basic Filter, jQuery Extensions
		Selects the last matched element.

		:lt() Selector																				Basic Filter, jQuery Extensions
		Select all elements at an index less than index within the matched set.

		Multiple Attribute Selector [name="value"][name2="value2"]		Attribute
		Matches elements that match all of the specified attribute filters.

		Multiple Selector (“selector1, selector2, selectorN”)				Basic
		Selects the combined results of all the specified selectors.

		Next Adjacent Selector (“prev + next”)									Hierarchy
		Selects all next elements matching "next" that are immediately preceded by a sibling "prev".

		Next Siblings Selector (“prev ~ siblings”)								Hierarchy
		Selects all sibling elements that follow after the "prev" element, have the same parent, and match the filtering "siblings" selector.

		:not() Selector																			Basic Filter
		Selects all elements that do not match the given selector.

		:nth-child() Selector																	Child Filter
		Selects all elements that are the nth-child of their parent.

		:odd Selector																			Basic Filter, jQuery Extensions
		Selects odd elements, zero-indexed. See also even.

		:only-child Selector																	Child Filter
		Selects all elements that are the only child of their parent.

		:parent Selector																		Content Filter, jQuery Extensions
		Select all elements that are the parent of another element, including text nodes.

		:password Selector																	Deprecated, Form, jQuery Extensions
		Selects all elements of type password.

		:radio Selector																			Deprecated, Form, jQuery Extensions
		Selects all elements of type radio.

		:reset Selector																			Deprecated, Form, jQuery Extensions
		Selects all elements of type reset.

		:selected Selector																	Form, jQuery Extensions
		Selects all elements that are selected.

		:submit Selector																		Deprecated, Form, jQuery Extensions
		Selects all elements of type submit.

		:text Selector																			Deprecated, Form, jQuery Extensions
		Selects all elements of type text.

		:visible Selector																		jQuery Extensions, Visibility Filter
		Selects all elements that are visible.
		*/

		static readonly char[] specialChars = new char[] { ':', '.', '#', '[', ']', '(', ')', '+', '~', '!', '>', '^', ',', '=', '\\', '"', '\'', '*', '|', '$', ' ' };
		private string GetArg(ref string selector, int start) {
			var name = "";
			int p = 0;
			if (selector[start] == '\'') {
				p = selector.IndexOf('\'', start + 1);
				while (p != -1 && selector[p - 1] == '\\') p = selector.IndexOf('\'', p + 1);
				if (p == -1) {
					name += selector.Substring(start + 1);
					selector = "";
				} else {
					name += selector.Substring(start + 1, p - start - 1);
					selector = selector.Substring(p + 1);
				}
			} else if (selector[start] == '"') {
				p = selector.IndexOf('"', start + 1);
				while (p != -1 && selector[p - 1] == '\\') p = selector.IndexOf('"', p + 1);
				if (p == -1) {
					name += selector.Substring(start);
					selector = "";
				} else {
					name += selector.Substring(start + 1, p - start - 1);
					selector = selector.Substring(p + 1);
				}
			} else {
				p = selector.IndexOfAny(specialChars, start);

				while (p != -1 && selector[p] == '\\') {
					name += selector.Substring(start, p - start);
					start = p;
					p = selector.IndexOfAny(specialChars, start);
				}
				if (p == -1) {
					name += selector.Substring(start);
					selector = "";
				} else {
					name += selector.Substring(start, p - start);
					selector = selector.Substring(p);
				}
			}
			return name;
		}

		Func<DocumentNode, bool> Term(Func<DocumentNode, bool> parent, ref string selector, out bool child) {
			Func<DocumentNode, bool> exp, pexp;

			child = false;

			while (selector[0] == ' ') selector = selector.Substring(1);
			switch (selector[0]) {
				case '(':
					selector = selector.Substring(1);
					exp = Expression(parent, ref selector);
					if (selector[0] != ')') throw new ArgumentException("Html Find: ) expected");
					selector = selector.Substring(1);
					return exp;
				case '*': return e => true;
				case '.':
					var name = GetArg(ref selector, 1);
					return e => e.Attribute["class"] == name;
				case '#':
					name = GetArg(ref selector, 1);
					return e => e.Id == name;
				case '[':
					name = GetArg(ref selector, 1);
					while (selector[0] == ' ') selector = selector.Substring(1);
					if (selector.StartsWith("=")) {
						selector = selector.Substring(1);
						while (selector[0] == ' ') selector = selector.Substring(1);
						var value = GetArg(ref selector, 0);
						if (selector[0] != ']') throw new ArgumentException("Html Find []: ] expected");
						selector = selector.Substring(1);
						return e => (e.Attribute[name] ?? "").Equals(value, StringComparison.InvariantCultureIgnoreCase);
					} else if (selector.StartsWith("!=")) {
						selector = selector.Substring(2);
						while (selector[0] == ' ') selector = selector.Substring(1);
						var value = GetArg(ref selector, 0);
						if (selector[0] != ']') throw new ArgumentException("Html Find []: ] expected");
						selector = selector.Substring(1);
						return e => !(e.Attribute[name] ?? "").Equals(value, StringComparison.InvariantCultureIgnoreCase);
					} else if (selector.StartsWith("*=")) {
						selector = selector.Substring(2);
						while (selector[0] == ' ') selector = selector.Substring(1);
						var value = GetArg(ref selector, 0);
						if (selector[0] != ']') throw new ArgumentException("Html Find []: ] expected");
						selector = selector.Substring(1);
						return e => (e.Attribute[name] ?? "").ToLower().Contains(value.ToLower());
					} else if (selector.StartsWith("~=")) {
						selector = selector.Substring(2);
						while (selector[0] == ' ') selector = selector.Substring(1);
						var value = GetArg(ref selector, 0);
						if (selector[0] != ']') throw new ArgumentException("Html Find []: ] expected");
						selector = selector.Substring(1);
						return e => (e.Attribute[name] ?? "").SplitList(' ').Contains(value);
					} else if (selector.StartsWith("|=")) {
						selector = selector.Substring(2);
						while (selector[0] == ' ') selector = selector.Substring(1);
						var value = GetArg(ref selector, 0);
						if (selector[0] != ']') throw new ArgumentException("Html Find []: ] expected");
						selector = selector.Substring(1);
						return e => e.Attribute[name] == value || (e.Attribute[name] ?? "").StartsWith(value + '-');
					} else if (selector.StartsWith("$=")) {
						selector = selector.Substring(2);
						while (selector[0] == ' ') selector = selector.Substring(1);
						var value = GetArg(ref selector, 0);
						if (selector[0] != ']') throw new ArgumentException("Html Find []: ] expected");
						selector = selector.Substring(1);
						return e => (e.Attribute[name] ?? "").EndsWith(value);
					} else if (selector.StartsWith("^=")) {
						selector = selector.Substring(2);
						while (selector[0] == ' ') selector = selector.Substring(1);
						var value = GetArg(ref selector, 0);
						if (selector[0] != ']') throw new ArgumentException("Html Find []: ] expected");
						selector = selector.Substring(1);
						return e => (e.Attribute[name] ?? "").StartsWith(value);
					} else {
						if (selector[0] != ']') throw new ArgumentException("Html Find []: ] expected");
						selector = selector.Substring(1);
						return e => e.Attribute[name] != null;
					}
				case ':':
					name = GetArg(ref selector, 1);
					int n;
					string ntext;
					switch (name) {
						case "button": return e => e.Is("input") && e.Attribute["type"] == "button";
						case "checkbox": return e => e.Is("input") && e.Attribute["type"] == "checkbox";
						case "checked": return e => e.Is("input") && e.Checked;
						case "contains":
							if (selector[0] != '(') throw new ArgumentException("Html Find :contains(): ( expected");
							var text = GetArg(ref selector, 1);
							if (selector[0] != ')') throw new ArgumentException("Html Find :contains(): ) expected");
							selector = selector.Substring(1);
							return e => e.InnerText.ToLower().Contains(text.ToLower());
						case "disabled": return e => e.Attribute["disabled"] != null;
						case "empty": return e => e.Children.Count == 0;
						case "enabled": return e => e.Attribute["disabled"] == null;
						case "eq":
							if (selector[0] != '(') throw new ArgumentException("Html Find :eq(): ( expected");
							ntext = GetArg(ref selector, 1);
							if (selector[0] != ')') throw new ArgumentException("Html Find :eq(): ) expected");
							selector = selector.Substring(1);
							n = 0;
							if (!int.TryParse(ntext, out n)) throw new ArgumentException("Html Find :eq(): index number expected");
							var n1 = 0;
							return e => n1++ == n;
						case "even":
							var n2 = 0;
							return e => n2++ % 2 == 0;
						case "file": return e => e.Is("input") && e.Attribute["type"] == "file";
						case "first-child": child = true; return e => e.Parent != null && e.Parent.Children.Count > 0 && e.Parent.Children[0] == e;
						case "first":
							int n3 = 0;
							return e => n3++ == 0;
						case "gt":
							if (selector[0] != '(') throw new ArgumentException("Html Find :gt(): ( expected");
							ntext = GetArg(ref selector, 1);
							if (selector[0] != ')') throw new ArgumentException("Html Find :gt(): ) expected");
							selector = selector.Substring(1);
							n = 0;
							if (!int.TryParse(ntext, out n)) throw new ArgumentException("Html Find :gt(): index number expected");
							int n4 = 0;
							return e => n4++ > n;
						case "has":
							if (selector[0] != '(') throw new ArgumentException("Html Find :has(): ( expected");
							var selector2 = GetArg(ref selector, 1);
							if (selector[0] != ')') throw new ArgumentException("Html Find :has(): ) expected");
							selector = selector.Substring(1);

							pexp = e => e.Parents.Any(e2 => parent(e2));
							exp = Expression(pexp, ref selector);
							return e => exp(e) && pexp(e);
						case "header": return e => e.FullName.Length == 2 && e.FullName[0] == 'h' && '1' <= e.FullName[1] && e.FullName[1] <= '6';
						case "hidden": return e => {
							if (!(e is Element)) return false;
							var el = (Element)e;
							var styles = el.Style.SplitList(';');
							var visibility = styles.FirstOrDefault(st => st.Contains("visibility") || st.Contains("display")).SplitList(':').ToList();
							return visibility[0] == "visibility" && visibility[1] == "hidden" || visibility[0] == "display" && visibility[1] == "none";
						};
						case "image": return e => e.Is("img");
						case "input": return e => e.Is("input") || e.Is("textarea") || e.Is("select") || e.Is("button");
						case "last-child": child = true; return e => e.Parent != null && e.Parent.Children.Count > 0 && e.Parent.Children[e.Parent.Children.Count - 1] == e;
						case "last":
							if (parent == null) parent = x => true;
							var res = All<Element>(e => parent(e)).LastOrDefault();
							return e => e == res;
						case "lt":
							if (selector[0] != '(') throw new ArgumentException("Html Find :gt(): ( expected");
							ntext = GetArg(ref selector, 1);
							if (selector[0] != ')') throw new ArgumentException("Html Find :gt(): ) expected");
							selector = selector.Substring(1);
							n = 0;
							if (!int.TryParse(ntext, out n)) throw new ArgumentException("Html Find :gt(): index number expected");
							int n5 = 0;
							return e => n5++ < n;
						case "not":
							if (selector[0] != '(') throw new ArgumentException("Html Find :not(): ( expected");
							var notselector = GetArg(ref selector, 1);
							if (selector[0] != ')') throw new ArgumentException("Html Find :not(): ) expected");
							selector = selector.Substring(1);
							exp = Expression(parent, ref selector);
							return e => !exp(e);
						case "nth-child":
							child = true;
							if (selector[0] != '(') throw new ArgumentException("Html Find :nth-child(): ( expected");
							ntext = GetArg(ref selector, 1);
							if (selector[0] != ')') throw new ArgumentException("Html Find :nth-child(): ) expected");
							selector = selector.Substring(1);
							n = 0;
							if (!int.TryParse(ntext, out n)) throw new ArgumentException("Html Find :nth-child(): index number expected");
							return e => e.Parent != null && e.Parent.Children.Count >= n && e.Parent.Children[n - 1] == e;
						case "odd":
							var n6 = 0;
							return e => n6++ % 2 != 0;
						case "only-child": return e => e.Parent != null && e.Parent.Children.Count == 1 && e.Parent.Children[0] == e;
						case "parent": return e => e.Children.Count > 0;
						case "password": return e => e.Is("input") && e.Attribute["type"] == "password";
						case "radio": return e => e.Is("input") && e.Attribute["type"] == "radio";
						case "reset": return e => e.Is("input") && e.Attribute["type"] == "reset";
						case "selected": return e => e.Is("option") && e.Selected;
						case "submit": return e => e.Is("input") && e.Attribute["type"] == "submit";
						case "text": return e => e.Is("input") && (e.Attribute["type"] == null || e.Attribute["type"] == "text");
						case "visible": return e => {
							var styles = e.Attribute["style"].SplitList(';');
							var visibility = styles.FirstOrDefault(st => st.Contains("visibility") || st.Contains("display")).SplitList(':').ToList();
							return (visibility[0] == "visibility" && visibility[1] != "hidden" || visibility[0] == "display" && visibility[1] != "none");
						};
						default: throw new ArgumentException(string.Format("Html Find: invalid selector {0}", name));
					}
				default:
					name = GetArg(ref selector, 0);
					return e => e.Is(name);
			}
		}

		Func<DocumentNode, bool> Factor(Func<DocumentNode, bool> parent, ref string selector, out bool child) {
			Func<DocumentNode, bool> exp, pexp, exp2;

			exp = Term(parent, ref selector, out child);
			if (selector.IsNullOrWhiteSpace()) return exp;
			if (selector[0] == ' ') {
				while (selector[0] == ' ') selector = selector.Substring(1);
				if (selector[0] != '+' && selector[0] != '>' && selector[0] != ',' && selector[0] != '(') {
					selector = ' ' + selector;
					return exp;
				}
			}
			switch (selector[0]) {
				case '[':
				case ':':
				case '.':
				case '#':
				case '(':
					bool child2;
					exp2 = Factor(parent, ref selector, out child2);
					if (child2) return e => e.Parent != null && exp(e.Parent) && exp2(e);
					else return e => exp(e) && exp2(e);
				default: return exp;
			}
		}



		Func<DocumentNode, bool> Set(Func<DocumentNode, bool> parent, ref string selector) {
			Func<DocumentNode, bool> exp, pexp, exp2;
			bool child;
			exp = Factor(parent, ref selector, out child);
			if (selector.IsNullOrWhiteSpace()) return exp;
			if (selector[0] == ' ') {
				while (selector[0] == ' ') selector = selector.Substring(1);
				if (selector[0] != '+' && selector[0] != '>' && selector[0] != ',' && selector[0] != '(') {
					selector = ' ' + selector;
					return exp;
				}
			}
			switch (selector[0]) {
				case ',':
					selector = selector.Substring(1);
					exp2 = Set(parent, ref selector);
					return e => exp(e) || exp2(e);
				default: return exp;
			}
		}

		internal Func<DocumentNode, bool> Expression(Func<DocumentNode, bool> parent, ref string selector) {
			Func<DocumentNode, bool> exp, pexp, exp2;

			exp = Set(parent, ref selector);
			if (selector.IsNullOrWhiteSpace()) return exp;
			if (selector[0] == ' ') {
				while (selector[0] == ' ') selector = selector.Substring(1);
				if (selector[0] != '+' && selector[0] != '>' && selector[0] != ',' && selector[0] != '(') {
					pexp = e => e.Parents.Any(p => exp(p));
					exp2 = Expression(pexp, ref selector);
					return e => pexp(e) && exp2(e);
				}
			}
			switch (selector[0]) {
				case '+':
					selector = selector.Substring(1);
					exp2 = Set(parent, ref selector);
					return e => {
						var prev = e.PreviousNode;
						return prev != null && exp(prev) && exp2(e);
					};
				case '~':
					selector = selector.Substring(1);
					exp2 = Set(parent, ref selector);
					return e => {
						var prevnode = e.PreviousNode;
						var prev = new List<DocumentNode>();
						while (prevnode != null && prevnode.Parent == e.Parent) {
							prev.Add(prevnode);
							prevnode = prevnode.PreviousNode;
						}
						return prev.Any(e2 => exp(e2)) && exp2(e);
					};
				case '>':
					selector = selector.Substring(1);
					exp2 = Set(parent, ref selector);
					return e => e.Parent != null && exp(e.Parent) && exp2(e);
				default:
					throw new NotSupportedException("Html Find: Invalid character '" + selector[0] + "'in selector '");
			}
		}

		public IEnumerable<DocumentNode> All(string selector) { return All(Expression(null, ref selector)); }
		public DocumentNode Find(string selector) { return All(selector).FirstOrDefault(); }
		public IEnumerable<T> All<T>(string selector) where T : DocumentNode { return All(selector).OfType<T>(); }
		public T Find<T>(string selector) where T : DocumentNode { return (T)All(selector).FirstOrDefault(e2 => e2 is T); }
		public DocumentNode this[string selector] { get { return Find(selector); } }

		// dom manipulation
		public virtual int Identation { get { return 0; } set { } }
		public string Tabs { get { return "\t".Repeat(Identation); } }

		// element properties
		public virtual bool Is(string tag) { return string.Compare(FullName, tag, true) == 0; }
		public virtual bool IsAny(params string[] tags) { return tags.Any(tag => Is(tag)); }
		public virtual string Name {
			get {
				int i = FullName.IndexOf(':');
				if (i >= 0) return FullName.Substring(i + 1);
				else return FullName;
			}
			set {
				int i = FullName.IndexOf(':');
				if (i >= 0) FullName = FullName.Substring(0, i + 1) + value;
				else FullName = value;
			}
		}
		public virtual string Prefix {
			get {
				int i = FullName.IndexOf(':');
				if (i > 1) return FullName.Substring(0, i);
				else return "";
			}
			set {
				int i = FullName.IndexOf(':');
				if (i >= 0) FullName = value + FullName.Substring(i + 1);
				else FullName = value + ":" + FullName;
			}
		}
		public virtual string FullName { get { return ""; } set { } }
		public virtual AttributeCollection Attributes { get { return new AttributeCollection(); } set { } }
		public virtual AttributeCollection Attribute { get { return Attributes; } set { Attributes = value; } }
		public virtual string InnerText { get { return ""; } set { } }
		public virtual string InnerHtml { get { return InnerText; } set { InnerText = value; } }
		public virtual string Id { get { return Attributes["id"] ?? string.Empty; } set { Attributes["id"] = value; } }
		public virtual ChildCollection Children { get { return new ChildCollection(); } set { } }
		public virtual string PlainText { get { return InnerText; } }

		public virtual string Class { get { return Attributes["class"] ?? string.Empty; } set { Attributes["class"] = value; } }
		public virtual string Src { get { return Attributes["src"] ?? string.Empty; } set { Attributes["src"] = value; } }
		public virtual string Href { get { return Attributes["href"] ?? string.Empty; } set { Attributes["href"] = value; } }
		public virtual string Style { get { return Attributes["style"] ?? string.Empty; } set { Attributes["style"] = value; } }
		public virtual string Value { get { return Attributes["value"] ?? string.Empty; } set { Attributes["value"] = value; } }
		public virtual bool Checked { get { return Attributes["checked"] != null; } set { Attributes["checked"] = value ? "checked" : null; } }
		public virtual bool Selected { get { return Attributes["selected"] != null; } set { Attributes["selected"] = value ? "selected" : null; } }
		public virtual bool IsServerControl { get { return Attributes.Contains("runat") && Attributes["runat"].ToLower() == "server"; } set { if (value) Attributes["runat"] = "server"; else Attributes.Remove("runat"); } }

		public virtual Document Follow() { throw new NotSupportedException(); }
		public virtual Stream Download() { throw new NotSupportedException(); }
		public virtual string DownloadText() { throw new NotSupportedException(); }

		public virtual Document ExtractLanguages(string languages) { throw new NotSupportedException(); }
	}

	public class ElementBase : DocumentNode {

		public ElementBase() : base() { }
		public ElementBase(string html) : base(html) { }
		/*
		public ElementBase Last {
			get {
				if (!(this is Container) || Children.Count == 0) return this;
				return Children.Last().Last;
			}
		}

		public ElementBase First {
			get {
				if (!(this is Container) || Children.Count == 0) return this;
				return Children.First().First;
			}
		}

		public new ElementBase Next {
			get {
				var p = Parent;
				if (p == null) return null;
				int i = p.Children.IndexOf(this);
				if (i < p.Children.Count-1) return p.Children[i+1].First;
				else {
					var pn = p.Next;
					if (pn == null) return null;
					return pn.First;
				}
			}
		}

		public new ElementBase Previous {
			get {
				Container p = Parent;
				if (p == null) return null;
				int i = p.Children.IndexOf(this);
				if (i > 0) return p.Children[i-1].Last;
				else {
					var pp = p.Previous;
					if (pp == null) return null;
					return pp.Last;
				}
			}
		}
		*/
		public new virtual IEnumerable<ElementBase> All(string selector) { return base.All<ElementBase>(selector); }
		public new virtual ElementBase Find(string selector) { return base.Find<ElementBase>(selector); }

		// DOM manipulation
		public virtual void Wrap(Element e) {
			if (Parent != null) {
				var p = Parent;
				int index = Index;
				p.Children.Remove(this);
				e.Children.Insert(0, this);
				p.Children.Insert(index, e);
			}
		}

		public virtual void Unwrap() {
			var p = Parent;
			if (p != null) {
				var pp = p.Parent;
				if (pp != null && p.Children.Count == 1) {
					p.Children.Remove(this);
					pp.Children.Replace(p, this);
				}
			}
		}

		public virtual void Remove() { if (Parent != null) Parent.Children.Remove(this); }
		public virtual void Replace(ElementBase e) { if (Parent != null) Parent.Children.Insert(Parent.Children.IndexOf(this), e); Remove(); }
		public virtual void After(ElementBase n) { if (Parent != null) Parent.Children.AddAt(Index + 1, n); }
		public virtual void Before(ElementBase n) { if (Parent != null) Parent.Children.AddAt(Index, n); }
		static Regex text2html = new Regex(@"<|>|\[\[|\]\]|<lang[:a-zA-z-/*+]*?>");
		static Regex html2text = new Regex(@"\[\[\[|\]\]\]|\[\[|\]\]");

		public static string ExtractLanguagesFromText(string text, string languages) {
			text = text2html.Replace(text, match => {
				switch (match.Value) {
					case "[[": return "[[[";
					case "]]": return "]]]";
					case "<": return "[[";
					case ">": return "]]";
					default: return match.Value;
				}
			});
			text = new ElementBase(text)
				.ExtractLanguages(languages)
				.Text;
			return html2text.Replace(text, match => {
				switch (match.Value) {
					case "[[[": return "[[";
					case "]]]": return "]]";
					case "[[": return "<";
					case "]]": return ">";
					default: return match.Value;
				}
			});
		}

		public virtual ElementBase ExtractLanguages(string languages) {
			languages = languages ?? "";

			var cs = languages.SplitList(',', ';').ToList();
			var langtags = All<Element>(e => e.Prefix == "lang" || e.FullName == "lang").ToList();
			var sections = new List<List<Element>>();
			sections.Add(new List<Element>());
			foreach (var tag in langtags) {
				sections.Last().Add(tag);
				if (tag.FullName == "lang") sections.Add(new List<Element>());
			}

			if (sections.Last().Count == 0) sections.Remove(sections.Last());

			foreach (var section in sections) {
				// find all lang tags that must be removed.
				var tagsToRemove = section
					.Where(tag =>
						!(tag.Name.SplitList(',', ';').Any(c => cs.Contains(c) || cs.Any(c2 => c2.StartsWith(c) || c2.EndsWith(c))) &&
						tag.Name.Contains('+') == cs.Count > 1));
				// if all tags of this section would be removed, look for the default "*" tag.
				if (tagsToRemove.Count() == section.Count) tagsToRemove = section.Where(tag => !tag.Name.Contains("*"));

				foreach (var tag in tagsToRemove) { // removes tags & content that don't match language
					if (tag.FullName == "lang") tag.Remove();
					else {
						var p = tag.Parent;
						if (p == null) continue;
						var i = tag.Index;
						p.Children.RemoveAt(i);
						while (i < p.Children.Count && !(p.Children[i].Prefix == "lang" || p.Children[i].FullName == "lang")) p.Children.RemoveAt(i);
					}
				}
				var remainingTags = section.Except(tagsToRemove);
				foreach (var tag in remainingTags) tag.Remove(); // remove remaining tags.
			}
			return this;
		}
	}

	public class UnableToLocateControlException : NotSupportedException {
		public UnableToLocateControlException(string err) : base(err) { }
		public UnableToLocateControlException(string err, Exception innerException) : base(err, innerException) { }
	}

	public abstract class Container : ElementBase {
		public Container() : base() { children = New<ChildCollection>(); }
		public Container(string html) : this() { Text = html; }

		ChildCollection children;
		public override ChildCollection Children { get { return children; } set { if (value != children) { children.Clear(); foreach (var child in value) children.Add(child.Clone()); } } }
		public override IEnumerable<DocumentNode> AllChildren { get { return Children; } }
		public override void Clear() { base.Clear(Children); Children.Clear(); }
		public virtual Type Type { get; set; }

		// documents
		internal int key = Services.Document.None;
		public virtual int ContentKey { // the ContentKey of the database document
			get {
				if (key != Services.Document.None) return key; // use key if it has a value
				if (Info != null) { // look for the ContentKey in the Info subelement.
					return Info.ContentKey;
				}
				if (Parent != null) return Parent.ContentKey; // return the Parent's ContentKey
				return Services.Document.None; // this element has no corresponding database document.
			}
			set {
				if (Info == null) Info = new DocumentInfo();
				Info.ContentKey = value;
			}
		}
		DocumentInfo info = null;
		public virtual Services.IDocumentInfo Info {
			get {
				if (info == null) {
					if (this is DocumentInfo) info = (DocumentInfo)this;
					else info = Children.OfType<DocumentInfo>().FirstOrDefault();
				}
				return info;
			}
			set {
				if (!(this is DocumentInfo)) {
					lock (this) {
						if (Info == null) {
							info = new DocumentInfo();
							Children.AddAt(0, info);
						}
						Services.Document.CopyInfo(value, info);
					}
				}
			}
		}
		public virtual Container DocumentElement { get { if (Parent == null || ContentKey != Parent.ContentKey) return this; if (Parent != null) return Parent.DocumentElement; return null; } }
		public virtual void CreateDocument() { if (ContentKey == Services.Document.None)	ContentKey = Services.Documents.Create(Document.Path).ContentKey; }

		public void Save() { Services.Domains.Files.SaveWithPath(Document.Text, Document.Domain, Document.Path); }
		public void Preview() { Save(); }
		public void Publish() {
			CreateDocument();
			DocumentElement.Info.Revision++;
			DocumentElement.Info.Published = DateTime.Now;
			DocumentElement.Info.Author = Services.Persons.Current;
			Save();
			Services.Documents.Publish(DocumentElement.Info, DocumentElement.Children.Text);
		}
		public void Revert(int revision) { if (ContentKey != Services.Document.None) Children.Text = Services.Documents.Revision(ContentKey, revision).Text; else throw new NotSupportedException("ContentKey needed to revert."); }

		Control FindDocument(Control c, int key) {
			if (c is IDocument && ((IDocument)c).ContentKey == key) return c;
			foreach (Control child in c.Controls) {
				var d = FindDocument(child, key);
				if (d != null) return d;
			}
			return null;
		}

		//TODO support dynamic creation of a ContentKey in order to find controls without ID.
		Control control;
		public Control Control {
			get {
				if (control != null) return control;
				else if (this == Document) return Document.Page;
				else if (this is Element && !((Element)this).Id.IsNullOrEmpty()) return Document.Page.FindControl(((Element)this).Id);
				else if (key != Services.Document.None) return FindDocument(Document.Page, key);
				else throw new UnableToLocateControlException("Cannot find control on page, because it has no proper ContentKey or ID.");
			}
			set { control = value; }
		}

		public static Container Open(Control control) {
			Container c = null;
			var doc = Document.Open(control.Page);
			if (doc != null) {
				if (control is Page) return doc;
				else if (control is IDocument && ((IDocument)control).ContentKey.HasValue) c = doc.DeepFind<Container>(e => e.ContentKey == ((IDocument)control).ContentKey.Value);
				else if (!control.ID.IsNullOrEmpty()) c = doc.DeepFind<Element>(e => e.Id == control.ID);
				else throw new UnableToLocateControlException("Cannot find control on page, because it has no proper ContentKey or ID.");
			}
			if (c != null) c.Control = control;
			return c;
		}
		public static Container Open(int ContentKey) {
			var info = Services.Documents.Current(ContentKey);
			if (info == null) return null;
			var doc = Document.Open(info.Domain, info.Path);
			if (doc != null) return doc.DeepFind<Container>(e => e.ContentKey == ContentKey);
			return null;
		}

		// dom
		public override string InnerText { get { return Children.Text; } set { Children.Text = value; } }
		public override bool HasRestrictedServerCode(EditorRight rights) { return Children.Any(ch => ch.HasRestrictedServerCode(rights)); }

		public virtual Container Rendered(string clientID) {
			var doc = Document.Rendered();
			if (doc != null) return doc.Find<Element>(e => e.Id == clientID);
			return null;
		}
		public virtual Container Rendered() { return Rendered(Control.ClientID); }
		public override string PlainText { get { return Children.Select(child => child.PlainText).StringList(" "); } }
	}

	public class ChildCollection : DocumentNode, IList<ElementBase> {

		List<ElementBase> list;

		public ChildCollection() : base() { list = new List<ElementBase>(); }
		public ChildCollection(string html) : this() { Text = html; }

		public override bool HasRestrictedServerCode(EditorRight rights) { return list.Any(n => n.HasRestrictedServerCode(rights)); }

		public ElementBase this[int index] { get { return list[index]; } set { var i = Nodes.IndexOf(list[index]); ReplaceNode(list[index], value); list[index] = value; } }
		public int IndexOf(ElementBase item) { return list.IndexOf(item); }
		public void Insert(int index, ElementBase item) {
			if (index < 0) throw new IndexOutOfRangeException("Index cannot be negative");
			var i = list.IndexOf(item);
			if (i >= 0) { if (i < index) index--; Remove(item); }
			if (item.ParentNode != null) item.ParentNode.Remove(item);
			if (index > list.Count) throw new IndexOutOfRangeException();
			//item.Index = index;
			list.Insert(index, item);
			if (index < list.Count - 1) base.Insert(Nodes.IndexOf(list[index + 1]), item);
			else base.Add(item);
			item.ParentNode = this;
		}
		public void RemoveAt(int index) { var item = list[index]; base.Remove(item); list.RemoveAt(index); }
		/* public override void Add(TextNode item) {
			// if (!(item is DocumentNode)) throw new ArgumentException();
			// base.Add(item);
			if (item is DocumentNode) Insert(Count, (DocumentNode)item);
			else if (!base.ChildNodes.Contains(item)) base.Add(item);
		} */
		public void Add(ElementBase item) { Insert(Count, item); }
		public void AddAt(int index, ElementBase item) { Insert(index, item); }
		public override void Add(TextNode node) { if (node is ElementBase) Add((ElementBase)node); else base.Add(node); }
		public override void Clear() { list.Clear(); base.Clear(); }
		public override void Insert(int index, TextNode node) { if (node is ElementBase) Insert(index, (ElementBase)node); else base.Insert(index, node); }
		public override bool Remove(TextNode node) { if (node is ElementBase) return Remove((ElementBase)node); else return base.Remove(node); }
		public bool Contains(ElementBase item) { return list.Contains(item); }
		public void CopyTo(ElementBase[] array, int arrayIndex) { list.CopyTo(array, arrayIndex); }
		public int Count { get { return list.Count; } }
		public bool IsReadOnly { get { return false; } }
		public bool Remove(ElementBase item) { base.Remove(item); return list.Remove(item); }
		public IEnumerator<ElementBase> GetEnumerator() { return list.GetEnumerator(); }
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return list.GetEnumerator(); }
		public override IEnumerable<DocumentNode> AllChildren { get { return list; } }
		public void AddRange(IEnumerable<ElementBase> list) { foreach (var e in list) Add(e); }

		public override string InnerText {
			get { return Text.Trim().Replace("\n" + Tabs, "\n"); }
			set {
				value = Environment.NewLine + Tabs + value.Trim().Replace("\n", "\n" + Tabs) + Environment.NewLine + Tabs;
				Text = value;
			}
		}
		public void Replace(ElementBase old, ElementBase newNode) {
			var index = old.Index;
			Remove(old); Insert(index, newNode);
		}

		public virtual void Wrap(Element e) { var p = Parent; e.Children = this; if (p != null) p.Add(e); }
		public virtual void Unwrap() {
			var p = ParentNode;
			if (p == null) return;
			var pp = p.ParentNode;
			if (pp == null || !(pp is ChildCollection)) return;
			var c = (ChildCollection)pp;
			var i = c.IndexOf(p);
			c.Remove(p);
			foreach (var n in this) c.AddAt(i++, n);
		}
		public override string PlainText { get { return Children.Select(child => child.PlainText).StringList(" "); } }
	}

	public class Attribute : DocumentNode {
		public virtual Token Spacer { get; protected set; }
		Token nameToken;
		public virtual Token NameToken {
			get { return nameToken; }
			protected set {
				var child = ParentNode != null && ParentNode is AttributeCollection;
				if (child) ParentNode.Remove(this);
				nameToken = value;
				if (child) ParentNode.Add(this);
			}
		}
		public virtual Token ValueToken { get; protected set; }
		public virtual Token EqualsToken { get; protected set; }

		public Attribute()
			: base() {
			Spacer = New<Token>(TokenClass.Whitespace); Spacer.Value = " ";
			NameToken = New<Token>(TokenClass.Identifier);
			EqualsToken = New<Token>(TokenClass.Equals);
			ValueToken = New<Token>(TokenClass.String);
			hasValue = false;
		}
		public Attribute(string html) : this() { Text = html; }

		public override void Clear() { base.Clear(Spacer, NameToken, ValueToken, EqualsToken); }

		public override string FullName {
			get { return NameToken.Value; }
			set {
				var parent = ParentNode;
				var child = parent != null && parent is AttributeCollection;
				if (child) parent.Remove(this);
				NameToken.Value = value;
				if (child) parent.Add(this);
			}
		}
		public override string Value {
			get { return ValueToken.Value; }
			set {
				ValueToken.Value = value;
				if (ParentNode != null) {
					if (value == null) ParentNode.Remove(this);
				}
				hasValue = !value.IsNullOrEmpty();
			}
		}
		public bool IsServerAttribute { get { return Name.ToLower() == "runat" && Value.ToLower() == "server"; } }
		public bool IsServerCode { get { return Value.Contains("<%=") || Value.Contains("<%$") || Value.Contains("<%:"); } }
		public override bool Remove(TextNode node) {
			if (node == NameToken && ParentNode != null && ParentNode is AttributeCollection) ParentNode.Remove(this);
			return base.Remove(node);
		}
		public override void Add(TextNode node) {
			base.Add(node);
			if (node == NameToken && ParentNode != null && ParentNode is AttributeCollection) ParentNode.Add(this); // re add if nametoken has changed
		}
		public bool Remove() { return ParentNode.Remove(this); }

		bool hasValue;
		public bool HasValue { get { return hasValue; } set { hasValue = value; if (!value) ValueToken.Value = ""; } }
		public override string PlainText { get { return ""; } }
	}

	public class AttributeCollection : DocumentNode, IList<Attribute> {

		class Collection : KeyedCollection<string, Attribute> {

			public Collection() : base(StringComparer.OrdinalIgnoreCase, 4) { }

			protected override string GetKeyForItem(Attribute item) {
				return item.Name;
			}
			protected override void ClearItems() {
				base.ClearItems();
				if (Dictionary != null) Dictionary.Clear();
			}
		}

		Collection dict;

		public AttributeCollection() : base() { dict = new Collection(); }
		public AttributeCollection(string html) : this() { Text = html; }

		public string this[string key] {
			get { if (dict.Contains(key)) return dict[key].Value; return null; }
			set {
				if (value.IsNullOrEmpty()) {
					if (dict.Contains(key)) Remove(dict[key]);
				} else {
					Attribute a;
					if (dict.Contains(key)) a = dict[key];
					else {
						a = New<Attribute>();
						a.Name = key;
						// Add(a);
					}
					a.Value = value;
				}
			}
		}

		public bool ContainsServerAttribute { get { return Contains("runat") && this["runat"].ToLower() == "server"; } }
		public bool ConatinsServerExpression { get { return ((IEnumerable<Attribute>)this).Any(a => a.IsServerCode); } }

		public void Add(Attribute item) { if (Contains(item.Name)) Remove(item.Name); base.Add(item); dict.Add(item); }
		public override void Add(TextNode node) { if (node is Attribute) Add((Attribute)node); else base.Add(node); }
		public override void Clear() { dict.Clear(); base.Clear(); }
		public bool Contains(Attribute item) { return dict.Contains(item.Name); }
		public bool Contains(string name) { return dict.Contains(name); }
		public void CopyTo(Attribute[] array, int arrayIndex) { dict.CopyTo(array, arrayIndex); }
		public int Count { get { return dict.Count; } }
		public bool IsReadOnly { get { return false; } }
		public bool Remove(Attribute item) {
			base.Remove(item);
			if (!dict.Remove(item)) return false;
			return true;
		}
		public override bool Remove(TextNode node) { if (node is Attribute) return Remove((Attribute)node); else return base.Remove(node); }
		public override void Insert(int index, TextNode node) { if (node is Attribute) Insert(index, (Attribute)node); base.Insert(index, node); }
		public void RemoveAt(int index) {
			var item = dict[index];
			if (item != null) {
				dict.Remove(item.Name.ToLower());
				base.Nodes.Remove(item);
			}
		}
		public bool Remove(string key) { return Remove(dict[key]); }
		public Attribute this[int index] {
			get { return dict.FirstOrDefault(x => x.Index == index); }
			set { RemoveAt(index); Insert(index, value); }
		}

		public IEnumerator<Attribute> GetEnumerator() { return dict.OfType<Attribute>().GetEnumerator(); }
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return dict.GetEnumerator(); }
		public override IEnumerable<DocumentNode> AllChildren { get { return dict.OfType<DocumentNode>().OrderBy(n => n.Index).ToList(); } }
		public int IndexOf(Attribute item) { return dict.IndexOf(item); }
		public void Insert(int index, Attribute item) {
			if (!dict.Contains(item)) Add(item);
		}
		public override AttributeCollection Attributes { get { return this; } }
		public override string PlainText { get { return ""; } }
	}
}