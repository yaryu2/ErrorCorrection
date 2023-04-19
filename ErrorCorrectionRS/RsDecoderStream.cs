namespace ErrorCorrectionRS;

public class RsDecoderStream : Stream
{
    private Stream _stream;
    private Decoder _decoder;
    private int[] _blockBuffer;
    private byte[] _inputBuffer;
    private int _checkSymbols;

    public RsDecoderStream(Stream stream, Decoder decoder)
    {
        this._stream = stream;
        this._decoder = decoder;

        _inputBuffer = new byte[decoder.BlockSize];
        _blockBuffer = new int[decoder.BlockSize];

        _checkSymbols = decoder.BlockSize - decoder.MessageSize;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (count != _decoder.MessageSize)
        {
            throw new InvalidOperationException();
        }

        int bytesRead = _stream.Read(_inputBuffer, 0, _decoder.BlockSize);

        if (bytesRead == 0)
        {
            return 0;
        }
        else if (bytesRead != _decoder.BlockSize)
        {
            throw new IOException("Didn't read a whole block");
        }

        Array.Copy(_inputBuffer, _blockBuffer, _decoder.BlockSize);

        _decoder.Decode(_blockBuffer);

        for (int i = 0; i < _decoder.MessageSize; i++)
        {
            buffer[offset + i] = (byte)_blockBuffer[_checkSymbols + i];
        }

        return count;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    public override bool CanRead => true;

    public override bool CanWrite => false;

    public override bool CanSeek => false;

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override long Length => throw new NotSupportedException();

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void Flush()
    {
        _stream.Flush();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }
}