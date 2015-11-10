﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;

namespace Silversite.FtpSync {

	class LocalDirectory: FileOrDirectory, IDirectory {

		public Sync Sync { get; set; }
		public Log Log { get { return Sync.Log; } }

		public Uri Url { get; set; }

		public string Path { get { return HttpUtility.UrlDecode(Url.LocalPath); } }

		public LocalDirectory(Sync sync, FileOrDirectory parent, Uri url) {
			Sync = sync;
            Parent = parent;
            if (!url.ToString().Contains(':')) {
                if (url.ToString() == ".") url = new Uri(Environment.CurrentDirectory);
                else url = new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, url.ToString()));
            }
			if (!url.IsFile) throw new NotSupportedException("url is no local file.");
			Url = url;
			var info = new DirectoryInfo(Path);
			Name = info.Name;
			Class = ObjectClass.Directory;
			ChangedUtc = info.LastWriteTimeUtc;
		}

		public IDirectory Source { get; set; }
		public IDirectory Destination { get; set; }
		public bool IsSource { get { return this == Source; } }

		public DirectoryListing List() {
			try {
				var info = new DirectoryInfo(Path);
				if (info.Exists) {
					var finfos = info.GetFileSystemInfos();
					var infos = info.GetFileSystemInfos()
						.Select(fi =>
							fi is FileInfo
								? new FileOrDirectory { Name = fi.Name, Class = ObjectClass.File, ChangedUtc = fi.LastWriteTimeUtc, Size = ((FileInfo)fi).Length, Parent = this }
								: new LocalDirectory(Sync, this, new Uri(fi.FullName)));
                    return new DirectoryListing(infos);
				} else {
					return new DirectoryListing();
				}
			} catch (Exception ex) {
				Sync.Failure(this, ex);
			}
			return new DirectoryListing();
		}

		public void WriteFile(Stream sstream, FileOrDirectory src) {
			if (sstream == null) return;
			try {
				var path = System.IO.Path.Combine(Path, src.Name);
				using (var dstream = File.Create(path)) {
					if (sstream is Silversite.Services.PipeStream) {
						((Silversite.Services.PipeStream)sstream).CopyTo(dstream);
					} else {
						Streams.Copy(sstream, dstream);
					}
				}
				File.SetLastAccessTimeUtc(path, src.ChangedUtc);
			} catch (Exception ex) {
				Sync.Failure(src, ex);
			}
		}

		public Stream ReadFile(FileOrDirectory src) {
			try {
				var path = System.IO.Path.Combine(Path, src.Name);
				return File.OpenRead(path);
			} catch (Exception ex) {
				Sync.Failure(src, ex);
			}
			return null;
		}

		public void DeleteFile(FileOrDirectory dest) {
			try {
				var path = System.IO.Path.Combine(Path, dest.Name);
				if (path != Sync.Log.LogFile) System.IO.File.Delete(path);
			} catch (Exception ex) {
				Sync.Failure(dest, ex);
			}
		}

		public void DeleteDirectory(FileOrDirectory dest) {
			try {
				System.IO.Directory.Delete(((LocalDirectory)dest).Path, true);
			} catch (Exception ex) {
				Sync.Failure(dest, ex);
			}
		}

		public void Delete(FileOrDirectory dest) {
			try {
				var path = System.IO.Path.Combine(Path, dest.Name);
				if (dest.Class == ObjectClass.File) {
					if (path != Sync.Log.LogFile) System.IO.File.Delete(path);
				}  else System.IO.Directory.Delete(path, true);
			} catch (Exception ex) {
				Sync.Failure(dest, ex);
			}
		}

		public IDirectory CreateDirectory(FileOrDirectory src) {
			try {
				string path;
				if (src == null) path = Path;
				else path = System.IO.Path.Combine(Path, src.Name);
				System.IO.Directory.CreateDirectory(path);
				return new LocalDirectory(Sync, this, new Uri(path));
			} catch (Exception ex) {
				Sync.Failure(src, ex);
			}
			return null;
		}
	}
}
