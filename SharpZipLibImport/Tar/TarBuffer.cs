using System.Buffers;
using System.Runtime.CompilerServices;

namespace SharpZipLib.Tar
{
    /// <summary>
    /// The TarBuffer class implements the tar archive concept
    /// of a buffered input stream. This concept goes back to the
    /// days of blocked tape drives and special io devices. In the
    /// C# universe, the only real function that this class
    /// performs is to ensure that files have the correct "record"
    /// size, or other tars will complain.
    /// <p>
    /// You should never have a need to access this class directly.
    /// TarBuffers are created by Tar IO Streams.
    /// </p>
    /// </summary>
    public class TarBuffer
    {
        /* A quote from GNU tar man file on blocking and records
		   A `tar' archive file contains a series of blocks.  Each block
		contains `BLOCKSIZE' bytes.  Although this format may be thought of as
		being on magnetic tape, other media are often used.

		   Each file archived is represented by a header block which describes
		the file, followed by zero or more blocks which give the contents of
		the file.  At the end of the archive file there may be a block filled
		with binary zeros as an end-of-file marker.  A reasonable system should
		write a block of zeros at the end, but must not assume that such a
		block exists when reading an archive.

		   The blocks may be "blocked" for physical I/O operations.  Each
		record of N blocks is written with a single 'write ()'
		operation.  On magnetic tapes, the result of such a write is a single
		record.  When writing an archive, the last record of blocks should be
		written at the full size, with blocks after the zero block containing
		all zeros.  When reading an archive, a reasonable system should
		properly handle an archive whose last record is shorter than the rest,
		or which contains garbage records after a zero block.
		*/

        #region Constants

        /// <summary>
        /// The size of a block in a tar archive in bytes.
        /// </summary>
        /// <remarks>This is 512 bytes.</remarks>
        public const int BlockSize = 512;

        /// <summary>
        /// The number of blocks in a default record.
        /// </summary>
        /// <remarks>
        /// The default value is 20 blocks per record.
        /// </remarks>
        public const int DefaultBlockFactor = 20;

        /// <summary>
        /// The size in bytes of a default record.
        /// </summary>
        /// <remarks>
        /// The default size is 10KB.
        /// </remarks>
        public const int DefaultRecordSize = BlockSize * DefaultBlockFactor;

        #endregion Constants

        /// <summary>
        /// Construct TarBuffer for reading inputStream setting BlockFactor
        /// </summary>
        /// <param name="inputStream">Stream to buffer</param>
        /// <param name="blockFactor">Blocking factor to apply</param>
        /// <returns>A new <see cref="Tar.TarBuffer"/> suitable for input.</returns>
        public TarBuffer(Stream inputStream, int blockFactor)
        {
            var recSize = blockFactor * BlockSize;

            InputStream = inputStream;
            CurrentBlockFactor = blockFactor;
            RecordSize = recSize;
            RecordBuffer = ArrayPool<byte>.Shared.Rent(recSize);

            CurrentRecordIndex = -1;
            CurrentBlockIndex = CurrentBlockFactor;
        }

        public void Reset()
        {
            if (InputStream is not null)
            {
                CurrentRecordIndex = -1;
                CurrentBlockIndex = CurrentBlockFactor;
            }
            else
            {
                CurrentRecordIndex = 0;
                CurrentBlockIndex = 0;
            }
        }

        public void ReturnArrays()
        {
            ArrayPool<byte>.Shared.Return(RecordBuffer);
        }


        /// <summary>
        /// Determine if an archive block indicates the End of an Archive has been reached.
        /// End of archive is indicated by a block that consists entirely of null bytes.
        /// All remaining blocks for the record should also be null's
        /// However some older tars only do a couple of null blocks (Old GNU tar for one)
        /// and also partial records
        /// </summary>
        /// <param name = "block">The data block to check.</param>
        /// <returns>Returns true if the block is an EOF block; false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEndOfArchiveBlock(byte[] block)
        {
            return !block.Any(x => x != 0);
        }

        internal void ReadBlock(byte[] buffer)
        {
            if (CurrentBlockIndex >= CurrentBlockFactor)
            {
                ReadRecord();
            }

            RecordBuffer.AsSpan().Slice(CurrentBlockIndex * BlockSize, BlockSize).CopyTo(buffer);
            CurrentBlockIndex++;
        }

        internal void SkipBlock()
        {
            if (CurrentBlockIndex >= CurrentBlockFactor)
            {
                ReadRecord();
            }
            CurrentBlockIndex++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ReadRecord()
        {
            CurrentBlockIndex = 0;

            int offset = 0;
            int bytesNeeded = RecordSize;
            do
            {
                int bytesRead = InputStream.Read(RecordBuffer, offset, bytesNeeded);

                if (bytesRead <= 0)
                {
                    for (; bytesRead < RecordSize; bytesRead++)
                    {
                        RecordBuffer[bytesRead] = 0;
                    }
                    break;
                }
                offset += bytesRead;
                bytesNeeded -= bytesRead;
            } while (bytesNeeded > 0);

            CurrentRecordIndex++;
        }

        /// <summary>
        /// Get the current block number, within the current record, zero based.
        /// </summary>
        /// <remarks>Block numbers are zero based values</remarks>
        /// <seealso cref="RecordSize"/>
        public int CurrentBlock
        {
            get { return CurrentBlockIndex; }
        }

        /// <summary>
        /// Gets or sets a flag indicating ownership of underlying stream.
        /// When the flag is true <see cref="Close" /> will close the underlying stream also.
        /// </summary>
        /// <remarks>The default value is true.</remarks>
        public bool IsStreamOwner { get; set; } = true;

        /// <summary>
        /// Get the current record number.
        /// </summary>
        /// <returns>
        /// The current zero based record number.
        /// </returns>
        public int CurrentRecord
        {
            get { return CurrentRecordIndex; }
        }

        /// <summary>
        /// Close the TarBuffer. If this is an output buffer, also flush the
        /// current block before closing.
        /// </summary>
        public void Close() 
        {
            if (InputStream is not null)
            {
                if (IsStreamOwner)
                {
                    InputStream.Dispose();
                }
            }
        }

        private Stream InputStream;

        private byte[] RecordBuffer;
        private int CurrentBlockIndex;
        private int CurrentRecordIndex;

        public int RecordSize = DefaultRecordSize;
        private int CurrentBlockFactor = DefaultBlockFactor;
    }
}
