using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Silversite.Web;

namespace Silversite.silversite.test {

	public class MyModule: Web.IAutostart {
		public static int N = 0;

		public void Startup() {
			N++;
			Services.Log.Write("Debug", "Autostart N: {0}", N);
			// Services.Log.Flush();
		}
		public void Shutdown() { }
	}

	public partial class AutostartModule: System.Web.UI.Page {

		protected void Page_Load(object sender, EventArgs e) {
			n.Text = MyModule.N.ToString();
		}
	}
}