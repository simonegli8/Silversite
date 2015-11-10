using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Silversite.EntityTest {

	[Table("Silversite_Test_Database_Cars")]
	public class Car {
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Key { get; set; }
		[MaxLength(64)]
		public string Model { get; set; }
	}

	public class CarContext: Data.Context<CarContext> {
		public CarContext() : base() { }
		public CarContext(Data.Database db) : base(db) { }
		public DbSet<Car> Cars { get; set; }
	}

}
