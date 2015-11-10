using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Starksoft.Net.Ftp;


namespace Silversite.FtpSync {

	public class FtpStream: Silversite.Services.PipeStream {

		public FtpClient Client { get; set; }
		public string Path { get; set; }
		public long Size { get; set; }
		public DateTime Start;
		public FileOrDirectory File { get; set; }
		public Log Log { get; set; }
		public FtpStream() : base() { Start = DateTime.Now; }

		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
			Client.Pass();
			Log.Download(Path, Size, DateTime.Now - Start);
		}
	}

}
