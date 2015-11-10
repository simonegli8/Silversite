using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Silversite.Web.UI {

	
	internal class DbVersionsDataSource: DbDataSource<Context, Data.ContextVersion> {

		public DbVersionsDataSource(): base() {
			Where = set => set.OrderBy(v => v.Context);
		}

	}
}