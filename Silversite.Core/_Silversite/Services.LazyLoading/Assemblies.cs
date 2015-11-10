using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace Silversite.Services.LazyLoading {

	public class AssemblyInfo {
		public string AssemblyName;
		string /*cache = null,*/ bin = null;
		/*public string CachePath {
			get {
				if (cache == null) cache = Silversite.Services.Paths.Combine(Lazy.RootPath + "/cache/", AssemblyName + ".dll");
				return cache;
			}
		}*/
		public string BinPath {
			get {
				if (bin == null) bin = Silversite.Services.Paths.Combine(Lazy.RootPath, AssemblyName + ".dll");
				return bin;
			}
		}
		System.Reflection.Assembly assembly = null;
		public System.Reflection.Assembly Assembly {
			get {
				if (assembly == null) assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName.StartsWith(AssemblyName + ","));
				return assembly;
			}
		}
		public bool Loaded { get { return Assembly != null; } }
		public System.Reflection.Assembly Load() {
			if (!Loaded) {
				/*
#pragma warning disable
				if (Lazy.ShadowCopy && (!Files.FileExists(CachePath) || Files.FileInfo(BinPath).LastWriteTimeUtc > Files.FileInfo(CachePath).LastWriteTimeUtc)) {
#pragma warning restore
					try {
						Files.Copy(BinPath, CachePath);
						var pdb = Silversite.Services.Paths.ChangeExtension(BinPath, ".pdb");
						if (Files.FileExists(pdb)) {
							var pdbcache = Silversite.Services.Paths.ChangeExtension(CachePath, ".pdb");
							Files.Copy(pdb, pdbcache);
						}
						var config = Silversite.Services.Paths.ChangeExtension(BinPath, ".config");
						if (Files.FileExists(config)) {
							var configcache = Silversite.Services.Paths.ChangeExtension(CachePath, ".config");
							Files.Copy(config, configcache);
						}
					} catch (Exception ex) {
						Log.Error("Error copying assembly {0} to assembly cache.", ex, BinPath);
					}
				}*/

				try {
#pragma warning disable
					Log.Write("Debug", "Lazy loading assembly {0}.", AssemblyName);
					/*if (Lazy.ShadowCopy) assembly = System.Reflection.Assembly.LoadFrom(Silversite.Services.Paths.Map(CachePath));
					else */
					assembly = System.Reflection.Assembly.Load(AssemblyName);
#pragma warning restore
				} catch (Exception ex) {
					Log.Error("Error loading assembly {0}.", ex, AssemblyName);
				}
			}
			return assembly;
		}
	}

	
	public class AssemblyCollection: KeyedCollection<string, AssemblyInfo> {
		protected override string GetKeyForItem(AssemblyInfo item) { return item.AssemblyName; }
		
		public AssemblyInfo this[string AssemblyName] { get { return Get(AssemblyName); } }

		private string Normalize(string assemblyName) { if (assemblyName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) return Paths.FileWithoutExtension(assemblyName); return assemblyName; }
		public AssemblyInfo Get(string assemblyName) {
			lock (this) {
				if (!Contains(assemblyName)) Add(new AssemblyInfo { AssemblyName = Normalize(assemblyName) });
				return base[assemblyName];
			}
		}

		public AssemblyInfo Add(string AssemblyName) { AssemblyName = Normalize(AssemblyName); if (!Contains(AssemblyName)) { var a = new AssemblyInfo { AssemblyName = AssemblyName }; Add(a); return a; } return this[AssemblyName]; }

		public AssemblyInfo Register(string AssemblyName, System.IO.Stream Assembly) {
			lock (this) {
				AssemblyName = Normalize(AssemblyName);
				if (!Contains(AssemblyName)) {
					var a = new AssemblyInfo { AssemblyName = AssemblyName };
					try { Files.Save(Assembly, a.BinPath); } catch { throw new Exception("Error copying assembly to " + a.BinPath); }
					Add(a);
					Lazy.Save();
					return a;
				}
				return Get(AssemblyName);
			}
		}

		public AssemblyInfo Register(string AssemblyName, string assemblyPath = null) {
			lock (this) {
				if (assemblyPath != null) assemblyPath = AssemblyName;
				AssemblyName = Normalize(AssemblyName);
				if (!Contains(AssemblyName)) {
					var a = new AssemblyInfo { AssemblyName = AssemblyName };
					try {
						Files.Copy(assemblyPath, a.BinPath);
						var pdb = Silversite.Services.Paths.ChangeExtension(assemblyPath, ".pdb");
						if (Files.FileExists(pdb)) {
							var pdbcache = Silversite.Services.Paths.ChangeExtension(a.BinPath, ".pdb");
							Files.Copy(pdb, pdbcache);
						}
						var config = Silversite.Services.Paths.ChangeExtension(assemblyPath, ".config");
						if (Files.FileExists(config)) {
							var configcache = Silversite.Services.Paths.ChangeExtension(a.BinPath, ".config");
							Files.Copy(config, configcache);
						}
					} catch { throw new Exception("Error copying assembly to " + a.BinPath); }
					Add(a);
					Lazy.Save();
					return a;
				}
				return Get(AssemblyName);
			}
		}

		public void Unregister(string AssemblyName) {
			lock (this) {
				AssemblyName = Normalize(AssemblyName);
				if (!Contains(AssemblyName)) {
					var dll = Get(AssemblyName).BinPath;
					Files.Delete(dll, Paths.ChangeExtension(dll, "pdb"), Paths.ChangeExtension(dll, "config"));
					Remove(AssemblyName);
					Lazy.Save();
				}
			}
		}

	}
}