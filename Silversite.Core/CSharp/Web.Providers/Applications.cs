using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Migrations;

namespace Silversite.Web.Providers {

	public class Application {

		const int NoKey = default(int);

		const string StandardApplication = "_SilversiteStandardApplication";

		public Application() { Name = StandardApplication; }

		[Key]
		public int Key { get; set; }
		[Required, MaxLength(255)]
		public string Name { get; set; }

		public static implicit operator Application(string name) { return new Application { Name = name }; }
		public static implicit operator string(Application app) { return app.Name; }
		public override bool Equals(object obj) { return (Name ?? "").Equals((string)(Application)obj); }
		public override int GetHashCode() { return (Name ?? "").GetHashCode(); }
		public override string ToString() { return (Name ?? "").ToString(); }

		//public static bool operator ==(Application a, Application b) { return (((object)a == null || (object)b == null) && (object)a == (object)b) || (a.Key != NoKey && a.Key == b.Key) || a.Name == b.Name; }
		//public static bool operator !=(Application a, Application b) { return (((object)a == null || (object)b == null) && (object)a != (object)b) || (a.Key != NoKey && b.Key != NoKey && a.Key != b.Key) || a.Name != b.Name; }

		internal static Dictionary<Type, Application> current = new Dictionary<Type, Application>();

		public static Application Current<T>(Context db = null) {
			Application app = null;
			if (!current.TryGetValue(typeof(T), out app)) app = This(StandardApplication);
			if (app.Key == NoKey) app = This(app.Name);
			current[typeof(T)] = app;
			if (db != null) db.Applications.Attach(app);
			return app;
		}

		public static void Set<T>(string name) {
			Application app = null;
			if (!current.TryGetValue(typeof(T), out app)) app = current[typeof(T)] = new Application();
			app.Name = string.IsNullOrWhiteSpace(name) ? StandardApplication : name;
			using (var db = new Context()) app = Current<T>();
		}

		public static Application This(string name) {
			using (var db = new Context()) {
				var app = db.Applications.FirstOrDefault(a => a.Name == name);
				if (app == null) {
					try {
						app = new Application { Name = name };
						db.Applications.Add(app);
						db.SaveChanges();
						Services.Log.Write("Debug", "Created new Application {0}.", name);
					} catch (System.Data.UpdateException) {
						app = db.Applications.FirstOrDefault(a => a.Name == name);
					}
				}
				db.Detach(app);
				return app;
			}
		}
	}

}