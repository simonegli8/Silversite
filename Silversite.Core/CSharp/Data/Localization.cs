using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Silversite.Data {

	/// <summary>
	/// A class describing a language for localization
	/// </summary>
	public class Language {
		/// <summary>
		/// The name of the CultureInfo of that language.
		/// </summary>
		[Key, MaxLength(16)]
		public string Culture { get; set; }
	}
	/// <summary>
	/// A class describing a currency.
	/// </summary>
	public class Currency {
		/// <summary>
		/// The 3 letter code for that currency.
		/// </summary>
		[Key, MaxLength(16)]
		public string Name { get; set; }
		/// <summary>
		/// A friendly name for that currency, like Swiss Franc.
		/// </summary>
		[MaxLength(128)]
		public string DisplayName { get; set; }
		/// <summary>
		/// The currency symbol, e.g. $ for USD.
		/// </summary>
		[MaxLength(16)]
		public string Symbol { get; set; }
		/// <summary>
		/// The minimal amount of money in that currency, e.g. 0.01 for 1 cent for USD.
		/// </summary>
		public decimal MinimalAmount { get; set; }
		/// <summary>
		/// The amount to which to round numbers in that currency.
		/// </summary>
		public decimal RoundTo { get; set; }
		/// <summary>
		/// The exchange rate that is used within Silversite.
		/// </summary>
		public double Rate { get; set; }
		/// <summary>
		/// The current exchange rate.
		/// </summary>
		public double CurrentRate { get; set; }
		/// <summary>
		/// True, if this is the base currency, to that all other currencies are exchanged to.
		/// </summary>
		public bool IsBaseCurrency { get; set; }
	}

	/// <summary>
	/// A class for getting country names in different languages.
	/// </summary>
	public class Country {
		/// <summary>
		/// Gets the name of a country in the specified language.denoted by  in the language specified by a culture name.
		/// </summary>
		/// <param name="country">The culture name or two letter ISO code of the country.</param>
		/// <param name="langauge">The culture name of the language.</param>
		/// <returns>Gets the name of a country in the specified language.denoted by  in the language specified by a culture name.</returns>
		public static string GetName(string country, string langauge) {
			//TODO country names.
			return new CultureInfo(country).DisplayName;
		}
 	}

}