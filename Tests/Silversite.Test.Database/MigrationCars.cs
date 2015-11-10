using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

namespace Silversite.EntityTest.Migrations {

	public class CarContext: DbContext {
		public CarContext() : base() { }
		public DbSet<Car> Cars { get; set; }
	}

}
