// davidegli

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Configuration;

namespace Silversite.Data {

	/// <summary>
	/// A configuration class that describes the settings for a Silversite DatabaseProvider assigned to a connection string's provider. 
	/// </summary>
	public class ProviderConfigurationElement: Configuration.Element {
		/// <summary>
		/// The connection string's provider
		/// </summary>
		[System.Configuration.ConfigurationProperty("db", IsRequired=true, IsKey=true)]
		public string Db { get { return (string)base["db"]; } set { base["db"] = value; } }
		/// <summary>
		/// The corresponding Silversite DatabaseProvider to use.
		/// </summary>
		[System.Configuration.ConfigurationProperty("provider", IsRequired=true)]
		public string Provider { get { return (string)base["provider"]; } set { base["provider"] = value; } }
	}
	/// <summary>
	/// A collection of provider configuration settings.
	/// </summary>
	public class ProviderConfigurationCollection: Configuration.Collection<string,ProviderConfigurationElement> {
		protected override object GetElementKey(System.Configuration.ConfigurationElement element) { return ((ProviderConfigurationElement)element).Db; }
	}

	/// <summary>
	/// A configuration class that describes the settings for a Silversite DatabaseProvider assigned to a connection string's provider. 
	/// </summary>
	public class ReplicationConfigurationElement: Configuration.Element {
		/// <summary>
		/// The name of the connection string's that replicate to each other. A name that ends with a minus '-' sign
		/// indicates a connection that triggers replication but does itself not store the data. This can be used in
		/// a webfarm where replication works on the internet connection strings, but the localhost connection also
		/// triggers replication, but is not stored, because the same connection is contained in the internet connection strings.
		/// </summary>
		[System.Configuration.ConfigurationProperty("servers", IsRequired=true, IsKey=true, DefaultValue="")]
		public string Servers { get { return (string)base["servers"] ?? ""; } set { base["servers"] = value; } }
		/// <summary>
		/// A list of default schemas for the replication connections. The syntax is:
		/// connection_string_name : schema {; connection_string_name: schema }
		/// </summary>
		[System.Configuration.ConfigurationProperty("schemas", IsRequired = false, DefaultValue = "")]
		public string Schemas { get { return (string)base["schemas"] ?? ""; } set { base["schemas"] = value; } }

	}
	/// <summary>
	/// A collection of provider configuration settings.
	/// </summary>
	public class ReplicationConfigurationCollection: Configuration.Collection<string, ReplicationConfigurationElement> {
		protected override object GetElementKey(System.Configuration.ConfigurationElement element) { return ((ReplicationConfigurationElement)element).Servers; }
	}

	public enum DatabaseType { SqlServer, SqlExpress, LocalDB, SqlCe, MySql, SqlLite, PostgreSql, Oracle, Db4o, MongoDb, Other } 
	/// <summary>
	/// The configuration section for database settings.
	/// </summary>
	[Configuration.Section(Path = DatabaseConfiguration.Path)]
	public class DatabaseConfiguration: Configuration.Section {
		/// <summary>
		/// The path to the configuration file.
		/// </summary>
		public new const string Path =  ConfigRoot + "/database.config";
		/// <summary>
		/// The name of the default connection string.
		/// </summary>
		[ConfigurationProperty("default", IsRequired = false, DefaultValue = null)]
		public string Default { get { return this["default"] as string ?? string.Empty; } set { this["default"] = value; } }
		/// <summary>
		/// The databases that are local and need no WebFarm wide locks. This is only used for resources locking in a WebFarm.
		/// </summary>
		[ConfigurationProperty("local", IsRequired = false, DefaultValue = null)]
		public string Local { get { return this["local"] as string ?? string.Empty; } set { this["local"] = value; } }

		/// <summary>
		/// The schemas to use. This simply a schema for a common default schema or  a comma or semicolon separated list of the form "defaultSchema; connectionStringName1: schema1; connectionStringName2: schema2; ...
		/// </summary>
		[ConfigurationProperty("schemas", IsRequired = false, DefaultValue = null)]
		public string Schemas { get { return this["schemas"] as string; } set { this["schemas"] = value; } }


		/// <summary>
		/// Gets the default connection string.
		/// </summary>
		public ConnectionStringSettings ConnectionString {
			get {
				var settings = ConnectionStrings[Default];
				if (settings == null && ConnectionStrings.Count > 0) settings = ConnectionStrings[0];
				return settings;
			}
		}

		[System.Configuration.ConfigurationProperty("providers")]
		[System.Configuration.ConfigurationCollection(typeof(ProviderConfigurationCollection))]
		public ProviderConfigurationCollection Providers { get { return (ProviderConfigurationCollection)base["providers"]; } }

		[System.Configuration.ConfigurationProperty("replication")]
		[System.Configuration.ConfigurationCollection(typeof(ReplicationConfigurationCollection))]
		public ReplicationConfigurationCollection Replication { get { return (ReplicationConfigurationCollection)base["replication"]; } }

	}

	public struct DbVersion {

