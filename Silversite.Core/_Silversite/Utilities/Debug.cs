using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.Diagnostics.Contracts;

namespace Silversite {

	[Serializable]
	public class AssertException: Services.SevereException {
		public AssertException() : base() { }
		public AssertException(string msg) : base(msg) { }
		public AssertException(string msg, Exception innerException) : base(msg, innerException) { }
	}

	public class Debug {

		[Obsolete("Use Contract.Assert from System.Diagnostics instead.")]
		[System.Diagnostics.Conditional("DEBUG")]
		public static void Assert(bool condition, string msg, params object[] args) {
			if (!condition) {
				msg = "Debug.Assert failed: " + msg;
				try {
					throw new AssertException(string.Format(msg, args));
				} catch (Exception ex) {
					Services.Log.Severe<AssertException>();
					Services.Log.Error(msg, ex, msg, args);
				}
#if BREAK
				Contract.Assert(condition);
#endif
			}
		}

		[Obsolete("Use Contract.Assert from System.Diagnostics instead.")]
		[System.Diagnostics.Conditional("DEBUG")]
		public static void Assert(bool condition, Func<string, Exception> raiseException, string msg, params object[] args) {
			if (!condition) {
				msg = "Debug.Assert failed: " + msg;
				try {
					raiseException(string.Format(msg, args));
				} catch (Exception ex) {
					var args2 = args.ToList();
					args2.Add(Services.Log.ErrorClass.Severe);
					Services.Log.Error(msg, ex, msg, args2.ToArray());
				}
#if BREAK
				Contract.Assert(condition);
#endif
			}
		}

		[Obsolete("Use Contract.Assert from System.Diagnostics instead.")]
		[System.Diagnostics.Conditional("DEBUG")]
		public static void Assert(bool condition) {
			Assert(condition, string.Empty);
		}
		[Obsolete("Use Contract.Assert from System.Diagnostics instead.")]
		[System.Diagnostics.Conditional("DEBUG")]
		public static void Assert(bool condition, Func<Exception> raiseException) {
			Assert(condition, s => raiseException(), string.Empty);
		}
		[System.Diagnostics.Conditional("BREAK")]
		public static void Break() {
			System.Diagnostics.Debugger.Break();
		}

		[System.Diagnostics.Conditional("BREAK")]
		public static void Break(bool condition) {
			if (condition) System.Diagnostics.Debugger.Break();
		}

		[System.Diagnostics.Conditional("BREAK")]
		public static void Break(bool condition, params object[] pars) {
			if (condition) {
				int i = 0;
				foreach (var par in pars) Message("Break Par[{0}]: {1}", i, pars[i++].ToString());
				System.Diagnostics.Debugger.Break();
			}
		}

#if DEBUG
		public const bool IsDebug = true;
#else
		public const bool IsDebug = false;
#endif
		public const bool IsRelease = !IsDebug;

		[System.Diagnostics.Conditional("DEBUG")]
		public static void Message(int level, string category, string msg, params object[] args) {
			if (msg == null) msg = "";
			if (args != null) msg = string.Format(msg, args);
			if (!msg.EndsWith(Environment.NewLine)) msg += Environment.NewLine;
			string p;
			switch (level) {
				default:
				case 0: p = "#> "; break;
				case 1: p = "@> "; break;
				case 2: p = "!> "; break;
				case 4: p = "?!> "; break;
			}
			System.Diagnostics.Debugger.Log(level, category, p + msg);
		}
		[System.Diagnostics.Conditional("DEBUG")]
		public static void Message(int level, string category, string msg) {
			Message(level, category, msg, null);
		}
		[System.Diagnostics.Conditional("DEBUG")]
		public static void Message(string msg, params object[] args) {
			Message(1, "Debug", msg, args);
		}
		[System.Diagnostics.Conditional("DEBUG")]
		public static void Message(string msg) {
			Message(1, "Debug", msg);
		}

	}
}