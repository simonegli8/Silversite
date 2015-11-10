using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Drawing;
using System.IO;
using System.Threading;
using System.ComponentModel;
using Web = Silversite.Web;
using Silversite;

namespace Silversite.Web.UI.Controls.Base {

	public partial class Login: System.Web.UI.UserControl {

		TextBox Email;
		Label msg;

		protected void Page_Load(object sender, EventArgs e) {
			if (login != null) {
				Email = login.FindControl("UserName") as TextBox;
				msg = login.FindControl("msg") as Label;
			} else {
				Email = null;
				msg = null;
			}
		}

		[Category("Behavior")]
		[Browsable(true)]
		public string MailTemplate { get; set; }

		public void SendPassword(object sender, EventArgs e) {
			MembershipUser user = Membership.GetUser(Email.Text);
			string password = string.Empty;
			try {
				password = user.GetPassword();
			} catch (Exception) {
				msg.Text = "Fehler beim Lesen des Passworts.";
				msg.ForeColor = Color.Red;
				return;
			}
			if (user != null) {
				Services.MailMessage mail = new Services.MailMessage("Ihr Passwort für " + Request.Url.Authority, string.Format(File.ReadAllText(Server.MapPath(MailTemplate)), password), new Services.MailAddress(user.Email));
				mail.IsBodyHtml = true;
				if (mail.Send()) {
					msg.Text = "Ihr Passwort wurde Ihnen per E-Mail zugesandt.";
					msg.ForeColor = Color.Black;
				} else {
					msg.Text ="Fehler beim Senden des Passworts.";
					msg.ForeColor = Color.Red;
				}
			} else {
				msg.Text = "Unter dieser E-Mail Adresse ist kein Benutzer registriert.";
				msg.ForeColor = Color.Red;
			}
		}

	}
}