		public DbVersion(int version, int hash) : this() { Context = null; Migrations = version; ModelHash = hash; FrameworkVersion = System.Reflection.Assembly.GetAssembly(typeof(DbContext)).GetName().Version; }
		public DbVersion(Type context, int version, int hash) : this(version, hash) { Context = context; }
		public Type Context;
		public int Migrations;
		public int ModelHash;
		public Version FrameworkVersion;

		public static DbVersion Zero<TContext>() where TContext: Context { return new DbVersion(typeof(TContext), 0, int.MinValue); }

		public static bool operator ==(DbVersion a, DbVersion b) { return (a.Context == null || a.Context == b.Context) && a.ModelHash == b.ModelHash && a.Migrations == b.Migrations && a.FrameworkVersion == b.FrameworkVersion; }
		public static bool operator !=(DbVersion a, DbVersion b) { return (a.Context != null && b.Context != null && a.Context != b.Context) || a.ModelHash != b.ModelHash || a.Migrations != b.Migrations || a.FrameworkVersion != b.FrameworkVersion; }
	}

	/// <summary>
	/// A service class that implements a database service. This class implements all of the methods of System.Data.Entity.Database in orther to be compatible with EntityFramework.
	/// All requests to the database go through a DbContext that derives from Silversite.Data.Context. For this context you can either use the default constructor, using the default Database as 
	/// connection (Silversite.Data.Database.Default) or a custom database by using Silversite.Data.Database.Custom.
	/// </summary>
	public class Database: Services.StaticService<Database, Data.DatabaseProvider> {

		static Database() {
			new EntityDatabaseProvider().Startup();
		}

		/// <summary>
		/// The DbFactory used by this database.
		/// </summary>
		public string DbProviderName { get; set; }
		/// <summary>
		/// The connection string used by this database.
		/// </summary>
		public string ConnectionString { get; set; }

		/// <summary>
		/// The configuration settings for the database.
		/// </summary>
		public static DatabaseConfiguration Configuration = new DatabaseConfiguration();
		/// <summary>
		/// The default constructor, setting up a database with the default connection.
		/// </summary>
		public Database() { this.ConnectionString = null; DbProviderName = null; }

		/// <summary>
		/// Returns a Database that uses the passed connection parameters.
		/// </summary>
		/// <param name="providerName">The provider class type name, for example "System.Data.SqlClient".</param>
		/// <param name="connectionString">The connection string.</param>
		/// <returns>Returns a Database that uses the passed connection parameters.</returns>
		public static Database Custom(string providerName, string connectionString) { return Provider.Custom(providerName, connectionString); }
		/// <summary>
		/// Returns a Database that uses the passed connection.
		/// </summary>
		/// <param name="con">The connection.</param>
		/// <returns>Returns a Database that uses the passed connection.</returns>
		public static Database Custom(DbConnection con) { return Provider.Custom(con); }
		/// <summary>
		/// Returns a Database that uses the passed named connection string either from Database.config or web.config.
		/// </summary>
		/// <param name="con">The connection.</param>
		/// <returns>Returns a Database that uses the passed named connection string.</returns>
		public static Database Custom(string connectionStringName) { return Provider.Custom(connectionStringName); }
		/// <summary>
		/// Returns a Database that uses the passed named connection string either from Database.config or web.config.
		/// </summary>
		/// <param name="con">The connection.</param>
		/// <returns>Returns a Database that uses the passed named connection string.</returns>
		public static Database CustomWithSchema(string connectionStringName, string schema) { return Provider.CustomWithSchema(connectionStringName, schema); }
		/// <summary>
		/// Gets the default database.
		/// </summary>
		public static Database Default { get { return Provider.Default; } }
		/// <summary>
		/// Initializes the context.
		/// </summary>
		/// <typeparam name="TContext">The context type.</typeparam>
		/// <param name="context">The contex.</param>
		public void Open<TContext>(Data.Context context) where TContext: Data.Context {
			Provider.Open<TContext>(this, context);
		}
		/// <summary>
		/// All Context types that have a schema in the database. To get instances of the Contexts use Services.Lazy.Types.New(contextName, db)
		/// where contextName is the type name of the Context as returned by Contexts, and db is the Database to connect with.
		/// </summary>
		public IEnumerable<string> Contexts { get { return Provider.Contexts(this); } }
		/// <summary>
		/// Returns true if the database exists.
		/// </summary>
		/// <returns>True if the database exits.</returns>		
		public bool Exists() { return HasProvider && Provider.Exists(this); }
		/// <summary>
		/// Creates the database. This method only creates the database. The tables are only created, when the corresponding context's are first accessed. Use Update if you want to create tables explicitly.
		/// </summary>
		public void Create() { Provider.Create(this); }
		/// <summary>
		/// Drops the database.
		/// </summary>
		public void Delete() { Provider.Delete(this); }
		/// <summary>
		/// Creates the database if it doesn't exist.
		/// </summary>
		/// <returns>True if the database was created.</returns>
		public bool CreateIfNotExists() { return Provider.CreateIfNotExists(this); }
		/// <summary>
		/// Executes an SQL command on the database.
		/// </summary>
		/// <param name="sql">The SQL command.</param>
		/// <param name="parameters">Parameters for the SQL command.</param>
		/// <returns>Returns the number of affected rows.</returns>
		public int ExecuteSqlCommand(string sql, params object[] parameters) { return Provider.ExecuteSqlCommand(this, sql, parameters); }
		/// <summary>
		/// Inititalizes the database. This method only creates the database. The tables are only created, when the corresponding context's are first accessed.
		/// </summary>
		/// <param name="force">Has no effect.</param>
		public void Initialize(bool force) { Provider.Initialize(this, force); }
		/// <summary>
		/// Executes an SQL query on the database.
		/// </summary>
		/// <typeparam name="TElement">The entity element type.</typeparam>
		/// <param name="sql">The SQL to execute.</param>
		/// <param name="parameters">The parameters for the SQL command.</param>
		/// <returns>An IEnumerable with the query data.</returns>
		public IEnumerable<TElement> SqlQuery<TElement>(string sql, params object[] parameters) { return Provider.SqlQuery<TElement>(this, sql, parameters); }
		/// <summary>
		/// Executes an SQL query on the database.
		/// </summary>
		/// <param name="elementType">The table's entity element type.</param>
		/// <param name="sql">The SQL command.</param>
		/// <param name="parameters">The command's parameters.</param>
		/// <returns>Returns an IEnumerable with the query's data.</returns>
		public IEnumerable SqlQuery(Type elementType, string sql, params object[] parameters) { return Provider.SqlQuery(this, elementType, sql, parameters); }
		/// <summary>
		/// Gets the DbConnection for the database
		/// </summary>
		/// <returns>The DbConnection</returns>
		public DbConnection Connection { get { return Provider.Connection(this); } }
		/// <summary>
		/// Gets the default connection factory.
		/// </summary>
		public static IDbConnectionFactory DefaultConnectionFactory { get { return Provider.DefaultConnectionFactory; } }
		/// <summary>
		/// Creates a backup of the database.
		/// </summary>
		/// <param name="file">The file to backup into.</param>
		public void Backup(string file) { Provider.Backup(this, file); }
		/// <summary>
		/// Restores the database from a backup.
		/// </summary>
		/// <param name="file">The backup file.</param>
		public void Restore(string file) { Provider.Restore(this, file); }
		/// <summary>
		/// Truncates the database.
		/// </summary>
		public void Truncate() { Provider.Truncate(this); }
		/// <summary>
		/// Sets the initialization strategy for the context. Usually you don't call this function and use the Silversite's default intitalization strategy DefaultInitalizer.
		/// </summary>
		/// <typeparam name="TContext">The context type.</typeparam>
		/// <param name="strategy">The database initializer.</param>
		public static void SetInitializer<TContext>(IDatabaseInitializer<TContext> strategy) where TContext: DbContext { Provider.SetInitializer<TContext>(strategy); }

