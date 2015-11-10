using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Silversite.Reflection;

namespace Silversite.Services {

	public class InstallerAttribute: Attribute { }

	public class Transaction {

		[ThreadStatic]
		static Stack<List<Action>> rollbackStack;

		public static void Step(Action task, Action rollback) {
			if (rollbackStack == null) {
				rollbackStack = new Stack<List<Action>>();
				Start();
			}
			rollbackStack.Peek().Add(rollback);
			Start();
			try {
				task();
				Commit();
			} catch (Exception ex) {
				Rollback();
				throw ex;
			}
		}
		public static void Start() { rollbackStack.Push(new List<Action>()); }
		public static void Commit() { rollbackStack.Pop(); }
		public static void Rollback() {
			try {
				var failed = rollbackStack.Pop();
				failed.Reverse();
				failed.Each(t => t());
			} catch (Exception ex) { }
		}
	}

	public class InstallConflictException: Exception {
	
		public class Conflict {
			public string File { get; set; }
			public string Package { get; set; }
		}

		public List<Conflict> Conflicts { get; set; }

		public InstallConflictException(IEnumerable<string> conflictingFiles) {
			Conflicts = new List<Conflict>();
			Conflicts.AddRange(conflictingFiles.Select(file => new Conflict { File = file }));
		}
	}

	public class Installer {

		public static XNamespace ic = "http://schemas.silversite.org/installer/config/2013";

		public const string Root = "~/Silversite/Extensions";
		public const string Cache = Root + "/Temp";
		public const string AdminUI = Root + "/Silversite.Installer/UI/Admin";
		public const string InstallerUI = Root + "/Silversite.Installer/UI/Installer";
		public const string InstallerConfig = Root + "/Silversite.Installer/Installer.State.xml";

		public static void Install(Uri package, ProgressEventHandler progress = default(ProgressEventHandler), bool force = false) {
			try {
				var name = Paths.FileWithoutExtension(package.AbsolutePath);
				var ext = Paths.Extension(package.AbsolutePath).ToLower();
				var packageRoot = Paths.Combine(Root, name);
				var packageFile = Paths.Combine(Cache, name);
				Transaction.Step(() => Files.Download(package, packageFile),
					() => Files.Delete(packageFile));
				var dlls = new List<string>();

				if (ext == "zip") {
					var conflicts = Zip.Content(packageFile)
						.Select(file => Paths.Combine("~", file))
						.Where(file => Files.Exists(file))
						.ToList();
					if (!force && conflicts.Count > 0) throw new InstallConflictException(conflicts);

					Transaction.Step(() => {
						Zip.Extract(packageFile, "~", null, progress);
					}, () => { // rollback
						Files.Delete(packageRoot);
					});
				} else if (ext == "dll") {
					var dll = Paths.Move(packageFile, packageRoot);
					Transaction.Step(() => {
						Files.CreateDirectory(packageRoot);
						Files.Copy(packageFile, dll);
					}, () => { // rollback
						Files.Delete(packageRoot);
					});
					Lazy.Assemblies.Register(dll).Load();
				}

				// register lazy assemblies
				dlls = Files.AllVirtual(packageRoot + "/**/*.dll").ToList();
				dlls.Each(dll => {
					Transaction.Step(() => Lazy.Assemblies.Register(dll),
						() => Lazy.Assemblies.Unregister(dll));
					Lazy.Assemblies[dll].Load();
				});

				// search package for special files.
				var ui = Files.AllVirtual(packageRoot + "/**/*.Admin.UI.ascx;", packageRoot + "/**/*.Admin.UI.ascx.fm.config",
					packageRoot + "/**/*.Installer.UI.ascx;", packageRoot + "/**/*.Installer.UI.ascx.fm.config").ToList();

				// save installer config
				// TODO save correct state
				var config = new XElement(ic + "InstallerState");
				dlls.Each(dll => config.Add(new XElement(ic + "Dll", new XAttribute("Source", dll), new XAttribute("Installed", Paths.Move(dll, Lazy.RootPath)))));
				ui.Each(aui => config.Add(new XElement(ic + "File", new XAttribute("Source", aui), new XAttribute("Installed", Paths.Move(aui, AdminUI)))));
				var configName = InstallerConfig + "/" + name + ".config";
				Transaction.Step(() => config.Save(configName, SaveOptions.OmitDuplicateNamespaces),
					() => Files.Delete(configName));

				// copy files
				config.Elements(ic + "UserInterface")
					.Each(x => 
						Transaction.Step(() => Files.Copy((string)x.Attribute("Source"), (string)x.Attribute("Installed")),
							() => Files.Delete((string)x.Attribute("Installed"))));

				// get installers
				var installers = dlls.SelectMany(dll => {
					var a = Lazy.Assemblies[dll].Load();
					return a.GetTypes()
						.Where(t => 
							t.IsPublic &&
							t.Follow(u => u.BaseType)
								.Any<Type>(v => v.FullName == "System.Configuration.Install") &&
							t.GetAttribute<InstallerAttribute>() != null)
						.Select(t => new { Installer = New.Object(t), State = new Hashtable() });
				});

				// run installers
				installers.Each(y => {
					Transaction.Step(() => y.Installer.Method("Install").Invoke(y.State),
						() => y.Installer.Method("Rollback").Invoke(y.State));
				});

				// commit installers
				installers.Each(x => x.Installer.Method("Commit").Invoke(x.State));

				// delete package file
				Files.Delete(packageFile);

				// precompile site
				Files.Precompile();

				// commit transaction
				Transaction.Commit();
			} catch (Exception ex) {
				Transaction.Rollback();
				throw ex;
			}
		}

		public static void Uninstall(string packageName) {
			if (packageName.Contains(':')) packageName = Paths.FileWithoutExtension(packageName);
			var name = packageName;
			// load config
			var configName = InstallerConfig + "/" + name + ".config";
			var config = XElement.Load(configName);
			
			var dlls = config.Elements(ic + "Dll")
				.Select(x => Paths.FileWithoutExtension((string)x.Attribute("Source")));
 
			// get installers
			var installers = dlls.SelectMany(dll => {
				var a = Lazy.Assemblies[dll].Load();
				return a.GetTypes()
					.Where(t => 
						t.IsPublic &&
						t.Follow(u => u.BaseType)
							.Any<Type>(v => v.FullName == "System.Configuration.Install") &&
						t.GetAttribute<InstallerAttribute>() != null)
					.Select(t => new { Installer = New.Object(t), State = new Hashtable() });
			});
			// uninstall installers
			installers.Each(x => x.Installer.Method("Uninstall").Invoke(x.State));

			// delete ui
			var ui = config.Elements(ic + "UserInterface")
				.Select(x => (string)x.Attribute("Installed"));
			ui.Each(file => Files.Delete(file));

			// delete installer config
			Files.Delete(configName);

			// delete package root
			Files.Delete(Paths.Combine(Root, packageName));
		}

		public IEnumerable<string> Installed {
			get { return Files.All(InstallerConfig + "/**/*.config").Select(file => Paths.FileWithoutExtension(file)); }
		}
	}
}