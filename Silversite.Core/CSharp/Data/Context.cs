// © davidegli

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Web;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;
using Silversite.Reflection;
using System.Reflection;
using Silversite;

namespace Silversite.Data {

	/// <summary>
	/// The base class for all DbContext's used in Silversite and by extensions. This class derives from System.Data.Entity.DbContext but that may change in the future, in order to enable lazy loading of the EntityFramework.
	/// The class has four constructors, two using the default database connection, the others a custom connection described through a Silversite.Data.Database instance.
	/// When implementing an extension for silversite, it makes sense to use on Context per assembly to access the database. (In order to use the nuget migration package, that cannot handle multiple Contexts.)
	/// </summary>
	public class Context: DbContext, IDisposable {

		[ThreadStatic]
		public static Services.WeakValueDictionary<Type, Context> current;
		/// <summary>
		/// The default constructor. Uses Silversite.Data.Database.Default as database connection.
		/// </summary>
		public Context() : base(Database.Default.Connection, true) {
			Database = Database.Default;
			if (current == null) current = new Services.WeakValueDictionary<Type, Context>();
			current[GetType()] = this;
		}
		/// <summary>
		/// Opens a context on the supplied database connection. 
		/// </summary>
		/// <param name="db">The database to connect to.</param>
		public Context(Database db) : base(db.Connection, true) {
			Database = db;
			if (current == null) current = new Services.WeakValueDictionary<Type, Context>();
			current[GetType()] = this;
		}

		/// <summary>
		/// The Database the Context belongs to.
		/// </summary>
		public new Database Database { get; private set; }
		/// <summary>
		/// If this context is created anew in a database Seed is called to seed data. 
		/// </summary>
		public virtual void Seed(DbVersion previousVersion) { }

		internal int BaseSaveChanges() { return base.SaveChanges(); }


		public static T Current<T>() where T: Context, new() {
			Context ctx = null;
			if (current != null && (!current.TryGetValue(typeof(T), out ctx) || ctx.IsDisposed)) ctx = new T();
			return (T)ctx;
		}

		/// <summary>
		/// Saves all changes made to the database.
		/// </summary>
		/// <returns>The number of entity instances affected.</returns>
		public override int SaveChanges() {
			int res = 0;
			/*
			if (Configuration.AutoDetectChangesEnabled) ChangeTracker.DetectChanges();
			
			// replication
			var edict = new Dictionary<object, DbEntityEntry>();
			var changed = ChangeTracker.Entries()
				.Where(e => e.State != System.Data.EntityState.Unchanged)
				.ToList();
			
			int res = 0;

			foreach (var server in Database.ReplicateTo.Where(d => d != Database)) {
				using (var db = (Context)New.Object(GetType(), server)) {
					// replicate changes
					foreach (var e in changed) {
						var set = db.Set(e.Entity.GetType());
						switch (e.State) {
						case System.Data.EntityState.Added: set.Add(e.Entity); break;
						case System.Data.EntityState.Deleted: set.Remove(e.Entity); break;
						case System.Data.EntityState.Modified: set.Attach(e.Entity); break;
						case System.Data.EntityState.Detached:
						case System.Data.EntityState.Unchanged:
						default: break;
						}
						edict.Add(e.Entity, e);
					}
					// set the state of the modified entities we just attached to Modified
					foreach (var uc in db.ChangeTracker.Entries().Where(en => en.State == System.Data.EntityState.Unchanged)) {
						uc.State = System.Data.EntityState.Modified;
						var oe = edict[uc.Entity];
						foreach (var prop in oe.OriginalValues.PropertyNames) {
							uc.OriginalValues[prop] = oe.OriginalValues[prop];
						}
					}
					try {
						res = db.BaseSaveChanges();
					} catch (System.Data.Entity.Validation.DbEntityValidationException eex) {
						Services.Log.Error("Replication Entity Validation Exception on Server {0}", eex, Database.Connection.DataSource);
						foreach (var err in eex.EntityValidationErrors.SelectMany(e => e.ValidationErrors)) {
							Services.Log.Error("Replication Entity Validation Error on Server {0}: {1}", Database.Connection.DataSource, err.ErrorMessage);
						}
						throw eex;
					} catch (Exception ex) {
						if (!db.Database.Offline) {
							Services.Log.Error("Replication Database Exception on Server {0}", ex, Database.Connection.DataSource);
							throw ex;
						}
					}
				}
			}

			if (Database.ReplicateTo.Count == 0 || Database.ReplicateTo.Contains(Database)) {
			*/
				try {
					res = base.SaveChanges();
				} catch (System.Data.Entity.Validation.DbEntityValidationException eex) {
					Services.Log.Error("Entity Validation Exception", eex);
					foreach (var err in eex.EntityValidationErrors.SelectMany(e => e.ValidationErrors)) {
						Services.Log.Error("Entity Validation Error: {0}", err.ErrorMessage);
					}
					throw eex;
				} catch (Exception ex) {
					Services.Log.Error("Database Exception", ex);
					throw ex;
				}
			//}
			return res;
		}

