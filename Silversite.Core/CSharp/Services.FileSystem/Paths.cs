using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Text;
using System.Text.RegularExpressions;
using Silversite;

[assembly: Silversite.Services.DependsOn(typeof(Silversite.Services.Paths))]

namespace Silversite.Services {

	public class Paths: Web.IHttpAutoModule {

		public const string SilversiteRoot = "~/Silversite";
		public const string Cache = SilversiteRoot + "/Cache";
		public const string Data = SilversiteRoot + "/Data";
		public const string Log = SilversiteRoot + "/Log";
		public const string Config = SilversiteRoot + "/Config";
		public const string Extensions = SilversiteRoot + "/Extensions";
		public const string Pages = SilversiteRoot + "/Pages";
		public const string Admin = SilversiteRoot + "/Admin";
		public const string AppCode = SilversiteRoot + "/AppCode";
		public const string Backup = SilversiteRoot + "/Backup";
		public const string Lazy = "~/Bin/Lazy";
		public const string Temp = SilversiteRoot + "/Temp";
		public const string UI = SilversiteRoot + "/UI";

		public static bool IsWritable(string path) {
			if (Files.Configuration == null) return true;
			return Match(Files.Configuration.WritablePaths, Normalize(path));
		}
	
		public static bool IsDomains(string path) {
			return Normalize(path).StartsWith(Silversite.Services.Domains.RootPath + "/");
		}

		public static void Split(string path, out string directory, out string name) {
			path = Normalize(path);
			int i = path.LastIndexOf('/');
			if (i >= 0 && i < path.Length) {
				directory = path.Substring(0, i);
				name = path.SafeSubstring(i+1);
			} else {
				directory = string.Empty;
				name = path;
			}
		}

		public static string Directory(string path) {
			if (string.IsNullOrEmpty(path) || path == "~") return "~";
			string dir, file;
			Split(path, out dir, out file);
			return dir;
		}

		public static string File(string path) {
			if (string.IsNullOrEmpty(path) || path == "~") return "";
			string dir, file;
			Split(path, out dir, out file);
			return file;
		}

		public static string Move(string file, string to) { return Combine(to, File(file)); }

		/// <summary>
		/// Returns the filename without path & extension 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string FileWithoutExtension(string path) {
			int i = path.LastIndexOf('.');
			int j = path.LastIndexOf('/')+1;
			if (i <= j) i = path.Length;
			return path.Substring(j, i - j);
		}

		/// <summary>
		/// Combines two paths, and resolves '..' segments.
		/// </summary>
		/// <param name="path1">The root path.</param>
		/// <param name="path2">The relative path to the root path.</param>
		/// <returns>The combined path.</returns>
		public static string Combine(string path1, string path2) {
			string path = "";
			int slash;

			if (path2.StartsWith("/..")) path2 = path2.Substring(1);

			while (path2.StartsWith("../")) { // resolve relative paths.
				if (path1.EndsWith("/")) {
					slash = path1.LastIndexOf('/', path1.Length-1);
					if (slash <= 0) slash = 0;
					path1 = path1.Substring(0, slash);
				} else {
					slash = path1.LastIndexOf('/', path1.Length-1);
					if (slash <= 0) slash = 0;
					slash = path1.LastIndexOf('/', slash-1);
					if (slash <= 0) slash = 0;
					path1 = path1.Substring(0, slash);
				}
				path2 = path2.Substring(3);
			}

			if (path2.StartsWith("~")) path2 = path2.Substring(1);
			if (path1.EndsWith("/")) {
				if (path2.StartsWith("/")) path2 = path2.Substring(1);
				path = path1 + path2;
			} else if (path2.StartsWith("/")) {
				path = path1 + path2;
			} else {
				path = path1 + "/" + path2;
			}
			return path;
		}

		public static string Combine(params string[] paths) {
			if (paths == null || paths.Length == 0) return "";
			else if (paths.Length == 1) return paths[0];
			else if (paths.Length == 2) return Combine(paths[0], paths[1]);
			else return Combine(paths[0], Combine(paths.Skip(1).ToArray()));
		}

		public static string Relative(string file, string relativePath) {
			if (relativePath.Contains(':')) return relativePath;
			return Paths.Combine(Paths.Directory(file) + "/", relativePath);
		}

