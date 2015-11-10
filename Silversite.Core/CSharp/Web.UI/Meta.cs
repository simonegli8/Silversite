using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Silversite.Web.UI {

	public class Meta: Head {

		protected override void OnLoad(EventArgs e) {

			var meta = new HtmlMeta();
			foreach (string key in this.Attributes.Keys) {
				meta.Attributes.Add(key, Attributes[key]);
			}
			Controls.Add(meta);

			base.OnLoad(e);
		}
	}
}