		protected override bool ShouldValidateEntity(System.Data.Entity.Infrastructure.DbEntityEntry entityEntry) {
			// Required to prevent bug - http://stackoverflow.com/questions/5737733
			if (Database.Connection.GetType().FullName == "System.Data.SqlServerCe.SqlCeConnection") {
				var res = entityEntry.State != System.Data.EntityState.Deleted && !entityEntry.CurrentValues.PropertyNames.Any(n => entityEntry.CurrentValues[n] is byte[]);
				return res;
			}
			return base.ShouldValidateEntity(entityEntry);
		}
		/// <summary>
		/// Detaches an entity from the Context, so it can be added to another Context.
		/// </summary>
		/// <param name="entity">The entity to detach.</param>
		public void Detach(object entity) {
			((System.Data.Entity.Infrastructure.IObjectContextAdapter)this).ObjectContext.Detach(entity);
		}
		/// <summary>
		/// When calling DependsOn in a OnModelBuilding method, it's possible to initialize or migrate other Contexts that this Context depends upon first.
		/// </summary>
		/// <typeparam name="TContext"></typeparam>
		public virtual void DependsOn<TContext>() where TContext: Context, new() {
			Action<Database> prev = null;
			if (Updater.TryGetValue(typeof(TContext), out prev)) Updater[typeof(TContext)] = db => { db.Update<TContext>(); prev(db); };
			else Updater.Add(typeof(TContext), (Database db) => db.Update<TContext>());
		}

		static Dictionary<Tuple<Type, Database>, System.IO.MemoryStream> edmx = new Dictionary<Tuple<Type,Database>,System.IO.MemoryStream>();
		static Dictionary<Tuple<Type, Database>, int> hash = new Dictionary<Tuple<Type, Database>, int>();

		Tuple<Type, Database> Key { get { return new Tuple<Type, Database>(GetType(), Database); } }

		System.IO.MemoryStream RawEdmx {
			get {
				System.IO.MemoryStream m = null;
				if (	!edmx.TryGetValue(Key, out m)) {
					m = edmx[Key] = new System.IO.MemoryStream();
					using (var xmlWriter = System.Xml.XmlWriter.Create(
						m,
						new System.Xml.XmlWriterSettings { Indent = true })) {
						EdmxWriter.WriteEdmx(this, xmlWriter);
					}
				}
				return m;
			}
		}

		internal string Edmx { get { return new System.IO.StreamReader(RawEdmx, System.Text.Encoding.UTF8, true).ReadToEnd(); } }

		internal byte[] EdmxGZipped {
			get {
				using (var memoryStream = new System.IO.MemoryStream()) {
					using (var gzipStream = new System.IO.Compression.GZipStream(
						memoryStream,
						System.IO.Compression.CompressionMode.Compress)) {
						RawEdmx.CopyTo(gzipStream);
					}

					return memoryStream.ToArray();
				}
			}
		}

		internal int ModelHash {
			get {
				int h;
				if (!hash.TryGetValue(Key, out h)) {
					hash[Key] = h = Services.Hash.Compute(RawEdmx.ToArray());
				}
				return h;
			}
		}

		internal static Dictionary<Type, Action<Database>> Updater = new Dictionary<Type, Action<Database>>(); 

		internal bool IsShared = false;
		internal int StackLevel = 0;
		internal System.DateTime Age = System.DateTime.Now;
		internal System.Data.StateChangeEventHandler KeepUpConnection = null;
		internal Tuple<HttpContext, Thread, Type, Database> PoolKey = null;
		internal bool Truncating = false;
		internal bool IsStack = true;
		internal object ContextLock = new object();
		internal int Uses = 0;

