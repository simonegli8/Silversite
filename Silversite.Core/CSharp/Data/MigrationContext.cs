// davidegli

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using Silversite.Reflection;

namespace Silversite.Data {

	/// <summary>
	/// A MigrationContext base class that wraps a System.Data.Entity.DbContext around a Silversite.Data.DbContext, so it can be processed by the EntityFramework Powershell command like
	/// Add-Migration and Update-Migration. This class solely serves this purpose.
	/// </summary>
	/// <typeparam name="TContext">The Context type that derives from Silversite.Data.DbContext.</typeparam>
	public class MigrationContext<TContext>: DbContext where TContext: Context {

		public MigrationContext() : base() { }
		public MigrationContext(string conString) : base(conString) { }
		public MigrationContext(System.Data.Common.DbConnection con, bool ownsConnection) : base(con, ownsConnection) { }
		public MigrationContext(System.Data.Entity.Infrastructure.DbCompiledModel model) : base(model) { }
		public MigrationContext(string conString, System.Data.Entity.Infrastructure.DbCompiledModel model) : base(conString, model) { }
		public MigrationContext(System.Data.Objects.ObjectContext context, bool ownsConnection) : base(context, ownsConnection) { }
		public MigrationContext(System.Data.Common.DbConnection con, System.Data.Entity.Infrastructure.DbCompiledModel model, bool ownsConnection) : base(con, model, ownsConnection) { }

		/// <summary>
		/// See <see cref="Silversite.Data.Entity.DbContext.OnModelCreating">Silversite.Data.Entity.DbContext.OnModelCreatingy</see>
		/// </summary>
		/// <param name="model"></param>
		protected override void  OnModelCreating(DbModelBuilder model) {
			base.OnModelCreating(model);

			/*TODO is this code needed?
			// call model.Entity<T>() for all entities
			var entities = typeof(TContext)
				.GetProperties()
				.Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
				.Select(p => p.PropertyType.GetGenericArguments().FirstOrDefault());

			var register = model.Method("Entity");
			foreach (var e in entities) register.Generic(e).Call();
			*/

			var db = Data.Database.Custom(this.Database.Connection);

			Context context;
			try {
				context = New.Object<TContext>(db);
			} catch {
				context = New.Object<TContext>();
			}

			// call context.OnModelCreating(model);
			context.Method("OnModelCreating").Call(model);
		}

	}

}