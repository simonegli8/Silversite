using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Silversite {

	public static class ControlExtensions {

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

		public static void Requires(this Control control, params string[] files) {
			foreach (var file in files.SelectMany(f => f.Tokens())) {
				switch (file.ToLower()) {
					case "jquery": Web.UI.Scripts.jQuery.Register(control.Page); break;
					case "jqueryui": Web.UI.Scripts.jQueryUI.Register(control.Page); break;
					default:
						var fl = file.ToLower();
						var ext = Services.Paths.Extension(fl);
						if (ext == "js") Web.UI.Scripts.Register(control.Page, file);
						else if (ext == "css") Web.UI.Css.Register(control.Page, file);
						else if (fl.StartsWith("jqueryui.")) Web.UI.Scripts.jQueryUI.Register(control.Page, file.FromOn("jqueryui."));
						break;
				}
			}
		}
	}
}