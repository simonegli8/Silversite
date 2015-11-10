using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Silversite.Html {

	public class TextNode {

		public TextNode() { Nodes = new List<TextNode>(); }
		public TextNode(string html) : this() { Text = html; }
		Parser parser = null;
		Writer writer = null;
		public virtual Parser Parser { get { if (parser == null && Parent != null) parser = Parent.Parser; return parser ?? (parser = new Parser()); } set { parser = value; } }
		public virtual Writer Writer { get { if (writer == null && Parent != null) writer = Parent.Writer; return writer ?? (writer = new Writer()); } set { writer = value; } }
		public Reader Reader { get { return Parser != null ? Parser.Reader : null; } set { if (Parser != null) Parser.Reader = value; } }
		public virtual List<string> Errors { get { return Parser != null ? Parser.Errors : new List<string>(); } }

		TextNode parent = null;
		public virtual TextNode Parent { get { return parent; } set { parent = value; if (value != null) { Parser = null; Writer = null; } } }
		public virtual TextNode FormerParent { get; set; } 

		public virtual T New<T>() where T: TextNode, new() { var n = new T(); Add(n); return n; }
		public virtual T New<T>(TokenClass tokenClass) where T: Token, new() { var t = New<T>(); t.Class = tokenClass; return t; }
		public virtual T New<T>(TokenClass tokenClass, ServerTagClass serverTagClass) where T: Token, new() { var t = New<T>(); t.Class = tokenClass; t.ServerTagClass = serverTagClass; return t; }
		public virtual T New<T>(TokenClass tokenClass, StringClass stringClass) where T: Token, new() { var t = New<T>(); t.Class = tokenClass; t.StringClass = stringClass; return t; }

		public virtual TextNode Previous {
			get {
				if (Parent == null) return null;
				int i = Parent.Nodes.IndexOf(this);
				if (i <= 0) return null;
				return Parent.Nodes[i-1];
			}
		}
		public virtual TextNode Next {
			get {
				if (Parent == null) return null;
				int i = Parent.Nodes.IndexOf(this);
				if (i >= Parent.Nodes.Count-1) return null;
				return Parent.Nodes[i+1];
			}
		}


		internal TextNode Last {
			get {
				if (Nodes.Count == 0) return this;
				return Nodes[Nodes.Count-1].Last;
			}
		}

		internal TextNode First {
			get {
				if (Nodes.Count == 0) return this;
				return Nodes[0].First;
			}
		}

		public TextNode NextInDoc {
			get {
				var p = Parent;
				if (p == null) return null;
				int i = p.Nodes.IndexOf(this);
				if (++i < p.Nodes.Count) return p.Nodes[i].First;
				else {
					p = p.NextInDoc;
					if (p == null) return null;
					return p.First;
				}
			}
		}

		internal TextNode PreviousInDoc {
			get {
				var p = Parent;
				if (p == null) return null;
				int i = p.Nodes.IndexOf(this);
				if (--i >= 0) return p.Nodes[i].Last;
				else {
					p = p.PreviousInDoc;
					if (p == null) return null;
					return p.Last;
				}
			}
		}

		public string Context { // the text surrounding this TextNode (for debugging)
			get {
				TextNode node = this;
				int n = 7;
				while (node.PreviousInDoc != null && n-- > 0) node = Previous;

				n = 15 - n;
				var str = new StringBuilder();
				while (node.NextInDoc != null && n-- > 0) {
					str.Append(node.Text);
					node = NextInDoc;
				}
				return str.ToString();
			}
		}


		public virtual void Add(TextNode node) { if (node.Parent != null && node.Parent != this) node.Parent.Remove(node); node.FormerParent = node.Parent = this; Nodes.Add(node); }
		public virtual bool Remove(TextNode node) { if (Nodes.Remove(node)) { node.Parent = null; return true; } return false; }
		public virtual void Insert(int index, TextNode node) {
			if (index < 0 || index > Nodes.Count) throw new IndexOutOfRangeException();
			if (node.Parent != null) node.Parent.Remove(node);
			Nodes.Insert(index, node);
			node.Parent = this;
		}
		public virtual int IndexOf(TextNode node) {
			if (Nodes.Contains(node)) return Nodes.IndexOf(node);
			return Parent !=  null ? Parent.Nodes.IndexOf(node) : -1;
		}
		public virtual void After(TextNode node) {
			if (Parent == null) throw new NotSupportedException("Node has no parent.");
			Parent.Insert(IndexOf(this)+1, node);
		}
		public virtual void Before(TextNode node) {
			if (Parent == null) throw new NotSupportedException("Node has no parent.");
			Parent.Insert(IndexOf(this), node);
		}

		public virtual void Clear() { foreach (var n in Nodes) n.Parent = null; Nodes.Clear(); }
		public virtual void Clear(params TextNode[] except) { foreach (var n in Nodes.ToList()) if (!except.Contains(n)) Remove(n); else n.Clear(); }
		public void ReplaceNode(TextNode oldNode, TextNode newNode) {
			if (oldNode == null && newNode == null) return;
			if (oldNode == null) Add(newNode);
			else if (newNode == null) Remove(oldNode);
			else if (oldNode != newNode) {
				int i = Nodes.Count; i = Nodes.IndexOf(oldNode);
				//oldNode.Parent = null;
				Nodes.Remove(oldNode);
				Nodes.Insert(i, newNode);
			}
			oldNode = newNode;
		}

		public void ReAdd() { if (Parent == null && FormerParent != null) FormerParent.Add(this); }

		public virtual List<TextNode> Nodes { get; protected set; }
		public IEnumerable<TextNode> TopDownNodes { get { yield return this; foreach (var n in Nodes) foreach (var nn in n.TopDownNodes) yield return nn; } }
		public IEnumerable<TextNode> BottomUpNodes { get { foreach (var n in Nodes) foreach (var nn in n.BottomUpNodes) yield return nn; yield return this; } }

		public virtual bool Parse(string text) {
			if (Parser != null) {
				Parser.Reader.Text = text;
				return Parser.Parse(this);
			}
			return false;
		}
		
		public virtual string Write() {
			var sw = new StringWriter();
			Writer.TextWriter = sw;
			Writer.Write(this);
			return Writer.PostProcess(sw.ToString());
		}

		public virtual string Text { get { return Write(); } set { Parse(value); } }

		public virtual TextNode Clone() {
			var n = Silversite.New.Object(this.GetType()) as TextNode;
			if (n == null) throw new NotSupportedException("TextNode must have a parameterless constructor.");
			n.Parser = Parser; n.Writer = Writer; n.Text = Text;
			return n;
		}
	}

}