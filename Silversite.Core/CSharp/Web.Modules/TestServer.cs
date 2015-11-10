using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Security;
using System.Web.Hosting;
using System.Configuration;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using System.Text;

[assembly: Silversite.Services.DependsOn(typeof(Silversite.Services.TestServer))]

namespace Silversite.Services {

	public class TestServer : IAutostart {

		static bool setup = false;
		static object Lock = new object();
		static void Setup() {
			bool s = false;
			lock (Lock) { s = setup; setup = true; }
			if (!s) {
				TestServers = new DevelopConfiguration().TestServers;
				var host = new Uri(Paths.Home).Host.ToLower();
				var machine = MachineName.ToLower();
				IsTestServer = Paths.Match(TestServers.ToLower(), host) || TestServers.ToLower().Tokens().Any(server => server == machine);
			}
		}

		static string testServers;
		public static string TestServers { get { Setup(); return testServers; } set { testServers = value; } }

		static bool isTestServer;
		public static bool IsTestServer { get { Setup(); return isTestServer; } private set { isTestServer = value; } }
		public static bool IsLiveServer { get { return !IsTestServer; } }

		public static string MachineName { get { return System.Environment.MachineName; } }

		public void Startup() {
			// set web.config to either debug or release.
			/*if (Files.FileExists("~/web.debug.config") && Files.FileExists("~/web.release.config")) {
				var wconf = Files.Load("~/web.config");
				var wconfx = new XmlDocument();
				wconfx.LoadXml(wconf);
				var wdebugtt = new XslTransform();
				var wreleasett = new XslTransform();
				wdebugtt.Load(Paths.Map("~/web.debug.config"));
				wreleasett.Load(Paths.Map("~/web.release.config"));
				var newconfsb = new StringBuilder();
				if (IsTestServer) {
					wdebugtt.Transform(wconfx, new XsltArgumentList(), new StringWriter(newconfsb));
					var newconf = newconfsb.ToString();
					if (newconf != wconf) Files.Save(newconf, "~/web.config");
					Log.Write("Administration", "Changed web.config to debug version.");
				} else {
					wreleasett.Transform(wconfx, new XsltArgumentList(), new StringWriter(newconfsb));
					var newconf = newconfsb.ToString();
					if (newconf != wconf) Files.Save(newconf, "~/web.config");
					Log.Write("Administration", "Changed web.config to release version.");
				}
			}*/
		}
		public void Shutdown() {
		}

	}
}