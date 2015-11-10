using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

[assembly: Silversite.Services.DependsOn(typeof(Silversite.Services.Newsletters))]

namespace Silversite.Services {

	public class Newsletters: StaticService<Newsletters, NewslettersProvider>, IAutostart {
		public void Startup() {
			Tasks.DoLater(30.Seconds(), () => {
				Modules.DependsOn<Providers>();
				Modules.DependsOn<Lazy>();
				Modules.DependsOn<Mail>();
				if (HasProvider) Provider.Startup();
			});
		}
		public void Shutdown() { if (HasProvider) Provider.Shutdown(); }
	}

	public abstract class NewslettersProvider : Provider<Newsletters> {
		public abstract void Startup();
		public abstract void Shutdown();
	}

}