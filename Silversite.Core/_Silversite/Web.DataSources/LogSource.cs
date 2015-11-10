using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Silversite.Web.UI {

	public class LogSource: Silversite.Web.UI.DbDataSource<Homesell.Data.Context, Services.LogMessage> {

		public string Category { get { return (string)ViewState["Category"]; } set { ViewState["Category"] = value; } }

		public bool NoCategory { get { return string.IsNullOrEmpty(Category); } }

		public LogSource(): base() {
			Where = set => 	set
				.Where(e => NoCategory || e.Category == Category)
				.OrderByDescending(e => e.Date);
			Select = set => set
				.Select(m => new { Key = m.Key, Category = m.Category, Date = m.Date, Text = m.Html });
		}
	}
}
