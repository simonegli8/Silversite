using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Hosting;

namespace Silversite.TestPages.silversite.test {
	public partial class parseall: System.Web.UI.Page {
	
		protected void Page_Load(object sender, EventArgs e) {

			if (!IsPostBack) {

				var files = Services.Files.AllVirtual("~/*.aspx;~/*.ascx;~/*.master;~/*.asmx;~/*.ashx;~/*.html;");

				var results = files
					.Select(name => new { Name = name, Html = Services.Files.LoadVirtual(name), Doc = Html.Document.Open(name) })
					.Where(d => d.Doc != null)
					.Select(f => new { Name = f.Name, Html = f.Html, Doc = f.Doc, Errors = f.Doc.Errors, Match = f.Html == f.Doc.Text });

				tree.Nodes.Clear();
				foreach (var res in results) {
					var node = new TreeNode();
					node.Text = res.Name +
						(res.Match ? " matches" : " <span style='color:red'>mismatch</span>") +
						((res.Errors.Count > 0) ? " <span style='color:red'>has errors</span>." : " no errors.");
					node.NavigateUrl = "~/Silversite/Test/htmlparser.aspx?page=" + res.Name;

					foreach (var err in res.Errors) {
						var errnode = new TreeNode();
						errnode.Text = "<span style='color:red'>" + err + "</span>";
						node.ChildNodes.Add(errnode);
					}
					tree.Nodes.Add(node);
				}

			}
		}
	}
}