		public static string Normalize(string path) {
			if (path == null) return "~";
			if (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);
			if (path.StartsWith("/")) {
				var app = HostingEnvironment.ApplicationVirtualPath;
				if (path.StartsWith(app, StringComparison.OrdinalIgnoreCase)) {
					path = path.Substring(app.Length);
					if (path.StartsWith("/")) return "~" + path;
					else return "~/" + path;
				} else throw new NotSupportedException(string.Format("{0}: Absolute path with invalid ApplicationPath.", path), new SevereException());
			} else if (path.StartsWith("~")) return path;
			else return path;
		}

		public static bool IsNonVirtual(string path) {
			var lpath = Normalize(path).ToLower();
			return lpath.EndsWith("web.config") ||
				lpath.EndsWith("global.asax") ||
				lpath.StartsWith("~/bin") ||
				lpath.StartsWith("~/app_code") ||
				lpath.StartsWith("~/app_data") ||
				lpath.StartsWith("~/app_globalresources") ||
				lpath.StartsWith("~/app_localresources") ||
				lpath.EndsWith(".sitemap");
		}
		
		public static string Domains(string domain, string path) {
			return Silversite.Services.Domains.Path(domain, path);
		}

		public static string Domains(string path) {
			return Silversite.Services.Domains.Path(path);
		}

		public static string Map(string path) {
			if (path.Contains(':')) return path;
			path = path.Replace('/', '\\');
			var appphyspath = HostingEnvironment.ApplicationPhysicalPath;
			if (appphyspath.EndsWith("\\")) appphyspath = appphyspath.Substring(0, appphyspath.Length - 1);
			if (path.StartsWith("~")) path = path.Substring(1);
			if (path.StartsWith("\\")) path = path.Substring(1);
			return appphyspath + "\\" + path;
			// return HostingEnvironment.MapPath(path);
		}

		public static string Unmap(string physicalPath) {
			var appphyspath = HostingEnvironment.ApplicationPhysicalPath;
			if (appphyspath.EndsWith("\\")) appphyspath = appphyspath.Substring(0, appphyspath.Length-1);
			if (physicalPath.StartsWith(appphyspath)) return physicalPath.Replace(appphyspath, "~").Replace("\\", "/");
			return physicalPath;
		}

		public static string NoDomains(string path) {
			if (IsDomains(path)) {
				var slash = Silversite.Services.Domains.RootPath.Length;
				slash = path.IndexOf('/', slash+1); // skip the domains domain subfolder.
				if (slash != -1) return "~" + path.Substring(slash);
			}
			throw new ArgumentException(string.Format("{0}: This path is not in a domains subfolder.", path));
		}

		public static string Absolute(string path) {
			if (path.StartsWith("~")) {
				path = Normalize(path).Substring(1);
				var app = HostingEnvironment.ApplicationVirtualPath;
				if (app.Length > 1) path = Combine(app, path);
				if (string.IsNullOrEmpty(path)) return "/";
				return path;
			} else {
				if (!path.StartsWith("/")) return "/" + path;
				return path;
			}
		}

		public static string AddSlash(string path) { return path.EndsWith("/") ? path : path + "/"; }
		public static string RemoveSlash(string path) { return path.EndsWith("/") ? path.Remove(path.Length - 1) : path; }

		public static string Extension(string path) {
			var name = File(path);
			int i = name.LastIndexOf('.');
			if (i > 0) return name.Substring(i+1).ToLower();
			return string.Empty;
		}

		public static string WithoutExtension(string path) {
			int i = path.LastIndexOf('.');
			int j = path.LastIndexOf('/');
			if (i > 0 && i > j) return path.Substring(0, i);
			return path;
		}

		public static string ChangeExtension(string path, string ext) {
			if (ext.StartsWith(".")) return WithoutExtension(path) + ext;
			else return WithoutExtension(path) + "." + ext;
		}
		static System.Threading.ManualResetEvent HomeSet = new System.Threading.ManualResetEvent(false);
		static string home;
		public static string Home {
			get {
				try {
					if (home == null) {
						var context = HttpContext.Current;
						if (context != null && context.Request != null) {
							string h = context.Request.Url.AbsoluteUri, appl = context.Request.ApplicationPath;
							if (appl.StartsWith("~")) appl = appl.Substring(1);
							if (appl != "/") {
								h = h.Substring(0, h.IndexOf(appl)) + appl;
							} else {
								h = context.Request.Url.Scheme + "://" + context.Request.Url.Authority;
							}
							if (h.EndsWith("/")) h = h.Substring(0, h.Length-1);
							home = h;
							HomeSet.Set();
						}
					}
				} catch (Exception ex) {
				}
				if (!HomeSet.WaitOne(500)) {
					home = Files.Configuration.Home;
				}
				return home;
			}
		}

