using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Silversite.Services {

	public class WebPay: StaticService<WebPay, WebPayProvider> {
		public static void OnlinePay(Person seller, Person buyer, decimal amount, string currency, string product) { Provider.OnlinePay(seller, buyer, amount, currency, product); }
		public static void OfflinePay(Person seller, Person buyer, decimal amount, string currency, string product) { Provider.OfflinePay(seller, buyer, amount, currency, product); }
	}

	public abstract class WebPayProvider: Provider<WebPay> {
		public abstract void OnlinePay(Person seller, Person buyer, decimal amount, string currency, string product);
		public abstract void OfflinePay(Person seller, Person buyer, decimal amount, string currency, string product);
	}
}