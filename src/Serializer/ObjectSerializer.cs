using System.Diagnostics;

namespace OpeNetLib.Serializer
{
    /// <summary>
    /// Serializes and deserializes objects to and from bytes.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    public class ObjectSerializer<T>
    {
        readonly Serialize<T> serializer;
        readonly Deserialize<T> deserializer;

        readonly int byteWidth;
        readonly bool isConstantByteWidth;

        /// <summary>
        /// Creates a new <see cref="ObjectSerializer{T}"/> using the specified serialize and deserialize methods.
        /// </summary>
        /// <param name="serialize">The method to use to serialize objects.</param>
        /// <param name="deserialize">The method to use to deserialize objects.</param>
        /// <param name="constantByteWidth">The constant byte width of serialized objects, if any.</param>
        public ObjectSerializer(Serialize<T> serialize, Deserialize<T> deserialize, int? constantByteWidth = null)
        {
            serializer = serialize;
            deserializer = deserialize;
            isConstantByteWidth = false;

            if (constantByteWidth is not null)
            {
                isConstantByteWidth = true;
                byteWidth = (int)constantByteWidth;
            }
        }

        /// <summary>
        /// Attempts to get the byte width of objects using this serializer.
        /// </summary>
        /// <param name="width">The constant byte width of serialized objects, if any.</param>
        /// <returns>Whether this serializer uses a constant byte-width.</returns>
        public bool TryGetByteWidth(out int width)
        {
            width = byteWidth;
            return isConstantByteWidth;
        }

        /// <summary>
        /// Deserializes a <typeparamref name="T"/> from a <see cref="byte"/>[].
        /// </summary>
        /// <param name="bytes">A <see cref="byte"/>[] to deserialize.</param>
        /// <returns>A new <typeparamref name="T"/>, deserialized from <paramref name="bytes"/>.</returns>
        public T Deserialize(byte[] bytes) => deserializer(bytes.AsSpan());

        /// <summary>
        /// Deserializes a <typeparamref name="T"/> from a span of bytes.
        /// </summary>
        /// <param name="bytes">A <see cref="byte"/> span to deserialize.</param>
        /// <returns>A new <typeparamref name="T"/>, deserialized from <paramref name="bytes"/>.</returns>
        public T Deserialize(Span<byte> bytes) => deserializer(bytes);

        /// <summary>
        /// Serializes a <typeparamref name="T"/> into a span of bytes.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>An array of bytes that represent the object.</returns>
        public byte[] Serialize(T obj)
        {
            byte[] ret = serializer(obj);

            if (ret.Length != byteWidth)
            {
                Debug.WriteLine($"{nameof(Serialize)} returned a byte[] with an unexpected length! (expected {byteWidth}, got {ret.Length})");
            }

            return ret;
        }
    }
}
