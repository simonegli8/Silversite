using System;
using System.Collections.Generic;
using System.Linq;

namespace Silversite.Services {

	public class WebSearch: StaticService<WebSearch, WebSearchProvider> {

		public static IEnumerable<Uri> Search(string keywords) { return Provider.Search(keywords); }

	}

	public abstract class WebSearchProvider: Provider<WebSearch> {
		public abstract IEnumerable<Uri> Search(string keywords);
	}

}