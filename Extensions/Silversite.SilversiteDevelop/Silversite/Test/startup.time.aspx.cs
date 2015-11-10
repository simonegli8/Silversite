using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Reflection;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Management;
using System.Threading;

namespace Silversite.Test {
	public partial class StartupTimeMeasure: System.Web.UI.Page {

		protected void Page_Load(object sender, EventArgs e) { if (!IsPostBack) n.Text = "5"; }

		protected void Start(object sender, EventArgs e) {
			int loops = 10;
			int.TryParse(n.Text, out loops);

			var t = new TimeSpan();
			var web = new WebClient();
			
			var iisexpress = Process.GetCurrentProcess();
			if (iisexpress.ProcessName != "iisexpress") { messages.Text = "You must run this site under iisexpress. Configure the Silversite.TestWeb project in VisualStudio to run in iisexpress."; return; }

			var w3wp = Process.GetProcessesByName("w3wp").FirstOrDefault();
			if (w3wp == null) { messages.Text = "IIS must be running"; return; }
			// var inetdir = Path.GetDirectoryName(w3wp.MainModule.FileName);
			var inetdir = Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\inetsrv";
			var cmd = Path.Combine(inetdir, "Appcmd.exe");
			string output;
			
			for (int i = 0; i < loops; i++) {

				var pinfo = new ProcessStartInfo(cmd, "recycle apppool \"SilversiteTest\"");
				pinfo.CreateNoWindow = true;
				pinfo.UseShellExecute = false;
				pinfo.WorkingDirectory = inetdir;
				pinfo.RedirectStandardOutput = true;

				try {
					var p = Process.Start(pinfo);
					p.WaitForExit();
					output = p.StandardOutput.ReadToEnd().Replace("\n", "\n<br/>");
				} catch (System.ComponentModel.Win32Exception ex) {
					output = ex.Message;
				}

				Services.Log.Write("Debug", output);

				messages.Text = output;

				if (output.StartsWith("ERROR")) return;

				// restart SQLEXPRESS
				pinfo = new ProcessStartInfo("cmd.exe", "/C NET STOP MSSQL$SQLEXPRESS");
				pinfo.UseShellExecute = true;
				pinfo.CreateNoWindow = true;
				pinfo.RedirectStandardOutput = false;
				
				try {
					var p = Process.Start(pinfo);
					p.WaitForExit();
				} catch { }

				pinfo = new ProcessStartInfo("cmd.exe", "/C NET START MSSQL$SQLEXPRESS");
				pinfo.UseShellExecute = true;
				pinfo.CreateNoWindow = true;
				pinfo.RedirectStandardOutput = false;

				try {
					var p = Process.Start(pinfo);
					p.WaitForExit();
				} catch { }

				Thread.Sleep(5000); // wait for SQLEXPRESS to start

				// init AppPool with an empty ASP.NET WebSite.
				web.DownloadString("http://localhost/emptyweb");

				Thread.Sleep(1000);

				// start Silversite WebSite
				var t0 = DateTime.Now;
				web.DownloadString(Request.Url);
				t += DateTime.Now - t0;
			}

			t = TimeSpan.FromTicks(t.Ticks / loops);
			
			time.Text = t.ToString("g");

			var dlls = Services.Files.DirectoryInfo("~/bin").EnumerateFiles("*.dll")
				.Union(Services.Files.DirectoryInfo("~/Silversite/bin").EnumerateFiles("*.dll"))
				.Select(f => f.Name)
				.ToArray(); 
			var assemblies = Services.Types.Assemblies.Where(a => dlls.Contains(a.FullName));
 
			var msg = new StringBuilder();
			msg.Append("Time: ");
			msg.Append(t.ToString("g"));
			msg.Append("<br/>");

			foreach (var a in assemblies) {
				var version = a.GetCustomAttributes(typeof(AssemblyVersionAttribute), false).FirstOrDefault() as AssemblyVersionAttribute;
				if (version != null) {
					msg.Append(a.FullName);
					msg.Append(": ");
					msg.Append(version.Version);
					msg.Append("<br/>");
				}
			}

			Services.Log.Write("StartupTimeHistory", msg.ToString()); 
		}
	}
}