		/* not working yet
		public static TContext Open<TContext>(Database db) where TContext: Context { return ContextPool.Open<TContext>(db); }
		public static TContext Open<TContext>() where TContext: Context { return ContextPool.Open<TContext>(); }
		public static TContext OpenPage<TContext>(Database db) where TContext: Context { return ContextPool.OpenPage<TContext>(db); }
		public static TContext OpenPage<TContext>() where TContext: Context { return ContextPool.OpenPage<TContext>(); }
		public static TContext OpenStack<TContext>() where TContext: Context { return ContextPool.OpenStack<TContext>(); }
		public static TContext OpenStack<TContext>(Database db) where TContext: Context { return ContextPool.OpenStack<TContext>(db); }
		*/
 
		internal long MemoryFootprint(object entity) {
			var type = entity.GetType();
			var ps = type.GetProperties(System.Reflection.BindingFlags.Public).Where(p => p.CanWrite && p.GetIndexParameters().Length == 0);
			long sum = 0;
			foreach (var p in ps) {
				var pt = p.PropertyType;
				var x = p.GetValue(entity);
				if (pt == typeof(string)) {
					sum += ((string)x).Length*2;
				} else if (pt.IsClass) {
					DbEntityEntry childentity = null;
					try {
						childentity = Entry(x);
					} catch { }
					if (childentity != null) continue;
					// otherwise complex type
					sum += MemoryFootprint(x);
				} else if (pt.IsValueType) {
					if (pt == typeof(int)) sum += sizeof(int);
					else if (pt == typeof(bool)) sum += sizeof(bool);
					else if (pt == typeof(short)) sum += sizeof(short);
					else if (pt == typeof(long)) sum += sizeof(long);
					else if (pt == typeof(System.TimeSpan)) sum += sizeof(long);
					else if (pt == typeof(System.DateTime)) sum += sizeof(long);
					else if (pt == typeof(double)) sum += sizeof(double);
					else if (pt == typeof(float)) sum += sizeof(float);
					else if (pt == typeof(char)) sum += sizeof(char);
				} else if (pt.IsArray && pt.GetElementType() == typeof(byte)) { // BLOB
					sum += ((byte[])x).Length;
				}
			}
			return sum;
		}
		/*
		public void Forget() {
			
			// clear all Local sets
			var sets = GetType()
				.GetProperties()
				.Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));
			
			foreach (System.Reflection.PropertyInfo set in sets) {
				var local = set.PropertyType.GetProperty("Local");
				var o = set.GetValue(this);
				local.PropertyType.Invoke(o, "Clear");
			}
		}

		public void Truncate() {
			if (!IsStack && PoolKey != null && PoolKey.Item1 == null) {
				bool truncate = false;
				lock (ContextLock) {
					if (!Truncating) Truncating = truncate = true;
				}
				if (truncate) {
					if (Uses % ContextPool.MaxUses == 0) Forget();
					else {
						Services.Tasks.DoLater(2000, () => {
							Services.Stopwatch.Start("Check DbContext MemoryFoortprint");
							var entities = ChangeTracker.Entries();
							long n = 0, mem = 0;
							truncate = false;
							foreach (var e in entities) {
								n++;
								if (n > ContextPool.MaxEntities) {
									truncate = true;
									break;
								}
								mem += MemoryFootprint(e);
								if (mem > ContextPool.MaxContextMemory) {
									truncate = true;
									break;
								}
							}
							Services.Stopwatch.Stop("Check DbContext MemoryFootprint");
							if (truncate) Forget();
							Truncating = false;
						});
					}
				}
			}
		}
		*/
		public bool IsDisposed = false;

		public void Dispose() {
			IsDisposed = true;
			base.Dispose();

			/*
			DisposeLocks();

			if (StackLevel > 0) {
				ContextPool.CloseStack(this);
				if (StackLevel == 0) Database.Connection.StateChange -= KeepUpConnection;
				Uses++;
				Truncate();
			} else {
				ContextPool.CloseStack(this);
				IsStack = false;
				if (IsShared) {
					Truncate();
				} else
				{ 
					IsDisposed = true;
					base.Dispose();
				}
			}*/

		}

