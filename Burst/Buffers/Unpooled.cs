namespace Burst.Buffers
{
    public static class Unpooled
    {
        // Create an unpooled buffer with the specified initial capacity
        public static IByteBuffer Buffer(int initialCapacity = 1024)
        {
            return new ByteBuffer(initialCapacity);
        }

        // Create an unpooled buffer that wraps an existing byte array (no copying)
        public static IByteBuffer WrappedBuffer(byte[] array)
        {
            var buffer = new ByteBuffer(array.Length);
            buffer.WriteBytes(array);
            return buffer;
        }

        // Create an unpooled buffer that wraps multiple byte arrays as a composite buffer
        public static IByteBuffer CompositeBuffer(params byte[][] arrays)
        {
            int totalLength = 0;
            foreach (var array in arrays)
            {
                totalLength += array.Length;
            }

            var compositeBuffer = new ByteBuffer(totalLength);
            foreach (var array in arrays)
            {
                compositeBuffer.WriteBytes(array);
            }

            return compositeBuffer;
        }

        // Create an empty buffer (capacity 0)
        public static IByteBuffer EmptyBuffer()
        {
            return new ByteBuffer(0);
        }

        // Create an unpooled buffer with a single byte
        public static IByteBuffer CopiedBuffer(byte value)
        {
            var buffer = new ByteBuffer(1);
            buffer.WriteByte(value);
            return buffer;
        }

        // Create an unpooled buffer from a string (using the provided encoding)
        public static IByteBuffer CopiedBuffer(string value, System.Text.Encoding encoding)
        {
            byte[] bytes = encoding.GetBytes(value);
            var buffer = new ByteBuffer(bytes.Length);
            buffer.WriteBytes(bytes);
            return buffer;
        }

        // Create an unpooled buffer from a span of bytes
        public static IByteBuffer CopiedBuffer(ReadOnlySpan<byte> span)
        {
            var buffer = new ByteBuffer(span.Length);
            buffer.WriteBytes(span);
            return buffer;
        }
    }
}
