using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

namespace Silversite.Web.UI {

	public class StopwatchDataSource: DbDataSource<Context, Services.StopwatchRecord> {

		public Services.Stopwatch.Times Results {
			get {
				Services.Stopwatch.Flush();
				using (var db = (Context)Context) {
					var test = Services.TestServer.IsTestServer;
					var query = from t in db.Stopwatch
						where t.Test == test
						group t by new { t.Page, t.Name } into g
						select new { Page = g.Key.Page, Name = g.Key.Name, Test = test, N = g.Sum(t => t.N), SumTicks = g.Sum(t => t.MeanTicks*t.N)};

					return new Services.Stopwatch.Times(query.Select(t => new Services.StopwatchRecord { Page = t.Page, Name = t.Name, Test = t.Test, N = t.N, MeanTicks = t.SumTicks / t.N, Time = TimeSpan.FromTicks((long)(t.SumTicks / t.N + 0.5))}));
				}
			}
		}

		public StopwatchDataSource() {
			Data = () => Results.AsQueryable();
		}
	}
}
