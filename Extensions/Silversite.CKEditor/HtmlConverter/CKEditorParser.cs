using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Silversite.Html {
	public class CKEditorParser: Html.Parser {

		public CKEditorParser() : base() { this.Reader = new CKEditorReader(); }

	}
}