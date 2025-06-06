﻿using System.Runtime.CompilerServices;
using CT = System.Threading.CancellationToken;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming

namespace SharpZipLib.Core
{
    internal static class ByteOrderStreamExtensions
    {
        internal static byte[] SwappedBytes(ushort value) => [(byte)value, (byte)(value >> 8)];
        internal static byte[] SwappedBytes(short value) => [(byte)value, (byte)(value >> 8)];
        internal static byte[] SwappedBytes(uint value) => [(byte)value, (byte)(value >> 8), (byte)(value >> 16), (byte)(value >> 24)];
        internal static byte[] SwappedBytes(int value) => [(byte)value, (byte)(value >> 8), (byte)(value >> 16), (byte)(value >> 24)];

        internal static byte[] SwappedBytes(long value) => [
            (byte)value,         (byte)(value >>  8), (byte)(value >> 16), (byte)(value >> 24),
            (byte)(value >> 32), (byte)(value >> 40), (byte)(value >> 48), (byte)(value >> 56)
        ];

        internal static byte[] SwappedBytes(ulong value) => [
            (byte)value,         (byte)(value >>  8), (byte)(value >> 16), (byte)(value >> 24),
            (byte)(value >> 32), (byte)(value >> 40), (byte)(value >> 48), (byte)(value >> 56)
        ];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static long SwappedS64(byte[] bytes) =>
            (long)bytes[0] << 0 | (long)bytes[1] << 8 | (long)bytes[2] << 16 | (long)bytes[3] << 24 |
            (long)bytes[4] << 32 | (long)bytes[5] << 40 | (long)bytes[6] << 48 | (long)bytes[7] << 56;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ulong SwappedU64(byte[] bytes) =>
            (ulong)bytes[0] << 0 | (ulong)bytes[1] << 8 | (ulong)bytes[2] << 16 | (ulong)bytes[3] << 24 |
            (ulong)bytes[4] << 32 | (ulong)bytes[5] << 40 | (ulong)bytes[6] << 48 | (ulong)bytes[7] << 56;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int SwappedS32(byte[] bytes) => bytes[0] | bytes[1] << 8 | bytes[2] << 16 | bytes[3] << 24;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static uint SwappedU32(byte[] bytes) => (uint)SwappedS32(bytes);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static short SwappedS16(byte[] bytes) => (short)(bytes[0] | bytes[1] << 8);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ushort SwappedU16(byte[] bytes) => (ushort)SwappedS16(bytes);

        internal static byte[] ReadBytes(this Stream stream, int count)
        {
            var bytes = new byte[count];
            var remaining = count;
            while (remaining > 0)
            {
                var bytesRead = stream.Read(bytes, count - remaining, remaining);
                if (bytesRead < 1) throw new EndOfStreamException();
                remaining -= bytesRead;
            }

            return bytes;
        }

        /// <summary> Read an unsigned short in little endian byte order. </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadLEShort(this Stream stream) => SwappedS16(stream.ReadBytes(2));

        /// <summary> Read an int in little endian byte order. </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadLEInt(this Stream stream) => SwappedS32(stream.ReadBytes(4));

        /// <summary> Read a long in little endian byte order. </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReadLELong(this Stream stream) => SwappedS64(stream.ReadBytes(8));

        /// <summary> Write an unsigned short in little endian byte order. </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteLEShort(this Stream stream, int value) => stream.Write(SwappedBytes(value), 0, 2);

        /// <inheritdoc cref="WriteLEShort"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task WriteLEShortAsync(this Stream stream, int value, CT ct)
            => await stream.WriteAsync(SwappedBytes(value), 0, 2, ct).ConfigureAwait(false);

        /// <summary> Write a ushort in little endian byte order. </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteLEUshort(this Stream stream, ushort value) => stream.Write(SwappedBytes(value), 0, 2);

        /// <inheritdoc cref="WriteLEUshort"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task WriteLEUshortAsync(this Stream stream, ushort value, CT ct)
            => await stream.WriteAsync(SwappedBytes(value), 0, 2, ct).ConfigureAwait(false);

        /// <summary> Write an int in little endian byte order. </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteLEInt(this Stream stream, int value) => stream.Write(SwappedBytes(value), 0, 4);

        /// <inheritdoc cref="WriteLEInt"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task WriteLEIntAsync(this Stream stream, int value, CT ct)
            => await stream.WriteAsync(SwappedBytes(value), 0, 4, ct).ConfigureAwait(false);

        /// <summary> Write a uint in little endian byte order. </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteLEUint(this Stream stream, uint value) => stream.Write(SwappedBytes(value), 0, 4);

        /// <inheritdoc cref="WriteLEUint"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task WriteLEUintAsync(this Stream stream, uint value, CT ct)
            => await stream.WriteAsync(SwappedBytes(value), 0, 4, ct).ConfigureAwait(false);

        /// <summary> Write a long in little endian byte order. </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteLELong(this Stream stream, long value) => stream.Write(SwappedBytes(value), 0, 8);

        /// <inheritdoc cref="WriteLELong"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task WriteLELongAsync(this Stream stream, long value, CT ct)
            => await stream.WriteAsync(SwappedBytes(value), 0, 8, ct).ConfigureAwait(false);

        /// <summary> Write a ulong in little endian byte order. </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteLEUlong(this Stream stream, ulong value) => stream.Write(SwappedBytes(value), 0, 8);

        /// <inheritdoc cref="WriteLEUlong"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task WriteLEUlongAsync(this Stream stream, ulong value, CT ct)
            => await stream.WriteAsync(SwappedBytes(value), 0, 8, ct).ConfigureAwait(false);
    }
}
