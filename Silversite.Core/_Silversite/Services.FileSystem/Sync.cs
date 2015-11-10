using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Silversite.Services {

	public class Sync: StaticService<Sync, SyncProvider> {

		public enum Mode { Clone, Add, Update };
		/// <summary>
		/// Usage: Files(sourceurl,desturl, [SyncMode.Update | SyncMode.Clone | SyncMode.Add],  logfilename);
		/// - Urls can be either ftp urls or local paths
		/// - Ftp urls are of the form protocol://username:password@server:port/path?ftp-options
		///   Ftp urls must be url encoded. For example a space must be written as %20.
		/// - Protocol can be either ftp or ftps
		/// - Ftp-options are delimited by a & and are of the form parameter or parameter=value
		/// - Available ftp-options are passive (passive ftp mode), active (active ftp mode), connections
		///   (number of concurrent connections), zip for compression if the server supports it, raw
		///   for no compression, and time for the server's time offset to utc time. If you omit time, sync
		///   will autodetect the time offset.
		/// - The connections option reqires an int value that limits the maximum concurent connections.
		/// - The default options are active&zip&connections=10
		/// - The Update mode tells sync to keep newer files in the destination.
		/// - The Clone mode tells sync to clone the source and discard all changes in the destination. This is the default.
		/// - The Add mode tells sync to add all files that are not present or outdated in the destination, but not
		///   to delete any files or overwrite newer files.
		/// - You can redirect output to a logfile by specifying a logfile name.
		/// </summary>
		public static void Files(Uri source, Uri dest, Mode mode, string logfile, string excludePaths = "", bool verbose = false) { Provider.Files(source, dest, mode, logfile, excludePaths, verbose); }
	}

	public abstract class SyncProvider: Provider<Sync> {
		public abstract void Files(Uri source, Uri dest, Sync.Mode mode, string logfile, string excludePaths, bool verbose);
	}

}