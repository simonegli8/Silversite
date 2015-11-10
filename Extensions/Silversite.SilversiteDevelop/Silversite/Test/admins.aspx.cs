using System;
using System.Collections;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace Silversite.silversite.test{
	public partial class Admins: System.Web.UI.Page {

		protected bool Popup {
			get { return popup.Visible; }
			set { popup.Visible = value; }
		}

		protected void NewAdmin(object sender, EventArgs e) {
			Popup = true;
		}

		protected void Finish(object sender, EventArgs e) {
			Popup = false;
		}

		protected void Page_Load(object sender, EventArgs e) {
		}
		
		protected void CreatingUser(object sender, EventArgs e) {
			Control template =	CreateUserWizard.CreateUserStep.ContentTemplateContainer;
			(template.FindControl("Email") as TextBox).Text = (template.FindControl("UserName") as TextBox).Text;
			DataBind();
		}

		protected void RowCommand(object sender, GridViewCommandEventArgs e) {
			if (e.CommandName == "Unlock") Services.Persons.Unlock(e.CommandArgument as string);
		}
	}
}
