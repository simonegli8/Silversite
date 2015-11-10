using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;

namespace Silversite.Services {

	public class Lang {

		// TODO access values from Lang.resx
		public static string Title(Person person, string culture = null) {
			if (string.IsNullOrEmpty(culture)) culture = person.Culture ?? "de-CH";
			Resources.Lang.Culture = new CultureInfo(culture);
			switch (person.Gender) {
				default:
				case Person.Genders.None: return "";
				case Person.Genders.Male: return Resources.Lang.MaleTitle;
				case Person.Genders.Female: return Resources.Lang.FemaleTitle;
				case Person.Genders.Miss: return Resources.Lang.MissTitle;
				case Person.Genders.Group: return Resources.Lang.GroupTitle;
				case Person.Genders.Company: return Resources.Lang.CompanyTitle;
			}
		}

		public static string Salutation(Person person, string culture = null) {
			if (string.IsNullOrEmpty(culture)) culture = person.Culture ?? "de-CH";
			Resources.Lang.Culture = new CultureInfo(culture);
			switch (person.Gender) {
				default:
				case Person.Genders.None: return Resources.Lang.GroupSalutation;
				case Person.Genders.Male: return Resources.Lang.MaleSalutation + " " + person.LastName;
				case Person.Genders.Female: return Resources.Lang.FemaleSalutation + " " + person.LastName;
				case Person.Genders.Miss: return Resources.Lang.MissSalutation + " " + person.LastName;
				case Person.Genders.Group: return Resources.Lang.GroupSalutation;
				case Person.Genders.Company: return Resources.Lang.CompanySalutation;
			}
		}



		public static string Unsubscribe(string culture, string email) {
			if (string.IsNullOrEmpty(culture)) culture = "en";
			Resources.Lang.Culture = new CultureInfo(culture);
			return string.Format("<a href='{0}?email={1}&secret={2}'>{3}</a>", Paths.Url("~/emails/unsubscribe.aspx"), email, new Random(Hash.Compute(email)).Next(), Resources.Lang.Unsubscribe);
		}
		
		public static string Country(string culture, string IsoCode) {
			// TODO return country name in culture language.
			return "United States";
		}

		public static CultureInfo FirstCulture(string country) {
			if (!country.Contains('-')) {
				return CultureInfo.GetCultures(CultureTypes.AllCultures)
					.FirstOrDefault(c => c.Name.EndsWith(country));
			} else return new CultureInfo(country);
		}
	
		public static NumberFormatInfo Currency(string country) {
			return FirstCulture(country).NumberFormat;
		}

		public string Money(int size, decimal amount, string country) {
			var nf = Currency(country);
			if (country == "CH") nf.CurrencySymbol = "CHF";
			var text = amount.ToString("c", nf);
			int places;
			if (amount == 0) places = 0;
			else places = (int)Math.Log10((double)Math.Abs(amount));
			if (places < 0) places = 0;
			if (places > size) places = size;
			places = size - places;
			var ident = "  ".Repeat(places);
			var p = text.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });
			if (p >= 0) text = text.UpTo(p) + ident + text.FromOn(p);
			return text.Replace(" ", "&nbsp;");
		}

	}
}