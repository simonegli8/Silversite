using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web.Hosting;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Globalization;

namespace Silversite.Services {

	[Configuration.Section(Path = FilesConfiguration.Path, Group = "silversite")]
	public class FilesConfiguration: Configuration.Section {

		public new const string Path = ConfigRoot + "/Silversite.config";

		[ConfigurationProperty("ftpUrl", IsRequired = false, DefaultValue = "ftp://localhost")]
		public string FtpUrl { get { return this["ftpUrl"] as string ?? string.Empty; } set { this["ftpUrl"] = value; } }

		[ConfigurationProperty("writablePaths", IsRequired = false, DefaultValue = "~/**")]
		public string WritablePaths { get { return this["writablePaths"] as string ?? "~/**"; } set { this["writablePaths"] = value; } }

		[ConfigurationProperty("home", IsRequired = false, DefaultValue = "")]
		public string Home { get { return this["home"] as string ?? string.Empty; } set { this["home"] = value; } }
	}

	public static class Files {

		public const string UserFilesFolder = "~/Silversite/Users";
		public const string DocumentsFolder = "Documents";
		public const string ImagesFolder = "Images";
		public const string MediaFolder = "Media";
		public const string PublicUserFilesFolder = "~/Silversite/Users/Public";
		public const string DomainsFolder = Domains.RootPath;
		public const string FullFileSystemAccessRole = "FullFileSystemAccess";


		class FtpClient: Ftp.FtpClient {

			public FtpClient(): base() {
				if (!IsAvailable) return;
				try {
					var url = new Uri(Configuration.FtpUrl);
					Host = url.Host;
					Port = url.IsDefaultPort ? 25 : url.Port;
					Open(url.User(), url.Password());
					ChangeDirectory(url.PathAndQuery);
				} catch (Exception ex) {
					Silversite.Services.Log.Error("Failed to connect to filesystem via FTP.", ex);
					throw ex;
				}
			}

			public string Normalize(string path) { return Paths.Normalize(path).Substring(1); }

			public void Split(string path, out string dir, out string file) { Paths.Split(Normalize(path), out dir, out file); }

			public void DeleteDirectoryRecursive(string path) {
				path = Normalize(path);
				if (path.Contains('/')) ChangeDirectory(path);
				else ChangeDirectory(path);
				foreach (var f in GetDirList()) {
					if (f.ItemType == Ftp.FtpItemType.Directory) DeleteDirectoryRecursive(f.ParentPath);
					else Delete(path);
				}
				ChangeDirectoryUp();
				string dir, name;
				Paths.Split(path, out dir, out name);
				DeleteDirectory(name);
			}
		}

		public static FilesConfiguration Configuration = null;

		static Files() {
			Configuration = new FilesConfiguration();
		}

		public static bool CanUseFtp { get { return !Configuration.FtpUrl.IsNullOrEmpty() && Providers.HasProvider(typeof(Ftp.FtpClient)); } }

		public static void Delete(IEnumerable<string> paths) {
			paths.Each(path => {
				if (path.Contains("*")) All(path).Each(p => Delete(p));
				path = Paths.Normalize(path);
				var ftppath = path.Substring(1);
				var diskpath = Paths.Map(path);
				if (Directory.Exists(diskpath)) {
					if (Paths.IsWritable(path)) {
						try {
							Directory.Delete(diskpath, true);
						} catch {
							var info = new DirectoryInfo(diskpath);
							var children = info.EnumerateFileSystemInfos("*", SearchOption.AllDirectories).ToList();
							foreach (var file in children.OfType<FileInfo>()) File.Delete(file.FullName);
							children.Reverse();
							foreach (var dir in children.OfType<DirectoryInfo>()) Directory.Delete(dir.FullName);
						}
					} else if (CanUseFtp) {
						using (var ftp = new FtpClient()) {
							ftp.DeleteDirectoryRecursive(ftppath);
							ftp.Close();
						}
					} else throw new IOException(string.Format("Path {0} is write protected.", path));
				} else if (File.Exists(diskpath)) {
					if (Paths.IsWritable(path)) File.Delete(diskpath);
					else if (CanUseFtp) {
						using (var ftp = new FtpClient()) {
							string dir, name;
							ftp.Split(path, out dir, out name);
							ftp.ChangeDirectory(dir);
							ftp.DeleteFile(name);
							ftp.Close();
						}
					} else throw new IOException(string.Format("Path {0} is write protected.", path));
				}
			});
		}
		public static void Delete(params string[] paths) { Delete((IEnumerable<string>)paths); }
	
