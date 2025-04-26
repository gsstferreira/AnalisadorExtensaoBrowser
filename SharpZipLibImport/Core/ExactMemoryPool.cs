using System.Buffers;

namespace SharpZipLib.Core
{
    /// <summary>
    /// A MemoryPool that will return a Memory which is exactly the length asked for using the bufferSize parameter.
    /// This is in contrast to the default ArrayMemoryPool which will return a Memory of equal size to the underlying
    /// array which at least as long as the minBufferSize parameter.
    /// Note: The underlying array may be larger than the slice of Memory
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class ExactMemoryPool<T> : MemoryPool<T>
    {
        public new static readonly MemoryPool<T> Shared = new ExactMemoryPool<T>();

        public override IMemoryOwner<T> Rent(int bufferSize = -1)
        {
            return (uint)bufferSize > int.MaxValue || bufferSize < 0
                ? throw new ArgumentOutOfRangeException(nameof(bufferSize))
                : (IMemoryOwner<T>)new ExactMemoryPoolBuffer(bufferSize);
        }

        protected override void Dispose(bool disposing)
        {
        }

        public override int MaxBufferSize => int.MaxValue;

        private sealed class ExactMemoryPoolBuffer(int size) : IMemoryOwner<T>, IDisposable
        {
            private T[]? array = ArrayPool<T>.Shared.Rent(size);
            private readonly int size = size;

            public Memory<T> Memory
            {
                get
                {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                    T[] array = this.array;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                    return array == null ? throw new ObjectDisposedException(nameof(ExactMemoryPoolBuffer)) : new Memory<T>(array)[..size];
                }
            }

            public void Dispose()
            {
                T[]? array = this.array;
                if (array is null)
                {
                    return;
                }

                this.array = null;
                ArrayPool<T>.Shared.Return(array);
            }
        }
    }
}
