using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using Silversite.Web.Providers;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Silversite.Web.Providers {

	public class Role {
		//Membership required
		[Key]
		public virtual Guid RoleId { get; set; }
		[Required]
		public int ApplicationKey { get; set; }
		[Required, ForeignKey("ApplicationKey")]
		public Application Application { get; set; }
		[Required, MaxLength(128)]
		public virtual string RoleName { get; set; }

		public virtual ICollection<User> Users { get; set; }

		//Optional
		[MaxLength(255)]
		public virtual string Description { get; set; }
	}

	public class RoleProvider: System.Web.Security.RoleProvider {

		private string _ApplicationName;
		public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config) {
			if (config == null) {
				throw new ArgumentNullException("config");
			}
			if (string.IsNullOrEmpty(name)) {
				name = "SilversiteRoleProvider";
			}
			if (string.IsNullOrEmpty(config["description"])) {
				config.Remove("description");
				config.Add("description", "Silversite Role Provider");
			}

			base.Initialize(name, config);

			ApplicationName = config["applicationName"];
		}

		public override string ApplicationName {
			get { return _ApplicationName; }
			set { Application.Set<RoleProvider>(_ApplicationName = value); }
		}

		public override bool RoleExists(string roleName) {
			if (string.IsNullOrEmpty(roleName)) {
				throw CreateArgumentNullOrEmptyException("roleName");
			}
			using (var db = new Context()) {
				var result = db.AppRoles<RoleProvider>().FirstOrDefault(r => r.RoleName == roleName);
				if (result != null) {
					return true;
				} else {
					return false;
				}
			}
		}

		public override bool IsUserInRole(string userName, string roleName) {
			if (string.IsNullOrEmpty(userName)) {
				throw CreateArgumentNullOrEmptyException("userName");
			}
			if (string.IsNullOrEmpty(roleName)) {
				throw CreateArgumentNullOrEmptyException("roleName");
			}
			using (var db = new Context()) {
				var user = db.AppUsers<RoleProvider>().FirstOrDefault(u => u.UserName == userName);
				if (user == null) {
					return false;
				}
				var role = db.AppRoles<RoleProvider>().FirstOrDefault(r => r.RoleName == roleName);
				if (role == null) {
					return false;
				}
				return user.Roles.Contains(role);
			}
		}

		public override string[] GetAllRoles() {
			using (var db = new Context()) {
				return db.AppRoles<RoleProvider>().Select(r => r.RoleName).ToArray();
			}
		}

		public override string[] GetUsersInRole(string roleName) {
			if (string.IsNullOrEmpty(roleName)) {
				throw CreateArgumentNullOrEmptyException("roleName");
			}
			using (var db = new Context()) {
				var role = db.AppRoles<RoleProvider>().FirstOrDefault(r => r.RoleName == roleName);
				if (role == null) {
					throw new InvalidOperationException("Role not found");
				}
				return role.Users.Select(u => u.UserName).ToArray();
			}
		}

		public override string[] FindUsersInRole(string roleName, string usernameToMatch) {
			if (string.IsNullOrEmpty(roleName)) {
				throw CreateArgumentNullOrEmptyException("roleName");
			}
			if (string.IsNullOrEmpty(usernameToMatch)) {
				throw CreateArgumentNullOrEmptyException("usernameToMatch");
			}
			using (var db = new Context()) {
				var query = from Rl in db.AppRoles<RoleProvider>()
							from Usr in Rl.Users
							where (Rl.RoleName == roleName && Usr.UserName.Contains(usernameToMatch))
							select Usr.UserName;
				return query.ToArray();
			}
		}

		public override bool DeleteRole(string roleName, bool throwOnPopulatedRole) {
			if (string.IsNullOrEmpty(roleName)) {
				throw CreateArgumentNullOrEmptyException("roleName");
			}
			using (var db = new Context()) {
				var role = db.AppRoles<RoleProvider>().FirstOrDefault(Rl => Rl.RoleName == roleName);
				if (role == null) {
					throw new InvalidOperationException("Role not found");
				}
				if (throwOnPopulatedRole) {
					var usersInRole = role.Users.Any();
					if (usersInRole) {
						throw new InvalidOperationException(string.Format("Role populated: {0}", roleName));
					}
				} else {
					foreach (User usr_loopVariable in role.Users) {
						var usr = usr_loopVariable;
						db.Users.Remove(usr);
					}
				}
				db.Roles.Remove(role);
				db.SaveChanges();
				return true;
			}
		}

		public override string[] GetRolesForUser(string userName) {
			if (string.IsNullOrEmpty(userName)) {
				throw CreateArgumentNullOrEmptyException("userName");
			}
			using (var db = new Context()) {
				var user = db.AppUsers<RoleProvider>().FirstOrDefault(u => u.UserName == userName);
				if (user == null) {
					throw new InvalidOperationException(string.Format("User not found: {0}", userName));
				}
				return user.Roles.Select(Rl => Rl.RoleName).ToArray();
			}
		}

		public override void CreateRole(string roleName) {
			if (string.IsNullOrEmpty(roleName)) {
				throw CreateArgumentNullOrEmptyException("roleName");
			}
			using (var db = new Context()) {
				var role = db.AppRoles<RoleProvider>().FirstOrDefault(Rl => Rl.RoleName == roleName);
				if (role != null) {
					throw new InvalidOperationException(string.Format("Role exists: {0}", roleName));
				}
				Role NewRole = new Role {
					RoleId = Guid.NewGuid(),
					RoleName = roleName,
					Application = Application.Current<RoleProvider>(db)
				};
				db.Roles.Add(NewRole);
				db.SaveChanges();
			}
		}

		public override void AddUsersToRoles(string[] usernames, string[] roleNames) {
			using (var db = new Context()) {
				var users = db.AppUsers<RoleProvider>().Where(usr => usernames.Contains(usr.UserName)).ToList();
				var roles = db.AppRoles<RoleProvider>().Where(rl => roleNames.Contains(rl.RoleName)).ToList();
				foreach (User user_loopVariable in users) {
					var user = user_loopVariable;
					foreach (Role role_loopVariable in roles) {
						var role = role_loopVariable;
						if (!user.Roles.Contains(role)) {
							user.Roles.Add(role);
						}
					}
				}
				db.SaveChanges();
			}
		}

		public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames) {
			using (var db = new Context()) {
				foreach (string username_loopVariable in usernames) {
					var username = username_loopVariable;
					string us = username;
					User user = db.AppUsers<RoleProvider>().FirstOrDefault(u => u.UserName == us);
					if (user != null) {
						foreach (string rolename_loopVariable in roleNames) {
							var rolename = rolename_loopVariable;
							var rl = rolename;
							Role role = user.Roles.FirstOrDefault(r => r.RoleName == rl);
							if (role != null) {
								user.Roles.Remove(role);
							}
						}
					}
				}
				db.SaveChanges();
			}
		}

		private ArgumentException CreateArgumentNullOrEmptyException(string paramName) {
			return new ArgumentException(string.Format("Argument cannot be null or empty: {0}", paramName));
		}

	}

}