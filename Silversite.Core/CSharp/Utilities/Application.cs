using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Concurrent;
using System.Threading;
using System.Web.Hosting;
using System.Diagnostics.Contracts;

[assembly: Silversite.Services.DependsOn(typeof(Silversite.Services.Application))]

namespace Silversite.Services {

	/// <summary>
	/// Implements Application wide locking for WebFarms.
	/// </summary>
	public class Application: IHttpHandler, Web.IHttpAutoModule, IAutostart {

		const string WebService = "~/Silversite/services/ApplicationEvents.ashx";
		static string WebServiceUrl = null;

		static HttpApplicationState state;
		public static HttpApplicationState State { get { return state; } private set { state = value; } }

		[Serializable]
		public class Key {
			public Key() { Keys = null; Name = null; }
			public Key(string prefix): this() { Prefix = null; }
			public Key(string prefix, object[] keys): this(prefix) { Keys = keys; Hash = Services.Hash.Compute(this); }
			public Key(string prefix, string name): this(prefix) { Keys = null; Hash = Services.Hash.Compute(name); Name = name; }
			public Key(string prefix, Key key): this(prefix) { Keys = key.Keys; Name = key.Name; Hash = key.Hash; }
			internal object[] Keys { get; private set; }
			internal string Name { get; set; }
			internal string Prefix { get; set; }
			public override bool Equals(object obj) {
				if (!(obj is Key)) return false;
				var key = (Key)obj;
				if (Name != null && Name != key.Name) return false;
				if (Hash != key.Hash) return false;
				if (Keys.Length != key.Keys.Length) return false;
				for (var i = 0; i < Keys.Length; i++) {
					if (!Keys[i].Equals(key.Keys[i])) return false;
				}
				return true;
			}
			internal int Hash { get; private set; }

			internal static Key New(string prefix, params object[] keys) { return new Key(prefix, keys); }
			internal static Key New(string prefix, string name) { return new Key(prefix, name); }
			internal static Key New(string prefix, Key key) { return new Key(prefix, key); }

			internal void Create() {
				if (Name != null) return;
				var hash = Hash;
				var random = new Random(hash);
				bool loop;
				IWithKey app;
				do {
					loop = false;
					Name = Prefix + "._" + random.Next();
					app = State[Name] as IWithKey;
				} while (app != null && !app.Key.Equals(this));
			}

			internal IWithKey Value { get { Create(); return State[Name] as IWithKey; } set { Create(); State[Name] = value; } } 
		}

		public interface IWithKey { Key Key { get; set; } }
 
		// events
		[Serializable]
		public class EventInfo: IWithKey {
			
			internal const string Prefix = "Silversite.ApplicationEvents";

			public EventInfo() { Key = Key.New(Prefix, "Default");Listeners = new List<string>(); }
			public EventInfo(string name): this() { Key = Key.New(Prefix, name); }
			public EventInfo(params object[] keys): this() { Key = Key.New(Prefix, keys); }
			public EventInfo(Key key): this() { Key = Key.New(Prefix, key); }

			public Key Key { get; set; }
			internal List<string> Listeners { get; private set; }

			[NonSerialized]
			HashSet<EventHandler> notify = new HashSet<EventHandler>();

			internal EventInfo Value { get { return Key.Value as EventInfo; } set { Key.Value = Listeners.Count > 0 && value.Raises.Count > 0 ? value : null; } }

			public event EventHandler Notify {
				add {
					if (notify.Count == 0) {
						State.Lock();
						var appevent = Value;
						if (appevent != null) {
							Listeners = appevent.Listeners;
						}
						Listeners.Add(WebServiceUrl);
						Value = this;
						notify.Add(value);
						State.UnLock();
					} else {
						notify.Add(value);
					}
				}
				remove {
					if (notify.Count <= 1) {
						State.Lock();
						var appevent = Value;
						if (appevent != null) {
							Listeners = appevent.Listeners;
						}
						Listeners.Remove(WebServiceUrl);
						Value = this;
						notify.Remove(value);
						State.UnLock();
					} else {
						notify.Remove(value);
					}
				}
			}

			[Serializable]
			internal class RaiseArgs {
				public RaiseArgs(object sender, EventArgs e, bool once, bool wait) { Id =  NewId(sender, e); Sender = sender; Args = e; Raised = DateTime.Now; Once = once; Wait = wait; Handler = false; }
				public RaiseArgs(int id) { Id = id; Handler = true; }
				internal int NewId(object sender, EventArgs e) {	return new Random((sender ?? 0).GetHashCode() + (e ?? EventArgs.Empty).GetHashCode() +(int)DateTime.Now.Ticks).Next(); }
				internal int Id;
				internal object Sender;
				internal EventArgs Args;
				internal DateTime Raised;
				internal bool Once;
				internal bool Wait;
				[NonSerialized]
				internal bool Handler;
			}

