using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Silversite.WebServices {
	/// <summary>
	/// Summary description for Upload
	/// </summary>
	public class Upload: IHttpHandler {

		public void ProcessRequest(HttpContext context) {

			var path = context.Request.QueryString["path"];
			if (string.IsNullOrEmpty(path)) path = context.Request.Form["path"];
			var user = Silversite.Services.Persons.Current;
	
			var files = context.Request.Files;
			if (files.Count > 0) {
				if (user != null) {
					foreach (var name in files.AllKeys) {
						if (files[name].ContentLength > 0) Silversite.Services.Files.Save(files[name].InputStream, Silversite.Services.Paths.Combine(user.AbsolutePath(path), name));
					}
				}
			}
		}

		public bool IsReusable {
			get {
				return false;
			}
		}
	}
}