		static void SaveRaw(Stream src, string path) {
			const int size = 8 * 1024;
			var buf = new byte[size];
			int n;
			if (src.CanSeek) src.Seek(0, SeekOrigin.Begin);
			using (var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)) {
				while (src.Position < src.Length) {
					n = src.Read(buf, 0, size);
					file.Write(buf, 0, n);
				}
			}
		}

		public static void Save(Stream src, string path) {
			if (Paths.IsWritable(path)) {
				SaveRaw(src, Paths.Map(path));
			} else if (CanUseFtp) {
				using (var ftp = new FtpClient()) {
					string dir, name;
					ftp.Split(path, out dir, out name);
					ftp.ChangeDirectory(dir);
					ftp.PutFile(src, name, Ftp.FileAction.Create);
					ftp.Close();
				}
			} else throw new IOException(string.Format("Path {0} is write protected.", path));
		}

		public static void Save(string text, string path) {
			using (var w = new StreamWriter(new MemoryStream(2*text.Length))) {
				w.Write(text);
				w.Flush();
				Save(w.BaseStream, path);
			}
		}

		public static void Save(byte[] buffer, string path) {
			using (var w = new BinaryWriter(new MemoryStream(buffer.Length))) {
				w.Write(buffer);
				w.Flush();
				Save(w.BaseStream, path);
			}
		}

		public static void Save(object x, string path) {
			using (var m = new MemoryStream()) {
				var bf = new BinaryFormatter();
				bf.Serialize(m, x);
				Save(m, path);
			}
		}

		public static void Save(XContainer x, string path) { Save(x.ToString(SaveOptions.OmitDuplicateNamespaces), path); }


		public static void SaveWithPath(Stream src, string path) {
			if (!DirectoryExists(Paths.Directory(path))) CreateDirectory(Paths.Directory(path));
			Save(src, path);
		}

		public static void SaveWithPath(string text, string path) {
			if (!DirectoryExists(Paths.Directory(path))) CreateDirectory(Paths.Directory(path));
			Save(text, path);
		}

		public static void SaveWithPath(byte[] buffer, string path) {
			if (!DirectoryExists(Paths.Directory(path))) CreateDirectory(Paths.Directory(path));
			Save(buffer, path);
		}

		public static void SaveWithPath(object x, string path) {
			if (!DirectoryExists(Paths.Directory(path))) CreateDirectory(Paths.Directory(path));
			Save(x, path);
		}
		public static void SaveWithPath(XContainer x, string path) { SaveWithPath(x.ToString(SaveOptions.OmitDuplicateNamespaces), path); }

		public static void SaveLines(IEnumerable<string> lines, string path) {
			using (var w = new StreamWriter(new MemoryStream())) {
				foreach (var line in lines) w.WriteLine(line);
				w.Flush();
				Save(w.BaseStream, path);
			}
		}

		public static void SaveLinesWithPath(IEnumerable<string> lines, string path) {
			if (!DirectoryExists(Paths.Directory(path))) CreateDirectory(Paths.Directory(path));
			SaveLines(lines, path);
		}

		public static string Load(string path) {
			using (var r = new StreamReader(Read(path), Encoding.UTF8, true)) {
				return r.ReadToEnd();
			}
		}

		public static List<string> LoadLines(string path) {
			var res = new List<string>();
			using (var r = new StreamReader(Read(path), Encoding.UTF8, true)) {
				while (!r.EndOfStream) res.Add(r.ReadLine());
			}
			return res;
		}

		public static byte[] LoadBuffer(string path) {
			using (var r = new BinaryReader(Read(path))) {
				return r.ReadBytes((int)r.BaseStream.Length);
			}
		}

		public static object LoadSerializable(string path) {
			using (var f = new FileStream(path, FileMode.Open, FileAccess.Read)) {
				var bf = new BinaryFormatter();
				return bf.Deserialize(f);
			}
		}

		public static XElement LoadXElement(string path) {
			using (var r = new StreamReader(Read(path), Encoding.UTF8, true)) {
				return XElement.Load(r, LoadOptions.PreserveWhitespace | LoadOptions.SetBaseUri | LoadOptions.SetLineInfo);
			}
		}