			List<RaiseArgs> Raises = new List<RaiseArgs>();

			internal void Raise(RaiseArgs args) {
				bool propagate = false;
				State.Lock();
				var appevent = Value;
				//RaiseArgs args = null;
				List<EventHandler> notifies = notify.ToList();
				if (appevent != null) {
					Listeners = appevent.Listeners;
					Raises = appevent.Raises;

					var now = DateTime.Now;

					var appargs = appevent.Raises.FirstOrDefault(r => r.Id == args.Id);
					if (appargs == null) {
						if (!args.Handler) {
							Raises.Add(args);
							propagate = true;
						} else args = null;
					} else {
						args = appargs;
						if (appargs != null && appargs.Once) {
							notify.Clear();
							var listener = Listeners.FirstOrDefault(l => WebServiceUrl != null && l.StartsWith(WebServiceUrl));
							if (listener != null) {
								Listeners.Remove(listener);
							}
						}
					}

					Raises.RemoveAll(r => now - r.Raised > new TimeSpan(0, 2, 0));
					Value = this;
				}
				State.UnLock();

				Action a = () => {
					if (propagate) {
						Listeners
							.Where(l => WebServiceUrl == null || !l.StartsWith(WebServiceUrl))
							.AwaitEach(l => Files.Html(new Uri(l + "?name=" + Key.Name + "&id=" + args.Id)));
					}

					if (args != null) notifies.ForEach(Handler => Handler(args.Sender, args.Args));
				};
					
				if (args.Wait) a();
				else Tasks.DoLater(a);

			}

			public void Raise(object sender, EventArgs e) { Raise(new RaiseArgs(sender, e, false, false)); }
			public void Raise(EventArgs e) { Raise(null, e); }
			public void Raise() { Raise(null, EventArgs.Empty); }
			public void RaiseOnce(object sender, EventArgs e) { Raise(new RaiseArgs(sender, e, true, false)); }
			public void RaiseOnce(EventArgs e) { RaiseOnce(null, e); }
			public void RaiseOnce() { Raise(null, EventArgs.Empty); }
			public void AwaitRaise(object sender, EventArgs e) { Raise(new RaiseArgs(sender, e, false, true)); }
			public void AwaitRaise(EventArgs e) { AwaitRaise(null, e); }
			public void AwaitRaise() { AwaitRaise(null, EventArgs.Empty); }
			public void AwaitRaiseOnce(object sender, EventArgs e) { Raise(new	RaiseArgs(sender, e, true, true)); }
			public void AwaitRaiseOnce(EventArgs e) { AwaitRaiseOnce(null, e); }
			public void AwaitRaiseOnce() { AwaitRaiseOnce(null, EventArgs.Empty); }

			public void Raise(int id) { Raise(new RaiseArgs(id)); }

			public void Add(EventHandler handler) { Notify += handler; }
			public void Remove(EventHandler handler) { Notify -= handler; }
			public void RemoveAll() {
				var first = notify.FirstOrDefault();
				if (first != null) {
					notify.RemoveWhere(h => h != first);
					Notify -= first;
				}
			}
		}

		public class EventCollection {
			ConcurrentDictionary<Key, EventInfo> Events = new ConcurrentDictionary<Key, EventInfo>();

			public EventInfo this[string name] { get { return Events.GetOrAdd(Key.New(EventInfo.Prefix, name), new EventInfo(name)); } }
			public EventInfo this[params object[] keys] { get { return Events.GetOrAdd(Key.New(EventInfo.Prefix, keys), new EventInfo(keys)); } }
			public EventInfo this[string prefix, Key key] { get { return Events.GetOrAdd(Key.New(EventInfo.Prefix, key), new EventInfo(key)); } }
		}

		public static readonly EventCollection Event = new EventCollection();

		// locks
		[Serializable]
		public class LockState: IWithKey, IDisposable {
			internal const string Prefix = "Silversite.ApplicationLocks";
			internal const string EventPrefix = Prefix + ".Event";

			public LockState() { Key = null; IsOwner = false; }
			public LockState(object[] keys) : this() { Key = Key.New(Prefix, keys); }
			public LockState(string name) : this() { Key = Key.New(Prefix, name); }

			public Key Key { get; set; }
			public void Dispose() { Unlock(); }

			[NonSerialized]
			internal DateTime Release;
			[NonSerialized]
			internal bool IsOwner;
			[NonSerialized]
			internal ManualResetEvent ResetEvent;
			[NonSerialized]
			internal bool LacksListener;
			internal string Name { get { Key.Create(); return Key.Name; } }
			internal LockState Value { get { return Key.Value as LockState; } set { Key.Value = value; } }
			[NonSerialized]
			int level = 0;

