using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Silversite.EntityTest {

	public class Database {

		public static Silversite.Data.Database Connection {
			get {
				var settings = ConfigurationManager.ConnectionStrings["Silversite.EntityTest"];
				if (settings == null) return null;
				return Silversite.Data.Database.Custom(settings.ProviderName, settings.ConnectionString);
			}
		}

		public static void Fill() {

			using (var p = new PersonContext()) {
				p.Persons.Add(new Person { City="Basel", Name="David" });
				p.Persons.Add(new Person { City="Basel", Name="Bettina" });
				p.Persons.Add(new Person { City="Basel", Name="Sergio" });
				p.Persons.Add(new Person { City="Thun", Name="Kim" });
				p.SaveChanges();
			}

			Services.Providers.Message("Persons Filled.");

			using (var c = new CarContext()) {
				c.Cars.Add(new Car { Model = "Audi" });
				c.Cars.Add(new Car { Model = "BMW" });
				c.Cars.Add(new Car { Model = "Mercedes" });
				c.Cars.Add(new Car { Model = "Fiat" });
				c.SaveChanges();
			}

			Services.Providers.Message("Cars filled.");
		}

		public static void List() {
			Services.Providers.Message("Cars:");
			using (var c = new CarContext()) {
				foreach (var car in c.Cars) Services.Providers.Message(null, "Key: {0}; Model: {1}", car.Key, car.Model);
			}

			Services.Providers.Message("Persons:");
			using (var c = new PersonContext()) {
				foreach (var p in c.Persons) Services.Providers.Message(null, "Key: {0}; Name: {1}; City: {2}", p.Key, p.Name, p.City);
			}
		}
	 }
}
