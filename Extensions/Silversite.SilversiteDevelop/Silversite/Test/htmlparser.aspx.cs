using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Web.Hosting;
using Silversite;

namespace Silversite.Test {

	public partial class Parser: System.Web.UI.Page	{

		const string parsedfile = "~/Silversite/Cache/windiff/parsedtext.txt";
		const string originalfile = "~/Silversite/Cache/windiff/originaltext.txt";

		protected void Page_Load(object sender, EventArgs e) {

			if (!IsPostBack) {
				file.Text = "~/Silversite/Test/editablecontent.aspx";
				windiff.Visible = false;
			}
			if (Request.QueryString["page"] != null) {
				file.Text = Request.QueryString["page"];
				Parse(this, EventArgs.Empty);
			}
		}

		TreeNode GetNode(Html.Element e) {
			var node = new TreeNode();
			node.Text = string.Format("<span style='color:green'>line</span> {0}: {1}", e.StartTag.StartToken.Line, Server.HtmlEncode(e.StartTag.Text));
			node.Target = "_top";
			node.NavigateUrl = "javascript:PopupText(\"" + HttpUtility.HtmlEncode(e.Text).Replace(" ", "&#xB7;").Replace("\n", "&#xB6;<br/>").Replace("\t", "&nbsp; &raquo; &nbsp;").Replace("\"", "&quot;") + "\");"; 
			foreach (var child in e.Children.OfType<Html.Element>()) {
				node.ChildNodes.Add(GetNode(child));
			}
			return node;
		}



		public void Parse(object sender, EventArgs e) {
			var orig = Services.Files.LoadVirtual(file.Text);
			if (orig == null) {
				message.Text = "File not found.";
				return;
			}

			Services.Domains.Files.SaveWithPath(orig, originalfile);

			var doc = Html.Document.Open(file.Text);

			CheckSpaces(doc);

			errors.DataSource = doc.Errors;
			errors.DataBind();

			var writer = doc.Text;
			Services.Domains.Files.SaveWithPath(writer, parsedfile);

			CheckSpaces(doc);


			var diff = windiff.Visible = writer != orig;

			if (diff) message.Text = "<span style='color:red'>Original and Parsed text do not match</span>";
			else message.Text = "Original and Parsed text match.";
 
			int lines = orig.Count(ch => ch == '\n');

			LineNumbers.Text = Enumerable.Range(1, lines + 1).StringList("{0}" + Environment.NewLine, "");

			Original.Text = Server.HtmlEncode(orig.Replace("\t", "  "));
			Writer.Text = Server.HtmlEncode(writer.Replace("\t", "  "));	
			
			tree.Nodes.Clear();
			foreach (var child in doc.Children.OfType<Html.Element>())  {
				tree.Nodes.Add(GetNode(child));
			}
		}

		protected void WinDiff(object sender, EventArgs e) {

			Process.Start(Services.Paths.Map("~/Silversite/tools/WinDiff.exe"), "\"" +
				Services.Paths.Map(Services.Domains.Path(originalfile)) + "\" \"" +
				Services.Paths.Map(Services.Domains.Path(parsedfile)) + "\"");
		}

		protected void ToggleSpaceMarks(object sender, EventArgs a) {
			var original = Services.Domains.Files.Load(originalfile);
			var parsed = Services.Domains.Files.Load(parsedfile);

			if (original.Contains(' ')) original = original.Replace(' ', '·').Replace('\t', '→');
			else original = original.Replace('·', ' ').Replace('→', '\t');
			if (parsed.Contains(' ')) parsed = parsed.Replace(' ', '·').Replace('\t', '→');
			else parsed = parsed.Replace('·', ' ').Replace('→', '\t');

			Services.Domains.Files.SaveWithPath(original, originalfile);
			Services.Domains.Files.SaveWithPath(parsed, parsedfile);
		}

		protected void CheckSpaces(Html.TextNode doc) {
			int zero = 0, other = 0, broken = 0, brokenother = 0;
			foreach (var ws in doc.TopDownNodes.OfType<Html.Token>().Where(t => t.IsWhitespace)) {
				if ((ws.Value == string.Empty) != (ws.Length == 0)) broken++;
				if (ws.Value == string.Empty) zero++;
				else other++;
				if (ws.Length > 0 && ws.Value == string.Empty) {
					brokenother++;
				}
			}
			Debug.Message("CheckSpaces: zero: {0}, other: {1}, broken: {2}, brokenother: {3}", zero, other, broken, brokenother);
		}
	}
}