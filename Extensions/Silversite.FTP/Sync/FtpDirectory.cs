using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Starksoft.Net.Ftp;
using Silversite.Services.Ftp;
//using FtpItemType = Silversite.Services.Ftp.ItemType;
//sing FileAction = Silversite.Services.Ftp.FileAction

namespace Silversite.FtpSync {

	public class FtpDirectory: FileOrDirectory, IDirectory {

        Uri url;
        public Uri Url { get { return url; } set { url = value; } }

		public Sync Sync { get; set; }
		public Log Log { get { return Sync.Log; } }
		public bool UseFXP { get { return Sync.UseFXP; } }
		public FtpConnections FtpConnections { get { return Sync.FtpConnections; } }

		public bool TransferProgress { get; set; }

		public FtpDirectory(Sync sync, FileOrDirectory parent, Uri url) {
			Sync = sync;
            Parent = parent;
			if (url.Scheme != "ftp" && url.Scheme != "ftps") throw new NotSupportedException();
			Url = url;
			Name = url.File();
			Class = ObjectClass.Directory;
			Changed = DateTime.Now.AddDays(2);
			if (parent is FtpDirectory) TransferProgress = ((FtpDirectory)parent).TransferProgress;
		}

		public IDirectory Source { get; set; }
		public IDirectory Destination { get; set; }
		public bool IsSource { get { return this == Source; } }

		bool UseCompression { get { return Url.Query.Contains("compress"); } }

		public DirectoryListing List() {
			FtpClient ftp = null;
			try {
				ftp = FtpConnections.Open(IsSource, ref url);
				//if (ftp.FileTransferType != TransferType.Ascii) ftp.FileTransferType = TransferType.Ascii;
				var list = ftp.GetDirList()
					.Select(fi =>
						fi.ItemType == FtpItemType.Directory 
						? new FtpDirectory(Sync, this, Url.Relative(fi.Name))
						: new FileOrDirectory { Name = fi.Name, Class = ObjectClass.File, Changed = fi.Modified, Size = fi.Size, Parent = this })
					.ToList();
				return new DirectoryListing(list);
			} catch (Exception ex) {
				Sync.Failure(this, ex, ftp);
			} finally {
				if (ftp != null) FtpConnections.Pass(ftp);
			}
			return new DirectoryListing();
		}

		static readonly TimeSpan Interval = TimeSpan.FromSeconds(10);

		class ProgressData {
			public string Path;
			public long Size;
			public long Transferred;
			public TimeSpan ElapsedTime;
		}

		Dictionary<FtpClient, ProgressData> progress = new Dictionary<FtpClient,ProgressData>();
		public void ShowProgress(object sender, TransferProgressEventArgs a) {
			if (TransferProgress) {
				var ftp = (FtpClient)sender;
				var p = progress[ftp];
				p.Transferred += a.BytesTransferred;
				if (a.ElapsedTime - p.ElapsedTime > Interval) {
					Log.Progress(p.Path, p.Size, p.Transferred, a.ElapsedTime);
					p.ElapsedTime = a.ElapsedTime;
				}
			}	
		}

		public void WriteFile(System.IO.Stream file, FileOrDirectory src) {
			FtpClient ftp = null;
			try {
				if (file == null) return;

				ftp = FtpConnections.Open(IsSource, ref url);

				var path = Url.Path() + "/" + src.Name;
				var start = DateTime.Now;
				
				if (!UseFXP) {
					if (ftp.FileTransferType != TransferType.Binary) ftp.FileTransferType = TransferType.Binary;
					if (TransferProgress) {
						progress[ftp] = new ProgressData { ElapsedTime = new TimeSpan(0), Path = path, Size = src.Size };
						ftp.TransferProgress += ShowProgress;
					}
					ftp.PutFile(file, src.Name, FileAction.Create);
				} else { // use FXP for direct server to server transfer.
					var srcftp = (FtpStream)file;
					srcftp.Client.FxpCopy(src.Name, ftp);
				}
				ftp.SetDateTime(src.Name, src.ChangedUtc);
			
				Log.Upload(path, src.Size, DateTime.Now - start);
			} catch (Exception e) {
				Sync.Failure(src, e, ftp);
			} finally {
				if (ftp != null) {
					if (TransferProgress) {
						ftp.TransferProgress -= ShowProgress;
						progress.Remove(ftp);
					}
					FtpConnections.Pass(ftp);
				}
			}
		}