		public static XDocument LoadXDocument(string path) {
			using (var r = new StreamReader(Read(path), Encoding.UTF8, true)) {
				return XDocument.Load(r, LoadOptions.PreserveWhitespace | LoadOptions.SetBaseUri | LoadOptions.SetLineInfo);
			}
		}

		public static void Append(string text, string path) {
			if (!DirectoryExists(Paths.Directory(path))) CreateDirectory(Paths.Directory(path));
			using (var w = new StreamWriter(Paths.Map(path), true)) w.Write(text); 
		}

		// vitual files
		public static bool IsRequestInEditOrPreviewMode {
			get {
				try {
					var ctx = HttpContext.Current;
					return ctx != null && ctx.Request != null && (ctx.Request.QueryString["silversite.pagemode"] == "edit" && ctx.Request.QueryString["silversite.pagemode"] == "preview");
				} catch { return false; }
			}
		}

		// TODO Implement a least common roles requirement to access a preview page based on the current users roles and the Editors properties of the EditableContent controls contained in the page.
		// e.g. save all allowed roles of the Editors properties in a separate file and check here, if the users roles matches any of them.
		public static bool CurrentUserIsEditor(string path) { 	return true; }

		public static Stream OpenVirtual(string path) { if (FileExistsVirtual(path)) { return HostingEnvironment.VirtualPathProvider.GetFile(Paths.Absolute(path)).Open(); } return null; }
		public static string LoadVirtual(string path) {
			using (var stream = OpenVirtual(path)) {
				if (stream != null) return new StreamReader(stream).ReadToEnd();
			}
			return null;
		}

		public static bool ExistsVirtual(string path) { return HostingEnvironment.VirtualPathProvider.FileExists(Paths.Absolute(path)); }
		public static bool DirectoryExistsVirtual(string path) { return HostingEnvironment.VirtualPathProvider.DirectoryExists(Paths.AddSlash(Paths.Absolute(path))); }
		public static bool FileExistsVirtual(string path) { return HostingEnvironment.VirtualPathProvider.FileExists(Paths.Absolute(path)); }

		static IEnumerable<string> AllRecursiveVirtual(System.Web.Hosting.VirtualDirectory dir, string patterns, bool loadedOnly) {
			if (dir is VirtualDirectory) ((VirtualDirectory)dir).LoadedOnly = loadedOnly;
			return 
				dir.Directories.OfType<System.Web.Hosting.VirtualDirectory>().SelectMany(d => AllRecursiveVirtual(d, patterns, loadedOnly))
				.Union(AllLocalVirtual(dir, patterns, loadedOnly), StringComparer.OrdinalIgnoreCase);
		}
		static IEnumerable<string> AllLocalVirtual(System.Web.Hosting.VirtualDirectory dir, string patterns, bool loadedOnly) {
			if (dir is VirtualDirectory) ((VirtualDirectory)dir).LoadedOnly = loadedOnly;
			return
				dir.Files.OfType<System.Web.Hosting.VirtualFile>()
				.Select(f => Services.Paths.Normalize(f.VirtualPath))
				.Where(path => Paths.Match(patterns, path));
		}
		static IEnumerable<string> AllRecursive(DirectoryInfo dir, string patterns) {
			return
				dir.Exists ?
					dir.GetDirectories().SelectMany(d => AllRecursive(d, patterns))
						.Union(AllLocal(dir, patterns), StringComparer.OrdinalIgnoreCase)
					: new string[0];
		}
		static IEnumerable<string> AllLocal(DirectoryInfo dir, string patterns) {
			return
				dir.Exists ? 
					dir.GetFiles()
						.Select(f => Paths.Unmap(f.FullName))
						.Where(path => Paths.Match(patterns, path))
					: new string[0];
		}

