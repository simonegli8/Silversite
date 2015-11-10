using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silversite.FtpSync {

	public class Directory {

		public static IDirectory Parse(Sync sync, Uri url) {
			if (url.IsFile || !url.ToString().Contains(':')) return new LocalDirectory(sync, null, url);
			else return new FtpDirectory(sync, null, url);
		}

	}
}
