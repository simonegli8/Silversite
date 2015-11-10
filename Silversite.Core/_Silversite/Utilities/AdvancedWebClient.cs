using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using Net = System.Net;

namespace Silversite.Services {

	public class AdvancedWebClient : Net.WebClient {

		public class LocalRequest : Net.WebRequest {
			public string Path { get; set; }
		}

		public Net.CookieCollection Cookies = new Net.CookieCollection();

		public bool UseRequestCookies { get; set; }

		public AdvancedWebClient(bool useRequestCookies = true, Encoding encoding = null) {
			try {
				UseDefaultCredentials = true;
				Encoding = encoding ?? Encoding.GetEncoding("ISO-8859-1");
				Headers[Net.HttpRequestHeader.UserAgent] = "Opera/9.80 (Windows NT 6.1; WOW64; U; de) Presto/2.10.289 Version/12.00";
				try {
					if (HttpContext.Current != null && HttpContext.Current.Request != null) Headers[Net.HttpRequestHeader.UserAgent] = HttpContext.Current.Request.UserAgent;
				} catch { }
				Headers[Net.HttpRequestHeader.AcceptEncoding] = "";
				Headers[Net.HttpRequestHeader.AcceptCharset] = "*";
				if (useRequestCookies && HttpContext.Current != null) {
					foreach (var key in HttpContext.Current.Request.Cookies.Keys.OfType<string>()) {
						var cookie = HttpContext.Current.Request.Cookies[key];
						var wc = new Net.Cookie(cookie.Name, cookie.Value, cookie.Path, HttpContext.Current.Request.Url.Host);
						wc.Expires = cookie.Expires;
						Cookies.Add(wc);
					}
				}
			} catch { }
		}

		protected override Net.WebRequest GetWebRequest(Uri address) {
			if (Paths.IsLocal(address) && HttpContext.Current != null) throw new NotSupportedException("A local WebRequest from a page can lead to a deadlock and is not supported.");
			Headers[Net.HttpRequestHeader.Host] = address.Host;
			Net.WebRequest request = base.GetWebRequest(address);
			//if (Paths.IsLocal(address)) request.Timeout = 10000;
			if (request is Net.HttpWebRequest) {
				var hr = (Net.HttpWebRequest)request;
				hr.KeepAlive = true;
				if (Cookies.Count > 0) {
					if (hr.CookieContainer == null) hr.CookieContainer = new Net.CookieContainer();
					foreach (Net.Cookie cookie in Cookies) hr.CookieContainer.Add(cookie);
				}
			}
			return request;
		}

		protected override Net.WebResponse GetWebResponse(Net.WebRequest request) {
			try {
				var response = base.GetWebResponse(request);
				if (response is Net.HttpWebResponse) {
					Cookies = (response as Net.HttpWebResponse).Cookies;
				}
				return response;
			} catch (Exception ex) {
				throw ex;
			}
		}


		protected override Net.WebResponse GetWebResponse(Net.WebRequest request, IAsyncResult result) {
			try {
				var response = base.GetWebResponse(request, result);
				if (response is Net.HttpWebResponse) {
					Cookies = (response as Net.HttpWebResponse).Cookies;
				}
				return response;
			} catch (Exception ex) {
				throw ex;
			}
		}

	}

}