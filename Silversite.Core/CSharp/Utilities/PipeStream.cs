using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;

namespace Silversite.Services {

	enum StreamConsumer { Undefined = 0, Reader = 1, Writer = 2, Closed = 3 }

	public class PipeStream : Stream {

		private object _lockForRead;
		private object _lockForAll;
		private Queue<object> _chunks;
		private object _currentChunk;
		private long _currentChunkPosition;
		private ManualResetEvent _doneWriting;
		private ManualResetEvent _dataAvailable;
		private WaitHandle[] _events;
		private int _doneWritingHandleIndex;
		private volatile bool _illegalToWrite;
		private Exception _exception;
		public bool IsWriteClosed { get; private set; }
		public bool IsReadClosed { get; private set; }
		static int id = 1;
		public int ID = id++;
		long length = 0;

		private ThreadLocal<StreamConsumer> _consumer = new ThreadLocal<StreamConsumer>();

		public PipeStream() {
			_chunks = new Queue<object>();
			_doneWriting = new ManualResetEvent(false);
			_dataAvailable = new ManualResetEvent(false);
			_events = new WaitHandle[] { _dataAvailable, _doneWriting };
			_doneWritingHandleIndex = 1;
			_lockForRead = new object();
			_lockForAll = new object();
			IsWriteClosed = false;
			IsReadClosed = false;
		}

		public override bool CanRead { get { return true; } }
		public override bool CanSeek { get { return false; } }
		public override bool CanWrite { get { return !_illegalToWrite; } }