		public static IEnumerable<string> AllVirtual(string patterns) {
			return patterns
				.Tokens(s => Paths.Directory(s))
				.Select(s => s.Contains('*') ? Paths.Directory(s.UpTo('*')) : s)
				.Select(s => s.IsNullOrEmpty() ? "~/" : s)
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.Select(d => VirtualDirectory(d))
				.Where(dir => dir != null)
				.SelectMany(dir => AllRecursiveVirtual(dir, patterns, false))
				.Distinct(StringComparer.OrdinalIgnoreCase);
		}
		public static IEnumerable<string> AllLoaded(string patterns) {
			return patterns
				.Tokens(s => Paths.Directory(s))
				.Select(s => s.Contains('*') ? Paths.Directory(s.UpTo('*')) : s)
				.Select(s => s.IsNullOrEmpty() ? "~/" : s)
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.Select(d => VirtualDirectory(d))
				.Where(dir => dir != null)
				.SelectMany(dir => AllRecursiveVirtual(dir, patterns, true))
				.Distinct(StringComparer.OrdinalIgnoreCase);
		}
		public static IEnumerable<string> DirectoryAllVirtual(string patterns) {
			return patterns
				.Tokens(s => Paths.Directory(s))
				.Select(s => s.Contains('*') ? Paths.Directory(s.UpTo('*')) : s)
				.Select(s => s.IsNullOrEmpty() ? "~/" : s)
				.Distinct(StringComparer.OrdinalIgnoreCase)
				//.Where(d => !d.Contains("*"))
				.Select(d => VirtualDirectory(d))
				.Where(dir => dir != null)
				.SelectMany(dir => AllLocalVirtual(dir, patterns, false))
				.Distinct(StringComparer.OrdinalIgnoreCase);
		}
		public static IEnumerable<string> DirectoryAllLoaded(string patterns) {
			return patterns
				.Tokens(s => Paths.Directory(s))
				.Select(s => s.Contains('*') ? Paths.Directory(s.UpTo('*')) : s)
				.Select(s => s.IsNullOrEmpty() ? "~/" : s)
				.Distinct(StringComparer.OrdinalIgnoreCase)
				//.Where(d => !d.Contains("*"))
				.Select(d => VirtualDirectory(d))
				.Where(dir => dir != null)
				.SelectMany(dir => AllLocalVirtual(dir, patterns, true))
				.Distinct(StringComparer.OrdinalIgnoreCase);
		}
		public static IEnumerable<string> All(string patterns) {
			return patterns
				.Tokens(s => Paths.Directory(s))
				.Select(s => s.Contains('*') ? Paths.Directory(s.UpTo('*')) : s)
				.Select(s => s.IsNullOrEmpty() ? "~/" : s)
				.Distinct(StringComparer.OrdinalIgnoreCase)
				//.Where(d => !d.Contains("*"))
				.Select(d => DirectoryInfo(d))
				.SelectMany(dir => AllRecursive(dir, patterns))
				.Distinct(StringComparer.OrdinalIgnoreCase);
		}
		public static IEnumerable<string> DirectoryAll(string patterns) {
			return patterns
				.Tokens(s => Paths.Directory(s))
				.Select(s => s.Contains('*') ? Paths.Directory(s.UpTo('*')) : s)
				.Select(s => s.IsNullOrEmpty() ? "~/" : s)
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.Select(d => DirectoryInfo(d))
				.SelectMany(dir => AllLocal(dir, patterns))
				.Distinct(StringComparer.OrdinalIgnoreCase);
		}
		public static IEnumerable<string> All(params string[] patterns) { return patterns.SelectMany(p => All(p)); }
		public static  IEnumerable<string> AllVirtual(params string[] patterns) { return patterns.SelectMany(p => AllVirtual(p)); }
		public static IEnumerable<string> AllLoaded(params string[] patterns) { return patterns.SelectMany(p => AllLoaded(p)); }

		public static System.Web.Hosting.VirtualFile VirtualFile(string path) {
			try {
				path = Paths.Absolute(path);
				if (HostingEnvironment.VirtualPathProvider.FileExists(path))
					return HostingEnvironment.VirtualPathProvider.GetFile(path);
				else return null;
			} catch { return null; }
		}
		public static System.Web.Hosting.VirtualDirectory VirtualDirectory(string path) {
			try {
				path = Paths.AddSlash(Paths.Absolute(path));
				if (HostingEnvironment.VirtualPathProvider.DirectoryExists(path))
					return HostingEnvironment.VirtualPathProvider.GetDirectory(path);
				else return null;
			} catch { return null; }
		}
		public static System.Web.Hosting.VirtualFileBase Virtual(string path) {
			return (System.Web.Hosting.VirtualFileBase)VirtualDirectory(path) ?? VirtualFile(path);
		}
		public static System.IO.FileSystemInfo Info(string path) {
			var file = FileInfo(path);
			var dir = DirectoryInfo(path);
			if (file.Exists) return file;
			if (dir.Exists) return dir;
			return null;
		}

