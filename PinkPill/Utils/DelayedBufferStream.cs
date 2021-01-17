using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PinkPill.Utils
{
    /// <summary>
    /// 这个 Stream 基本做的事情是把传入的数据等待 100ms 再一起发出去
    /// </summary>
    public class DelayedBufferStream : Stream
    {
        readonly AnonymousPipeServerStream _pipeServer;
        readonly AnonymousPipeClientStream _pipeClient;
        readonly Queue<byte[]> _queue;
        DateTime _lastWriteTime;
        volatile bool _closed;

        EventHandler<byte[]> OnMessage;

        public void SendMessage(ReadOnlySpan<byte> bytes)
        {
            _pipeServer.Write(bytes);
        }

        public DelayedBufferStream(EventHandler<byte[]> onMessage)
        {
            OnMessage = onMessage;
            _pipeServer = new AnonymousPipeServerStream(PipeDirection.Out);
            _pipeClient = new AnonymousPipeClientStream(PipeDirection.In, _pipeServer.ClientSafePipeHandle);
            _queue = new Queue<byte[]>();
            _lastWriteTime = DateTime.Now;

            Task.Factory.StartNew(() =>
            {
                while (!_closed)
                {
                    lock (_queue)
                    {
                        if ((DateTime.Now - _lastWriteTime).TotalMilliseconds < 100 || _queue.Count == 0) continue;

                        // merge spans
                        var bytes = new byte[_queue.Sum(array => array.Length)];
                        var span = new Span<byte>(bytes);
                        var position = 0;
                        foreach (var array in _queue)
                        {
                            array.CopyTo(span[position..]);
                            position += array.Length;
                        }

                        OnMessage(this, bytes);
                        _queue.Clear();
                    }
                    Thread.Sleep(100);
                }
            }, TaskCreationOptions.LongRunning);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _pipeClient.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var span = new Span<byte>(buffer)[offset..(offset + count)];
            lock (_queue)
            {
                _queue.Enqueue(span.ToArray());
                _lastWriteTime = DateTime.Now;
            }

        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _closed = true;
        }

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => 0;
        public override void SetLength(long value) { }

        public override bool CanRead { get; } = true;
        public override bool CanSeek { get; } = false;
        public override bool CanWrite { get; } = true;
        public override long Length { get; } = 0;
        public override long Position { get; set; }
    }
}
