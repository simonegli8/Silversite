using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Silversite.Services.Ftp;

namespace Silversite.FtpSync {

	public class FtpClient: Starksoft.Net.Ftp.FtpClient {

		public FtpClient(Sync sync, string host, int port, FtpSecurityProtocol protocol, int Index, bool isSource) : base(host, port, protocol) { Sync = sync; this.Index = Index; this.IsSource = isSource; SupportsFXP = false; }

		public int Index { get; set; }

		public bool IsSource { get; set; }

		public Sync Sync { get; set; }
		public bool SupportsFXP { get; set; }

		public void Pass() { Sync.FtpConnections.Pass(this); }
	}
}
