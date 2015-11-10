using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Xml.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web.Security;

namespace Silversite.Services {

	public partial class Hash {

		public static int Compute(byte[] data) {
			unchecked {
				const int p = 16777619;
				int hash = (int)2166136261;

				for (int i = 0; i < data.Length; i++)
					hash = (hash ^ data[i]) * p;

				hash += hash << 13;
				hash ^= hash >> 7;
				hash += hash << 3;
				hash ^= hash >> 17;
				hash += hash << 5;
				hash &= 0x7FFFFFFF;
				return hash;
			}
		}

		public static int Compute(string s) { return Compute(System.Text.Encoding.UTF8.GetBytes(s)); }

		public static int Compute(XElement e) {
			using (var m = new MemoryStream())
			using (var w = System.Xml.XmlWriter.Create(m, new System.Xml.XmlWriterSettings() { NewLineChars = "", Encoding = System.Text.Encoding.UTF8, Indent = false })) {
				e.Save(w);
				return Compute(m.GetBuffer());
			}
		}

		public static int Compute(object serializable) {
			if (serializable is string) return Compute((string)serializable);
			if (serializable is byte[]) return Compute((byte[])serializable);
			return Compute(ToBytes(serializable));
		}

		// Convert an object to a byte array
		public static byte[] ToBytes(object obj)
		{
			if(obj == null) return new byte[1] { 0 };
			if (obj is string) {
				if (((string)obj) == "") return new byte[1] { 4 };
				var buf = System.Text.Encoding.UTF8.GetBytes((string)obj);
				using (var ms = new MemoryStream()) {
					using (var zs = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionMode.Compress, true)) {
						zs.Write(buf, 0, buf.Length);
					}
					if (ms.Length < buf.Length) return ms.ToArray().Prepend((byte)2).ToArray();
					else return buf.Prepend((byte)3).ToArray();
				}
			} else {
				using (var ms = new MemoryStream()) {
					ms.Write(new byte[1] { 1 }, 0, 1);
					using (var zs = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionMode.Compress, true)) {
						var f = new BinaryFormatter();
						f.Serialize(zs, obj);
					}
					return ms.ToArray();
				}
			}
		}

		// Convert a byte array to an Object
		public static object ToObject(byte[] bytes)
		{
			switch (bytes[0]) {
				case 0: return null;
				default:
				case 1:
					using (var m = new MemoryStream()) {
						m.Write(bytes, 1, bytes.Length-1);
						m.Flush();
						m.Seek(0, SeekOrigin.Begin);
						using (var zs = new System.IO.Compression.DeflateStream(m, System.IO.Compression.CompressionMode.Decompress, true)) {
							var f = new BinaryFormatter();
							return f.Deserialize(zs);
						}
					}
					break;
				case 2:
					using (var m = new MemoryStream()) {
						m.Write(bytes, 1, bytes.Length-1);
						m.Flush();
						m.Seek(0, SeekOrigin.Begin);
						using (var zs = new System.IO.Compression.DeflateStream(m, System.IO.Compression.CompressionMode.Decompress, true)) {
							return System.Text.Encoding.UTF8.GetString(zs.ToArray());
						}
					}
				case 3: return System.Text.Encoding.UTF8.GetString(bytes.Skip(1).ToArray());
				case 4: return "";
			}
		}
	}
}