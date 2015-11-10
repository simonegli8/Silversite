using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Silversite.SilversiteDevelop {
	public class DisableLog: Services.IAutostart {

		public void Startup() {
			Services.Modules.DependsOn(typeof(Services.Log));
			Services.Log.Enabled = false;
		}
		public void Shutdown() { }
	}
}