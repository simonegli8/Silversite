using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Silversite {

	public static class DbContextExtensions {

		public static void Detach(this System.Data.Entity.DbContext context, object entity) {
			((System.Data.Entity.Infrastructure.IObjectContextAdapter)context).ObjectContext.Detach(entity);
		}

	}

}