		public static Stream Read(string path) {
			var diskpath = Paths.Map(path);
			if (File.Exists(diskpath)) return new FileStream(diskpath, FileMode.Open, FileAccess.Read);
			throw new FileNotFoundException(string.Format("File {0} not found.", path));
		}

		public static Stream Write(string path) {
			var diskpath = Paths.Map(path);
			if (Paths.IsWritable(path)) return new FileStream(diskpath, FileMode.Create, FileAccess.Write);
			else if (CanUseFtp) {
				var pipe = new PipeStream();
				Tasks.Do(() => {
					using (pipe)
					using (var ftp = new FtpClient()) {
						string dir, name;
						ftp.Split(path, out dir, out name);
						ftp.ChangeDirectory(dir);
						ftp.PutFile(pipe, name, Ftp.FileAction.Create);
						ftp.Close();
					}
				});
				return pipe;
			}
			throw new IOException(string.Format("Path {0} is write protected.", path));
		}

		public static bool Exists(string path) {
			var diskpath = Paths.Map(path);
			return File.Exists(diskpath) || Directory.Exists(diskpath);
		}
		public static bool DirectoryExists(string path) { return Directory.Exists(Paths.Map(path)); }
		public static bool FileExists(string path) { return File.Exists(Paths.Map(path)); }

		public static FileInfo FileInfo(string path) { return new FileInfo(Paths.Map(path)); }
		public static DirectoryInfo DirectoryInfo(string path) { return new DirectoryInfo(Paths.Map(path)); }

		public static void Move(string src, string dest) {
			if (src.Contains(";") || src.Contains('*') || src.Contains('?')) {
				var srcdir = src;
				if (src.Contains('*')) srcdir = srcdir.UpTo('*');
				All(src).Each(f => {
					Move(f, Paths.Combine(dest, Paths.Directory(f.Substring(srcdir.Length)), Paths.File(f)));
				});
			} else {
				if (src == dest) return;
				if (Paths.IsWritable(dest)) {
					if (DirectoryExists(src)) Directory.Move(Paths.Map(src), Paths.Map(dest));
					else {
						try {
							File.Move(Paths.Map(src), Paths.Map(dest));
						} catch (Exception ex) {
							var dir = Paths.Directory(dest);
							if (!DirectoryExists(dir)) {
								CreateDirectory(dir);
								File.Move(Paths.Map(src), Paths.Map(dest));
							} else {
								throw ex;
							}
						}
					}
				} else if (CanUseFtp) {
					if (DirectoryExists(src)) {
						CreateDirectory(dest);
						var info = DirectoryInfo(src);
						foreach (var obj in info.EnumerateFileSystemInfos()) Move(Paths.Combine(src, info.Name), Paths.Combine(dest, info.Name));
						Delete(src);
					} else {
						using (var file = Read(src)) {
							SaveWithPath(file, dest);
						}
						Delete(src);
					}
				} else throw new IOException(string.Format("Path {0} is write protected.", dest));
			}
		}

		public static void Copy(string src, string dest) {
			if (src.Contains(";") || src.Contains('*') || src.Contains('?')) {
				var srcdir = src;
				if (src.Contains('*')) srcdir = srcdir.UpTo('*');
				All(src).Each(f => {
					Copy(f, Paths.Combine(dest, Paths.Directory(f.Substring(srcdir.Length)), Paths.File(f)));
				});
			} else {
				if (src == dest) return;
				if (DirectoryExists(src)) {
					//TODO bug
					dest = Paths.Combine(dest, Paths.File(src));
					CreateDirectory(dest);
					var info = DirectoryInfo(src);
					foreach (var obj in info.EnumerateFileSystemInfos()) Copy(Paths.Combine(src, obj.Name), Paths.Combine(dest, obj.Name));
				} else {
					if (Paths.IsWritable(dest)) {
						try {
							File.Copy(Paths.Map(src), Paths.Map(dest), true);
						} catch (Exception ex) {
							var dir = Paths.Directory(dest);
							if (!DirectoryExists(dir)) {
								CreateDirectory(dir);
								File.Copy(Paths.Map(src), Paths.Map(dest), true);
							} else {
								throw ex;
							}
						}
					} else if (CanUseFtp) {
						using (var file = Read(src)) {
							SaveWithPath(file, dest);
						}
					} else throw new IOException(string.Format("Path {0} is write protected.", dest));
				}
			}
		}

