using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Silversite.Services {

	public class DbProviderConfigurationElement: Configuration.Element {
		[System.Configuration.ConfigurationProperty("provider", IsRequired = true, IsKey = true)]
		public string Provider { get { return (string)base["provider"]; } set { base["provider"] = value; } }
		[System.Configuration.ConfigurationProperty("assemblies", IsRequired = true, IsKey = true)]
		public string Assemblies { get { return (string)base["assemblies"]; } set { base["assemblies"] = value; } }
		[System.Configuration.ConfigurationProperty("auto", IsRequired=false, DefaultValue=null)]
		public TimeSpan? Auto { get { TimeSpan t; var s = (string)base["auto"]; if (s != null && TimeSpan.TryParse(s, out t)) return t; return null; } set { base["auto"] = value.ToString(); } }
	}

	public class DbProviderConfigurationCollection: Configuration.Collection<string, DbProviderConfigurationElement> {
		protected override object GetElementKey(System.Configuration.ConfigurationElement element) { return ((DbProviderConfigurationElement)element).Provider; }
	}

}

namespace Silversite.Services.LazyLoading {

	public class DbProviderInfo {
		public IEnumerable<AssemblyInfo> AssemblyInfos { get; private set; }
		public string Provider { get; set; }

		string assemblies = null;
		public string Assemblies {
			get { return assemblies; }
			set {
				assemblies = value;
				AssemblyInfos = assemblies.SplitList(';').Select(a => new AssemblyInfo { AssemblyName = a }).ToList();
			}
		}

		public bool Loaded { get { return Assemblies == null || AssemblyInfos.Any(a => a.Loaded); } }
		public void Load() { if (Assemblies != null) AssemblyInfos.ForEach(a => a.Load()); }
		public TimeSpan? Auto { get; set; }
	}

	public class DbProviderCollection: Lazy.Collection<string, DbProviderInfo, DbProviderConfigurationCollection, DbProviderConfigurationElement> {
		protected override string GetKeyForItem(DbProviderInfo item) { return item.Provider; }

		public override void Save() { Save(Lazy.Configuration.RegisteredDbProviders, info => new DbProviderConfigurationElement { Provider = info.Provider, Assemblies = info.Assemblies, Auto = info.Auto }); }
		public override void Load() {
			Load(Lazy.Configuration.RegisteredDbProviders, e => {
				var info = new DbProviderInfo { Provider = e.Provider, Assemblies = e.Assemblies, Auto = e.Auto };
				if (e.Auto != null) Tasks.DoLater(e.Auto.Value, () => info.Load());
				return info;
			});
		}

		protected virtual void Add(string ProviderAssemblyQualifiedName) {
			ProviderAssemblyQualifiedName = TypeName(ProviderAssemblyQualifiedName);
			Add(new DbProviderInfo { Provider = ProviderAssemblyQualifiedName });
		}

		public virtual void Register(string ProviderAssemblyQualifiedName) {
			ProviderAssemblyQualifiedName = TypeName(ProviderAssemblyQualifiedName);
			lock (this) {
				if (!Contains(ProviderAssemblyQualifiedName)) {
					Add(ProviderAssemblyQualifiedName);
					Save();
					Lazy.Save();
				}
			}
		}
		public virtual void Register(string ProviderAssemblyQualifiedName, params string[] assemblyPaths) {
			ProviderAssemblyQualifiedName = TypeName(ProviderAssemblyQualifiedName);
			lock (this) {
				if (!Contains(ProviderAssemblyQualifiedName)) {
					var t = new DbProviderInfo { Provider = ProviderAssemblyQualifiedName, Assemblies = assemblyPaths.Select(a => Paths.FileWithoutExtension(a)).StringList(";") };
					foreach (var assembly in assemblyPaths) Lazy.Assemblies.Register(Paths.FileWithoutExtension(assembly), assembly);
					Add(t);
					Lazy.Save();
				}
			}
		}

		public virtual void Load(string ProviderName) {
			lock (this) {
				var info = Info(ProviderName);
				if (info != null) info.Load();
			}
		}

		public virtual DbProviderInfo Info(string ProviderName) {
			if (Contains(ProviderName)) return this[ProviderName];
			return null;
		}
	}
}
