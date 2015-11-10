using System;
using System.Collections;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace Silversite.silversite.test {
	public partial class CreateAdmin: System.Web.UI.Page {

		protected void CreatingUser(object sender, EventArgs e) {
			Control template =	CreateUserWizard.CreateUserStep.ContentTemplateContainer;
			(template.FindControl("Email") as TextBox).Text = (template.FindControl("UserName") as TextBox).Text;
		}

		protected void Finish(object sender, EventArgs e) {
			Response.Redirect("~/test/admins.aspx");
		}
	}
}
