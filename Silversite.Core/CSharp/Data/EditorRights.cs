using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Silversite.Services {

	/// <summary>
	/// A Enum describing user permission.
	/// </summary>
	public enum Permission { Allowed, Forbidden }
	
	/// <summary>
	/// A class describing User rights to edit a single document or document category.
	/// </summary>
	internal class EditRight {
		/// <summary>
		/// The document or document category this right applies to.
		/// </summary>
		public string DocumentCategory { get; set; }
		/// <summary>
		/// The user's permission
		/// </summary>
		public Permission Permission { get; set; }
		public EditRight() { }
		public EditRight(string categoryWithPermission) {
			if (categoryWithPermission.StartsWith("+") || categoryWithPermission.StartsWith("-")) {
				DocumentCategory = categoryWithPermission.Substring(1);
				Permission = (categoryWithPermission[0] == '+') ? Permission.Allowed : Permission.Forbidden;
			} else {
				DocumentCategory = categoryWithPermission;
				Permission = Permission.Allowed;
			}
		}
	}

	/// <summary>
	/// A class describing rights of users to edit documents.
	/// </summary>
	public class EditRights {
		/// <summary>
		/// A user or a role
		/// </summary>
		[Key]
		[MaxLength(128)]
		public string UserOrRole { get; set; }
		/// <summary>
		/// True if this right applies to a single user (as opposed to a role group).
		/// </summary>
		public bool IsUser { get; set; }
		/// <summary>
		/// True if this right applies to a role group (as opposed to a single user).
		/// </summary>
		[NotMapped]
		public bool IsGroup { get { return !IsUser; } set { IsUser = !value; } }
		/// <summary>
		/// A string with a comma or semicolon separated list of documents or document categories.
		/// </summary>
		[MaxLength]
		public string DocumentCategories { get; set; }
		/// <summary>
		/// True if the passed string is a known user (and not a user role).
		/// </summary>
		/// <param name="db">The DbContext to access the database.</param>
		/// <param name="userOrRole">A string containing a user or a role.</param>
		/// <returns></returns>
		private static bool IsNameUser(Silversite.Context db, string userOrRole) {
			var user = Persons.Find(userOrRole);
			return user != null;
		}
		/// <summary>
		/// True if the the person is allowed to edit the document.
		/// </summary>
		/// <param name="doc">A document.</param>
		/// <param name="p">A person.</param>
		/// <returns>True if the the person is allowed to edit the document.</returns>
		public static bool IsEditable(IDocumentInfo doc, Person p) {
			if (doc == null) return true;
			var cats = (doc.Categories ?? string.Empty).SplitList(',', ';').ToList();
			if (cats.Contains("*")) return true;
			if (p == null) return false;
			if (cats.Contains("?")) return true;

			if (p.EditorSettings.EditableDocuments == null) {	// build the editable documents cache for this person.
				var edocs = p.EditorSettings.EditableDocuments = new List<EditRight>();
				using (var db = new Silversite.Context()) {
					// first find the rights for this user
					var rights = db.EditRights.Find(p.UserName);
					if (rights != null) {
						edocs.AddRange(rights.DocumentCategories.SplitList(s => new EditRight(s), ',',';'));
					}
					// and then for the users roles
					foreach (var role in p.Roles) {
						rights = db.EditRights.Find(role);
						if (rights != null) {
							edocs.AddRange(rights.DocumentCategories.SplitList(s => new EditRight(s), ',',';'));
						}
					}
				}
			}

			foreach (var right in p.EditorSettings.EditableDocuments) {
				foreach (var cat in cats) {
					if (Paths.Match(right.DocumentCategory, cat)) {
						return right.Permission == Permission.Allowed;
					}
				}
			}
			return false;
		}
		/// <summary>
		/// Sets the person's or the role's rights so they can edit the document or the document category.
		/// </summary>
		/// <param name="userOrRole">A user or a role.</param>
		/// <param name="documentCategories">A comma or semicolon separated list of documents or document categories.</param>
		public static void Set(string userOrRole, string documentCategories) {
			using (var db = new Silversite.Context()) {
				var rights = db.EditRights.Find(userOrRole);
				if (rights != null) {
					rights.DocumentCategories = documentCategories;
				} else {
					rights = new EditRights { UserOrRole= userOrRole, DocumentCategories = documentCategories, IsUser = IsNameUser(db, userOrRole) };
					db.EditRights.Add(rights);
				}
				db.SaveChanges(); 
			}
		}
		/// <summary>
		/// Gets the permissions for a user or a role.
		/// </summary>
		/// <param name="userOrRole">A user.</param>
		/// <returns>Returns the permissions for this user as a semicolon separated list string.</returns>
		public static string Get(string userOrRole) {
			using (var db = new Silversite.Context()) {
				var rights = db.EditRights.Find(userOrRole);
				if (rights != null) return rights.DocumentCategories;
				return null;
			}
		}
		/// <summary>
		/// sets the user's or role's persmission for a document or documentCategory
		/// </summary>
		/// <param name="userOrRole"></param>
		/// <param name="category"></param>
		/// <param name="a"></param>
		public static void Set(string userOrRole, string category, Permission a) {
			string categoryWithPermission;
			if (!(category.StartsWith("+") || category.StartsWith("-"))) categoryWithPermission = ((a == Permission.Allowed) ? "+" : "-") + category;
			else {
				categoryWithPermission = category;
				category = category.Substring(1);
			}

			using (var db = new Silversite.Context()) {
				var rights = db.EditRights.Find(userOrRole);
				if (rights != null) {
					var categoriesWithPermission = rights.DocumentCategories.SplitList<string>(s => s, ',', ';').ToList();
					var categories = categoriesWithPermission.Select(c => (c.StartsWith("+") || c.StartsWith("-")) ? c.Substring(1) : c).ToList();
					if (categories.Contains(category)) {
						categoriesWithPermission[categories.IndexOf(category)] = categoryWithPermission;
					} else {
						categoriesWithPermission.Add(categoryWithPermission);
					}
					rights.DocumentCategories = categoriesWithPermission.StringList("; ");
				} else {
					rights = new EditRights { UserOrRole= userOrRole, DocumentCategories = categoryWithPermission, IsUser = IsNameUser(db, userOrRole) };
					db.EditRights.Add(rights);
				}
				db.SaveChanges();
			}
		}
	}
}