		public System.IO.Stream ReadFile(FileOrDirectory src) {
			FtpClient ftp = null;
			Task task = null;
			try {
				ftp = FtpConnections.Open(IsSource, ref url);
				if (ftp.FileTransferType != TransferType.Binary) ftp.FileTransferType = TransferType.Binary;
				var file = new FtpStream();
				file.Client = ftp;
				file.Path = Url.Path() + "/" + src.Name;
				file.Size = src.Size;
				file.File = src;
				file.Log = Log;
				if (!UseFXP) {
					task = Services.Tasks.Do(() => {
						using (file) {
							try {
								if (TransferProgress) {
									progress[file.Client] = new ProgressData { ElapsedTime = new TimeSpan(0), Path = file.Path, Size = file.Size };
									file.Client.TransferProgress += ShowProgress;
								}
								file.Start = DateTime.Now;
								ftp.GetFile(file.File.Name, file, false);
							} catch (Exception ex) {
								file.Exception(ex);
								Sync.Failure(file.File, ex, file.Client);
							} finally {
								if (TransferProgress) {
									file.Client.TransferProgress -= ShowProgress;
									progress.Remove(file.Client);
								}
							}
						}
					});
				}
				return file;
			} catch (Exception e) {
				Sync.Failure(src, e, ftp);
			} finally {
				if (task == null && ftp != null) FtpConnections.Pass(ftp);
			}
			return null;
		}

		public void DeleteFile(FileOrDirectory dest) {
			FtpClient ftp = null;
			try {
				ftp = FtpConnections.Open(IsSource, ref url);
				ftp.DeleteFile(dest.Name);
			} catch (Exception ex) {
				Sync.Failure(dest, ex, ftp);
			} finally {
				if (ftp != null) FtpConnections.Pass(ftp);
			}
		}

		public void DeleteDirectory(FileOrDirectory dest) {
			FtpClient ftp = null;
			try {
				var dir = (FtpDirectory)dest;
				int con = FtpConnections.Count(IsSource, dir.url);
				if (con == 0) con = 1;
				var list = dir.List();

				Parallel.ForEach<FtpDirectory>(list.OfType<FtpDirectory>(), new ParallelOptions { MaxDegreeOfParallelism = con }, (d) => { d.DeleteDirectory(d); });

				ftp = FtpConnections.Open(IsSource, ref dir.url);

				con = FtpConnections.Count(IsSource, dir.url);
				if (con == 0) con = 1;

				foreach (var file in list.Where(f => f.Class == ObjectClass.File)) ftp.DeleteFile(file.Name);

				ftp.ChangeDirectory(ftp.CorrectPath("/" + dest.RelativePath));
				ftp.ChangeDirectoryUp();
				ftp.DeleteDirectory(dest.Name);
			} catch (Exception ex) {
				Sync.Failure(dest, ex, ftp);
			} finally {
				if (ftp != null) FtpConnections.Pass(ftp);
			}
		}

		public void Delete(FileOrDirectory dest) {
			if (dest.Class == ObjectClass.File) DeleteFile(dest);
			else DeleteDirectory(dest);
		}

		public IDirectory CreateDirectory(FileOrDirectory dest) {
			FtpClient ftp = null;
			try {
				ftp = FtpConnections.Open(IsSource, ref url);
			
				//var path = ftp.CorrectPath(Url.Path());
				//if (dest != null) path = path + "/" + dest.Name;
				//var curpath = ftp.CurrentDirectory;
				//var ps = path.Split('/');
				//var cs = curpath.Split('/');
				//var j = cs.Length-1;	
                //var i = Math.Min(ps.Length, j+1);
                //while (j > i-1) { ftp.ChangeDirectoryUp(); j--; }
                //while (j > 0 && ps[j] != cs[j]) { ftp.ChangeDirectoryUp(); j--; i = j+1; }
				
				//while (i < ps.Length) { str.Append("/"); str.Append(ps[i++]); }

				//var dir = str.ToString();
				ftp.MakeDirectory(dest.Name);
				
				//if (url.Query()["old"] != null) ftp.ChangeDirectoryMultiPath(path);
				//else ftp.ChangeDirectory(path);

				if (dest != null) return new FtpDirectory(Sync, this, Url.Relative(dest.Name));
				else return this;
			} catch (Exception ex) {
				Sync.Failure(dest, ex, ftp);
			} finally {
				if (ftp != null) FtpConnections.Pass(ftp);
			}
			return null;
		}

	}
}
