using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Silversite;
using Silversite.Services;

namespace Silversite.FileManager {

	public class TreeBuilder: Panel {

		public Silversite.Web.UI.FileManager M { get; set; }
		public TreeBuilder(Silversite.Web.UI.FileManager m) { M = m; }

		System.Web.UI.WebControls.TreeView Tree;

		public virtual void CreateTree() {
			Controls.Clear();
			//var dirs = Files.DirectoryAllVirtual(Root + "/*");
			
			//Tree.
		}

	}

}