using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Silversite.Web.UI {

	public partial class LogControlBase: System.Web.UI.UserControl {

		public string Category { get { return category.SelectedValue; } set { try { category.SelectedValue = value; } catch { } } }

		protected void Page_Init(object sender, EventArgs e) {
			// var scm = Page.Master.FindControl("ScriptManager");
			// scm.Parent.Remove(scm);
		}

		protected void Page_Load(object sender, EventArgs e) {
			Services.Log.Flush();
		}

		public void DataBind() {
			DateTime t;
			DateTime.TryParse(time.Text, out t);
			if (t == new DateTime()) t = DateTime.Now + TimeSpan.FromDays(1);
			source.Where = (set) => set.Where(log =>
				(string.IsNullOrEmpty(category.SelectedValue) || log.Category == category.SelectedValue) && log.Date <= t).OrderByDescending(log => log.Date);
			grid.PageSize = int.Parse(pagesize.SelectedValue);
			source.DataBind();
		}

		public void ClearClick(object sender, EventArgs e) {
			Services.Log.Clear(category.SelectedValue);
			DataBind();
		}

		public string GetDateString(DateTime date) {
			var now = DateTime.Now;
			if (now.Year != date.Year || now.DayOfYear != date.DayOfYear) return date.ToLongDateString() + " " + date.ToLongTimeString();
			else return date.ToLongTimeString();
		}

		protected override object SaveViewState() {
			DataBind();
			return base.SaveViewState();
		}
	}
}