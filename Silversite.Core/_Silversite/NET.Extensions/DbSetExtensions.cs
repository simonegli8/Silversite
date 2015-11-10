using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Linq.Expressions;


namespace Silversite {
	public static class DbSetExtensions {

		public static void Remove<T>(this DbSet<T> set, IQueryable<T> elements) where T : class { foreach (var e in elements) set.Remove(e); }
		public static void Remove<T>(this DbSet<T> set, IEnumerable<T> elements) where T : class { set.Remove(elements.AsQueryable()); }
		public static void Remove<T> (this DbSet<T> set, Func<T, bool> selector) where T: class { foreach (var e in set.Where(selector)) set.Remove(e); }
		public static void RemoveAll<T>(this DbSet<T> set) where T : class { foreach (var e in set) set.Remove(e); }

		public static void AddRange<T>(this DbSet<T> set, IEnumerable<T> range) where T : class { foreach (T x in range) set.Add(x); }
		public static void Add<T>(this DbSet<T> set, IEnumerable<T> range) where T : class { set.AddRange(range); }

		public static DbQuery<T> Include<T>(this DbQuery<T> query, Expression<Func<T, object>> subSelector) {
			return query.Include(((subSelector.Body as MemberExpression).Member as System.Reflection.PropertyInfo).Name);
		}

		// set.Remove(set.Where(selector));

		//public static T FirstOrDefault<T>(this IQueryable<T> set, Func<T, bool> selector) where T: class { return set.FirstOrDefault<T>(selector); }
		//public static T FirstOrDefault<T>(this IEnumerable<T> set, Func<T, bool> selector) where T: class { return set.FirstOrDefault<T>(selector); }

	}
}