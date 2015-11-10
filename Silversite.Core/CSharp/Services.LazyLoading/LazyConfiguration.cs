using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Silversite.Services {

	[Configuration.Section(Path=LazyConfiguration.ConfigPath, Name="lazyloading")]
	public class LazyConfiguration: Configuration.Section {

		public const string ConfigPath = ConfigRoot + "/LazyLoading.config";

		[System.Configuration.ConfigurationProperty("types")]
		[System.Configuration.ConfigurationCollection(typeof(TypeConfigurationCollection))]
		public TypeConfigurationCollection RegisteredTypes { get { return (TypeConfigurationCollection)base["types"]; } }

		[System.Configuration.ConfigurationProperty("dbProviders")]
		[System.Configuration.ConfigurationCollection(typeof(DbProviderConfigurationCollection))]
		public DbProviderConfigurationCollection RegisteredDbProviders { get { return (DbProviderConfigurationCollection)base["dbProviders"]; } }

		[System.Configuration.ConfigurationProperty("controls")]
		[System.Configuration.ConfigurationCollection(typeof(TypeConfigurationCollection))]
		public TypeConfigurationCollection RegisteredControls { get { return (TypeConfigurationCollection)base["controls"]; } }

		[System.Configuration.ConfigurationProperty("providers")]
		[System.Configuration.ConfigurationCollection(typeof(LazyLoading.ProviderConfigurationCollection))]
		public LazyLoading.ProviderConfigurationCollection RegisteredProviders { get { return (LazyLoading.ProviderConfigurationCollection)base["providers"]; } }

		[System.Configuration.ConfigurationProperty("paths")]
		[System.Configuration.ConfigurationCollection(typeof(LazyLoading.PathConfigurationCollection))]
		public LazyLoading.PathConfigurationCollection RegisteredPaths { get { return (LazyLoading.PathConfigurationCollection)base["paths"]; } }

		[System.Configuration.ConfigurationProperty("handlers")]
		[System.Configuration.ConfigurationCollection(typeof(LazyLoading.HandlerConfigurationCollection))]
		public LazyLoading.HandlerConfigurationCollection RegisteredHandlers { get { return (LazyLoading.HandlerConfigurationCollection)base["handlers"]; } }
	}

}