		public static void CreateDirectory(IEnumerable<string> paths) {
			foreach (var path in paths) {
				var diskpath = Paths.Map(path);
				if (!Directory.Exists(diskpath)) {
					if (Paths.IsWritable(path)) Directory.CreateDirectory(diskpath);
					else if (CanUseFtp) {
						using (var ftp = new FtpClient()) {
							string dir, name;
							ftp.Split(path, out dir, out name);
							if (!dir.IsNullOrEmpty()) ftp.ChangeDirectory(dir);
							ftp.MakeDirectory(name);
							ftp.Close();
						}
					} else throw new IOException(string.Format("Path {0} is write protected.", path));
				}
			}
		}
		public static void CreateDirectory(string paths) { CreateDirectory(paths.Tokens()); }
		public static void CreateDirectory(params string[] paths) { CreateDirectory(paths); }

		public static void Serve(string path, string mimeType = null) {
			var info = FileInfo(path);
			var ctx = HttpContext.Current;
			HttpResponse response = null;
			if (ctx != null) response = ctx.Response;
			if (info.Exists && response != null) {
				response.Clear();
				response.AddHeader("Content-Disposition", string.Format("attachment; filename=\"{0}\"", info.Name));
				response.AddHeader("Content-Length", info.Length.ToString());
				if (mimeType == null) response.ContentType = MimeType.OfFile(path);
				else response.ContentType = mimeType;
				response.WriteFile(path);
				response.End();
			}
		}

