using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Silversite.Services {
	public static class ControlUtility {

		public static string ResolvePageUrl(this Control control, string url) {
			url = control.ResolveClientUrl(url);
			if (url.StartsWith("~")) {
				var td = control.Page.AppRelativeTemplateSourceDirectory;
				if (!td.EndsWith("/")) td = td + "/";
				int i = 0;
				while (i < td.Length && i < url.Length && td[i] == url[i]) i++;
				if (i > td.Length) i--;
				while (i > 0 && td[i] != '/') i--;
				url = url.Substring(i + 1);
				for (int j = i + 1; j < td.Length; j++) {
					if (td[j] == '/') url = "../" + url;
				}
			}
			return url;
		}

	}
}