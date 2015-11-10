using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

namespace Silversite.EntityTest.Migrations {

	public class CatContext: DbContext {
		public CatContext() : base() { }
		public DbSet<Cat> Cats { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder) {
			base.OnModelCreating(modelBuilder);
			modelBuilder.Entity<Cat>().HasKey(t => new { t.Key, t.Name });
		}
	}

}
