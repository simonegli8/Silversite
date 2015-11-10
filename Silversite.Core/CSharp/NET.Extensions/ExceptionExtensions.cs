using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;

namespace Silversite {

	public static class ExceptionExtensions {

		public static int FrameCount(this Exception ex) {
			return ex.StackTrace.SplitList('\n').Count();
		}

		public static string RelativeStackTrace(this Exception ex) {
			var str = new StringBuilder();
			var frames = ex.StackTrace.Split('\n');
			for (int frame = 0; frame < ex.FrameCount(); frame++) {
				string method, file, text;
				int line;
				ex.Info(frame, out method, out file, out line, out text);
				if (file != null) {
					str.Append("at ");
					str.Append(method);
					str.Append("; ");
					str.Append(file);
					str.Append(":");
					str.Append(line);
					str.AppendLine();
				} else {
					str.AppendLine(frames[frame]);
				}
			}
			return str.ToString();
		}

		public static void Info(this Exception ex, int frame, out string method, out string SourceFile, out int SourceLine, out string SourceText) {
			var trace = ex.StackTrace.SplitList('\n').Skip(frame).FirstOrDefault();

			var regex = @"\s*at\s+([A-Za-z0-9_.,( ]+\))(\s+in\s+(([A-Za-z]:\\|\\\\).+):line\s+(\d+))?";
			var match = Regex.Match(trace, regex);

			if (match.Success) {
				method = match.Groups[1].Value;
				if (match.Groups[3].Success) {
					SourceFile = match.Groups[3].Value.Trim();
					var folders = SourceFile.Split('\\');
					int i = 1;
					while (i < folders.Length-1) {
						var path = "~/" + folders.Skip(i).StringList('/');
						if (Silversite.Services.Files.Exists(path)) {
							SourceFile = path;
							break;
						}
						i++;
					}
				} else SourceFile = null;
				var line = 0;
				if (match.Groups[5].Success) int.TryParse(match.Groups[5].Value, out line);
				SourceLine = line;
			} else {
				method = null;
				SourceFile = null;
				SourceLine = 0;
			}

			SourceText = null;
			try {
				if (!string.IsNullOrEmpty(SourceFile) && Services.Files.Exists(SourceFile)) {
					var lines = Services.Files.LoadLines(SourceFile);
					string header = "";
					string footer = "";
					if (SourceLine > lines.Count - 5) header = Environment.NewLine.Repeat(SourceLine - lines.Count + 5);
					if (SourceLine < 5) footer = Environment.NewLine.Repeat(5 - SourceLine) + lines;
					int start = Math.Max(SourceLine - 4, 1) - 1;
					int end = Math.Min(SourceLine + 4, lines.Count) - 1;
					SourceText = header + lines.Skip(start).Take(end - start + 1).StringList(Environment.NewLine) + footer;
				}
			} catch { }
		}

		public static void Info(this Exception ex, out string Method, out string SourceFile, out int SourceLine, out string SourceText) { Info(ex, 0, out Method, out SourceFile, out SourceLine, out SourceText); }
	}
}