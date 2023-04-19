namespace ErrorCorrectionRS;

public class RsEncoderStream : Stream
{
    private Stream stream;
    private Encoder encoder;
    private int[] blockBuffer;
    private byte[] outputBuffer;
    private int checkSymbols;

    public RsEncoderStream(Stream stream, Encoder encoder)
    {
        if (stream.CanWrite == false)
        {
            throw new ArgumentException("Must be a writable stream", "stream");
        }

        this.stream = stream;
        this.encoder = encoder;

        blockBuffer = new int[encoder.BlockSize];
        checkSymbols = encoder.BlockSize - encoder.MessageSize;

        outputBuffer = new byte[encoder.BlockSize];
    }

    public override bool CanWrite => true;

    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanTimeout => stream.CanTimeout;

    public override void Close()
    {
        base.Close();
        stream.Close();
    }

    public override long Position
    {
        get => throw new InvalidOperationException();
        set => throw new InvalidOperationException();
    }

    public override long Length => throw new InvalidOperationException();

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
        if (count != encoder.MessageSize)
        {
            throw new InvalidOperationException("RsEncoderStream only accepts writes that are exactly the size " +
                                                "of a single reed-solmon block. Use BlockStreamWriteAdapter to buffer writes of differing sizes.");
        }

        Array.Clear(blockBuffer, 0, checkSymbols);

        Array.Copy(buffer, offset, blockBuffer, checkSymbols, count);

        encoder.Encode(blockBuffer);

        for (int i = 0; i < this.blockBuffer.Length; i++)
        {
            outputBuffer[i] = (byte)blockBuffer[i];
        }

        stream.Write(outputBuffer, 0, outputBuffer.Length);
    }

    public override void WriteByte(byte value)
    {
        throw new InvalidOperationException();
    }

    public override void Flush()
    {
        stream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new InvalidOperationException();
    }
}