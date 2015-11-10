using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

[assembly: Silversite.Services.DependsOn(typeof(Silversite.Web.DomainRewriteModule))]

namespace Silversite.Web {

	/// <summary>
	/// Rewrites requests for domain specific files.
	/// </summary>
	public class DomainRewriteModule: IHttpAutoModule {

		/// <summary>
		/// Initializes the module.
		/// </summary>
		/// <param name="app">The HttpApplication.</param>
		public void Init(HttpApplication app) {
			app.BeginRequest += RewritePath;
		}

		/// <summary>
		/// Disposes the module.
		/// </summary>
		public void Dispose() { }

		/// <summary>
		/// The handler for BeginRequest, that rewrites the path if there is a file either in the domain root path or outside the domain root path. Files in the domain root path have precedence over files outside.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The event arguments.</param>
		public void RewritePath(object sender, EventArgs e) {
			var app = (HttpApplication)sender;
			var path = app.Request.AppRelativeCurrentExecutionFilePath;
			string rewritepath;
			if (Services.Paths.IsDomains(path)) {
				if (Services.Files.ExistsVirtual(path)) return;
				rewritepath = Services.Paths.NoDomains(path);
			} else {
				rewritepath = Services.Domains.Path(path);
			}
			if (Services.Files.ExistsVirtual(rewritepath)) app.Context.RewritePath(rewritepath, false);
		}
	}
}