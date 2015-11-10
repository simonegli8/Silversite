using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace Silversite {

	public static class UriExtensions {
		public static string User(this Uri url) {
			var info = url.UserInfo;
			if (string.IsNullOrEmpty(info)) info = "anonymous";
			return info.Split(':').First();
		}

		public static string Password(this Uri url) {
			var info = url.UserInfo;
			if (string.IsNullOrEmpty(info)) info = "anonymous";
			return info.Split(':').Last();
		}

		public static Html.Document Document(this Uri url, Action<Services.AdvancedWebClient> webClientSetup = null) {
			return Html.Document.Open(url, webClientSetup);
		}

		public static Stream Download(this Uri url, Action<Services.AdvancedWebClient> webClientSetup = null) {
			return Services.Files.Download(url, webClientSetup);
		}
		public static string DownloadText(this Uri url, Action<Services.AdvancedWebClient> webClientSetup = null) {
			return Services.Files.Html(url, webClientSetup);
		}

	}

}