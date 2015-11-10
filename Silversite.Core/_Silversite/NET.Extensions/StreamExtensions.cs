using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace Silversite {

	public static class StreamExtensions {

		public static MemoryStream ToStream(this byte[] array) { return new MemoryStream(array); }

		public static byte[] ToArray(this Stream stream) {
			if (stream.CanSeek) {
				if (stream.Length > int.MaxValue) throw new NotSupportedException("Streams bigger than 2GB are not supported.");
				stream.Seek(0, System.IO.SeekOrigin.Begin);
				var buf = new byte[stream.Length];
				stream.Read(buf, 0, (int)stream.Length);
				return buf;
			} else {
				var m = new MemoryStream();
				stream.CopyTo(m);
				return m.ToArray();
			}
		}

#if NET35
		public static void CopyTo(this System.IO.Stream src, System.IO.Stream dest) {

			if (src is PipeStream) ((PipeStream)src).CopyTo(dest);

			const int Size = 8*1024;

			byte[] buffer = new byte[Size];

			var len = src.Read(buffer, 0, Size);
			while (len > 0) {
				dest.Write(buffer, 0, len);
				len = src.Read(buffer, 0, Size);
			}
		}
#endif
	}

}