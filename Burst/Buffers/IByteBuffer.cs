
namespace Burst.Buffers
{
    public interface IByteBuffer
    {
        int Capacity { get; }
        bool IsReadable { get; }
        bool IsWritable { get; }
        int ReadableBytes { get; }
        int ReaderIndex { get; set; }
        int WritableBytes { get; }
        int WriterIndex { get; set; }

        void Clear();
        void Dispose();
        IByteBuffer Duplicate();
        byte[] GetArray();
        byte ReadByte();
        int ReadInt();
        long ReadLong();
        short ReadShort();
        void Release();
        void Retain();
        IByteBuffer Slice(int index, int length);
        ByteBuffer WrappedBuffer(byte[] array);
        void WriteByte(byte value);
        void WriteBytes(ReadOnlySpan<byte> src);
        void ReadBytes(Span<byte> dst);
        void WriteInt(int value);
        void WriteLong(long value);
        void WriteShort(short value);
    }
}