		public override void Flush() { }
		public override long Length { get { _doneWriting.WaitOne(); return length; } }
		public override long Position {
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		public override long Seek(long offset, SeekOrigin origin) {
			throw new NotSupportedException();
		}
		public override void SetLength(long value) {
			throw new NotSupportedException();
		}

		public override int Read(byte[] buffer, int offset, int count) {
			if (_consumer.Value == StreamConsumer.Writer) throw new NotSupportedException("You cannot read & write to a PipeStream from the same thread.");
			_consumer.Value = StreamConsumer.Reader;

			lock (_lockForAll) if (_exception != null) { var ex = _exception; _exception = null; throw ex; }

			if (buffer == null) throw new ArgumentNullException("buffer");
			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException("offset");
			if (count < 0 || offset + count > buffer.Length)
				throw new ArgumentOutOfRangeException("count");
			if (_dataAvailable == null)
				throw new ObjectDisposedException(GetType().Name);

			if (count == 0) return 0;

			while (true) {

				if (IsWriteClosed == true) System.Diagnostics.Debugger.Break();

				int handleIndex = WaitHandle.WaitAny(_events);
				lock (_lockForRead) {
					lock (_lockForAll) {
						if (_currentChunk == null) {
							if (_chunks.Count == 0) {
								if (handleIndex == _doneWritingHandleIndex)
									return 0;
								else continue;
							}
							_currentChunk = _chunks.Dequeue();
							_currentChunkPosition = 0;
						}
					}

					if (_currentChunk is Stream) {
						var read = ((Stream)_currentChunk).Read(buffer, offset, count);
						if (read  != count) {
							_currentChunk = null;
							_currentChunkPosition = 0;
							lock (_lockForAll) {
								if (_chunks.Count == 0) _dataAvailable.Reset();
							}
						} else {
							_currentChunkPosition += count;
						}
						return read;

					} else {

						long bytesAvailable =
							((_currentChunk is Stream) ? ((Stream)_currentChunk).Length : ((byte[])_currentChunk).Length) - _currentChunkPosition;
						int bytesToCopy;
						if (bytesAvailable > count) {
							bytesToCopy = count;
							Buffer.BlockCopy((byte[])_currentChunk, (int)_currentChunkPosition, buffer, offset, count);
							_currentChunkPosition += count;
						} else {
							bytesToCopy = (int)bytesAvailable;
							Buffer.BlockCopy((byte[])_currentChunk, (int)_currentChunkPosition, buffer, offset, bytesToCopy);
							_currentChunk = null;
							_currentChunkPosition = 0;
							lock (_lockForAll) {
								if (_chunks.Count == 0) _dataAvailable.Reset();
							}
						}
						return bytesToCopy;
					}
				}
			}
		}

		public new virtual void CopyTo(Stream stream) {
		if (_consumer.Value == StreamConsumer.Writer) throw new NotSupportedException("You cannot read & write to a PipeStream from the same thread.");
			_consumer.Value = StreamConsumer.Reader;

			if (_dataAvailable == null)
				throw new ObjectDisposedException(GetType().Name);

			lock (_lockForAll) if (_exception != null) { var ex = _exception; _exception = null; throw ex; }

			while (true) {

				if (IsWriteClosed == true) System.Diagnostics.Debugger.Break();

				int handleIndex = WaitHandle.WaitAny(_events);
				lock (_lockForRead) {
					lock (_lockForAll) {
						if (_currentChunk == null) {
							if (_chunks.Count == 0) {
								if (handleIndex == _doneWritingHandleIndex)
									return;
								else continue;
							}
							_currentChunk = _chunks.Dequeue();
							_currentChunkPosition = 0;
						}
					}

					if (_currentChunk is Stream) {
						((Stream)_currentChunk).CopyTo(stream);
					} else {
						var buffer = (byte[])_currentChunk;
						stream.Write(buffer, (int)_currentChunkPosition, buffer.Length);
					}
					_currentChunk = null;
					_currentChunkPosition = 0;
					lock (_lockForAll) {
						if (_chunks.Count == 0) _dataAvailable.Reset();
					}
				}
			}
		}

		public override void Write(byte[] buffer, int offset, int count) {
			if (_consumer.Value == StreamConsumer.Reader) throw new NotSupportedException("You cannot read & write to a PipeStream from the same thread.");
			_consumer.Value = StreamConsumer.Writer;

			lock (_lockForAll) if (_exception != null) { var ex = _exception; _exception = null; throw ex; }

			if (buffer == null) throw new ArgumentNullException("buffer");
			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException("offset");
			if (count < 0 || offset + count > buffer.Length)
				throw new ArgumentOutOfRangeException("count");
			if (_dataAvailable == null)
				throw new ObjectDisposedException(GetType().Name);

			if (count == 0) return;

			byte[] chunk = new byte[count];
			Buffer.BlockCopy(buffer, offset, chunk, 0, count);
			lock (_lockForAll) {
				if (_illegalToWrite)
					throw new InvalidOperationException(
						"Writing has already been completed.");
				length += count;
				_chunks.Enqueue(chunk);
				_dataAvailable.Set();
			}
		}

		public virtual void Write(Stream stream) {
			if (_consumer.Value == StreamConsumer.Reader) throw new NotSupportedException("You cannot read & write to a PipeStream from the same thread.");
			_consumer.Value = StreamConsumer.Writer;

			if (_dataAvailable == null)
				throw new ObjectDisposedException(GetType().Name);

			lock (_lockForAll) {
				if (_exception != null) { var ex = _exception; _exception = null; throw ex; }
				if (_illegalToWrite)
					throw new InvalidOperationException("Writing has already been completed.");

				length += stream.Length;
				_chunks.Enqueue(stream);
				_dataAvailable.Set();
			}
		}

		public void Exception(Exception ex) { lock (_lockForAll) _exception = ex; }

		public override void Close() {
			if (_consumer.Value == StreamConsumer.Reader) {
				_consumer.Value = StreamConsumer.Closed;
				base.Close();
				if (_dataAvailable != null) {
					_dataAvailable.Close();
					_dataAvailable = null;
				}
				if (_doneWriting != null) {
					_doneWriting.Close();
					_doneWriting = null;
				}
			} else if (_consumer.Value == StreamConsumer.Writer || _consumer.Value == StreamConsumer.Undefined) {
				_consumer.Value = StreamConsumer.Closed;
				if (_consumer.Value == StreamConsumer.Reader) throw new NotSupportedException("You cannot read & write to a PipeStream from the same thread.");
				if (_dataAvailable == null)
					throw new ObjectDisposedException(GetType().Name);
				lock (_lockForAll) {
					_illegalToWrite = true;
					_doneWriting.Set();
				}
			}
		}

		protected override void Dispose(bool disposing) {
			Close();
			base.Dispose(disposing);
		}
	}

}