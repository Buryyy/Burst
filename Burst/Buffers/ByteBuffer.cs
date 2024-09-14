using System.Buffers;
using System.Runtime.InteropServices;

namespace Burst.Buffers
{
    public sealed class ByteBuffer : IByteBuffer
    {
        private readonly IMemoryOwner<byte> _memoryOwner;
        private readonly Memory<byte> _memory;
        private int _readerIndex;
        private int _writerIndex;
        private int _markedReaderIndex;
        private int _markedWriterIndex;
        private int _refCount;
        private bool _disposed;

        public int Capacity => _memory.Length;
        public int ReaderIndex { get => _readerIndex; set => _readerIndex = value; }
        public int WriterIndex { get => _writerIndex; set => _writerIndex = value; }
        public int ReadableBytes => _writerIndex - _readerIndex;
        public int WritableBytes => _memory.Length - _writerIndex;
        public bool IsReadable => ReadableBytes > 0;
        public bool IsWritable => WritableBytes > 0;

        // Constructor: initialize buffer with an initial capacity
        public ByteBuffer(int initialCapacity = 4096)
        {
            _memoryOwner = MemoryPool<byte>.Shared.Rent(initialCapacity);
            _memory = _memoryOwner.Memory;
            _readerIndex = 0;
            _writerIndex = 0;
            _refCount = 1; // Initial reference count
        }

        // Create a ByteBuffer that wraps an existing array (no copying)
        public ByteBuffer WrappedBuffer(byte[] array)
        {
            var buffer = new ByteBuffer(array.Length);
            array.CopyTo(buffer._memory.Span);
            buffer._writerIndex = array.Length;
            return buffer;
        }

        public void ReadBytes(Span<byte> dst)
        {
            EnsureReadable(dst.Length);
            _memory.Span.Slice(_readerIndex, dst.Length).CopyTo(dst);
            _readerIndex += dst.Length;
        }

        // Write a byte array to the buffer
        public void WriteBytes(ReadOnlySpan<byte> src)
        {
            EnsureWritable(src.Length);
            src.CopyTo(_memory.Span.Slice(_writerIndex));
            _writerIndex += src.Length;
        }

        public void WriteByte(byte value)
        {
            EnsureWritable(sizeof(byte));
            _memory.Span[_writerIndex++] = value;
        }

        public void WriteInt(int value)
        {
            EnsureWritable(sizeof(int));
            MemoryMarshal.Write(_memory.Span.Slice(_writerIndex), in value);
            _writerIndex += sizeof(int);
        }

        public void WriteShort(short value)
        {
            EnsureWritable(sizeof(short));
            MemoryMarshal.Write(_memory.Span.Slice(_writerIndex), in value);
            _writerIndex += sizeof(short);
        }

        public void WriteLong(long value)
        {
            EnsureWritable(sizeof(long));
            MemoryMarshal.Write(_memory.Span.Slice(_writerIndex), in value);
            _writerIndex += sizeof(long);
        }

        public byte ReadByte()
        {
            EnsureReadable(sizeof(byte));
            return _memory.Span[_readerIndex++];
        }

        public short ReadShort()
        {
            EnsureReadable(sizeof(short));
            var value = MemoryMarshal.Read<short>(_memory.Span.Slice(_readerIndex));
            _readerIndex += sizeof(short);
            return value;
        }

        public int ReadInt()
        {
            EnsureReadable(sizeof(int));
            var value = MemoryMarshal.Read<int>(_memory.Span.Slice(_readerIndex));
            _readerIndex += sizeof(int);
            return value;
        }

        public long ReadLong()
        {
            EnsureReadable(sizeof(long));
            var value = MemoryMarshal.Read<long>(_memory.Span.Slice(_readerIndex));
            _readerIndex += sizeof(long);
            return value;
        }

        // Mark the current reader index
        public void MarkReaderIndex()
        {
            _markedReaderIndex = _readerIndex;
        }

        // Reset the reader index to the marked index
        public void ResetReaderIndex()
        {
            _readerIndex = _markedReaderIndex;
        }

        // Mark the current writer index
        public void MarkWriterIndex()
        {
            _markedWriterIndex = _writerIndex;
        }

        // Reset the writer index to the marked index
        public void ResetWriterIndex()
        {
            _writerIndex = _markedWriterIndex;
        }

        // Retain (increase the reference count)
        public void Retain()
        {
            _refCount++;
        }

        // Release (decrease the reference count)
        public void Release()
        {
            if (--_refCount == 0)
            {
                Dispose();
            }
        }

        // Slice the buffer
        public IByteBuffer Slice(int index, int length)
        {
            if (index < 0 || length <= 0 || index + length > _writerIndex)
                throw new ArgumentOutOfRangeException();

            var slice = new ByteBuffer(length);
            _memory.Span.Slice(index, length).CopyTo(slice._memory.Span);
            slice.WriterIndex = length;

            return slice;
        }

        // Duplicate the buffer
        public IByteBuffer Duplicate()
        {
            var duplicate = new ByteBuffer(Capacity);
            _memory.Span.Slice(0, _writerIndex).CopyTo(duplicate._memory.Span);
            duplicate.WriterIndex = _writerIndex;
            duplicate.ReaderIndex = _readerIndex;

            return duplicate;
        }

        // Clear the buffer
        public void Clear()
        {
            _readerIndex = 0;
            _writerIndex = 0;
        }

        // Get the underlying buffer as an array
        public byte[] GetArray()
        {
            return _memory.ToArray();
        }

        // Ensure there is enough space for writing
        private void EnsureWritable(int length)
        {
            if (WritableBytes < length)
            {
                throw new InvalidOperationException("Not enough writable bytes.");
            }
        }

        // Ensure there is enough data to read
        private void EnsureReadable(int length)
        {
            if (ReadableBytes < length)
            {
                throw new InvalidOperationException("Not enough readable bytes.");
            }
        }

        // Dispose the buffer (return to pool)
        public void Dispose()
        {
            if (!_disposed)
            {
                _memoryOwner.Dispose(); // Dispose the memory owner
                _disposed = true;
            }
        }
    }
}
