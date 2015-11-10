using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace Silversite.Services.LazyLoading {

	public class PathConfigurationElement: Configuration.Element {
		[System.Configuration.ConfigurationProperty("paths", IsRequired=true, IsKey=true)]
		public string Paths { get { return (string)base["paths"]; } set { base["paths"] = value; } }
		[System.Configuration.ConfigurationProperty("assemblies", IsRequired=true)]
		public string Assemblies { get { return (string)base["assemblies"]; } set { base["assemblies"] = value; } }
		[System.Configuration.ConfigurationProperty("auto", IsRequired=false, DefaultValue=null)]
		public TimeSpan? Auto { get { TimeSpan t; var s = (string)base["auto"]; if (s != null && TimeSpan.TryParse(s, out t)) return t; return null; } set { base["auto"] = value.ToString(); } }
	}
	public class PathConfigurationCollection: Configuration.Collection<string, PathConfigurationElement> {
		protected override object GetElementKey(System.Configuration.ConfigurationElement element) { return ((PathConfigurationElement)element).Paths; }
	}

	public class PathInfo {
		public PathInfo() { Assemblies = new List<AssemblyInfo>(); }
		public List<AssemblyInfo> Assemblies { get; private set; }
		public string Path;
		public TimeSpan? Auto { get; set; }
	}


	public class PathCollection: Lazy.Collection<string, PathInfo, PathConfigurationCollection, PathConfigurationElement> {
		protected override string GetKeyForItem(PathInfo item) { return item.Path.ToLower(); }

		public override void Save() {
			lock (this) {
				if (loaded) {
					Lazy.Configuration.RegisteredPaths.Clear();
					Lazy.Configuration.RegisteredPaths.AddRange(
						this.SelectMany(info => info.Assemblies.Select(a => new { Path = info.Path, AssemblyName = a.AssemblyName, Auto = info.Auto }))
							.GroupBy(info => info.AssemblyName)
							.Select(info => new { Paths = info.Select(g => g.Path).StringList(";"), Assembly = info.Key, Auto = info.Select(g => g.Auto).Max() })
							.GroupBy(info => info.Paths)
							.SelectMany(group => group.Select(info => new PathConfigurationElement { Paths = info.Paths, Assemblies = group.Select(g => g.Assembly).StringList(";"), Auto = info.Auto }))
					);
				}
				saved = loaded;
			}
		}

		public override void Load() {
			lock (this) {
				if (saved) Clear();
				foreach (PathConfigurationElement e in Lazy.Configuration.RegisteredPaths) {
					Add(e.Paths, e.Assemblies);
					if (e.Auto != null) {
						InfoPaths(e.Paths).ForEach(i => i.Auto = i.Auto != null ? new TimeSpan(Math.Min(i.Auto.Value.Ticks, e.Auto.Value.Ticks)) : e.Auto);
						Tasks.DoLater(e.Auto.Value, () => Load(e.Paths.SplitList(',', ';').FirstOrDefault()));
					}
				}
				loaded = true;
				if (!saved) Save();
			}
		}

		private void Add(string paths, string assemblies) {
			var pathsList = paths.ToLower().SplitList(';', ',');
			foreach (var p in pathsList) {
				foreach (var a in assemblies.SplitList(';').Select(an => Lazy.Assemblies.Add(an))) {
					PathInfo info;
					if (Contains(p)) {
						info = this[p];
						info.Assemblies.Add(a);
					} else {
						info = new PathInfo { Path = p };
						info.Assemblies.Add(a);
						Add(info);
					}
				}
			}
		}

		public void Register(string Path, string Assemblies) {
			lock (this) {
				Add(Path, Assemblies);
				Save();
				Lazy.Save();
			}
		}
		public void Register(string Path, string AssemblyName, System.IO.Stream Assembly) {
			lock (this) {
				Lazy.Assemblies.Register(AssemblyName, Assembly);
				Add(Path, AssemblyName);
				Lazy.Save();
			}
		}
		public void Register(string Path, string AssemblyName, string assemblyPath) {
			lock (this) {
				Lazy.Assemblies.Register(AssemblyName, assemblyPath);
				Add(Path, AssemblyName);
				Lazy.Save();
			}
		}

		public IEnumerable<PathInfo> InfoPaths(string Path) {
			Path = Services.Paths.Normalize(Path);
			lock (this) {
				Path = Path.ToLower();
				if (Contains(Path)) yield return this[Path];
				foreach (var info in this) {
					if (Paths.Match(info.Path, Path)) yield return info;
				}
				yield break;
			}
		}

		public List<PathInfo> Infos(string Path) {
			return InfoPaths(Path).ToList();
		}

		public IEnumerable<System.Reflection.Assembly> LoadPaths(string Path) {
			lock (this) {
				var infos = Infos(Path);

				foreach (var info in infos) {
					if (info.Path.Contains('/') && info.Path.EndsWith("**")) { // write custom web.config to load all assemblies in all virtual aspx pages.
						var webconfig = info.Path.Replace("**", "web.config");
						var str = new StringBuilder(@"<?xml version='1.0'?>
<!--lazypaths-->
<configuration>
	<system.web>
		<compilation>
			<assemblies>
");
						foreach (var a in info.Assemblies) {
							str.Append("					<add assembly='");
							str.Append(a.AssemblyName);
							str.AppendLine("' />");
						}
						str.Append(@"			</assemblies>
		</compilation>
	</system.web>
</configuration>
");
						var dir = Silversite.Services.Paths.Directory(webconfig);
						if (!Files.DirectoryExists(dir)) Files.CreateDirectory(dir);
						var text = str.ToString();
						try {
							var disk = Files.Load(webconfig);
							if (disk == null || disk.Contains("<!--lazypaths-->") && disk != text) Files.Save(text, webconfig);
						} catch (System.IO.FileNotFoundException ex) {
							Files.Save(text, webconfig);
						}
					}

					foreach (var a in info.Assemblies) {
						if (!a.Loaded) 
							yield return a.Load();
					}
				}
			}
		}

		public List<System.Reflection.Assembly> Load(string Path) {
			return LoadPaths(Path).ToList();
		}

		public List<System.Reflection.Assembly> LoadAll() {
			return this.AsEnumerable().SelectMany(info => LoadPaths(info.Path.SplitList(';', ',').FirstOrDefault())).ToList();
		}

	}
}