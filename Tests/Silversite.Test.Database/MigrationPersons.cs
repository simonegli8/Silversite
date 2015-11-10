using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using Silversite;

namespace Silversite.EntityTest.Migrations {

	public class PersonContext: DbContext {
		public PersonContext() : base() { }
		public DbSet<Person> Persons { get; set; }
	}

}
