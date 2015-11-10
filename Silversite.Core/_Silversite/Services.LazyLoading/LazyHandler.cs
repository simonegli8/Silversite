using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Silversite.Web {

	public class LazyHandler: IHttpHandler {

		public bool IsReusable {
			get { return true; }
		}

		public void ProcessRequest(HttpContext context) {
			var path = context.Request.AppRelativeCurrentExecutionFilePath;
			var handler = Services.Lazy.Handlers.New(path);
			if (handler != null) handler.ProcessRequest(context);
			else throw new NullReferenceException(string.Format("Lazy handler for path {0} not found.", path));
		}
	}
}