		/// <summary>
		/// Returns true if the database schema is compatible to the current version of the Context.
		/// </summary>
		/// <typeparam name="TContext">The type of the Context.</typeparam>
		/// <returns>True if the database schema is compatible to the current version of the Context.</returns>
		public bool CompatibleWithModel<TContext>() where TContext: Context { return Provider.CompatibleWithModel<TContext>(this); }

		/// <summary>
		/// Migrate the database schema to the current version of the context, according to all DbMigration classes found for this Context.
		/// </summary>
		/// <typeparam name="TContext">The type of the Context.</typeparam>
		public void Update<TContext>()  where TContext: Context { Provider.Update<TContext>(this); }
		/// <summary>
		/// Drops the context. All tables in the database for this context will be dropped.
		/// </summary>
		/// <typeparam name="TContext">The context type.</typeparam>
		public void Drop<TContext>() where TContext: Context { Provider.Drop<TContext>(this); }

		/// <summary>
		/// Returns the DbVersion for the given Context.
		/// </summary>
		/// <typeparam name="TContext">The Context type</typeparam>
		/// <returns>The Version of the database corresponding to this Context.</returns>
		public DbVersion Version<TContext>() where TContext: Context { return Provider.Version<TContext>(this); }

		/// <summary>
		/// Returns true if the database is on the local machine.
		/// </summary>
		public bool IsLocal { get { return Provider.IsLocal(this); } }

		/// <summary>
		/// Return true if the database is offline.
		/// </summary>
		public bool Offline { get { return Provider.Offline(this); } }

		/// <summary>
		/// Gets a list of all databases this database is replicated to.
		/// </summary>
		internal List<Database> ReplicateTo { get { return Provider.ReplicateTo(this); } }
		/// <summary>
		/// Gets the configured default schema for this database.
		/// </summary>
		public string Schema { get { return Provider.Schema(this); } }

		public DatabaseType Type { get { return Provider.Type(this); } }
		/// <summary>
		/// Returns a version string for the database server.
		/// </summary>
		public string ServerVersion { get { return Provider.ServerVersion(this); } }
		/// <summary>
		/// Returns the EntityFramework Version.
		/// </summary>
		public Version Runtime { get { return typeof(DbContext).Assembly.GetName().Version; } }

	}

}