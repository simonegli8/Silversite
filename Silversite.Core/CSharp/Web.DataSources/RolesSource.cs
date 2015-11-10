using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace Silversite.Web.UI {

	public class RolesSource: GenericDataSource {

		public override void OnExecuteSelect(GenericSelectArgs a) {
			a.SetData(Roles.GetAllRoles());
		}
		public override void OnExecuteDelete(GenericKeyDataArgs a) {
			var role = a.Values.OfType<string>().FirstOrDefault();
			if (role != null) Roles.DeleteRole(role);
		}
		public override void OnExecuteInsert(GenericDataArgs a) {
			var role = a.Values.OfType<string>().FirstOrDefault();
			var roles = Roles.GetAllRoles();
			if (role != null && !roles.Contains(role)) Roles.CreateRole(role);
		}
		public override void OnExecuteUpdate(GenericUpdateArgs a) {
			var role = a.OldValues.OfType<string>().FirstOrDefault();
			var newrole = a.Values.OfType<string>().FirstOrDefault();
			var roles = Roles.GetAllRoles();
			if (role != null && role != newrole && !roles.Contains(newrole)) {
				Roles.CreateRole(newrole);
				var users = Roles.GetUsersInRole(role);
				Roles.AddUsersToRole(users, newrole);
				Roles.DeleteRole(role);
			}
		}

	}

}