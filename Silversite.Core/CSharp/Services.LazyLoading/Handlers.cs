using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace Silversite.Services.LazyLoading {

	public class HandlerConfigurationElement: TypeConfigurationElement {
		[System.Configuration.ConfigurationProperty("paths", IsRequired=true, IsKey=true)]
		public string Paths { get { return (string)base["paths"]; } set { base["paths"] = value; } }
		[System.Configuration.ConfigurationProperty("auto", IsRequired=false, DefaultValue=null)]
		public TimeSpan? Auto { get { TimeSpan t; var s = (string)base["auto"]; if (s != null && TimeSpan.TryParse(s, out t)) return t; return null; } set { base["auto"] = value.ToString(); } }
	}
	public class HandlerConfigurationCollection: Configuration.Collection<string, HandlerConfigurationElement> {
		protected override object GetElementKey(System.Configuration.ConfigurationElement element) { return ((HandlerConfigurationElement)element).Paths; }
	}

		
	public class HandlerInfo: TypeInfo {
		public string Paths;
		public TimeSpan? Auto { get; set; }
	}
	public class HandlerCollection: Lazy.Collection<string, HandlerInfo, HandlerConfigurationCollection, HandlerConfigurationElement> {
		protected override string GetKeyForItem(HandlerInfo item) { return item.Paths; }
		public override void Save() { Save(Lazy.Configuration.RegisteredHandlers, info => new HandlerConfigurationElement { Type = info.TypeAssemblyQualifiedName, Paths = info.Paths, Auto = info.Auto }); }
		public override void Load() {
			Load(Lazy.Configuration.RegisteredHandlers, e => {
				var info = new HandlerInfo { TypeAssemblyQualifiedName = e.Type, Paths = e.Paths, Auto = e.Auto };
				if (e.Auto != null) Tasks.DoLater(e.Auto.Value, () => info.Load());
				return info;
			});
		}

		protected virtual void Add(string paths, string TypeAssemblyQualifiedName) {
			TypeAssemblyQualifiedName = TypeName(TypeAssemblyQualifiedName);
			Add(new HandlerInfo { TypeAssemblyQualifiedName = TypeAssemblyQualifiedName, Paths = paths });
		}

		public virtual void Register(string paths, string TypeAssemblyQualifiedName) {
			TypeAssemblyQualifiedName = TypeName(TypeAssemblyQualifiedName);
			lock (this) {
				if (!Contains(paths)) {
					Add(paths, TypeAssemblyQualifiedName);
					Save();
					Lazy.Save();
				}
			}
		}
		public virtual void Register(string paths, string TypeAssemblyQualifiedName, System.IO.Stream Assembly) {
			TypeAssemblyQualifiedName = TypeName(TypeAssemblyQualifiedName);
			lock (this) {
				if (!Contains(TypeAssemblyQualifiedName)) {
					var t = new HandlerInfo { Paths = paths, TypeAssemblyQualifiedName = TypeAssemblyQualifiedName };
					Lazy.Assemblies.Register(t.AssemblyName, Assembly);
					Add(t);
					Lazy.Save();
				}
			}
		}
		public virtual void Register(string paths, string TypeAssemblyQualifiedName, string assemblyPath) {
			TypeAssemblyQualifiedName = TypeName(TypeAssemblyQualifiedName);
			lock (this) {
				if (!Contains(TypeAssemblyQualifiedName)) {
					var t = new HandlerInfo { Paths = paths, TypeAssemblyQualifiedName = TypeAssemblyQualifiedName };
					Lazy.Assemblies.Register(t.AssemblyName, assemblyPath);
					Add(t);
					Lazy.Save();
				}
			}
		}

		public virtual HandlerInfo Info(string path) {
			HandlerInfo[] copy;
			lock (this) copy = this.ToArray();
			foreach (var h in copy) {
				var paths = h.Paths.Split(';',',');
				foreach (var p in paths) {
					var star = p.IndexOf('*');
					if (star == -1) {
						if (path == p) return h;
					} else {
						if (path.StartsWith(p.Substring(0, star)) && path.EndsWith(p.Substring(star+1))) return h;
					}
				}
			}
			return null;
		}
		public virtual Type Type(string path) { var info = Info(path); if (info != null) return info.Type; return null; }

		public virtual IHttpHandler New(string path) {
			var type = Type(path);
			if (type != null) return Silversite.New.Object(type) as IHttpHandler;
			return null;
		}
	}
}
