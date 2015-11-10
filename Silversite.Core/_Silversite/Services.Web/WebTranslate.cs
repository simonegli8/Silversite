using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;

namespace Silversite.Services {

	public class WebTranslate: StaticService<WebTranslate, WebTranslateProvider> {
		public string Translate(string text, CultureInfo sourceLanguage, CultureInfo destinationLanguage) { return Provider.Translate(text, sourceLanguage, destinationLanguage); }
	}
	
	public abstract class WebTranslateProvider: Provider<WebTranslate> {
		public abstract string Translate(string text, CultureInfo sourceLanguage, CultureInfo destinationLanguage);
	}

}