			internal void Unlock() {
				LockState t;
				try {
					State.Lock();
					var applock = Value;
					if (applock == null) {
						lock (Key) {
							if (ResetEvent != null) {
								ResetEvent.Set();
								ResetEvent = null;
							} else {
								level--;
								if (level == 0) {
									Monitor.Exit(this);
									Locks.TryRemove(Key, out t);
								} else if (level < 0) level = 0;
							}
						}
						return;
					} else {
						Release = applock.Release;
						Value = null;
					}
				} catch {
				} finally {
					State.UnLock();
				}

				// notify all listeners
				Event[EventPrefix, Key].RaiseOnce();

				lock (Key) {
					level--;
					if (level == 0) {
						Monitor.Exit(this);
						Locks.TryRemove(Key, out t);
					} else if (level < 0) level = 0;
				}
			}

			internal LockState Lock(TimeSpan timeout) {
				if (Monitor.TryEnter(this, timeout)) {
					level++;
					if (level > 1) return this;
					try {
						State.Lock();
						var applock = Value;
						if (applock == null) {
							Contract.Assert(!IsOwner);
							Release = DateTime.Now + timeout;
							Value = this;
							IsOwner = true; // this Server owns the lock.
						} else {
							Release = applock.Release;
							if (WebServiceUrl == null) LacksListener = true;
							else Event[EventPrefix, Key].Add((sender, args) => Unlock());
							IsOwner = false; // another Server owns the lock.
							Value = this;
							if (!LacksListener) ResetEvent = new ManualResetEvent(false);
						}
					} finally {
						State.UnLock();
					}
					if (!IsOwner) {
						if (Release < DateTime.Now) Unlock();
						else {
							if (LacksListener) { // do polling
								LockState applock;
								do {
									applock = null;
									Thread.Sleep(100);
									try {
										State.Lock();
										applock = Value;
										if (applock != null && WebServiceUrl != null) {
											Release = applock.Release;
											Event[EventPrefix, Key].Add((sender, args) => Unlock());
											IsOwner = false; // another Server owns the lock.
											Value = this;
											LacksListener = false;
											ResetEvent = new ManualResetEvent(false);
											applock = null;
										}
									} finally {
										State.UnLock();
									}
								} while (applock != null);
							}
							if (ResetEvent != null) ResetEvent.WaitOne(timeout);
						}
					}
				} else {
					Unlock();
				}
				return this;
			}
		}

		static ConcurrentDictionary<Key, LockState> Locks = new ConcurrentDictionary<Key, LockState>();

		public static IDisposable Lock(TimeSpan timeout, params object[] keys) { return Locks.GetOrAdd(Key.New(LockState.Prefix, keys), new LockState(keys)).Lock(timeout); }
		public static IDisposable Lock(TimeSpan timeout, string name) { return Locks.GetOrAdd(Key.New(LockState.Prefix, name), new LockState(name)).Lock(timeout); }
		public static IDisposable Lock(int milliseconds, params object[] keys) { return Lock(TimeSpan.FromMilliseconds(milliseconds), keys); }
		public static IDisposable Lock(int milliseconds, string name) { return Lock(TimeSpan.FromMilliseconds(milliseconds), name); }
		public static IDisposable Lock<T>(TimeSpan timeout, params object[] keys) { return Lock(timeout, keys.Prepend(typeof(T)).ToArray()); }
		public static IDisposable Lock<T>(int milliseconds, params object[] keys) { return Lock(TimeSpan.FromMilliseconds(milliseconds), keys.Prepend(typeof(T)).ToArray()); }

		public static void Unlock(params object[] keys) {
			LockState state;
			if (Locks.TryGetValue(Key.New(LockState.Prefix, keys), out state)) state.Unlock();
		}
		public static void Unlock(string name) {
			LockState state;
			if (Locks.TryGetValue(Key.New(LockState.Prefix, name), out state)) state.Unlock();
		}
		public static void Unlock<T>(params object[] keys) { Unlock(keys.Prepend(typeof(T)).ToArray()); }
	
		// event handler
		public void ProcessRequest(HttpContext context) {
			var name = context.Request.QueryString["name"];
			var id = int.Parse(context.Request.QueryString["id"]);
			Event[name].Raise(id);
		}

		public void Dispose() { }

		public void Init(HttpApplication app) {
			try {
				app.BeginRequest += (sender, args) => {
					try {
						if (State == null) State = HttpContext.Current.Application;
						if (WebServiceUrl == null) Tasks.DoLater(() => WebServiceUrl = Paths.Url(WebService));
					} catch { }
				};
			} catch { }
		}

		public bool IsReusable { get { return true; } }

		public void Startup() { }

		public void Shutdown() {
			// unlock all owned locks.
			Locks.Values
				.Where(l => l.IsOwner)
				.ForEach(l => l.Unlock());
		}
	}

}