		public static void Serve(Stream stream, string name, string mimeType = null) {
			var ctx = HttpContext.Current;
			HttpResponse response = null;
			if (ctx != null) response = ctx.Response;
			if (response != null) {
				response.Clear();
				response.AddHeader("Content-Disposition", string.Format("attachment; filename=\"{0}\"", Paths.File(name)));
				response.AddHeader("Content-Length", stream.Length.ToString());
				if (mimeType == null) response.ContentType = MimeType.OfFile(stream, name);
				else response.ContentType = mimeType;
				if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);
				stream.CopyTo(response.OutputStream);
				response.End();
			}
		}

		private static bool ByteArrayHasPrefix(byte[] prefix, byte[] byteArray) {
			if (((prefix == null) || (byteArray == null)) || (prefix.Length > byteArray.Length)) {
				return false;
			}
			for (int i = 0; i < prefix.Length; i++) {
				if (prefix[i] != byteArray[i]) return false;
			}
			return true;
		}

		private static string GetStringUsingEncoding(HttpContext ctx, byte[] data) {
			System.Text.Encoding encoding = null;
			string contentType;
			int index = -1;
			encoding = ctx.Response.ContentEncoding;

			if (encoding == null || encoding == Encoding.GetEncoding("iso-8859-1")) {
				System.Text.Encoding[] encodingArray = new System.Text.Encoding[] { System.Text.Encoding.UTF8, System.Text.Encoding.UTF32, System.Text.Encoding.Unicode, System.Text.Encoding.BigEndianUnicode };
				for (int i = 0; i < encodingArray.Length; i++) {
					byte[] preamble = encodingArray[i].GetPreamble();
					if (ByteArrayHasPrefix(preamble, data)) {
						encoding = encodingArray[i];
						index = preamble.Length;
						break;
					}
				}
			}
			if (encoding == null) {
				encoding = Encoding.GetEncoding("iso-8859-1");
			}
			if (index == -1) {
				byte[] prefix = encoding.GetPreamble();
				if (ByteArrayHasPrefix(prefix, data)) {
					index = prefix.Length;
				} else {
					index = 0;
				}
			}
			return encoding.GetString(data, index, data.Length - index);
		}

		public static string Execute(string path) {
			using (var m = new MemoryStream()) 
			using (var w = new StreamWriter(m)) {
				path = Paths.Absolute(path.Replace(Paths.Home, "~"));
				var ctx = HttpContext.Current;
				var oldpath = ctx.Request.Url.AbsolutePath;
				ctx.RewritePath(path);
				ctx.Server.Execute(path, w, false);
				ctx.RewritePath(oldpath);
				w.Flush();
				return GetStringUsingEncoding(ctx, m.ToArray());
			}
		}
	
		public static string Url(string path) { return Paths.Url(path); }
		public static string Html(Uri url, Action<AdvancedWebClient> setup = null) { return Html(url.ToString(), setup); }
		public static string Html(string path, Action<AdvancedWebClient> setup = null) {
			if (HttpContext.Current == null || !Paths.IsLocal(path)) {
				if (!path.Contains(':')) path = Paths.Url(path);
				var web = new AdvancedWebClient(false);
				if (setup != null) setup(web);
				return web.DownloadString(Paths.Url(path));
			} else {
				return Execute(path);
			}
		}

		public static Stream Download(Uri url, Action<AdvancedWebClient> setup = null, string toFile = null) {
			if (toFile != null) Save(Download(url, setup, null), toFile);
			switch (url.Scheme) {
				case "http":
				case "https":
					if (Paths.IsLocal(url)) {
						var path = Uri.UnescapeDataString(url.AbsoluteUri.Replace(Paths.Home, "~"));
						return OpenVirtual(path);
					} else {
						var web = new AdvancedWebClient(true);
						if (setup != null) setup(web);
						return web.OpenRead(url.ToString());
					}
				case "ftp":
				case "ftps":
					if (Providers.HasProvider(typeof(Ftp.FtpClient))) {

						Ftp.FtpSecurityProtocol protocol;
						if (url.Scheme == "ftp") protocol = Ftp.FtpSecurityProtocol.None;
						else protocol = Ftp.FtpSecurityProtocol.Tls1OrSsl3Explicit;

						var path = url.PathAndQuery;
						if (path.Contains('?')) path = path.Substring(0, path.IndexOf('?'));

						var user = url.UserInfo;
						var password = "anonymous";
						if (user.IsNullOrEmpty()) {
							user = "anonymous";
						} else if (user.Contains(":")) {
							int p = user.IndexOf(':');
							password = user.Substring(p+1);
							user = user.Substring(0, p);
						}

						var stream = new PipeStream();
						Tasks.Do(() => {
							using (var ftp = new Ftp.FtpClient(url.Host, url.Port, protocol)) {
								ftp.Open(user, password);
								ftp.GetFile(path, stream, false);
							}
						});
						return stream;
					} else throw new NotSupportedException("There is no FTP Provider available.");
				default:
					if (url.IsFile) return Read(url.LocalPath);
					throw new NotSupportedException("Only http, https, ftp & ftps url's are supported.");
			}
		}
		public static void Download(Uri url, string dest) { Download(url, null, dest); }

		public static void Upload(Stream stream, Uri url) {
			using (stream) {
				switch (url.Scheme) {
					case "ftp":
					case "ftps":
						if (Providers.HasProvider(typeof(Ftp.FtpClient))) {

							Ftp.FtpSecurityProtocol protocol;
							if (url.Scheme == "ftp") protocol = Ftp.FtpSecurityProtocol.None;
							else protocol = Ftp.FtpSecurityProtocol.Tls1OrSsl3Explicit;

							var path = url.PathAndQuery;
							if (path.Contains('?')) path = path.Substring(0, path.IndexOf('?'));

							var user = url.UserInfo;
							var password = "anonymous";
							if (user.IsNullOrEmpty()) {
								user = "anonymous";
							} else if (user.Contains(":")) {
								int p = user.IndexOf(':');
								password = user.Substring(p+1);
								user = user.Substring(0, p);
							}

							using (var ftp = new Ftp.FtpClient(url.Host, url.Port, protocol)) {
								ftp.Open(user, password);
								ftp.PutFile(stream, path, Ftp.FileAction.Create);
							}
						} else throw new NotSupportedException("There is no FTP Provider available.");
						break;
					default:
						if (url.IsFile) Save(stream, url.LocalPath);
						throw new NotSupportedException("Only ftp & ftps url's are supported.");
				}
			}
		}
		public static void Upload(string file, Uri url) { Upload(Read(file), url); }

		public static void Sync(Uri source, Uri dest, Sync.Mode mode, string logfile, string excludePaths = "", bool verbose = false) { Services.Sync.Files(source, dest, mode, logfile, excludePaths, verbose); }

		public static string Temp { get { return Paths.Temp + "/" + new Guid().ToString(); } }

		/// <summary>
		/// Precompiles all pages of the site.
		/// </summary>
		public static void Precompile() {
			Lazy.Paths.LoadAll();
			AllVirtual("*.aspx;*.ashx;*.asmx;*.xic.xaml")
				.AwaitAll(file => Download(new Uri(Paths.Url(file))));
		}

	}
}