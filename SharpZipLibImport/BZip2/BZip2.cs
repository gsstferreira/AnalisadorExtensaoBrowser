using SharpZipLib.Core;

namespace SharpZipLib.BZip2
{
    /// <summary>
    /// An example class to demonstrate compression and decompression of BZip2 streams.
    /// </summary>
    public static class BZip2
    {
        /// <summary>
        /// Decompress the <paramref name="inStream">input</paramref> writing
        /// uncompressed data to the <paramref name="outStream">output stream</paramref>
        /// </summary>
        /// <param name="inStream">The readable stream containing data to decompress.</param>
        /// <param name="outStream">The output stream to receive the decompressed data.</param>
        /// <param name="isStreamOwner">Both streams are closed on completion if true.</param>
        public static void Decompress(Stream inStream, Stream outStream, bool isStreamOwner)
        {
            ArgumentNullException.ThrowIfNull(inStream);
            ArgumentNullException.ThrowIfNull(outStream);
            try
            {
                using BZip2InputStream bzipInput = new(inStream);
                bzipInput.IsStreamOwner = isStreamOwner;
                StreamUtils.Copy(bzipInput, outStream, new byte[4096]);
            }
            finally
            {
                if (isStreamOwner)
                {
                    // inStream is closed by the BZip2InputStream if stream owner
                    outStream.Dispose();
                }
            }
        }

        /// <summary>
        /// Compress the <paramref name="inStream">input stream</paramref> sending
        /// result data to <paramref name="outStream">output stream</paramref>
        /// </summary>
        /// <param name="inStream">The readable stream to compress.</param>
        /// <param name="outStream">The output stream to receive the compressed data.</param>
        /// <param name="isStreamOwner">Both streams are closed on completion if true.</param>
        /// <param name="level">Block size acts as compression level (1 to 9) with 1 giving
        /// the lowest compression and 9 the highest.</param>
        public static void Compress(Stream inStream, Stream outStream, bool isStreamOwner, int level)
        {
            ArgumentNullException.ThrowIfNull(inStream);
            ArgumentNullException.ThrowIfNull(outStream);
            try
            {
                using BZip2OutputStream bzipOutput = new(outStream, level);
                bzipOutput.IsStreamOwner = isStreamOwner;
                StreamUtils.Copy(inStream, bzipOutput, new byte[4096]);
            }
            finally
            {
                if (isStreamOwner)
                {
                    // outStream is closed by the BZip2OutputStream if stream owner
                    inStream.Dispose();
                }
            }
        }
    }
}
