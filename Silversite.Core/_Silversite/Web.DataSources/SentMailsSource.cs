using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Silversite.Web.UI {

	public class SentMailsSource: DbDataSource<Homesell.Data.Context, Services.SentMail> {

		public SentMailsSource(): base() {
			Where = set => set.OrderByDescending(m => m.Sent);
		}

	}

}