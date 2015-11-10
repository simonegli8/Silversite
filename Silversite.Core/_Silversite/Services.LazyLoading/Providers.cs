using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Silversite.Services.LazyLoading {


	public class ProviderConfigurationElement: TypeConfigurationElement {
		[System.Configuration.ConfigurationProperty("service", IsRequired=true)]
		public string Service { get { return (string)base["service"]; } set { base["service"] = value; } }
		[System.Configuration.ConfigurationProperty("auto", IsRequired=false, DefaultValue=null)]
		public TimeSpan? Auto { get { TimeSpan t; var s = (string)base["auto"]; if (s != null && TimeSpan.TryParse(s, out t)) return t; return null; } set { base["auto"] = value.ToString(); } }
	}
	public class ProviderConfigurationCollection: Configuration.Collection<string, ProviderConfigurationElement> {
		protected override object GetElementKey(System.Configuration.ConfigurationElement element) { return ((ProviderConfigurationElement)element).Type; }
	}


	public class ProviderInfo: TypeInfo {
		public string Service { get; set; }
	}

	public class ProviderCollection: Lazy.Collection<string, ProviderInfo, ProviderConfigurationCollection, ProviderConfigurationElement> {
		protected override string GetKeyForItem(ProviderInfo item) { return item.TypeAssemblyQualifiedName; }

		protected override void InsertItem(int index, ProviderInfo item) {
			base.InsertItem(index, item);
			Lazy.Types.Add(item);
		}

		public override void Load() {
			Load(Lazy.Configuration.RegisteredProviders, e => {
				var info = new ProviderInfo { TypeAssemblyQualifiedName = e.Type, Service = e.Service, Auto = e.Auto };
				if (e.Auto != null) Tasks.DoLater(e.Auto.Value, () => info.Load());
				return info;
			});
		}
		public override void Save() { Save(Lazy.Configuration.RegisteredProviders, info => new ProviderConfigurationElement { Type = info.TypeAssemblyQualifiedName, Service = info.Service, Auto = info.Auto }); }

		protected void Add(string TypeAssemblyQualifiedName, string Service) {
			TypeAssemblyQualifiedName = TypeName(TypeAssemblyQualifiedName);
			Add(new ProviderInfo { TypeAssemblyQualifiedName = TypeAssemblyQualifiedName, Service = Service });
		}

		public new virtual bool Contains(string TypeAssemblyQualifiedName) {
			TypeAssemblyQualifiedName = TypeName(TypeAssemblyQualifiedName);
			return base.Contains(TypeAssemblyQualifiedName);
		}


		public virtual void Register(string TypeAssemblyQualifiedName, string Service) {
			TypeAssemblyQualifiedName = TypeName(TypeAssemblyQualifiedName);
			lock (this) {
				if (!Contains(TypeAssemblyQualifiedName)) {
					Add(TypeAssemblyQualifiedName, Service);
					Save();
					Lazy.Save();
				}
			}
		}
		public virtual void Register(string TypeAssemblyQualifiedName, string Service, System.IO.Stream Assembly) {
			TypeAssemblyQualifiedName = TypeName(TypeAssemblyQualifiedName);
			lock (this) {
				if (!Contains(TypeAssemblyQualifiedName)) {
					var t = new ProviderInfo { TypeAssemblyQualifiedName = TypeAssemblyQualifiedName, Service = Service };
					Lazy.Assemblies.Register(t.AssemblyName, Assembly);
					Add(t);
					Lazy.Save();
				}
			}
		}
		public virtual void Register(string TypeAssemblyQualifiedName, string Service, string assemblyPath) {
			TypeAssemblyQualifiedName = TypeName(TypeAssemblyQualifiedName);
			lock (this) {
				if (!Contains(TypeAssemblyQualifiedName)) {
					var t = new ProviderInfo { TypeAssemblyQualifiedName = TypeAssemblyQualifiedName, Service = Service };
					Lazy.Assemblies.Register(t.AssemblyName, assemblyPath);
					Add(t);
					Lazy.Save();
				}
			}
		}

		public virtual object New(string TypeAssemblyQualifiedName, params object[] constructorArgs) {
			TypeAssemblyQualifiedName = TypeName(TypeAssemblyQualifiedName);
			lock (this) {
				if (Contains(TypeAssemblyQualifiedName)) {
					return this[TypeAssemblyQualifiedName].New(constructorArgs);
				} else {
					var type = System.Type.GetType(TypeAssemblyQualifiedName);
					if (type == null) throw new TypeAccessException("The type " + TypeAssemblyQualifiedName + " could not be found.");
					return Silversite.New.Object(type, constructorArgs);
				}
			}
		}

		public virtual ProviderInfo Info(string TypeAssemblyQualifiedName) {
			TypeAssemblyQualifiedName = TypeName(TypeAssemblyQualifiedName);
			if (Contains(TypeAssemblyQualifiedName)) return this[TypeAssemblyQualifiedName]; 
			return null;
		}
	}

}