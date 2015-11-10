using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Silversite.Web.UI;

namespace Silversite.FileManager {

	public class NoActionHandler : FileHandler {

		public override void Open(Silversite.Web.UI.FileManager m, string path) { }

	}
}