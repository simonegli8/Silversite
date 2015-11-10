using System;
using System.Collections.Generic;
using System.Linq;

namespace Silversite.Services {

	public class WebCurrency: StaticService<WebCurrency, WebCurrencyProvider> {
		public static void GetExchangeRate(string sourceCurrency, string destinationCurrency, out double buy, out double sell) { Provider.GetExchangeRate(sourceCurrency, destinationCurrency, out buy, out sell); }
	}

	public abstract class WebCurrencyProvider : Provider<WebCurrency> {
		public abstract void GetExchangeRate(string sourceCurrency, string destinationCurrency, out double buy, out double sell);
	}
}