using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading.Tasks;

namespace Silversite.Web.UI.Pages.Base {
	public partial class entitytest: System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
		}

		protected void StartTest(object sender, EventArgs e) {
			msg.Start(null);
			Task.Factory.StartNew(() => {
				EntityTest.Database.Fill();
				EntityTest.Database.List();
				Services.Providers.Finished(this);
			});
		}
	}
}