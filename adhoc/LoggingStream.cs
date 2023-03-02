namespace AdhocConsole
{
    public class LoggingStream : Stream
    {
        private readonly Stream _innerStream;
        private readonly Action<string> _logger;

        public LoggingStream(Stream innerStream, Action<string> logger)
        {
            _innerStream = innerStream;
            _logger = logger;
        }

        public override bool CanRead
        {
            get
            {
                _logger("CanRead");
                return _innerStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                _logger("CanSeek");
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                _logger("CanWrite");
                return _innerStream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                _logger("Length");
                return _innerStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                _logger("Get Position");
                return _innerStream.Position;
            }
            set
            {
                _logger("Set Position");
                _innerStream.Position = value;
            }
        }

        public override void Close()
        {
            _logger("Flush");

            _innerStream.Close();
        }

        public override void Flush()
        {
            _logger("Flush");
            _innerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            _logger("Read");
            return _innerStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            _logger("Seek");
            return _innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _logger("SetLength");
            _innerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _logger("Write");
            _innerStream.Write(buffer, offset, count);
        }
    }
}