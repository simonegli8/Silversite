using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Silversite.Html {

	public class CKEditorWriter: Html.Writer {	

		public override void WriteElement(Element e) {
			WriteTag(e.StartTag); WriteChildren(e.Children); if (e.EndTag != null) WriteTag(e.EndTag);
		}
	}
}