using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Silversite.Services;

namespace Silversite.FtpSync {

	public class Log {

		public Sync Sync { get; set; }

		public string LogFile { get; set; }

		public int Uploads = 0;
		public int Downloads = 0;
		public long UploadSize = 0;
		public long DownloadSize = 0;
		public int Errors = 0;

		const int KB = 1024;
		const int MB = 1024 * KB;
		const int GB = 1024 * MB;
		const int MaxLogSize = 10*MB;

		public static object Lock = new object();
		bool checkDir = true;

		StringBuilder buffer = new StringBuilder();

		public Log(string file, Sync sync) { Sync = sync; LogFile = file; }

		public string Size(long size) {
			if (size > GB) return string.Format("{0:F2} GB", size / (1.0 * GB));
			if (size > 100 * KB) return string.Format("{0:F2} MB", size / (1.0 * MB));
			return string.Format("{0:F0} KB", size / (1.0 * KB));
		}

		public void Debug(string text) {
			if (Sync.Verbose) Text(text);
		}

		public void Flush() {
			try {
				lock (Lock) {
					if (!string.IsNullOrEmpty(LogFile)) {
						if (checkDir) {
							checkDir = false;
							var dir = Paths.Directory(LogFile);
							if (!Files.DirectoryExists(dir)) Files.CreateDirectory(dir);
						}

						var log = Files.FileInfo(LogFile);
						if (log.Exists && log.Length > MaxLogSize) {
							var loglines = Files.LoadLines(LogFile);
							loglines.RemoveRange(0, loglines.Count / 2);
							Files.SaveLines(loglines, LogFile);
						}

						Files.Append(Paths.Map(LogFile), buffer.ToString());

						Silversite.Services.Log.Write("FTP-Sync", buffer.Insert(0, "<div class='Silversite_Log_BigText'>").Append("</div>").ToString());

						buffer.Remove(0, buffer.Length);
					}
				}
			} catch (Exception ex) {
				Console.WriteLine("Error writing to the logfile " + Sync.Log);
				Console.WriteLine(ex.Message);
			}
		}

		public void LogText(string text, bool newline) {
			lock (Lock) {
				if (Sync.Log != null) {
					try {
						if (newline) text = text + "\r\n";
						buffer.Append(text);
					} catch (Exception ex) {
						Console.WriteLine("Error writing to the logfile " + Sync.Log);
						Console.WriteLine(ex.Message);
					}
				}
			}
		}
		
		public void Text(string text) { lock(Lock) { Console.WriteLine(text); LogText(text, true); } }
		public void RedText(string text) { lock (Lock) { var oldc = Console.ForegroundColor; Console.ForegroundColor = ConsoleColor.Red; Text(text); Console.ForegroundColor = oldc; } }
		public void CyanText(string text) { lock (Lock) { var oldc = Console.ForegroundColor; Console.ForegroundColor = ConsoleColor.Cyan; Text(text); Console.ForegroundColor = oldc; } }
		public void GreenText(string text) { lock (Lock) { var oldc = Console.ForegroundColor; Console.ForegroundColor = ConsoleColor.Green; Text(text); Console.ForegroundColor = oldc; } }
		public void YellowText(string text) { lock (Lock) { var oldc = Console.ForegroundColor; Console.ForegroundColor = ConsoleColor.Yellow; Text(text); Console.ForegroundColor = oldc; } }
		public void YellowLabel(string text) { lock (Lock) { var oldc = Console.ForegroundColor; Console.ForegroundColor = ConsoleColor.Yellow; Console.Write(text); LogText(text, false); Console.ForegroundColor = oldc; } }
		public void RedLabel(string text) { lock (Lock) { var oldc = Console.ForegroundColor; Console.ForegroundColor = ConsoleColor.Red; Console.Write(text); LogText(text, false); Console.ForegroundColor = oldc; } }
		public void Label(string text) { lock (Lock) { Console.Write(text); LogText(text, false); } }

		public void Exception(Exception e) { lock (Lock) { Errors++; RedText("Error"); RedText(e.Message); if (Sync.Verbose) { RedText(e.StackTrace); } /* System.Diagnostics.Debugger.Break(); */ } }
		public void Exception(FtpClient ftp, Exception e) {
			if (ftp == null) Exception(e);
			else {
				lock (Lock) {
					var prefix = "FTP" + ftp.Index + "! ";
					Errors++; RedLabel(prefix);  RedText("Error");
					var lines = e.Message.Split('\n');
					foreach (var line in lines) { RedLabel(prefix); RedText(line); }
					if (Sync.Verbose) {
						lines = e.StackTrace.Split('\n');
						foreach (var line in lines) { RedLabel(prefix); RedText(line); }
					}
					// System.Diagnostics.Debugger.Break();
				}
			}
		}
		public void Upload(string path, long size, TimeSpan time) { GreenText(string.Format("Uploaded {0}    =>    {1} at {2:F3}/s.", path, Size(size), Size((long)(size / time.TotalSeconds + 0.5)))); Uploads++; UploadSize += size; }
		public void Download(string path, long size, TimeSpan time) { GreenText(string.Format("Downloaded {0}    =>    {1} at {2:F3}/s.", path, Size(size), Size((long)(size / time.TotalSeconds + 0.5)))); Downloads++; DownloadSize += size; }
		public void Progress(string path, long size, long part, TimeSpan time) { CyanText(string.Format("Transfer of {0}    =>    {1:F1}% at {2:F3}/s.", path, (part*100.0 / size), Size((long)(part / time.TotalSeconds + 0.5)))); }


		public void Summary(TimeSpan t) {
			Text("");
			GreenText(string.Format("####    =>    {0} Files and {1} transfered in {2:F3} seconds at {3}/s. {4} Errors.",
				Math.Max(Uploads, Downloads), Size(UploadSize + DownloadSize), t.TotalSeconds, Size((long)(Math.Max(UploadSize, DownloadSize) / t.TotalSeconds + 0.5)), Errors));
			Text("");
			Text("");
			Text("");

			Flush();
		}

	}
}
