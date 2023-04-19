using System.Diagnostics;

namespace ErrorCorrectionRS;

public class BlockStreamWriteAdapter : Stream
{
    private Stream blockSink;
    private byte[] leftovers;
    private int leftoversSize;

    public BlockStreamWriteAdapter(Stream blockSink, int blockSize)
    {
        if (blockSink.CanWrite == false)
        {
            throw new ArgumentException("The provided stream does not support writing", "blockSink");
        }

        this.blockSink = blockSink;
        this.BlockSize = blockSize;

        this.leftovers = new byte[blockSize];
        this.leftoversSize = 0;
    }

    public int BlockSize { get; private set; }

    public override bool CanRead => false;

    public override bool CanWrite => true;

    public override bool CanSeek => false;

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override long Length => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count)
    {
        if (leftoversSize + count >= BlockSize)
        {
            if (leftoversSize > 0)
            {
                int copyLength = BlockSize - leftoversSize;

                Array.Copy(buffer, offset, leftovers, leftoversSize, copyLength);

                blockSink.Write(leftovers, 0, BlockSize);

                count -= copyLength;
                offset += copyLength;
                leftoversSize = 0;
            }

            while (count >= BlockSize)
            {
                blockSink.Write(buffer, offset, BlockSize);

                count -= BlockSize;
                offset += BlockSize;
            }
        }

        if (count > 0)
        {
            Debug.Assert(
                count + this.leftoversSize < this.BlockSize,
                "Attempted to save leftovers equal or larger than a block",
                "The point of leftovers is to store bytes that couldn't be written because they didn't fill a full block. "
                + "However, we found that we have enough leftovers to fill a full block which means we should've written one more "
                + "block to the output, but didn't. The math is broken somewhere"
            );

            Array.Copy(buffer, offset, this.leftovers, this.leftoversSize, count);

            leftoversSize += count;
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
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
        if (leftoversSize > 0)
        {
            Array.Clear(leftovers, leftoversSize, BlockSize - leftoversSize);
            blockSink.Write(leftovers, 0, BlockSize);
            blockSink.Flush();
        }
    }
}