using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Silversite.Web.UI {

	public class UsersSource: DbDataSource<Silversite.Context, Services.Person> {

		string roles;
		string[] rolesList;
		public string Roles { get { return (string)ViewState["Roles"]; } set { ViewState["Roles"] = value; rolesList = roles.SplitList(',', ';').ToArray(); } }

		public UsersSource() {
			Where = set => set.ToList().Where(u => string.IsNullOrEmpty(Roles) || u.Roles.Any(role => rolesList.Contains(role))).OrderBy(u => new { u.LastName, u.FirstName, u.City, u.Address }).AsQueryable();
		}

	}

}