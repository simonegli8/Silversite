
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace Silversite.Services {

	[Configuration.Section(Path = ConfigPath, Name = "development")]
	public class DevelopConfiguration: Configuration.Section {

		const string DefaultAdminUser = "";
		public const string ConfigPath = ConfigRoot + "/Silversite.config";

		[ConfigurationProperty("autoLogin", IsRequired=false, DefaultValue=DefaultAdminUser)]
		public string AutoLogin { get { return this["autoLogin"] as string ?? DefaultAdminUser; } set { this["autoLogin"] = value; } }

		[ConfigurationProperty("testServers", IsRequired=false, DefaultValue="")]
		public string TestServers { get { return this["testServers"] as string ?? ""; } set { this["testServers"] = value; } }

	}

}