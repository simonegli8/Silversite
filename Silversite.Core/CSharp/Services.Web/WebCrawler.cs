using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

namespace Silversite.Services {

	public class CompanyCategory {
		[Key]
		public int Key { get; set; }
		[MaxLength(128)]
		public string Name { get; set; }
	}

	public class WebAddress : Person {
		public WebAddress() : base() { Documents = new List<Document>(); Categories = new List<CompanyCategory>(); }
		public ICollection<CompanyCategory> Categories { get; set; }
		/*public override ICollection<Document> Documents {
			get { return new List<Document>(); }
			protected set {	}
		}*/
	}

	public class WebCrawlerContext: Data.Context<WebCrawlerContext> {

		public WebCrawlerContext(): base() { }
		public WebCrawlerContext(Data.Database db): base(db) { }

		public DbSet<CompanyCategory> Categories { get; set; }
		public DbSet<WebAddress> Addresses { get; set; }

		protected override void OnModelCreating(DbModelBuilder model) {

			DependsOn<Silversite.Context>();

			model.Entity<WebAddress>()
				.Map(m => m.MapInheritedProperties());

			base.OnModelCreating(model);
		}
	}

	public class WebCrawler: StaticService<WebCrawler, WebCrawlerProvider> {

		public static WebCrawlerContext Context { get { return /* Data.Context.Open<WebCrawlerContext>(); */ new WebCrawlerContext(); } }
		public static DbSet<WebAddress> Addresses { get { return Context.Addresses; } } 

		public static void FindCompanies(string categories, string countries, string states = null, string cities = null, string zips = null) {
			
			const int N = 200;

			using (var db = new WebCrawlerContext()) {
				db.Configuration.AutoDetectChangesEnabled = false;
				Providers.Registered
					.OfType<WebCrawlerProvider>()
					.AwaitAll(p => {
						var buf = new List<WebAddress>();
						foreach (var adr in p.FindCompanies(categories, countries, states, cities, zips)) {
							buf.Add(adr);
							if (buf.Count > N) {
								lock (db) {
									db.Addresses.AddRange(buf);
									db.SaveChanges();
								}
								buf.Clear();
							}
						}
					});
			}
		}

	}

	public abstract class WebCrawlerProvider : Provider<WebCrawler> {
		public abstract IEnumerable<WebAddress> FindCompanies(string categories, string countries, string states = null, string cities = null, string zips = null);
	}
}