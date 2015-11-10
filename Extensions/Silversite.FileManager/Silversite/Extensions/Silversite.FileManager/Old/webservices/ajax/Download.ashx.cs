using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Silversite.WebServices {
	/// <summary>
	/// Summary description for Upload
	/// </summary>
	public class Download: IHttpHandler {

		public void ProcessRequest(HttpContext context) {

			var path = context.Request.QueryString["path"];
			var user = Silversite.Services.Persons.Current;

			if (user == null) Silversite.Services.Files.Response(path);
			else Silversite.Services.Files.Response(user.AbsolutePath(path));
		
		}

		public bool IsReusable {
			get {
				return false;
			}
		}
	}
}