using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Silversite.Web.UI {

	public class UsersSource: DbDataSource<Homesell.Data.Context, Homesell.Data.UserInfo> {

		public UsersSource() {
			Where = set => set.OrderBy(u => new { u.Name, Vorname = u.FirstName, Ort = u.City, Adresse = u.Address }).AsQueryable();
		}

	}

}