		class LockInfo {
			public int StackLevel;
			public IDisposable Lock;
			public LockInfo(IDisposable lk, Context c) { Lock = lk; StackLevel = c.StackLevel; }
		}
		
		List<LockInfo> Locks = new List<LockInfo>();

		void DisposeLocks() {
			foreach (var lo in Locks.Where(lk => lk.StackLevel >= StackLevel).ToList()) {
				lo.Lock.Dispose();
				Locks.Remove(lo);
			}
		}
		/* not working yet
		public readonly static TimeSpan DefaultLockTimeOut = 2.Minutes();
		public virtual TimeSpan LockTimeOut { get; set; }

		public IDisposable Lock<T>() { var lk = Lock(typeof(T)); Locks.Add(new LockInfo(lk, this)); return lk; }
		public IDisposable Lock(Type type) {
			IDisposable lk;
			if (Database.IsLocal) lk = Services.Application.Lock(LockTimeOut, System.Environment.MachineName, Database.DbProviderName, Database.ConnectionString, type);
			else lk =  Services.Application.Lock(LockTimeOut, Database.DbProviderName, Database.ConnectionString, type);
			Locks.Add(new LockInfo(lk, this));
			return lk;
		}
		public IDisposable Lock() {
			IDisposable lk;
			if (Database.IsLocal) lk = Services.Application.Lock(LockTimeOut, System.Environment.MachineName, Database.DbProviderName, Database.ConnectionString);
			else lk = Services.Application.Lock(LockTimeOut, Database.DbProviderName, Database.ConnectionString);
			Locks.Add(new LockInfo(lk, this));
			return lk;
		}
		*/
		/// <summary>
		/// Creates the EDMX Model for this Context <see cref="System.Data.Entity.DbContext.OnModelCreating">OnModelCreating of EntityFramework DbContext.</see>
		/// </summary>
		/// <param name="model">The Model to initialize.</param>
		protected override void OnModelCreating(DbModelBuilder model) {

			// start silversite startup code.
			Services.Modules.DependsOn<Data.ContextPool>();
			Services.Modules.DependsOn<Services.Lazy>();
			Services.Modules.DependsOn<Services.Providers>();
			Services.Modules.DependsOn<Data.EntityDatabaseProvider>();


			base.OnModelCreating(model);

			// change default table names
			var entities = GetType()
				.GetProperties()
				.Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
				.Select(p => p.PropertyType.GetGenericArguments().FirstOrDefault());

			var register = model.Method("Entity"); // rename tables to fully qualified type names.
			foreach (Type e in entities) {
				if (e.GetAttribute<TableAttribute>() == null) {
					// call model.Entity<T>()
					var def = register.Generic(e).Call();
					// call model.Entity<T>().ToTable(...)
					var name = e.FullName.Replace('.', '_');
					var schema = Database.Schema;
					//if (!string.IsNullOrEmpty(schema) && Database.Type == DatabaseType.SqlCe) { // this code 
					if (!string.IsNullOrEmpty(schema)) {
						name = schema + "_" + name;
						schema = null;
					}
					def.Method<string, string>("ToTable").Call(name, schema);
				} /*else if (e.IsSubclassOf(typeof(Data.MigrationHistory)) && Database.Type == DatabaseType.SqlCe) {
					var def = register.Generic(e).Call();
					def.Method<string, string>("ToTable").Call("__MigrationHistory", null);
				} */
			}
		}

		~Context() { ContextPool.CloseStack(this); }

		static int id = 0;
		public int ContextId = id++;
	}

	/// <summary>
	/// A Context class, that automatically calls Database.Open on itself when instantiating. Use this class to derive your own DbContext classes from.
	/// </summary>
	/// <typeparam name="TSelf">The type of the instance that derives from this class.</typeparam>
	public class Context<TSelf>: Context where TSelf: Context {
		public Context() : base() { Database.Open<TSelf>(this); }
		public Context(Database db) : base(db) { Database.Open<TSelf>(this); }
	}

	// TODO not yet working, so we make it internal
	internal class ContextPool: Services.IAutostart {

		public const long MB = 1024*1024;
		public const long MaxContextMemory = 32*MB;
		public const long MaxEntities = 100;
		public const int MaxUses = 20;

		public static ConcurrentDictionary<Tuple<HttpContext, Thread, Type, Database>, Context> Contexts = new ConcurrentDictionary<Tuple<HttpContext, Thread, Type, Database>, Context>();
		[ThreadStatic]
		public static Stack<Context> stack = null;

