using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Silversite;

namespace Silversite.EntityTest {

	[Table("Silversite_Test_Database_Persons")]
	public class Person {
		[Key]
		public int Key { get; set; }
		[MaxLength(128)]
		public string Name { get; set; }
		[MaxLength(64)]
		public string City { get; set; }
	}

	public class PersonContext: Data.Context<PersonContext> {
		public PersonContext() : base() { }
		public PersonContext(Data.Database db) : base(db) { }
		public DbSet<Person> Persons { get; set; }
	}

}
