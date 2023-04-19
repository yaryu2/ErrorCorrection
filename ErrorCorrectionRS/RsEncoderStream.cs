﻿using ErrorCorrection;

namespace ErrorCorrectionRS
{
    public class RsEncoderStream : Stream
    {
        private Stream stream;

        private Encoder encoder;

        private int[] blockBuffer;
        private byte[] outputBuffer;

        private int checkSymbols;

        public RsEncoderStream(Stream stream, Encoder encoder) : base()
        {
            if (stream.CanWrite == false)
            {
                throw new ArgumentException("Must be a writable stream", "stream");
            }

            this.stream = stream;
            this.encoder = encoder;

            this.blockBuffer = new int[encoder.BlockSize];
            this.checkSymbols = encoder.BlockSize - encoder.MessageSize;

            this.outputBuffer = new byte[encoder.BlockSize];
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanTimeout
        {
            get { return stream.CanTimeout; }
        }

        public override void Close()
        {
            base.Close();
            stream.Close();
        }

        public override long Position
        {
            get { throw new InvalidOperationException(); }
            set { throw new InvalidOperationException(); }
        }

        public override long Length
        {
            get { throw new InvalidOperationException(); }
        }

        public override void SetLength(long value)
        {
            throw new InvalidOperationException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new InvalidOperationException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count != this.encoder.MessageSize)
            {
                throw new InvalidOperationException("RsEncoderStream only accepts writes that are exactly the size " +
                                                    "of a single reed-solmon block. Use BlockStreamWriteAdapter to buffer writes of differing sizes.");
            }

            Array.Clear(this.blockBuffer, 0, this.checkSymbols);

            Array.Copy(buffer, offset, this.blockBuffer, this.checkSymbols, count);

            this.encoder.Encode(this.blockBuffer);

            for (int i = 0; i < this.blockBuffer.Length; i++)
            {
                this.outputBuffer[i] = (byte)this.blockBuffer[i];
            }

            this.stream.Write(this.outputBuffer, 0, this.outputBuffer.Length);
        }

        public override void WriteByte(byte value)
        {
            throw new InvalidOperationException();
        }

        public override void Flush()
        {
            this.stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException();
        }
    }
}