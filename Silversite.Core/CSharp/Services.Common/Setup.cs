using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

[assembly: Silversite.Services.DependsOn(typeof(Silversite.Services.Setup))]

namespace Silversite.Services {

	[Configuration.Section(Path=ConfigPath)]
	public class SetupConfiguartion: Configuration.Section {
		public const string ConfigPath = Configuration.Section.ConfigRoot + "/Silversite.config";

		[ConfigurationProperty("start", IsRequired = false, DefaultValue = false)]
		public bool Start { get { return (bool)(this["start"] ?? true); } set { this["start"] = value; } }

	}

	public class Setup: IAutostart, Web.IHttpAutoModule {

		static bool setup = false;
		SetupConfiguartion configuration;

		public void Startup() {
			Modules.DependsOn<Lazy>();

			if (!Files.DirectoryExists("~/Silversite")) {
				Installer.Install(new Uri("http://store.silversite.org/packages/Silversite.Setup.dll"));
				setup = true;
			}
		}

		public void Shutdown() { }
		
		public void  Dispose() { }
		
		public void  Init(HttpApplication context) {
			try {
				if (setup && context.Response != null) {
					setup = false;
					context.Response.Redirect("~/Silversite/Setup?return=" + HttpUtility.HtmlEncode(context.Request.RawUrl));
				}
			} catch (Exception ex) {
			}
		}
	}
}