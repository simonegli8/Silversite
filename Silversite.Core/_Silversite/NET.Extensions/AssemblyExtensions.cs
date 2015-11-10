using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;

namespace Silversite {

	public static class AssemblyExtensions {
	
		public static Type[] GetLoadableTypes(this Assembly a) {
			try {
				return a.GetTypes();
			} catch (ReflectionTypeLoadException ex) {
				return ex.Types.Where(t => t != null).ToArray();
			}
		}

		public static string DisplayName(this Assembly a) {
			var name = a.FullName;
			int comma = name.IndexOf(',');
			if (comma < 0) return name;
			return name.Substring(0, comma);
		}

	}
}