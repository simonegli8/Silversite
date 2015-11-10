using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.CodeDom.Compiler;
using System.Reflection;
using Microsoft.CSharp;
using Microsoft.VisualBasic;

[assembly: Silversite.Services.DependsOn(typeof(Silversite.Services.AppCode))]

namespace Silversite.Services {

	[AttributeUsage(AttributeTargets.Assembly)]
	public class AppCodeAttribute: Attribute {
		public int Files { get; set; }
		public int Hash { get; set; } 
	}

	public class AppCode: IAutostart {
		public const string Root = Paths.AppCode;
		public const string AttributeFile = "Silversite.Core.AppCode.cs";
		public const string CSAssemblyName = Paths.Lazy + "/Silversite.AppCode.CSharp.dll";
		public const string VBAssemblyName= Paths.Lazy + "/Silversite.AppCode.VisualBasic.dll";

		System.IO.FileSystemWatcher cswatcher;

		private void Compile(object sender, EventArgs args) {
			var files = Files.All(Root + "/**/*.cs;" + Root + "/**/*.vb")
				.Where(file => !file.EndsWith(AttributeFile))
				.Select(file => Paths.Map(file))
				.ToList();
			var hash = files.Sum(file => Hash.Compute(Files.Load(file)));

			if (Files.FileExists(CSAssemblyName)) {
				var a = Assembly.Load("Silversite.AppCode.CSharp");
#if NET40
				var attr = a.GetCustomAttributes(typeof(AppCodeAttribute), true).OfType<AppCodeAttribute>().FirstOrDefault();
#else
				var attr = a.GetCustomAttribute<AppCodeAttribute>();
#endif
				if (attr.Files == files.Count && attr.Hash == hash) return;
			} else if (files.Count == 0) return;

			var attrfile = Root + "/" + AttributeFile;
			Files.Save("[assembly:Silversite.Services.AppCode(Files=" + files.Count + ", Hash=" + hash + ")]", attrfile);
			files.Add(Paths.Map(attrfile));

			var par = new CompilerParameters(Files.All("~/Bin/*.dll;~/Bin/Lazy/*.dll").Select(file => Paths.Map(file)).ToArray());
			par.ReferencedAssemblies.AddRange(Files.All("~/Bin/*.dll;~/Bin/Lazy/*.dll").ToArray());
			par.IncludeDebugInformation = true;
			par.GenerateInMemory = false;
			par.CompilerOptions = "";
			var cscompiler = CSharpCodeProvider.CreateProvider("C#");
			var vbcompiler = VBCodeProvider.CreateProvider("VisualBasic");

			par.OutputAssembly = Paths.Map(CSAssemblyName);
			cscompiler.CompileAssemblyFromFile(par, files.Where(f => f.EndsWith(".cs")).ToArray());
			par.OutputAssembly = Paths.Map(VBAssemblyName);
			vbcompiler.CompileAssemblyFromFile(par, files.Where(f => f.EndsWith(".vb")).ToArray());
		}

		public void Startup() {
			Tasks.DoLater(() => {
				cswatcher = new System.IO.FileSystemWatcher(Paths.Map(Root));
				cswatcher.Changed += Compile;
				cswatcher.Created += Compile;
				cswatcher.Deleted += Compile;
			
				Compile(this, EventArgs.Empty);

				cswatcher.EnableRaisingEvents = true;
			});
		}

		public void Shutdown() {
		}
	}

}