using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Silversite.Web.UI {

	public class Conditional: System.Web.UI.WebControls.Panel {
		public override void RenderBeginTag(System.Web.UI.HtmlTextWriter writer) { }
		public override void RenderEndTag(System.Web.UI.HtmlTextWriter writer) { }

		public string Roles {
			get { return (string)ViewState["Roles"] ?? ""; }
			set { ViewState["Roles"] = value;
				var userroles = System.Web.Security.Roles.GetRolesForUser();
				Visible = value.Tokens().Intersect(userroles).Any();
			}
		} 
	}

}