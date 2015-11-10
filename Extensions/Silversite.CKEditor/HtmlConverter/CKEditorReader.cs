using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Silversite.Html {
	public class CKEditorReader: Html.Reader {

		public override string Text {
			get { return base.Text; }
			set { base.Text = value.Replace("[&lt;", "<").Replace("&gt;]", ">").Replace("[<", "[&lt;").Replace(">]", "&gt;]"); }
		}

	}
}