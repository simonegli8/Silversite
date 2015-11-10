using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace Silversite.Web.UI {

	public class ControlWizard: FileManager {

		protected override void CreateChildControls() {
			base.CreateChildControls();

			var bar = new FileManager();
			bar.Path = Services.Paths.Directory(Path);

			Controls.Add(bar.Service.Content(bar));
			Controls.Add(Service.Content(this));
		}
	}

}