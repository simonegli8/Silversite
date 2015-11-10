using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Silversite.SilversiteDevelop.silversite.test {
	public partial class resolveurl : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e)
		{

			resolve.Text = ResolveUrl(url.Text);
			clientresolve.Text = ResolveClientUrl(url.Text);

		}
	}
}