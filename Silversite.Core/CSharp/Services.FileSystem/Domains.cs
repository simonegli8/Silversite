using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Hosting;
using System.Text;
using Srvc = Silversite.Services;

namespace Silversite.Services {

	public struct Domain : IComparable<Domain>, IComparable {
		string pattern;
		public string Pattern { get { return pattern ?? (pattern = Domains.Default.Pattern); } set { pattern = value; } }
		public string Application {
			get { return Pattern.FromOn('@').UpTo(';'); }
			set {
				if (Application != value) {
					if (!string.IsNullOrEmpty(Application)) {
						Pattern = Pattern.Tokens()
							.Where(token => !token.StartsWith("@"))
							.StringList(';');
					}
					Pattern += ";@" + value;
				}
			}
		}
		public bool HasCustomApplication { get { return Application != ""; } }
		public bool Match(string authority) { return Paths.Match(Pattern, authority); }
		public bool Match(Uri url) { return Match(url.Authority); }
		public bool Exists { get { return Domains.Collection.Contains(this); } }
		public static implicit operator Domain(string pattern) { return new Domain { Pattern = pattern }; }
		public override string ToString() { return Pattern; }
		public override int GetHashCode() { return Pattern.GetHashCode(); }
		public override bool Equals(object obj) { return obj is string && (string)obj == Pattern || obj is Domain && ((Domain)obj).Pattern == Pattern; }

		public int CompareTo(Domain other) { return Pattern.CompareTo(other.Pattern); }
		public int CompareTo(object obj) { return Pattern.CompareTo(((Domain)obj).Pattern); }
	}

	/// <summary>
	/// A class that handles domains. Domains are the internet domains that are hosted by a silversite instance. Each domain is represented by a folder in the Domains.RootPath. All domain specific pages are stored in that folder.
	/// </summary>
	public static class Domains {

		/// <summary>
		/// This class is used for all domain specific file access, similar to the class Silversite.Services.Files but specific for access to the files of a domain, whereas Silversite.Services.Files is for general file access.
		/// Each method has an overload that takes a domain, that operates specific to that domain and an overload without domain that works with the current or default domain.
		/// </summary>
		public static class Files {

			/// <summary>
			/// Creates the directory of a path in the current domain.
			/// </summary>
			/// <param name="path">The path to create.</param>
			public static void CreatePath(string path) {
				var dir = Paths.Directory(path);
				if (!string.IsNullOrEmpty(dir) && !DirectoryExists(dir)) CreateDirectory(dir);
			}
			/// <summary>
			/// Creates the directory of a path in the specified domain.
			/// </summary>
			/// <param name="domain">The domain where to create the directory.</param>
			/// <param name="path">The path to create.</param>
			public static void CreatePath(Domain domain, string path) {
				var dir = Paths.Directory(path);
				if (!string.IsNullOrEmpty(dir) && !DirectoryExists(domain, dir)) CreateDirectory(domain, dir);
			}

