using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using System.IO;
using System.Drawing;
using System.Runtime.Serialization;
using System.Web;
using Srvc = Silversite.Services;

namespace Silversite.WebServices {

	[DataContract]
	public class Message {
		[DataMember]
		public string Error;
		[DataMember]
		public int Code;

		public Message() {
			Error = "No Error";
			Code = 0;
		}
	}	

	[DataContract]
	public class FileObjectInfo {

		public const string IconsPath = "~/Silversite/Images/fileicons/";

		[DataMember]
		public string FullName;
		[DataMember]
		public string Name;
		[DataMember]
		public string Extension;
		[DataMember]
		public string MimeType;
		[DataMember]
		public string Preview;

		[DataMember]
		public DateTime DateCreated;
		[DataMember]
		public DateTime DateModified;
		[DataMember]
		public int Height;
		[DataMember]
		public int Width;
		[DataMember]
		public long Size;
		[DataMember]
		public bool IsDirectory;
		[DataMember]
		public bool IsImage;
		[DataMember]
		public bool Exists;

		public Image Image;

		public void CopyTo(FileObjectInfo dest) {
			dest.FullName = FullName;
			dest.Name = Name;
			dest.Extension = Extension;
			dest.MimeType = MimeType;
			dest.Preview = Preview;
			dest.DateCreated = DateCreated;
			dest.DateModified = DateModified;
			dest.Height = Height;
			dest.Width = Width;
			dest.Size = Size;
			dest.IsDirectory = IsDirectory;
			dest.IsImage = IsImage;
			dest.Exists = Exists;
		}

		public FileObjectInfo(string path, bool getSize, bool showThumbs) {
			path = Srvc.Paths.Normalize(path);
			var user = Srvc.Persons.Current;
			Exists = false;
			if (user == null || (path.StartsWith(Srvc.Files.UserFilesFolder + "/") && !path.StartsWith(user.HomePath + "/"))) return;
			var diskpath = HostingEnvironment.MapPath(path);
			FullName = path;
			string dir;
			Srvc.Paths.Split(path, out dir, out Name);

			Preview = Srvc.Files.Url(IconsPath + "default.png");

			var dinfo = Srvc.Files.DirectoryInfo(path);
			IsDirectory = dinfo.Exists;
			if (IsDirectory) {
				DateCreated = dinfo.CreationTime;
				DateModified = dinfo.LastWriteTime;
				Exists = true;
				Preview = Srvc.Files.Url(IconsPath + "_Open.png");
				MimeType = null;
				Extension = null;
			} else {
				Extension = System.IO.Path.GetExtension(Name).ToLower();
				var info = Srvc.Files.FileInfo(path);
				Exists = info.Exists;
				if (Exists) {
					MimeType = Srvc.MimeType.OfFile(path);
					DateCreated = info.CreationTime;
					DateModified = info.LastWriteTime;
					Size = info.Length;
					var ext = Extension;
					IsImage = ext == ".png" || ext == ".jpeg" || ext == ".jpg" || ext == ".gif" || ext == ".bmp" || ext == ".tif" || ext == ".tiff";
					if (IsImage) {
						if (showThumbs)	Preview = Srvc.Files.Url(path + ".thumbnail?height=128&width=128");
						else Preview = Srvc.Files.Url(path + ".thumbnail?height=300&width=400");
						if (getSize) {
							try {
								Image = Image.FromFile(path);
								Height = Image.Size.Height;
								Width = Image.Size.Width;
							} catch { }
						}
					} else {
						if (!string.IsNullOrWhiteSpace(ext) && ext.Length > 1) {
							ext = ext.Substring(1);
							var icon = IconsPath + ext + ".png";
							if (Srvc.Files.Exists(icon)) Preview = Srvc.Files.Url(icon);
						}
					}
				}
			}
		}

	}

	[DataContract]
	public class FileInfo: FileObjectInfo {
		public FileInfo(string path, bool getSizes, bool ShowThumbs) : base(path, getSizes, ShowThumbs) { }
	}

	[DataContract]
	public class DirectoryInfo: FileObjectInfo {
		
		[DataMember]
		public List<FileObjectInfo> Entries = new List<FileObjectInfo>();

		public DirectoryInfo(string root, string path, bool getSizes, bool showThumbs, int entriesDepth): base(path, getSizes, showThumbs) {
			var user = Srvc.Persons.Current;
			if (user != null && entriesDepth != 0) {

				root = Srvc.Paths.Normalize(root);
				path = Srvc.Paths.Normalize(path);
				var fullpath = Srvc.Paths.Combine(root, path);
				if (root == Srvc.Files.UserFilesFolder || root.StartsWith(Srvc.Files.UserFilesFolder + "/")) {
					if (fullpath != user.HomePath && !fullpath.StartsWith(user.HomePath + "/")) return;
				}

				var usershome = HostingEnvironment.MapPath(Srvc.Files.UserFilesFolder);
				if (string.IsNullOrEmpty(path) || path == "~" || path == "/" || path == "~/") {
					var home = user.HomePath;
					if (user.IsInRole(Srvc.Files.FullFileSystemAccessRole)) {
						root = "";
						Entries.Add(new DirectoryInfo(root, Srvc.Files.PublicUserFilesFolder, getSizes, showThumbs, entriesDepth - 1));
						Entries.Add(new DirectoryInfo(root, home, getSizes, showThumbs, entriesDepth - 1));

					} else {
						root = home;
						Entries.Add(new DirectoryInfo(root, Srvc.Files.PublicUserFilesFolder, getSizes, showThumbs, entriesDepth - 1));
					}
				}
				var mpath = HostingEnvironment.MapPath(Srvc.Paths.Combine(root, path));
				var i = mpath.Length - path.Length;
				if (entriesDepth != 1) {
					Entries.AddRange(Directory.EnumerateDirectories(mpath, "*.*").Select(f => new DirectoryInfo(root, f.Substring(i).Replace('\\', '/'), getSizes, showThumbs, entriesDepth - 1)));
					Entries.AddRange(Directory.EnumerateFiles(mpath, "*.*").Select(f => new FileInfo(f.Substring(i).Replace('\\', '/'), getSizes, showThumbs)));
				}
			}
		}
	}

	[DataContract]
	public class Text: Message {
		[DataMember]
		public string Content;
	}	
}