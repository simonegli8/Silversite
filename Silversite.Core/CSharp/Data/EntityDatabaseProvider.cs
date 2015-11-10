// davidegli
// TODO: this class could be moved to an optional assembly Silversite.Entity.dll. If all the references to System.Data.Entity classes would be replaced by interfaces, this would allow for lazy loading of the entity framework,
// to allow for even faster startup times.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Silversite.Data {
	
	/// <summary>
	/// A concrete provider for the Database service that uses EntityFramework CodeFirst.
	/// </summary>
	public class EntityDatabaseProvider: DatabaseProvider {

		/*
		/// <summary>
		/// A database with a DbConnection
		/// </summary>
		public class DbWithConnection: Data.Database {
			public DbWithConnection() { }
			/// <summary>
			/// Intitalizes the DbWithConnection with the DbConnection
			/// </summary>
			/// <param name="con">The DbConnection</param>
			public DbWithConnection(System.Data.Common.DbConnection con): this() { DbConnection = con; }
			/// <summary>
			/// The DbConnection.
			/// </summary>
			public System.Data.Common.DbConnection DbConnection { get; set; }
		}
		*/

		static Services.WeakValueDictionary<Tuple<string, string, string>, Database> dbcache = new Services.WeakValueDictionary<Tuple<string, string, string>, Database>();

		public override Data.Database CustomWithSchema(string providerName, string connectionString, string schema) {

			lock (dbcache) {
	
				if (string.IsNullOrEmpty(connectionString)) {
					if (string.IsNullOrEmpty(Data.Database.Configuration.Default)) {
						connectionString = "Data Source=|DataDirectory|Silversite.CE4.sdf";
						providerName = "System.Data.SqlCeServer.4.0";
					} else {
						var setting = Data.Database.Configuration.ConnectionStrings[Data.Database.Configuration.Default];
						connectionString = setting.ConnectionString;
						providerName = setting.ProviderName;
					}
				}

				var key = new Tuple<string, string, string>(providerName, connectionString, schema);

				Database db;
				if (dbcache.TryGetValue(key, out db)) return db;

				var pconf = Database.Configuration.Providers.FirstOrDefault<ProviderConfigurationElement>(p => Services.Paths.Match(p.Db, providerName));
				if (pconf != null) {
					try {
						var p = Services.Lazy.Types.New(pconf.Provider, null) as DatabaseProvider;
						if (p == null) throw new NotSupportedException("Cannot create instance of DatabaseProvider type " + pconf.Provider);
						return dbcache[key] = p.CustomWithSchema(providerName, connectionString, schema);
					} catch (Exception ex) {
						Services.Log.Error("Error creating custom DatabaseProvider", ex);
					}
				}

				db = new Database();
				db.DbProviderName = providerName;
				db.ConnectionString = connectionString;
				dbcache[key] = db;
				Schemes[db] = schema;

				return db;
			}
		}

		/// <summary>
		/// Returns a Database that uses the passed connection parameters
		/// </summary>
		/// <param name="providerName">The provider class type name, for example "System.Data.SqlClient".</param>
		/// <param name="connectionString">The connection string</param>
		/// <returns>Returns a Database that uses the passed connection parameters</returns>
		public override Data.Database Custom(string providerName, string connectionString) { return CustomWithSchema(providerName, connectionString, ""); }

		/// <summary>
		/// Returns a database that uses the connection.
		/// </summary>
		/// <param name="con">The connection.</param>
		/// <returns>Returns a database that uses the connection.</returns>
		public override Database Custom(DbConnection con) {
			string providerName = null;
			if (con.GetType().FullName == "System.Data.SqlServerCe.SqlCeConnection") {
				providerName = "System.Data.SqlServerCe." + con.ServerVersion.Split('.').Take(2).StringList(".");
			} else providerName = Services.Paths.WithoutExtension(con.GetType().FullName);

			return Custom(providerName, con.ConnectionString);
		}
		/// <summary>
		/// Returns a Database that uses the passed named connection string either from Database.config or web.config.
		/// </summary>
		/// <param name="con">The connection.</param>
		/// <returns>Returns a Database that uses the passed named connection string.</returns>
		public override Database Custom(string connectionStringName) {
			if (string.IsNullOrWhiteSpace(connectionStringName)) return Default;
			else {
				System.Configuration.ConnectionStringSettings cssettings = null;
				try {
					cssettings = Database.Configuration.ConnectionStrings[connectionStringName];
				} catch { }
				try {
					if (cssettings == null) cssettings = System.Web.Configuration.WebConfigurationManager.ConnectionStrings[connectionStringName];
				} catch { }
				if (cssettings == null) return Default;
				else return Custom(cssettings.ProviderName, cssettings.ConnectionString);
			}
		}

		/// <summary>
		/// Returns a Database that uses the passed named connection string either from Database.config or web.config.
		/// </summary>
		/// <param name="con">The connection.</param>
		/// <returns>Returns a Database that uses the passed named connection string.</returns>
		public override Database CustomWithSchema(string connectionStringName, string schema) {
			if (string.IsNullOrWhiteSpace(connectionStringName)) return Default;
			else {
				System.Configuration.ConnectionStringSettings cssettings = null;
				try {
					cssettings = Database.Configuration.ConnectionStrings[connectionStringName];
				} catch { }
				try {
					if (cssettings == null) cssettings = System.Web.Configuration.WebConfigurationManager.ConnectionStrings[connectionStringName];
				} catch { }
				if (cssettings == null) return Default;
				else return CustomWithSchema(cssettings.ProviderName, cssettings.ConnectionString, schema);
			}
		}
		/// <summary>
		/// Gets the default database
		/// </summary>
		public override Data.Database Default { get { return Custom(null, null); } }

		// returns an instance of TContext
		private Data.Context New<TContext>(Data.Database db) {
			return Services.Lazy.Types.New(typeof(TContext).AssemblyQualifiedName, db) as Data.Context;
		}

		/// <summary>
		/// Initializes the context
		/// </summary>
		/// <typeparam name="TContext">The context type</typeparam>
		/// <param name="db">The database</param>
		/// <param name="context">The contex</param>
		public override void Open<TContext>(Data.Database db, Data.Context context) { 	System.Data.Entity.Database.SetInitializer<TContext>(new Data.MigrateDatabaseToLatestVersion<TContext>()); }
		/// <summary>
		/// Returns true if the database exists.
		/// </summary>
		/// <param name="db">The database</param>
		/// <returns>True if the database exits.</returns>
		public override bool Exists(Data.Database db) { lock (this) { try { return System.Data.Entity.Database.Exists(Connection(db)); } catch { return false; } } }
		/// <summary>
		/// Creates the database.
		/// </summary>
		/// <param name="db">The database</param>
		public override void Create(Data.Database db) {
			lock (this) {
				var key = ContextKey.New<MigrationContext>(db);
				if (!Versions.ContainsKey(key)) Versions.Add(key, DbVersion.Zero<MigrationContext>()); // set Context version for MigrationContext to 0, so that Version will not load the ContextVersions table.
				Update<MigrationContext>(db); // initialize the schema for the MigrationContext.
			}
		}
		/// <summary>
		/// Drops the database
		/// </summary>
		/// <param name="db">The database.</param>
		public override void Delete(Data.Database db) { lock (this) System.Data.Entity.Database.Delete(Connection(db)); }
		/// <summary>
		/// Creates the database if it doesn't exist.
		/// </summary>
		/// <param name="db">The database</param>
		/// <returns>True if the database was created</returns>
		public override bool CreateIfNotExists(Data.Database db) { lock (this) { if (!Exists(db)) { Create(db); return true; } return false; } }
		/// <summary>
		/// Executes an SQL command on the database.
		/// </summary>
		/// <param name="db">The database</param>
		/// <param name="sql">The SQL command</param>
		/// <param name="parameters">Parameters for the SQL command</param>
		/// <returns>Returns the number of affected rows.</returns>
		public override int ExecuteSqlCommand(Data.Database db, string sql, params object[] parameters) {
			using (var ctx = new MigrationContext(db)) {
				return ((DbContext)ctx).Database.ExecuteSqlCommand(sql, parameters);
			}
		}
		/// <summary>
		/// Inititalizes the database. This method only creates the database. The tables are only created, when the corresponding context's are first accessed.
		/// </summary>
		/// <param name="db">The database</param>
		/// <param name="force">Has no effect.</param>
		public override void Initialize(Data.Database db, bool force) { CreateIfNotExists(db); }
		/// <summary>
		/// Executes an SQL query on the database
		/// </summary>
		/// <typeparam name="TElement">The entity element type</typeparam>
		/// <param name="db">The database</param>
		/// <param name="sql">The SQL to execute</param>
		/// <param name="parameters">The parameters for the SQL command</param>
		/// <returns>An IEnumerable with the query data</returns>
		public override IEnumerable<TElement> SqlQuery<TElement>(Data.Database db, string sql, params object[] parameters) {
			using (var ctx = new MigrationContext(db)) {
				return ctx.Database.SqlQuery<TElement>(sql, parameters);
			}
		}
		/// <summary>
		/// Executes an SQL query on the database
		/// </summary>
		/// <param name="db">The database</param>
		/// <param name="elementType">The table's entity element type</param>
		/// <param name="sql">The SQL command</param>
		/// <param name="parameters">The command's parameters</param>
		/// <returns>Returns an IEnumerable with the query's data.</returns>
		public override IEnumerable SqlQuery(Data.Database db, Type elementType, string sql, params object[] parameters) {
			using (var ctx = new MigrationContext(db)) {
				return ctx.Database.SqlQuery(elementType, sql, parameters);
			}
		}
		/// <summary>
		/// Gets the DbConnection for the database
		/// </summary>
		/// <param name="db">The database</param>
		/// <returns>The DbConnection</returns>
		public override System.Data.Common.DbConnection Connection(Data.Database db) {
			DbConnection con;
			if (string.IsNullOrEmpty(db.DbProviderName)) {
				var cfactory = System.Data.Entity.Database.DefaultConnectionFactory;
				con = cfactory.CreateConnection(db.ConnectionString);
			} else {
				Services.Lazy.DbProviders.Load(db.DbProviderName); // load db provider assembly.
				var dfactory = DbProviderFactories.GetFactory(db.DbProviderName);
				con = dfactory.CreateConnection();
				con.ConnectionString = db.ConnectionString;
			}
			return con;
		}
		/// <summary>
		/// Gets the default connection factory
		/// </summary>
		public override IDbConnectionFactory DefaultConnectionFactory { get { return System.Data.Entity.Database.DefaultConnectionFactory; } }
		/// <summary>
		/// Creates a backup of the database
		/// </summary>
		/// <param name="db">The database</param>
		/// <param name="file">The file to backup into</param>
		public override void Backup(Data.Database db, string file) { throw new NotImplementedException(); } // TODO
		/// <summary>
		/// Restores the database from a backup
		/// </summary>
		/// <param name="db">The database</param>
		/// <param name="file">The backup file</param>
		public override void Restore(Data.Database db, string file) { throw new NotImplementedException(); } // TODO
		/// <summary>
		/// Truncates the database
		/// </summary>
		/// <param name="db">The database</param>
		public override void Truncate(Data.Database db) { throw new NotImplementedException(); } // TODO
		/// <summary>
		/// Sets the initialization strategy for the context. Usually you don't call this function and use the Silversite's default intitalization strategy DefaultInitalizer
		/// </summary>
		/// <typeparam name="TContext">The context type</typeparam>
		/// <param name="strategy">The database initializer</param>
		public override void SetInitializer<TContext>(IDatabaseInitializer<TContext> strategy) {
			System.Data.Entity.Database.SetInitializer<TContext>(strategy);
		}

		// a class that logs DbMigrator messages to the provider message queue. 
		private class Logger: MigrationsLogger {
			EntityDatabaseProvider provider;
			public Logger(EntityDatabaseProvider provider) { this.provider = provider; }
			public override void Info(string message) {
				Services.Providers.Message(provider, message);
				Services.Log.Write("Database Migration", message);
			}
			public override void Warning(string message) {
				Services.Providers.Message(provider, message);
				Services.Log.Write("Database Migration", message);
			}
			public override void Verbose(string message) {
				Services.Log.Write("Database Migration", message);
			}
		}
		// a unique key for a Context with a database.
		private class ContextKey: Tuple<string, string, string> {
			public ContextKey(string type, Database db): base(type, db.ConnectionString, db.DbProviderName) { }
			public ContextKey(Type type, Database db): this(type.InvariantName(), db) { }
			public bool IsSameDb(Database db) { return db.DbProviderName == Item2 && db.ConnectionString == Item3; }
			public static ContextKey New<TContext>(Database db) where TContext: Context {
				return new ContextKey(typeof(TContext), db);
			}
			public override bool Equals(object obj) {
				var ck = obj as ContextKey;
				return ck != null && Item1 == ck.Item1 &&  Item2 == ck.Item2 && Item3 == ck.Item3;
			}
			public override int GetHashCode() {
				return Item1.GetHashCode() + Item2.GetHashCode() + Item3.GetHashCode();
			}
		}

		private class Migrator: MigratorLoggingDecorator {
			int version = -1;
			public int Migrations { get { if (version == -1) version = GetLocalMigrations().Count(); return version; } } // the current version of the Context.
			public bool Updating;
			public Migrator(MigratorBase migrator, MigrationsLogger logger): base(migrator, logger) { }
		}
		private class Migrator<TContext>: Migrator where TContext: Context {
			static Dictionary<ContextKey, Migrator> migrators = new Dictionary<ContextKey, Migrator>();

			static MigratorBase GetBaseMigrator(DbConnectionInfo db) {
				var type = typeof(TContext);
				var configType = type.Assembly.GetTypes().FirstOrDefault(t => t.IsSubclassOf(typeof(DbMigrationsConfiguration<TContext>)) || t.IsSubclassOf(typeof(DbMigrationsConfiguration<MigrationContext<TContext>>)));
				if (configType == null) throw new NotSupportedException("There is no DbMigrationsConfiguration for the Context " + type.InvariantName() + ".");
				var config = Silversite.New.Object(configType) as DbMigrationsConfiguration;
				config.TargetDatabase = db;
				return new DbMigrator(config);
			}
			public Migrator(DbConnectionInfo db, EntityDatabaseProvider provider): base(GetBaseMigrator(db), new Logger(provider)) { }

			public static Migrator<TContext> For(EntityDatabaseProvider provider, Database db) {
				var key = ContextKey.New<TContext>(db);
				Migrator migrator = null;
				lock (migrators) {
					migrators.TryGetValue(key, out migrator);
					if (migrator == null) {
						var con =  new DbConnectionInfo(db.ConnectionString, db.DbProviderName);
						migrator = new Migrator<TContext>(con, provider);
						migrators.Add(key, migrator);
					}
					return (Migrator<TContext>)migrator;
				}
			}
		}

		private Migrator<TContext> MigratorFor<TContext>(Database db) where TContext: Context { return Migrator<TContext>.For(this, db); }
	
		/// <summary>
		/// Return true if the database schema is compatible with the current version of the Context.
		/// </summary>
		/// <typeparam name="TContext">The Context type.</typeparam>
		/// <param name="db">The database.</param>
		/// <returns>True if the database schema is compatible with the current version of the Context.</returns>
		public override bool CompatibleWithModel<TContext>(Database db) {
			var migrator = MigratorFor<TContext>(db);
			lock (migrator) {
				var ver = Version<TContext>(db);
				if (migrator.Migrations == ver.Migrations) {
					using (var context = Silversite.New.Object<TContext>(db)) return ver.ModelHash == context.ModelHash;
				}
				return false;
			}
		}

		/// <summary>
		/// Migrate the database to the current version of the context.
		/// </summary>
		/// <typeparam name="TContext">The Context type</typeparam>
		/// <param name="db">The database connection.</param>
		public override void Update<TContext>(Database db) {
			var migrator = MigratorFor<TContext>(db);
			lock (migrator) {
				if (migrator.Updating) return; // avoid reentrancy.
				
				TContext context = null;
				
				try {
					migrator.Updating = true;
					var version = Version<TContext>(db);
					
					var migrate = migrator.Migrations != version.Migrations || DbVersion.Zero<TContext>().FrameworkVersion != version.FrameworkVersion;
					
					if (!migrate) {
						context = Silversite.New.Object<TContext>(db);
						migrate = context.ModelHash != version.ModelHash;
						if (migrate && version.ModelHash != int.MinValue) {
							Services.Log.Write("Database", "Incompatible Context Edmx Model for {0}. Trying Auto Migrations if enabled.", typeof(TContext).FullName);
							Debug.Break();
						}
					}

					if (migrate) {
						try {
							//Update dependencies
							if (Context.Updater.ContainsKey(typeof(TContext))) Context.Updater[typeof(TContext)](db);

							if (context == null) context = Silversite.New.Object<TContext>(db);

							using (var mh = new MigrationContext(db)) {
								try {
									mh.RestoreMigrations<TContext>(); // set entity framework migration history for the Context.
								} catch (Exception ex) {
									if (typeof(TContext) != typeof(MigrationContext)) throw ex;
								}

								try {
									migrator.Update(); // run the migrations.
								} catch (AutomaticMigrationsDisabledException) { // this exception can happen because the edmx is database dependent.
								}

								mh.SaveMigrations<TContext>(); // backup entity framework migration history.
								SetVersion<TContext>(db, new DbVersion(typeof(TContext), migrator.Migrations, context.ModelHash)); // save context version.
								mh.SaveChanges();

							}
							context.Seed(version);
						} catch (Exception ex) {
							if (typeof(TContext) != typeof(MigrationContext)) throw ex;
						}
					}
				} finally {
					migrator.Updating = false;
					if (context != null) context.Dispose();
				}
			}
			Services.Lazy.IsLazy(typeof(TContext)); // register lazy type if TContext is from a lazy assembly.
		}
		/// <summary>
		/// Drop the database schema for the context.
		/// </summary><
		/// <typeparam name="TContext">The Context type</typeparam>
		/// <param name="db">The database connection.</param>
		public override void Drop<TContext>(Database db) {
			var migrator = MigratorFor<TContext>(db);
			lock (migrator) {
				using (var mh = new MigrationContext(db)) {
					if (Version<TContext>(db).Migrations > 0) {
						mh.RestoreMigrations<TContext>(); // set entity framework migration history for the Context.
						try {
							migrator.Update(null); // run the migrations.
						} catch (AutomaticMigrationsDisabledException) { // this exception can happen because the edmx is database dependent.
						}
						if (typeof(TContext) != typeof(MigrationContext)) {
							mh.SaveMigrations<TContext>(); // backup entity framework migration history.
							SetVersion<TContext>(db, DbVersion.Zero<TContext>()); // change context version.
							mh.SaveChanges();
						}
					}
				}
			}
		}


		static Dictionary<ContextKey, DbVersion> Versions = new Dictionary<ContextKey, DbVersion>(); // versions cache

		/// <summary>
		/// Gets the DbVbersion for a DbContext in a Database.
		/// </summary>
		/// <typeparam name="TContext">The DbContext type to check for.</typeparam>
		/// <param name="db">The database.</param>
		/// <returns>The version of the DbContext that's stored inside the database.</returns>
		public override DbVersion Version<TContext>(Database db) {
			lock (Versions) {
				var mhversion = DbVersion.Zero<TContext>();
				var key = ContextKey.New<MigrationContext>(db);
				if (!Versions.TryGetValue(key, out mhversion)) {
					Versions.Add(key, DbVersion.Zero<MigrationContext>());
					try {
						using (var mh = Silversite.New.Object<MigrationContext>(db)) { // cache all versions from database in memory
							//var mcversion = new DbVersion(typeof(MigrationContext), Migrator<MigrationContext>.For(this, db).Migrations, mh.ModelHash);
							//Versions.Add(key, mcversion);
							foreach (var ver in mh.ContextVersions) {
								var vkey = new ContextKey(ver.Context, db);
								var value = new DbVersion(typeof(TContext), ver.Migrations, ver.ModelHash);
								value.FrameworkVersion = System.Version.Parse(ver.FrameworkVersion);
								if (Versions.ContainsKey(vkey)) Versions[vkey] = value;
								else Versions.Add(vkey, value);
							}
						}
					} catch {
						Versions[key] = DbVersion.Zero<MigrationContext>();
						Update<MigrationContext>(db);
					}	
				}
					
				var version = DbVersion.Zero<TContext>();
				Versions.TryGetValue(ContextKey.New<TContext>(db), out version);
 				return version;
			}
		}
		internal static DbVersion SetVersion<TContext>(Database db, DbVersion version) where TContext: Context {
			using (var mh = Silversite.New.Object<MigrationContext>(db)) {
				var old = db.Version<TContext>();
				if (version != old) {
					var key = ContextKey.New<TContext>(db);
					lock (Versions) {
						if (!Versions.ContainsKey(key)) Versions.Add(key, version);
						else Versions[key] = version;
					}
					if (version.Migrations == 0) {
						mh.ContextVersions.Remove(ver => ver.Context == Services.Types.InvariantName(typeof(TContext)));
						mh.SaveChanges();
					} else {

						mh.ContextVersions.AddOrUpdate(ver => ver.Context, new ContextVersion { Context = typeof(TContext).InvariantName(), Migrations = version.Migrations, ModelHash = version.ModelHash, FrameworkVersion = version.FrameworkVersion.ToString() });
						mh.SaveChanges();
						if (old.Migrations == 0 && typeof(TContext) == typeof(MigrationContext)) { // if the migration history context has been created, then read the context versions.
							lock (Versions) {
								Versions.Clear();
								foreach (var ver in mh.ContextVersions) Versions.Add(new ContextKey(ver.Context, mh.Database), new DbVersion(typeof(TContext), ver.Migrations, ver.ModelHash));
							}
						}
					}
				}
				return old;
			}
		}

		/// <summary>
		/// Returns the type names of all DbContexts that are stored in a database.
		/// </summary>
		/// <param name="db">The database.</param>
		/// <returns>Returns the type names of all DbContexts that are stored in a database.</returns>
		public override IEnumerable<string> Contexts(Database db) {
			var v = Version<MigrationContext>(db); // load all context versions for database db.
			return Versions.Keys.Where(key => key.IsSameDb(db)).Select(key => key.Item1);
		}

		static readonly Regex dbIsLocal = new System.Text.RegularExpressions.Regex(@"([.]*[=\s].\\[.]*)|([.]*[=\s]\(local\)[.]*)|([.]*[=\s]localhost[,:\s][.]*)|([.]*[=\s]127.0.0.1[,:\s][.]*)",
			RegexOptions.Compiled | RegexOptions.IgnoreCase);
		public override bool IsLocal(Database db) {
			if (db.DbProviderName.StartsWith("System.Data.SqlCeServer.") || db.DbProviderName.StartsWith("System.Data.SQLite")) return true;
			else {
				var local = db.ConnectionString
					.ToLower()
					.SplitList(',',';')
					.Where(t => t.Contains("database") || t.Contains("source") || t.Contains("server"))
					.Any(t => dbIsLocal.IsMatch(t));

				if (local) return true;

				var localservers = Database.Configuration.Local.SplitList(',',';').ToList();
				return Database.Configuration.ConnectionStrings
					.OfType<System.Configuration.ConnectionStringSettings>()
					.Any(cs => cs.ConnectionString == db.ConnectionString && localservers.Contains(cs.Name)); 
			}
		}

		/// <summary>
		/// True if the database is offline.
		/// </summary>
		/// <param name="db">The database to check.</param>
		/// <returns>True if the database is offline.</returns>
		public override bool Offline(Database db) { return MigrateDatabaseToLatestVersion<MigrationContext>.Offline(db); }

		private Database ReplicationDb(Data.ReplicationConfigurationElement r, string cstrname) {
			if (cstrname.EndsWith("-")) cstrname = cstrname.UpTo(cstrname.Length - 1);
			var schemas = r.Schemas.Split(';',',').Select(s => s.Split('=', ':').ToArray());
			string defaultSchema = "";
			var ds = schemas.FirstOrDefault(s => s.Length == 1);
			if (ds != null) defaultSchema = ds[0];

			var schema = schemas.FirstOrDefault(s => s[0] == cstrname && s.Length == 2);
			if (schema != null) return Database.CustomWithSchema(cstrname, schema[1]);
			else if (!string.IsNullOrEmpty(defaultSchema)) return Database.CustomWithSchema(cstrname, defaultSchema);
			else return Database.Custom(cstrname);
		}

		static Dictionary<Database, List<Database>> replicateTo = new Dictionary<Database,List<Database>>();
		//TODO doesn't work yet, so it's still internal
		internal override List<Database> ReplicateTo(Database db) {
			List<Database> replTo = null;
			if (!replicateTo.TryGetValue(db, out replTo)) {
				var servers = Database.Configuration.Replication
					.OfType<Data.ReplicationConfigurationElement>()
					.Select(
						r => new {
							Servers = r.Servers
								.SplitList(',', ';')
								.Select(name => new {
									Trigger = name.EndsWith("-"),
									Db = ReplicationDb(r, name)
								})
						});
				replTo = servers
					.Where(list => list.Servers.Any(d => d.Db == db))
					.SelectMany(list => list.Servers)
					.Where(d => d.Db != db || !d.Trigger)
					.Select(d => d.Db)
					.ToList();	
				replicateTo[db] = replTo;
			}
			return replTo;
		}

		static Dictionary<Database, string> Schemes = new Dictionary<Database, string>();
		static string DefaultSchema = "";

		public override string Schema(Database db) {
			if (DefaultSchema == "") {
				DefaultSchema = null;
				Database.Configuration.Schemas
					.Split(',', ';')
					.Select(s => s.Split('=', ':').ToArray())
					.ForEach(s => {
						if (s.Length == 1) DefaultSchema = s[0];
						else {
							var d = Database.Custom(s[0]);
							if (!Schemes.ContainsKey(d)) Schemes.Add(d, s[1]);
						}
					});
			}
			string schema = DefaultSchema;
			Schemes.TryGetValue(db, out schema);
			return schema;
		}

		public override DatabaseType Type(Database db) {
			var provider = db.DbProviderName;
			if (provider.Contains("SqlServerCe")) return DatabaseType.SqlCe;
			if (provider.Contains("MySql")) return DatabaseType.MySql;
			if (provider.Contains("SqlLite")) return DatabaseType.SqlLite;
			if (provider.Contains("MogoDb")) return DatabaseType.MongoDb;
			if (provider.Contains("Oracle")) return DatabaseType.Oracle;
			if (provider.Contains("Postgre")) return DatabaseType.PostgreSql;
			if (provider.Contains("Db4o")) return DatabaseType.Db4o;
			if (provider.Contains("SqlClient")) return DatabaseType.SqlServer;
			return DatabaseType.Other;
		}

		public override string ServerVersion(Database db) { return db.Connection.ServerVersion; }
	}


}