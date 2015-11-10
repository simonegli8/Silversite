using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Text.RegularExpressions;

namespace Silversite.Services {

	public class Security {
		
		[Serializable]
		class EncryptedObject {
			public object Object { get; set; }
			public string[] Keys { get; set; }
		}

		public static string Encrypt(object obj, params string[] keys) {
			return HttpUtility.UrlEncode(Convert.ToBase64String(Hash.ToBytes(MachineKey.Encode(Hash.ToBytes(new EncryptedObject { Object = obj, Keys = keys }), MachineKeyProtection.All))));
		}

		public static object Decrypt(string text, params string[] keys) {
			try {
				var eobj = (EncryptedObject)Hash.ToObject(MachineKey.Decode((string)Hash.ToObject(Convert.FromBase64String(HttpUtility.UrlDecode(text))), MachineKeyProtection.All));
				if (eobj.Keys.All(key => keys.Any(key2 => key == key2))) return eobj.Object;
			} catch (Exception ex) { }
			return null;
		}

		public static string Secret(TimeSpan maxAge = default(TimeSpan), params string[] keys) { return Encrypt(maxAge == default(TimeSpan) ? DateTime.MaxValue : DateTime.Now + maxAge, keys); }

		public static bool Secure(string secret, params string[] keys) {
			var validUntil = (DateTime?)Decrypt(secret, keys);
			return validUntil.HasValue && validUntil.Value >=  DateTime.Now;
		}

		public static string SecureUrl(string url, TimeSpan maxAge = default(TimeSpan), params string[] keys) {
			url = url.Replace("&amp;", "&");
			return url + (url.Contains('?') ? "&" : "?") + "!!" + Encrypt(maxAge == default(TimeSpan) ? DateTime.MaxValue : DateTime.Now + maxAge, keys.Prepend(url).ToArray());
		}

		static Regex securetokens = new Regex("(\\?|&|&amp;)!![^&]+$");

		public static bool SecureRequest(params string[] keys) {
			try {
				var url = HttpContext.Current.Request.Url.AbsoluteUri;
				var match = securetokens.Match(url);
				if (!match.Success) return false;
				var rawurl = url.Replace(match.Value, "");
				var secret = match.Value.Substring(3);
				return Secure(secret, keys.Prepend(rawurl).ToArray());
			} catch {
				return false;
			}
		}

	}
}