		public static string Url(string path) {
			if (path == null) path = "";
			if (path.StartsWith("/")) {
				var app = HostingEnvironment.ApplicationVirtualPath;
				if (path.StartsWith(app, StringComparison.OrdinalIgnoreCase)) {
					path = path.Substring(app.Length);
				} else throw new NotSupportedException(string.Format("{0}: Absolute path with invalid ApplicationPath.", path), new SevereException());
			}

			//TODO domains paths.
			return Paths.Combine(Home, path);
		}

		public void Init(HttpApplication app) {
			app.BeginRequest += new EventHandler((sender, args) => {
				try {
					if (HttpContext.Current != null && Home != null) Modules.Remove(this);
				} catch { }
			});
		}
		public void Dispose() { }
		/// <summary>
		/// Returns a url as it would be typed in in the browser for a specific local path.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		/* public static string Url(string path) {
			var ctx = HttpContext.Current;
			HttpRequest r = null;
			string application = string.Empty;
			if (ctx != null) r = ctx.Request;
			if (r != null) application = r.Url.Scheme + "://" +  r.Url.Authority + r.ApplicationPath;
			return Paths.Combine(application, path);
		} */
		/// <summary>
		/// Returns an url as it would be typed in in the browser for a specific local path with parameters to string.Format.
		/// </summary>
		/// <param name="path">The path with format parameters.</param>
		/// <param name="args">The arguments to the format parameters.</param>
		/// <returns>The url pointing to the supplied path from a remote browser.</returns>
		public static string Url(string path, params object[] args) { return Url(string.Format(path, args)); }

		public static bool IsLocal(string url) {
			return !url.Contains(":") || url.StartsWith(Home);
		}
		public static bool IsLocal(Uri url) { return IsLocal(url.AbsoluteUri); }

		private static bool MatchSingle(string pattern, string path) {
			pattern = pattern.StartsWith("!") ? pattern.Substring(1) : Regex.Escape(pattern).Replace("\\*\\*", ".*").Replace("\\*", "[^/\\\\]*").Replace("\\?", ".");
			return Regex.Match(path, pattern).Success;
		}
		/// <summary>
		/// Checks wether the path matches one of a comma or semicolon separated list of file patterns or a single file pattern.
		/// </summary>
		/// <param name="patterns">A comma or semicolon separared list of patterns or a single pattern</param>
		/// <param name="path">The path to check.</param>
		/// <returns>True if one of the patterns matches the path.</returns>
		public static bool Match(string patterns, string path) {
			foreach (var p in patterns.Tokens()) {
				if (MatchSingle(p, path)) return true;
			}
			return false;
		}

		public static bool ExcludeMatch(string patterns, string path) {
			var file = Paths.File(path);
			foreach (var p in patterns.Tokens()) {
				if (p == file || MatchSingle(p, path)) return true;
			}
			return false;
		}

		public static string Encode(string path) {
			var ichars = System.IO.Path.GetInvalidFileNameChars()
				.Concat(System.IO.Path.GetInvalidPathChars())
				.Where(ch => ch != '+')
				.Prepend('+')
				.Distinct()
				.ToList();
			var s = new StringBuilder(path);
			foreach (var ch in ichars) {
				s = s.Replace(ch.ToString(), "+" + ((IEnumerable<byte>)Encoding.UTF8.GetBytes(new char[] { ch })).StringList("{0:X}", ""));
			}
			return s.ToString();
		}

		public static string Decode(string path) {
			var regex = new Regex("~([0-9A-F]+)");
			return regex.Replace(path,
				new MatchEvaluator(m => {
					var match = m.Groups[1].Value;
					var x = uint.Parse(match.UpTo(8), System.Globalization.NumberStyles.AllowHexSpecifier);
					var ch = Encoding.UTF8.GetString(new byte[] { (byte)(x % 0xFF), (byte)(x >> 8 % 0xFF), (byte)(x >> 16 % 0xFF), (byte)(x >> 24 % 0xFF) })[0];
					return ch + match.Substring(Encoding.UTF8.GetBytes(new char[] { ch }).Length*2);
				}));
		}

		public static string Local(string url) {
			if (!url.Contains(":")) return url;
			if (url.StartsWith(Home)) return url.Replace(Home, "~");
			throw new ArgumentOutOfRangeException("Paths.Local: Url is not a local url.");
		}

	}
}