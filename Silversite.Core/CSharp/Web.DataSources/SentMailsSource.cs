using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Silversite.Web.UI {

	internal class SentMailsSource: DbDataSource<Silversite.Context, Data.StoredMail> {

		//TODO Source is not defined because it is not unique (SentMails & ScheduledMails).
		public SentMailsSource(): base() {
			Where = set => set.OrderByDescending(m => m.Sent);
		}

	}

}