			public static void CopyConfig(Domain domain, string path) {
				if (Path(domain, path).StartsWith(path)) return;
				if (Srvc.Files.DirectoryExists(Path(domain, path))) path = Paths.Combine(path, "web.config");
				else path = Paths.Combine(Paths.Directory(path), "web.config");
				var dpath = Path(domain, path);
				var src = Srvc.Files.LoadVirtual(path);
				var dest = Srvc.Files.Load(dpath);
				if (src != dest) {
					if (src == null && dest != null) Srvc.Files.Delete(dpath);
					else Srvc.Files.Copy(path, dpath);
				}
			}
			public static void CopyConfig(string path) { CopyConfig(Domains.Current, path); }
			public static void Delete(string path) { Srvc.Files.Delete(Path(path)); }
			public static void Delete(Domain domain, string path) { Srvc.Files.Delete(Path(domain, path)); }
			public static void Save(Stream src, string path) { Srvc.Files.Save(src, Path(path)); CopyConfig(path); }
			public static void Save(Stream src, Domain domain, string path) { Srvc.Files.Save(src, Path(domain, path)); CopyConfig(domain, path); }
			public static void SaveWithPath(Stream src, string path) { CreatePath(path); Save(src, path); CopyConfig(path); }
			public static void SaveWithPath(Stream src, Domain domain, string path) { CreatePath(domain, path); Save(src, domain, path); CopyConfig(domain, path); }
			public static void Save(string text, string path) { Srvc.Files.Save(text, Path(path)); CopyConfig(path); }
			public static void Save(string text, Domain domain, string path) { Srvc.Files.Save(text, Path(domain, path)); CopyConfig(domain, path); }
			public static void SaveWithPath(string text, string path) { CreatePath(path); Save(text, path); CopyConfig(path); }
			public static void SaveWithPath(string text, Domain domain, string path) { CreatePath(domain, path); Save(text, domain, path); CopyConfig(domain, path); }
			public static string Load(string path) { return Srvc.Files.Load(Path(path)); }
			public static string Load(Domain domain, string path) { return Srvc.Files.Load(Path(domain, path)); }
			public static Stream Read(string path) { return Srvc.Files.Read(Path(path)); }
			public static Stream Read(Domain domain, string path) { return Srvc.Files.Write(Path(domain, path)); }
			public static Stream Write(string path) { return Srvc.Files.Read(Path(path)); }
			public static Stream Write(Domain domain, string path) { return Srvc.Files.Write(Path(domain, path)); }
			public static bool Exists(string path) { return Srvc.Files.Exists(Path(path)); }
			public static bool Exists(Domain domain, string path) { return Srvc.Files.Exists(Path(domain, path)); }
			public static bool DirectoryExists(string path) { return Srvc.Files.DirectoryExists(Path(path)); }
			public static bool DirectoryExists(Domain domain, string path) { return Srvc.Files.DirectoryExists(Path(domain, path)); }
			public static bool FileExists(string path) { return Srvc.Files.FileExists(Path(path)); }
			public static bool FileExists(Domain domain, string path) { return Srvc.Files.FileExists(Path(domain, path)); }
			public static FileInfo FileInfo(string path) { return Srvc.Files.FileInfo(Path(path)); }
			public static FileInfo FileInfo(Domain domain, string path) { return Srvc.Files.FileInfo(Path(domain, path)); }
			public static DirectoryInfo DirectoryInfo(string path) { return Srvc.Files.DirectoryInfo(Path(path)); }
			public static DirectoryInfo DirectoryInfo(Domain domain, string path) { return Srvc.Files.DirectoryInfo(Path(domain, path)); }
			public static void Move(string src, string dest) { Srvc.Files.Move(Path(src), Path(dest)); CopyConfig(dest); }
			public static void Move(Domain domain, string src, string dest) { Srvc.Files.Move(Path(domain, src), Path(domain, dest)); CopyConfig(domain, dest); }
			public static void Move(Domain srcdomain, string src, Domain destdomain, string dest) { Srvc.Files.Move(Path(srcdomain, src), Path(destdomain, dest)); CopyConfig(destdomain, dest); }
			public static void Copy(string src, string dest) { Srvc.Files.Copy(Path(src), Path(dest)); CopyConfig(dest); }
			public static void Copy(Domain domain, string src, string dest) { Srvc.Files.Copy(Path(domain, src), Path(domain, dest)); CopyConfig(domain, dest); }
			public static void Copy(Domain srcdomain, string src, Domain destdomain, string dest) { Srvc.Files.Copy(Path(srcdomain, src), Path(destdomain, dest)); CopyConfig(destdomain, dest); }
			public static void CreateDirectory(string path) { Srvc.Files.CreateDirectory(Path(path)); CopyConfig(path); }
			public static void CreateDirectory(Domain domain, string path) { Srvc.Files.CreateDirectory(Path(domain, path)); CopyConfig(domain, path); }
			public static void Download(string path) { Srvc.Files.Serve(Path(path)); }
			public static void Download(Domain domain, string path) { Srvc.Files.Serve(Path(domain, path)); }
			public static string Url(string path) { return Srvc.Files.Url(Path(path)); }
			public static string Url(Domain domain, string path) { return Srvc.Files.Url(Path(domain, path)); }
		}

