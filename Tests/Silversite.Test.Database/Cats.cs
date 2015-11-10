using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Silversite.EntityTest {

	[Table("Silversite_Test_Database_Cats")]
	public class Cat {
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Key { get; set; }
		public string Name { get; set; }
		public string Color { get; set; }
	}

	public class CatContext: Data.Context<CatContext> {
		public CatContext() : base() { }
		public DbSet<Cat> Cats { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder) {
			base.OnModelCreating(modelBuilder);
			modelBuilder.Entity<Cat>().HasKey(t => new { t.Key, t.Name });
		}
	}

}
