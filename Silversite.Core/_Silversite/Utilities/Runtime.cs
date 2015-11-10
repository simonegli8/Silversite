using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security;
using System.Runtime.InteropServices;

namespace Silversite.Services {

	public static class Runtime {
		public static bool IsMono { get { return Type.GetType ("Mono.Runtime") != null; } }
		public static bool IsMicrosoft { get { return !IsMono; } }
		public static bool IsWindows { get { return IsMicrosoft; } }
		public static bool IsNonWindows { get { return !IsMicrosoft; } }
		public static bool Is32BitProcess { get { return IntPtr.Size == 4; } }
		public static bool Is64BitProcess { get { return IntPtr.Size == 8; } }
		public static bool Is64BitOS {
			get {
				if (Is64BitProcess) return true;
				bool flag;
				return ((DoesWin32MethodExist("kernel32.dll", "IsWow64Process") && IsWow64Process(GetCurrentProcess(), out flag)) && flag);
			}
		}
		public static bool Is32BitOS { get { return !Is64BitOS; } }

		public static readonly Version Version = System.Environment.Version;

		///<summary>
		/// The function determins whether a method exists in the export 
		/// table of a certain module.
		/// </summary>
		/// <param name="moduleName">The name of the module</param>
		/// <param name="methodName">The name of the method</param>
		/// <returns>
		/// The function returns true if the method specified by methodName 
		/// exists in the export table of the module specified by moduleName.
		/// </returns>
		static bool DoesWin32MethodExist(string moduleName, string methodName) {
			IntPtr moduleHandle = GetModuleHandle(moduleName);
			if (moduleHandle == IntPtr.Zero) return false;
			return (GetProcAddress(moduleHandle, methodName) != IntPtr.Zero);
	  }

	  [DllImport("kernel32.dll")]
	  static extern IntPtr GetCurrentProcess();

	  [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
	  static extern IntPtr GetModuleHandle(string moduleName);

	  [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
	  static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)]string procName);

	  [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	  [return: MarshalAs(UnmanagedType.Bool)]
	  static extern bool IsWow64Process(IntPtr hProcess, out bool wow64Process);

		public static AspNetHostingPermissionLevel Trust {
			get {
				for (var level = AspNetHostingPermissionLevel.Unrestricted; level >= AspNetHostingPermissionLevel.None; level -= 100) {
					//AppDomain.CurrentDomain.PermissionSet;
					if (SecurityManager.IsGranted(new AspNetHostingPermission(level))) return level;
				}
				return AspNetHostingPermissionLevel.None;
			}
		}

	}

}