using System.Collections.Concurrent;
using System.Text;

namespace SharpZipLib.Core
{
    internal class StringBuilderPool
    {
        public static StringBuilderPool Instance { get; } = new StringBuilderPool();
        private readonly ConcurrentQueue<StringBuilder> pool = new();

        public StringBuilder Rent()
        {
            return pool.TryDequeue(out var builder) ? builder : new StringBuilder();
        }

        public void Return(StringBuilder builder)
        {
            builder.Clear();
            pool.Enqueue(builder);
        }
    }
}
