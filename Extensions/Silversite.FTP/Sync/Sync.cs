using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Silversite.FtpSync {

    public class Sync {

        static readonly TimeSpan dt = TimeSpan.FromMinutes(1); // minimal file time resolution

		public Services.Sync.Mode Mode { get; set; }
        public Log Log { get; set; }
		public bool Verbose { get; set; }
		public string ExcludePatterns { get; set; }
		public FtpConnections FtpConnections { get; set; }
		public bool UseFXP { get { return false; } }

		public Sync(Services.Sync.Mode mode, bool verbose, string excludePatterns, string logfile) {
			Log = new Log(logfile, this);
			FtpConnections = new FtpConnections(this);
		}

		class FailureInfo { public FileOrDirectory File; public Exception Exception; }

		Queue<FailureInfo> Failures = new Queue<FailureInfo>();

		public void Failure(FileOrDirectory file, Exception ex) { lock (Failures) Failures.Enqueue(new FailureInfo { File=file, Exception = ex }); Log.Exception(ex); }
		public void Failure(FileOrDirectory file, Exception ex, FtpClient ftp) { lock (Failures) Failures.Enqueue(new FailureInfo { File=file, Exception = ex }); Log.Exception(ftp, ex); }

		IDirectory Root(FileOrDirectory fd) { if (fd.Parent == null) return (IDirectory)fd; else return Root(fd.Parent); }

		List<FailureInfo> MyFailures(IDirectory sroot, IDirectory droot) {
			List<FailureInfo> list = new List<FailureInfo>();

			lock (Failures) {
				int n = Failures.Count;
				while (n-- > 0) {
					var failure = Failures.Dequeue();
					var root = Root(failure.File);
					if (root == sroot || root == droot) list.Add(failure);
					else Failures.Enqueue(failure);
				}
			}
			return list;
		}

		public void RetryFailures(IDirectory sroot, IDirectory droot) {

			var list = MyFailures(sroot, droot);

			if (list.Count > 0) {
				Log.YellowText("####    Retry failed transfers...");
				var set = new HashSet<IDirectory>();
				foreach (var failure in list) {
					IDirectory dir;
					if (failure is IDirectory) dir = (IDirectory)failure.File;
					else dir = (IDirectory)failure.File.Parent;

					if (!set.Contains(dir.Source)) {
						set.Add(dir.Source);
						Directory(dir.Source, dir.Destination);
					}
				}
			}

			Log.Errors = 0;

			if (Failures.Count > 0) {

				Log.YellowText("####    Summary of errors:");
				foreach (var failure in Failures) Log.Exception(failure.Exception);

				Log.Errors = Failures.Count;

				Log.YellowText("####    Failed transfers:");
				foreach (var failure in Failures) Log.RedText("Failed transfer: " + failure.File.RelativePath);
			}

			list = MyFailures(sroot, droot); // dequeue recurrant failures.
		}

        public void Directory(IDirectory sdir, IDirectory ddir) {

			if (ddir == null || sdir == null) return;

			sdir.Source = ddir.Source = sdir;
			sdir.Destination = ddir.Destination = ddir;

			int con;

			if (sdir is LocalDirectory && ddir is LocalDirectory) con = 1;
			else {
				con = Math.Max(FtpConnections.Count(true, sdir.Url), FtpConnections.Count(false, ddir.Url));
				if (ddir is FtpDirectory) {
					((FtpDirectory)ddir).TransferProgress = true;
				} else if (sdir is FtpDirectory) {
					((FtpDirectory)sdir).TransferProgress = true;
				}
			}
			if (con == 0) con = 1;
			var list = sdir.List().Where(file => !Silversite.Services.Paths.Match(ExcludePatterns, file.RelativePath)).ToList();
			var dlist = ddir.List();
			//ddir.CreateDirectory(null);
			
			Parallel.ForEach<FileOrDirectory>(list, new ParallelOptions { MaxDegreeOfParallelism = con }, 
				(src) => {
					FileOrDirectory dest = null;
					lock(dlist) { if (dlist.Contains(src.Name)) dest = dlist[src.Name]; }
					if (dest != null && dest.Class != src.Class && (src.Changed > dest.Changed || Mode == Silversite.Services.Sync.Mode.Clone)) {
						ddir.Delete(dest);
						dest = null;
					}
					if (src.Class == ObjectClass.File) { // src is a file
						if (dest == null
							|| ((Mode == Silversite.Services.Sync.Mode.Update || Mode == Silversite.Services.Sync.Mode.Add) && src.Changed > dest.Changed)
							|| (Mode == Silversite.Services.Sync.Mode.Clone && (src.Changed > dest.Changed + dt))) {
							var s = sdir.ReadFile(src);
							if (s != null) {
								using (s) {
									ddir.WriteFile(s, src);
								}
							}
						}
					} else { // src is a directory
						if (dest == null) Directory((IDirectory)src, ddir.CreateDirectory(src));
						else Directory((IDirectory)src, (IDirectory)dest);
					}
					lock (dlist) { dlist.Remove(src.Name); }
				});
			if (Mode != Silversite.Services.Sync.Mode.Add) {
				foreach (var dest in dlist) ddir.Delete(dest);
			}
		}

        public void Directory(Uri src, Uri dest) {
            try {
                var start = DateTime.Now;
                FtpConnections.Allocate(true, src);
                FtpConnections.Allocate(false, dest);

                // messages
                if (src.Scheme == "ftp" || src.Scheme == "ftps") {
                    var ftp = FtpConnections.Open(true, ref src);
                    Log.Text("Source host: " + src.Authority + "    Server Time:" + ftp.ServerTimeString);
                    FtpConnections.Pass(ftp);
                }
                if (dest.Scheme == "ftp" || dest.Scheme == "ftps") {
                    var ftp = FtpConnections.Open(false, ref dest);
                    Log.Text("Destination host: " + dest.Authority + "    Server Time:" + ftp.ServerTimeString);
                    FtpConnections.Pass(ftp);
                }

                Log.Text(string.Format("Mode: {0}; Log: {1}; Verbose: {2}, Exclude: {3}", Mode, Log, Verbose, ExcludePatterns));
                Log.Text("");
			
				var sdir = Silversite.FtpSync.Directory.Parse(this, src);
				var ddir = Silversite.FtpSync.Directory.Parse(this, dest);
                
				Directory(sdir, ddir);

				RetryFailures(sdir, ddir);

                FtpConnections.Close();

                Log.Summary(DateTime.Now - start);
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }
    }
}
