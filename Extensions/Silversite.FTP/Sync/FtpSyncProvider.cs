using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Silversite.Services.FtpSync {

	public class SilversiteSyncProvider: SyncProvider {
		public override void Files(Uri source, Uri dest, Silversite.Services.Sync.Mode mode, string logfile, string excludePatterns, bool verbose) {
			var sync = new Silversite.FtpSync.Sync(mode, verbose, excludePatterns, logfile);
			sync.Directory(source, dest);
		}
	}

}