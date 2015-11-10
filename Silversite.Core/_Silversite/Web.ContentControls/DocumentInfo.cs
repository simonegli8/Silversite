using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Silversite.Web.UI {

	public class DocumentInfo: Control, Services.IDocumentInfo {
		public int ContentKey { get { return (int)(ViewState["ContentKey"] ?? Services.Document.None); } set { ViewState["ContentKey"] = value; } }
		public string Title { get { return (string)ViewState["Title"]; } set { ViewState["Title"] = value; } }
		public string Author { get { return (string)ViewState["Author"]; } set { ViewState["Author"] = value; } }
		public DateTime Published { get { return (DateTime)ViewState["Published"]; } set { ViewState["Published"] = value; } }
		public int Revision { get { return (int)(ViewState["Revision"] ?? 0);  } set { ViewState["Revision"] = value; } }
		public bool IsCurrentRevision { get { return (bool)(ViewState["IsCurrentRevision"] ?? true); } set { ViewState["IsCurrentRevision"] = value; } }
		public string Tags { get { return (string)ViewState["Tags"]; } set { ViewState["Tags"] = value; } }
		public string Categories { get { return (string)ViewState["Categories"]; } set { ViewState["Categories"] = value; } }
		public string Notes { get { return (string)ViewState["Notes"]; } set { ViewState["Notes"] = value; } }
		Services.Person Services.IDocumentInfo.Author { get { return Services.Persons.Find(Author); } set { Author = value.UserName; } }
	}
}