		public static Stack<Context> Stack { get { if (stack == null) stack = new Stack<Context>(); return stack; } }

		public static bool IsShared(Context c) {
			var key = new Tuple<HttpContext, Thread, Type, Database>(null, Thread.CurrentThread, c.GetType(), c.Database);
			return Contexts.ContainsKey(key);
		}

		static readonly System.TimeSpan MaxAge = 10.Minutes(); // ten minutes

		public static TContext Open<TContext>(HttpContext hc, Database db) where TContext: Context {
			var key = new Tuple<HttpContext, Thread, Type, Database>(hc, Thread.CurrentThread, typeof(TContext), db);
			Context context = null;
			lock (Contexts) {
				if (!Contexts.TryGetValue(key, out context)) {
					context = New.Object<TContext>(db);
					context.IsShared = hc == null;
					Contexts.TryAdd(key, context);
				}
			}

			if (context.StackLevel > 1) {
				// context = New.Object<TContext>(db);
				context.StackLevel++;
			} else {
				context.StackLevel++;

				context.KeepUpConnection = new System.Data.StateChangeEventHandler((sender, e) => {
					var con = (DbConnection)sender;
					if (e.CurrentState == System.Data.ConnectionState.Broken) con.Close();
					if (e.CurrentState == System.Data.ConnectionState.Closed) {
						try { con.Open(); } catch { }
						context.Age = System.DateTime.Now;
					}
				});
				context.Database.Connection.StateChange += context.KeepUpConnection;

				if (context.Database.Connection.State == System.Data.ConnectionState.Broken) context.Database.Connection.Close();
				if ((System.DateTime.Now - context.Age) > MaxAge && context.Database.Connection.State != System.Data.ConnectionState.Closed) context.Database.Connection.Close();
				if (context.Database.Connection.State == System.Data.ConnectionState.Closed) {
					try { context.Database.Connection.Open(); } catch { }
					context.Age = System.DateTime.Now;
				}
			}
			context.PoolKey = key; 
			return (TContext)context;
		}

		public static TContext Open<TContext>(Database db) where TContext: Context { return Open<TContext>(null, db); }
		public static TContext Open<TContext>() where TContext: Context { return Open<TContext>(null, Database.Default); }
		public static TContext OpenPage<TContext>(Database db) where TContext: Context { return Open<TContext>(HttpContext.Current, db); }
		public static TContext OpenPage<TContext>() where TContext: Context { return Open<TContext>(HttpContext.Current, Database.Default); }

		public static TContext OpenStack<TContext>(Database db) where TContext: Context {
			var ctx = Stack.OfType<TContext>().FirstOrDefault();
			
			if (ctx == null) ctx = New.Object<TContext>(db);
			else ctx.StackLevel++;

			ctx.IsStack = true;

			return ctx;
		}
		public static TContext OpenStack<TContext>() where TContext: Context { return OpenStack<TContext>(Database.Default); }

		internal static void CloseStack(Context c) {
			c.StackLevel--;
			if (Stack.Count == 0) return;
			if (Stack.Peek() == c) Stack.Pop();
			else if (Stack.Contains(c)) {
				// remove c from Stack
				var tmp = new Stack<Context>();
				var t = Stack.Pop();
				while (t != c && Stack.Count > 0) {
					tmp.Push(t);
					t = Stack.Pop();
				}
				while (tmp.Count > 0) Stack.Push(tmp.Pop());
			}
		}

		public long MemoryFootprint(Context context) {
			var entities = context.ChangeTracker.Entries();
			return entities.Sum(e => context.MemoryFootprint(e));
		}

		public void Startup() {
			Services.Tasks.Recurring(2*MaxAge.Milliseconds, () => {
				// periodically dispose contexts.
				var list = Contexts.Keys.ToList();
				foreach (var key in list) {
					Context context;
					if (Contexts.TryRemove(key, out context) && context.StackLevel == 0) { context.IsShared = false; context.Dispose(); }
				}
			});
		}

		public void Shutdown() {
			while (Contexts.Count > 0) {
				Context context;
				if (Contexts.TryRemove(Contexts.Keys.First(), out  context)) { context.IsShared = false; context.StackLevel = 0; context.Dispose(); }
			}
		}
	}

}