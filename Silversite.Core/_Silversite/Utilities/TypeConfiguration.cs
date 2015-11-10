using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Silversite.Services {

	public class TypeConfigurationElement: Configuration.Element {
		[System.Configuration.ConfigurationProperty("type", IsRequired=true, IsKey=true)]
		public string Type { get { return (string)base["type"]; } set { base["type"] = value; } }
	}
	public class TypeConfigurationCollection: Configuration.Collection<string, TypeConfigurationElement> {
		protected override object GetElementKey(System.Configuration.ConfigurationElement element) { return ((TypeConfigurationElement)element).Type; }
	}
	
}
