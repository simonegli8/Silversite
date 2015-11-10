// davidegli

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using System.Data.Entity;

namespace Silversite.Data {

	/// <summary>
	/// This class is used to access the migration history table in the database created by the EntityFramework.
	/// </summary>
	[Table("__MigrationHistory", Schema="dbo")]
	internal class MigrationHistory {
		[Key, MaxLength(255)]
		public string MigrationId { get; set; }
		[Required, MaxLength]
		public byte[] Model { get; set; }
		[Required, MaxLength(32)]
		public string ProductVersion { get; set; }

		public static MigrationHistory Empty {
			get {
				var model = "";
				var modelbytes = System.Text.Encoding.UTF8.GetBytes(model);
				return new MigrationHistory {
					MigrationId = "201205031552190_EmtyDatabase_v1.0",
					ProductVersion = DbVersion.Zero<MigrationContext>().FrameworkVersion.ToString(),
					Model = modelbytes,
				};
			}
		}
	}
	/// <summary>
	/// This class is a table, where silversite stores the data from the EntityFramework migration history table per context.
	/// </summary>
	//[Table("Silversite_ContextMigrationHistory")]
	internal class ContextMigrationHistory {
		public ContextMigrationHistory(): base() { }
		public ContextMigrationHistory(Type context, MigrationHistory m): this() {
			Context = context.InvariantName();
			MigrationId = m.MigrationId;
			Model = m.Model;
			ProductVersion = m.ProductVersion;
		}

		[Key]
		public int Key { get; set; }
		[MaxLength(255)]
		public string Context { get; set; }
		[MaxLength(255)]
		public string MigrationId { get; set; }
		[Required]
		public byte[] Model { get; set; }
		[Required, MaxLength(32)]
		public string ProductVersion { get; set; }

		public static implicit operator MigrationHistory(ContextMigrationHistory cmh) {
			return new MigrationHistory { MigrationId = cmh.MigrationId, Model = cmh.Model, ProductVersion = cmh.ProductVersion };
		}
	}
	/// <summary>
	/// This table stores the schema versions per Context.
	/// </summary>
	//[Table("Silversite_ContextVersions")]
	internal class ContextVersion {
		/// <summary>
		/// The type's FullName of the Context
		/// </summary>
		[Key, MaxLength(255)]
		public string Context { get; set; }
		/// <summary>
		/// The Context's schema version. (The number of explicit migrations for that context)
		/// </summary>
		public int Migrations { get; set; }
		/// <summary>
		/// A hash of the edmx model.
		/// </summary> 
		public int ModelHash { get; set; }

		[Required, MaxLength(32)]
		public string FrameworkVersion { get; set; }
	}

	internal class EFMigrationContext: Context<EFMigrationContext> {
		/// <summary>
		/// The entity framework dbo.__MigrationHistory table.
		/// </summary>
		public DbSet<MigrationHistory> MigrationHistory { get; set; }
	}

	/// <summary>
	/// A database context that stores information about context schema versions. Every silversite database has at least this context and its schema.
	/// </summary>
	internal class MigrationContext: Context<MigrationContext> {
		public MigrationContext() : base() { }
		public MigrationContext(Database db) : base(db) { }

		/// <summary>
		/// The schema versions of Contexts accessing the database. 
		/// </summary>
		public DbSet<ContextVersion> ContextVersions { get; set; }
		/// <summary>
		/// The entity framework dbo.__MigrationHistory table.
		/// </summary>
		public DbSet<MigrationHistory> MigrationHistory { get; set; }
		/// <summary>
		/// A per Context copy of the entity framework dbo.__MigrationHistory table.
		/// </summary>
		public DbSet<ContextMigrationHistory> ContextMigrationHistory { get; set; }

		public void SaveMigrations<TContext>() where TContext: Context { // backup entity framework migration history.
			var typename = typeof(TContext).InvariantName();
			ContextMigrationHistory.Remove(h => h.Context == typename);
			var mh = MigrationHistory.ToList().Where(h => h.MigrationId != Data.MigrationHistory.Empty.MigrationId); 
			ContextMigrationHistory.AddRange(mh.Select(h => new ContextMigrationHistory(typeof(TContext), h)));
			SaveChanges();
		}

		public void RestoreMigrations<TContext>() where TContext: Context { // set entity framework migration history to the Context.
			var migrationContext = typeof(TContext) == typeof(MigrationContext);
			/*if (migrationContext && Database.Version<TContext>().Migrations == 0) { // only restore entity migration history if this is not the initial create migration.
				return;
			}*/
			MigrationHistory.RemoveAll();

			var typename = typeof(TContext).InvariantName();
			var list = ContextMigrationHistory.Where(cmh => cmh.Context == typename).ToList();
			if (list.Count > 0) {
				MigrationHistory.AddRange(list.Select(mh => (MigrationHistory)mh));
			} else {
				MigrationHistory.Add(Data.MigrationHistory.Empty);
			}
			SaveChanges();
		}

		/*
		public static MigrationContext Open() { return Data.Context.Open<MigrationContext>(); }
		public static MigrationContext Open(Data.Database db) { return Data.Context.Open<MigrationContext>(db); }
		public static MigrationContext OpenPage() { return Data.Context.OpenPage<MigrationContext>(); }
		public static MigrationContext OpenPage(Data.Database db) { return Data.Context.OpenPage<MigrationContext>(db); }
		public static MigrationContext OpenStack() { return Data.Context.OpenStack<MigrationContext>(); }
		public static MigrationContext OpenStack(Data.Database db) { return Data.Context.OpenStack<MigrationContext>(db); }
		 */

	}

}