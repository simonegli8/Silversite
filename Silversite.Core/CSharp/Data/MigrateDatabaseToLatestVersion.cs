// davidegli

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Silversite.Data {

	/// <summary>
	/// The default initializer for all Data.Contexts. It creates the database if it does not exist, and applies all DbMigrations if the database schema is not cosistent with the Context Version.
	/// In order for this to work, the Context must override the Version property and the SetMigrations method that adds the migrations to the Migrations list.
	/// </summary>
	/// <typeparam name="T">The Context's type.</typeparam>
	public class MigrateDatabaseToLatestVersion<T>: IDatabaseInitializer<T> where T: Context {

		class Info {
			public bool hasDatabase = false;
			public bool triedDatabaseCreate = false;
			public bool testedIfDatabaseExists = false;
			public bool freshDatabase = false;
			public HashSet<Type> testedForVersion = new HashSet<Type>();
		}
		static Dictionary<Database, Info> dbs = new Dictionary<Database, Info>();
		static object Lock = new object();

		public static bool Offline(Database db) {
			using (var mc = new MigrationContext()) {
				return !dbs[db].hasDatabase;
			}
		}

		/// <summary>
		/// Resets the DefaultInitializer so it performs all checks.
		/// </summary>
		public static void Reset() { dbs = new Dictionary<Database, Info>(); }
		/// <summary>
		/// Initializes the database and schema for the supplied ContextCreates the database if it does not exist, and applies all DbMigrations if the database schema is not cosistent with the Context Version.
		/// In order for this to work, the Context must override the Version property and the SetMigrations method that adds the migrations to the Migrations list.
		/// </summary>
		/// <param name="context">The Context.</param>
		public void InitializeDatabase(T context) {
			var key = context.Database;
			Info info = null;
			lock (Lock) {
				if (dbs.ContainsKey(key)) info = dbs[key];
				else {
					info = new Info();
					dbs[key] = info;
				}
			}
			lock (info) {
				if (info.hasDatabase || (!info.testedIfDatabaseExists && context.Database.Exists())) {
					info.testedIfDatabaseExists = info.hasDatabase = true;
				}
				info.testedIfDatabaseExists = true;
			}
			lock (info) {
				if (!info.hasDatabase && !info.triedDatabaseCreate) {
					info.triedDatabaseCreate = true;
					info.hasDatabase = true;
					context.Database.Create();
					// if(typeof(T) == typeof(MigrationContext)) info.testedForVersion.Add(context.GetType());
				}
			}
			lock (info) {
				if (!info.testedForVersion.Contains(context.GetType())) {
					context.Database.Update<T>();
					info.testedForVersion.Add(context.GetType());
				}
			}
		}
	}
}