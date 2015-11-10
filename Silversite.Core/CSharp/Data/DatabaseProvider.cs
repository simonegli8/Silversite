// davidegli

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Silversite.Data {

	public abstract class DatabaseProvider: Services.Provider<Data.Database> {
		/// <summary>
		/// Returns a Database that uses the passed connection parameters
		/// </summary>
		/// <param name="providerName">The provider class type name, for example "System.Data.SqlClient".</param>
		/// <param name="connectionString">The connection string</param>
		/// <returns>Returns a Database that uses the passed connection parameters</returns>
		public abstract Data.Database Custom(string DbFactoryType, string connectionString);
		/// <summary>
		/// Returns a Database that uses the passed connection.
		/// </summary>
		/// <param name="con">The connection.</param>
		/// <returns>Returns a Database that uses the passed connection.</returns>
		public abstract Data.Database Custom(DbConnection con);
		/// <summary>
		/// Returns a Database that uses the passed named connection string either from Database.config or web.config.
		/// </summary>
		/// <param name="con">The connection.</param>
		/// <returns>Returns a Database that uses the passed named connection string.</returns>
		public abstract Database Custom(string connectionStringName);	/// <summary>
		/// Returns a Database that uses the passed named connection string either from Database.config or web.config and uses the schema
		/// as default schema.
		/// </summary>
		/// <param name="con">The connection.</param>
		/// <returns>Returns a Database that uses the passed named connection string.</returns>
		public abstract Database CustomWithSchema(string connectionStringName, string schema);
		/// <summary>
		/// Returns a Database that uses the passed connection parameters
		/// </summary>
		/// <param name="con">The connection.</param>
		/// <returns>Returns a Database that uses the passed named connection string.</returns>
		public abstract Database CustomWithSchema(string DbFactoryType, string connectionString, string schema);

		/// <summary>
		/// Gets the default database
		/// </summary>
		public abstract Data.Database Default { get; }
		/// <summary>
		/// Initializes the context
		/// </summary>
		/// <typeparam name="TContext">The context type</typeparam>
		/// <param name="db">The database</param>
		/// <param name="context">The contex</param>
		public abstract void Open<TContext>(Data.Database db, Data.Context context) where TContext: Data.Context;
		/// <summary>
		/// Returns true if the database exists.
		/// </summary>
		/// <param name="db">The database</param>
		/// <returns>True if the database exits.</returns>
		public abstract bool Exists(Data.Database db);
		/// <summary>
		/// Creates the database 
		/// </summary>
		/// <param name="db">The database</param>
		public abstract void Create(Data.Database db);
		/// <summary>
		/// Drops the database
		/// </summary>
		/// <param name="db">The database.</param>
		public abstract void Delete(Data.Database db);
		/// <summary>
		/// Creates the database if it doesn't exist.
		/// </summary>
		/// <param name="db">The database</param>
		/// <returns>True if the database was created</returns>
		public abstract bool CreateIfNotExists(Data.Database db);
		// public abstract bool CompatibleWithModel(Data.Database db, bool throwIfNoMetadata);
		/// <summary>
		/// Executes an SQL command on the database.
		/// </summary>
		/// <param name="db">The database</param>
		/// <param name="sql">The SQL command</param>
		/// <param name="parameters">Parameters for the SQL command</param>
		/// <returns>Returns the number of affected rows.</returns>
		public abstract int ExecuteSqlCommand(Data.Database db, string sql, params object[] parameters);
		/// <summary>
		/// Inititalizes the database. This method only creates the database. The tables are only created, when the corresponding context's are first accessed.
		/// </summary>
		/// <param name="db">The database</param>
		/// <param name="force">Has no effect.</param>
		public abstract void Initialize(Data.Database db, bool force);
		/// <summary>
		/// Executes an SQL query on the database
		/// </summary>
		/// <typeparam name="TElement">The entity element type</typeparam>
		/// <param name="db">The database</param>
		/// <param name="sql">The SQL to execute</param>
		/// <param name="parameters">The parameters for the SQL command</param>
		/// <returns>An IEnumerable with the query data</returns>
		public abstract IEnumerable<TElement> SqlQuery<TElement>(Data.Database db, string sql, params object[] parameters);
		/// <summary>
		/// Executes an SQL query on the database
		/// </summary>
		/// <param name="db">The database</param>
		/// <param name="elementType">The table's entity element type</param>
		/// <param name="sql">The SQL command</param>
		/// <param name="parameters">The command's parameters</param>
		/// <returns>Returns an IEnumerable with the query's data.</returns>
		public abstract IEnumerable SqlQuery(Data.Database db, Type elementType, string sql, params object[] parameters);
		/// <summary>
		/// Gets the DbConnection for the database
		/// </summary>
		/// <param name="db">The database</param>
		/// <returns>The DbConnection</returns>
		public abstract DbConnection Connection(Data.Database db);
		/// <summary>
		/// Gets the default connection factory
		/// </summary>
		public abstract IDbConnectionFactory DefaultConnectionFactory { get; }
		/// <summary>
		/// Creates a backup of the database
		/// </summary>
		/// <param name="db">The database</param>
		/// <param name="file">The file to backup into</param>
		public abstract void Backup(Data.Database db, string file);
		/// <summary>
		/// Restores the database from a backup
		/// </summary>
		/// <param name="db">The database</param>
		/// <param name="file">The backup file</param>
		public abstract void Restore(Data.Database db, string file);
		/// <summary>
		/// Truncates the database
		/// </summary>
		/// <param name="db">The database</param>
		public abstract void Truncate(Data.Database db);
		/// <summary>
		/// Sets the initialization strategy for the context. Usually you don't call this function and use the Silversite's default intitalization strategy DefaultInitalizer
		/// </summary>
		/// <typeparam name="TContext">The context type</typeparam>
		/// <param name="strategy">The database initializer</param>
		public abstract void SetInitializer<TContext>(IDatabaseInitializer<TContext> strategy) where TContext: DbContext;
		/// <summary>
		/// Migrate the database to another version of the context.
		/// </summary>
		/// <typeparam name="TContext">The context type</typeparam>
		/// <param name="db">The database</param>
		public abstract void Update<TContext>(Database db) where TContext: Data.Context;
		/// <summary>
		/// Drops the context. All tables in the database for this context will be dropped.
		/// </summary>
		/// <typeparam name="TContext">The context type</typeparam>
		/// <param name="db">The database</param>
		public abstract void Drop<TContext>(Data.Database db) where TContext: Data.Context;
		/// <summary>
		/// Returns true if the schema of the database is compatible with the current version of the Context.
		/// </summary>
		/// <typeparam name="TContext">The context type.</typeparam>
		/// <param name="db">The database</param>
		/// <returns>True if the schema of the database is compatible with the current version of the Context.</returns>
		public abstract bool CompatibleWithModel<TContext>(Data.Database db) where TContext: Data.Context;
		/// <summary>
		/// All Context types that have a schema in the database. To get instances of the Contexts use Services.Lazy.Types.New(contextName, db)
		/// where contextName is the type name of the Context as returned by Contexts, and db is the Database to connect with.
		/// </summary>
		/// <param name="db">The database.</param>
		/// <returns>All Context types that have a schema in the database.</returns>
		public abstract IEnumerable<string> Contexts(Database db);

		public abstract DbVersion Version<TContext>(Database db) where TContext: Context;

		public abstract bool IsLocal(Database db);

		public abstract bool Offline(Database db);

		internal abstract List<Database> ReplicateTo(Database db);

		public abstract string Schema(Database db);

		public abstract DatabaseType Type(Database db);

		public abstract string ServerVersion(Database db);
	}

}