		public const string RootPath = Paths.Pages;
		public static readonly Domain Default = "_;*";
		public const string DomainArgument = "Silversite.Domain";

		public class DomainCollection : List<Domain> {

			public static DomainCollection Load() {
				var col = new DomainCollection();
				var info = Srvc.Files.DirectoryInfo(RootPath);
				if (info.Exists) {
					var dirs = info.EnumerateDirectories().ToList();
					col.AddRange(dirs.Select(d => Paths.Decode(d.Name))
						.OrderBy(d => d)
						.Select(d => (Domain)d));

				}
				return col;
			}
		}

		public static DomainCollection Collection;

		static Domains() {
			Collection = DomainCollection.Load();
			if (Collection.Count == 0) Create(Default);
		}

		public static Domain FromUrl(Uri url) { return FromAuthority(url.Authority); }
		public static Domain FromAuthority(string authority) {
			if (string.IsNullOrEmpty(authority)) return Default;
			return Collection.Where(d => d.Match(authority)).FirstOrDefault();
		}
		public static Domain FromPath(string path) {
			if (Paths.IsDomains(path)) {
				path = Paths.Normalize(path);
				int i = path.IndexOf('/', RootPath.Length + 1);
				if (i < 0) i = path.Length;
				var domain = (Domain)Paths.Decode(path.Substring(RootPath.Length + 1, i - RootPath.Length - 1));
				return domain;
			}
			return Default;
		}

		public static Domain Current {
			get {
				try {
					if (HttpContext.Current == null) return Default;
					var authority = HttpContext.Current.Request.QueryString[DomainArgument];
					if (authority == null && HttpContext.Current.Session != null) {
						authority = (string)HttpContext.Current.Session[DomainArgument];
					}
					if (authority == null) authority = HttpContext.Current.Request.Url.Authority;
					return FromAuthority(authority);
				} catch {
					return Default;
				}
			}
		}

		public static string Path(Domain domain, string path) {
			path = Paths.Normalize(path);
			if (Paths.IsDomains(path)) {
				int i = path.IndexOf('/', RootPath.Length + 1);
				if (i > 0 && i < path.Length) {
					path = Paths.Normalize(path.Substring(i + 1));
				} else path = "~";
			}
			return Paths.Combine(RootPath, Paths.Encode(domain.Pattern), path);
		}

		public static string Path(string path) {
			return Path(Current, path);
		}

		public static void Create(Domain domain) {
			if (!domain.Exists) {
				try {
					Files.CreateDirectory(domain, "~/");
					if (domain.HasCustomApplication) {
						Files.CreateDirectory(domain, "~/Silversite/Users");
						Files.CreateDirectory(domain, "~/Silversite/Users/Public");
						Files.CreateDirectory(domain, "~/Silversite/Users/Public/Pictures");
						Files.CreateDirectory(domain, "~/Silversite/Users/Public/Documents");
						Files.CreateDirectory(domain, "~/Silversite/Users/Public/Media");
					}
					Collection.Add(domain);
					Collection.Sort();
					Log.Write("Administration", "Domain {0} created.", domain);
				} catch (Exception ex) {
					Log.Error("Failed to create domain folder for domain {0}.", ex, domain);
					System.Diagnostics.Debugger.Break();
				}
			}
		}

		public static void Delete(Domain domain) {
			if (Collection.Remove(domain)) {
				try {
					Files.Delete(domain, string.Empty);
					Log.Write("Administration", "Domain {0} deleted.", domain);
				} catch (Exception ex) {
					Log.Error("Failed to delete domain folder for domain {0}.", ex, domain);
					System.Diagnostics.Debugger.Break();
				}
			}
		}

	}
}