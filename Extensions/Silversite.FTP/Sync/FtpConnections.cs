using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Starksoft.Net.Ftp;
using Silversite.Services;
using Silversite.Services.Ftp;

namespace Silversite.FtpSync {

	public class FtpConnections {

		public Sync Sync { get; set; }
		public Log Log { get { return Sync.Log; } }

		Dictionary<string, ResourceQueue<FtpClient>> Queue = new Dictionary<string, ResourceQueue<FtpClient>>();
		Dictionary<string, int?> TimeOffsets = new Dictionary<string, int?>();

		public FtpConnections(Sync sync) { Sync = sync; }

		string Key(FtpClient ftp) { return ftp.Host + ":" + ftp.Port.ToString() + (ftp.IsSource ? "S" : "D"); }
		string Key(bool isSource, Uri uri) { return uri.Host + ":" + uri.Port.ToString() + (isSource ? "S" : "D"); }

		int? Connections(Uri url) {
			var query = url.Query();
			int con;
			if (int.TryParse((query["connections"] ?? "").ToString(), out con)) return con;
			else return null;
		}

		string Proxy(Uri url) {
			var query = url.Query();
			string proxy = (query["proxy"] ?? "").ToString();
			return proxy;
		}

		int? TimeOffset(Uri url) {
			var query = url.Query();
			int zone = 0;
			string zonestr = (string)query["time"];
			if (string.IsNullOrEmpty(zonestr)) return null;
			zonestr = zonestr.ToLower();
			if (zonestr == "z" || zonestr == "utc") return 0;
			if (!int.TryParse(zonestr, out zone)) return null;
			return zone;
		}

		int clientIndex = 0;

		public FtpClient Open(bool isSource, ref Uri url) {
			var queue = Queue[Key(isSource, url)];
			var ftp = queue.DequeueOrBlock();
			try {
				if (ftp == null) {
					ftp = new Silversite.FtpSync.FtpClient(Sync, url.Host, url.Port, url.Scheme == "ftps" ? FtpSecurityProtocol.Tls1Explicit : FtpSecurityProtocol.None, ++clientIndex, isSource);
					//ftp.IsLoggingOn = Sync.Verbose;
					if (Sync.Verbose) {
						ftp.ClientRequest += new EventHandler<FtpRequestEventArgs>((sender, args) => {
							lock (Log.Lock) { Log.YellowLabel("FTP" + ftp.Index + "> "); Log.Text(args.Request.Text); }
						});
						ftp.ServerResponse += new EventHandler<FtpResponseEventArgs>((sender, args) => {
							lock (Log.Lock) { Log.Label("FTP" + ftp.Index + ": "); Log.Text(args.Response.Text); }
						});
					}
					if (url.Query()["passive"] != null || url.Query()["active"] == null) ftp.DataTransferMode = TransferMode.Passive;
					else ftp.DataTransferMode = TransferMode.Active;
					ftp.AutoChecksumValidation = HashingFunction.None;
					if (url.Query()["md5"] != null) ftp.AutoChecksumValidation = HashingFunction.Md5;
					else if (url.Query()["sha"] != null) ftp.AutoChecksumValidation = HashingFunction.Sha1;
					else if (url.Query()["crc"] != null) ftp.AutoChecksumValidation = HashingFunction.Crc32;
				} else {
					if (!ftp.IsConnected) ftp.Reopen();
				}
				if (!ftp.IsConnected) {
					if (!string.IsNullOrEmpty(url.UserInfo)) {
						if (url.UserInfo.Contains(':')) {
							var user = url.UserInfo.Split(':');
							ftp.Open(user[0], user[1]);
						} else {
							ftp.Open(url.UserInfo, string.Empty);
						}
					} else {
						ftp.Open("Anonymous", "anonymous");
					}
					// enable UTF8
					ftp.Quote("OPTS UTF8 ON");
				}

				// change path
				var path = url.Path();
				if (!path.StartsWith("/")) path = "/" + path;
				path = ftp.CorrectPath(path);
				if (url.Query()["raw"] != null && ftp.IsCompressionEnabled) ftp.CompressionOff();
				if (url.Query()["zip"] != null && ftp.IsCompressionEnabled) ftp.CompressionOn();
				if (ftp.CurrentDirectory != path) {
					try {
						if (url.Query()["old"] != null) ftp.ChangeDirectoryMultiPath(path);
						else ftp.ChangeDirectory(path);
					} catch (Exception ex) {
						ftp.MakeDirectory(path);
						if (url.Query()["old"] != null) ftp.ChangeDirectoryMultiPath(path);
						else ftp.ChangeDirectory(path);
					}
				}
				// get server local time offset
				var offset = TimeOffset(url);
				if (offset.HasValue) ftp.TimeOffset = offset;
				else if (!ftp.TimeOffset.HasValue) {
					lock (queue) {
						var offsetclient = queue.FirstOrDefault(client => client != null && client.TimeOffset.HasValue);
						if (offsetclient != null) ftp.TimeOffset = offsetclient.TimeOffset;
						ftp.TimeOffset = ftp.ServerTimeOffset;
					}
				}
			} catch (FtpDataConnectionException ex) {
				if (url.Query()["passive"] == null) {
					url = new Uri(url.ToString() + (url.Query().Count > 0 ? "&" : "%3F") + "passive");
					//ftp.Close();
					ftp.DataTransferMode = TransferMode.Passive;
					Pass(ftp);
					return Open(isSource, ref url);
				} else {
					Log.Exception(ex);
				}
			} catch (Exception e) {
				Log.Exception(e);
			}
			return ftp;
		}

		public void Pass(FtpClient client) {
			if (client != null) { }
			Queue[Key(client)].Enqueue(client);
		}

		public int Count(bool isSource, Uri url) { if (url.IsFile) return 1; else return Queue[Key(isSource, url)].Count; }

		public int Allocate(bool isSource, Uri url) {
			var key = Key(isSource, url);
			if (!url.IsFile) {
				Queue[key] = new ResourceQueue<FtpClient>();
				var n = Connections(url) ?? 10;
				var i = n;
				while (i-- > 0) Queue[key].Enqueue(null);
				return n;
			}
			return 1;
		}

		public void Close() {
			foreach (var queue in Queue.Values) {
				while (queue.Count > 0) {
					var ftp = queue.Dequeue();
					if (ftp != null) {
						if (ftp.IsConnected) ftp.Close();
						ftp.Dispose();
					}
				}
			}
		}
	}
}
