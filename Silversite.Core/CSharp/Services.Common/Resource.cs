using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Web;
using System.Resources;
using System.Web.Resources;

namespace Silversite.Services {

	public class Resource {

		static ConcurrentDictionary<Type, ResourceManager> managers = new ConcurrentDictionary<Type,ResourceManager>();

		public static ResourceManager Manager<T>() { return managers.AddOrUpdate(typeof(T), new ResourceManager(typeof(T)), (type, m) => m); }
		public static string String<T>(string name) { return Manager<T>().GetString(name); }
		public static object Object<T>(string name) { return Manager<T>().GetObject(name); }
		public static System.IO.UnmanagedMemoryStream Stream<T>(string name) { return Manager<T>().GetStream(name); } 
	}
}