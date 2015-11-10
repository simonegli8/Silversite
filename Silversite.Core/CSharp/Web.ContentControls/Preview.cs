using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Silversite.Web.UI {

	public class Preview: Panel {
		public Preview() { Visible = false; }

		/*
		protected override void OnInit(EventArgs e) {
			if (Parent is EditableContent) {
				var content = (EditableContent)Parent;
				if (content.Mode == EditableContent.Modes.View || content.Info.Revision == content.PublishedInfo.Revision) {
					Controls.Clear();
				} else {
					while (Parent.Controls.Count > 1) { // reomve all parents control except Preview
						if (Parent.Controls[0] is Preview) Parent.Controls.RemoveAt(1);
						else Parent.Controls.RemoveAt(0);
					}
					while (Controls.Count > 0) { // empty preview and insert in parent control
						var ctl = Controls[0];
						Controls.RemoveAt(0);
						Parent.Controls.AddAt(Parent.Controls.Count, ctl);
					}
					// Parent.Controls.RemoveAt(0); // remove Preview
				}
				base.OnInit(e);
			}
		} */
	}

}