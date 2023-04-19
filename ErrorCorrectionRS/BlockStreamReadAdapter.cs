namespace ErrorCorrectionRS;

public class BlockStreamReadAdapter : Stream
{
    private Stream blockSource;

    private byte[] leftovers;

    private int leftoverSize;

    public BlockStreamReadAdapter(Stream blockSource, int blockSize)
    {
        this.blockSource = blockSource;
        BlockSize = blockSize;

        leftovers = new byte[blockSize];
        leftoverSize = 0;
    }

    public override bool CanRead => true;

    public override bool CanWrite => false;

    public override bool CanSeek => false;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public int BlockSize { get; private set; }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int written = 0;

        if (leftoverSize > 0)
        {
            int writeSize = Math.Min(leftoverSize, count);

            Array.Copy(leftovers, 0, buffer, offset, writeSize);

            count -= writeSize;
            offset += writeSize;
            written += writeSize;

            if (writeSize < leftoverSize)
            {
                for (int i = 0; i < leftoverSize - writeSize; i++)
                {
                    leftovers[i] = leftovers[i + writeSize];
                }

                leftoverSize -= writeSize;

                return written;
            }
            else
            {
                leftoverSize = 0;
            }
        }

        while (count >= BlockSize)
        {
            int bytesRead = blockSource.Read(buffer, offset, BlockSize);

            if (bytesRead == 0)
            {
                return written;
            }
            else
            {
                offset += BlockSize;
                count -= BlockSize;
                written += BlockSize;
            }
        }

        if (count >= BlockSize)
        {
            throw new Exception();
        }

        if (count > 0)
        {
            int amountRead = blockSource.Read(leftovers, 0, BlockSize);

            if (amountRead == 0)
            {
                return written;
            }
            
            Array.Copy(leftovers, 0, buffer, offset, count);

            int newLeftoverSize = BlockSize - count;
            
            for (int i = 0; i < newLeftoverSize; i++)
            {
                leftovers[i] = leftovers[i + count];
            }
            
            written += count;
            leftoverSize = newLeftoverSize;
        }

        return written;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void Flush()
    {
        blockSource.Flush();
    }
}