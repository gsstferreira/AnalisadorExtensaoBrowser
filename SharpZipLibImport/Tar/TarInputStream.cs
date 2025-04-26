using SharpZipLib.Core;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

namespace SharpZipLib.Tar
{
    /// <summary>
    /// The TarInputStream reads a UNIX tar archive as an InputStream.
    /// methods are provided to position at each successive entry in
    /// the archive, and the read each entry as a normal input stream
    /// using read().
    /// </summary>
    public class TarInputStream : Stream
    {
        #region Constructors
        /// <summary>
        /// Construct a TarInputStream with default block factor
        /// </summary>
        /// <param name="inputStream">stream to source data from</param>
        /// <param name="nameEncoding">The <see cref="Encoding"/> used for the Name fields, or null for ASCII only</param>
        public TarInputStream(Stream inputStream, Encoding nameEncoding)
        {
            hasHitEOF = false;
            this.inputStream = inputStream;
            tarBuffer = new TarBuffer(inputStream, TarBuffer.DefaultBlockFactor);
            encoding = nameEncoding;

            headerBuffer = ArrayPool<byte>.Shared.Rent(TarBuffer.BlockSize);
            nameBuffer = ArrayPool<byte>.Shared.Rent(TarBuffer.BlockSize);
            recBuffer = ArrayPool<byte>.Shared.Rent(TarBuffer.BlockSize);
        }

        /// <summary>
        /// Construct a TarInputStream with user specified block factor
        /// </summary>
        /// <param name="inputStream">stream to source data from</param>
        /// <param name="blockFactor">block factor to apply to archive</param>
        /// <param name="nameEncoding">The <see cref="Encoding"/> used for the Name fields, or null for ASCII only</param>
        public TarInputStream(Stream inputStream, int blockFactor, Encoding nameEncoding)
        {
            hasHitEOF = false;
            this.inputStream = inputStream;
            tarBuffer = new TarBuffer(inputStream, blockFactor);
            encoding = nameEncoding;

            headerBuffer = ArrayPool<byte>.Shared.Rent(TarBuffer.BlockSize);
            nameBuffer = ArrayPool<byte>.Shared.Rent(TarBuffer.BlockSize);
            recBuffer = ArrayPool<byte>.Shared.Rent(TarBuffer.BlockSize);
        }

        #endregion Constructors

        /// <summary>
        /// Gets or sets a flag indicating ownership of underlying stream.
        /// When the flag is true <see cref="Stream.Dispose()" /> will close the underlying stream also.
        /// </summary>
        /// <remarks>The default value is true.</remarks>
        public bool IsStreamOwner
        {
            get { return tarBuffer.IsStreamOwner; }
            set { tarBuffer.IsStreamOwner = value; }
        }

        #region Stream Overrides

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading
        /// </summary>
        public override bool CanRead
        {
            get { return inputStream.CanRead; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking
        /// This property always returns false.
        /// </summary>
        public override bool CanSeek
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating if the stream supports writing.
        /// This property always returns false.
        /// </summary>
        public override bool CanWrite
        {
            get { return false; }
        }

        /// <summary>
        /// The length in bytes of the stream
        /// </summary>
        public override long Length
        {
            get { return inputStream.Length; }
        }

        /// <summary>
        /// Gets or sets the position within the stream.
        /// Setting the Position is not supported and throws a NotSupportedExceptionNotSupportedException
        /// </summary>
        /// <exception cref="NotSupportedException">Any attempt to set position</exception>
        public override long Position
        {
            get { return inputStream.Position; }
            set { throw new NotSupportedException("TarInputStream Seek not supported"); }
        }

        /// <summary>
        /// Flushes the baseInputStream
        /// </summary>
        public override void Flush()
        {
            inputStream.Flush();
        }

        /// <summary>
        /// Reads a byte from the current tar archive entry.
        /// </summary>
        /// <returns>A byte cast to an int; -1 if the at the end of the stream.</returns>
        public override int ReadByte()
        {
            var oneByteBuffer = ArrayPool<byte>.Shared.Rent(1);
            var num = Read(oneByteBuffer, 0, 1);
            if (num <= 0)
            {
                // return -1 to indicate that no byte was read.
                return -1;
            }

            var result = oneByteBuffer[0];
            ArrayPool<byte>.Shared.Return(oneByteBuffer);
            return result;
        }

        /// <summary>
        /// Reads bytes from the current tar archive entry.
        ///
        /// This method is aware of the boundaries of the current
        /// entry in the archive and will deal with them appropriately
        /// </summary>
        /// <param name="buffer">
        /// The buffer into which to place bytes read.
        /// </param>
        /// <param name="offset">
        /// The offset at which to place bytes read.
        /// </param>
        /// <param name="count">
        /// The number of bytes to read.
        /// </param>
        /// <returns>
        /// The number of bytes read, or 0 at end of stream/EOF.
        /// </returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return Read(buffer.AsMemory().Slice(offset, count));
        }

        private int Read(Memory<byte> buffer)
        {
            int offset = 0;
            int totalRead = 0;

            if (entryOffset >= entrySize)
            {
                return 0;
            }

            long numToRead = buffer.Length;

            if (numToRead + entryOffset > entrySize)
            {
                numToRead = entrySize - entryOffset;
            }

            if (readBuffer != null)
            {
                int sz = numToRead > readBuffer.Memory.Length ? readBuffer.Memory.Length : (int)numToRead;

                readBuffer.Memory[..sz].CopyTo(buffer.Slice(offset, sz));

                if (sz >= readBuffer.Memory.Length)
                {
                    readBuffer.Dispose();
                    readBuffer = null;
                }
                else
                {
                    int newLen = readBuffer.Memory.Length - sz;
                    var newBuf = ExactMemoryPool<byte>.Shared.Rent(newLen);
                    readBuffer.Memory.Slice(sz, newLen).CopyTo(newBuf.Memory);
                    readBuffer.Dispose();

                    readBuffer = newBuf;
                }

                totalRead += sz;
                numToRead -= sz;
                offset += sz;
            }

            while (numToRead > 0)
            {
                tarBuffer.ReadBlock(recBuffer);

                var sz = (int)numToRead;

                if (TarBuffer.BlockSize > sz)
                {
                    var recBuf = recBuffer.AsSpan();

                    recBuf[..sz].CopyTo(buffer.Slice(offset, sz).Span);
                    readBuffer?.Dispose();

                    readBuffer = ExactMemoryPool<byte>.Shared.Rent(TarBuffer.BlockSize - sz);
                    recBuf[sz..TarBuffer.BlockSize].CopyTo(readBuffer.Memory.Span);
                }
                else
                {
                    sz = TarBuffer.BlockSize;
                    recBuffer.AsSpan().CopyTo(buffer.Slice(offset, TarBuffer.BlockSize).Span);
                }

                totalRead += sz;
                numToRead -= sz;
                offset += sz;
            }

            entryOffset += totalRead;

            return totalRead;
        }

        public string GetContentAsString()
        {
            int totalRead = 0;
            long numToRead = entrySize;

            if (entryOffset > 0)
            {
                throw new InvalidOperationException("Stream position is not at the entry start.");
            }
            else
            {
                contentStringBuilder.Clear();
                if (readBuffer != null)
                {
                    int sz = numToRead > readBuffer.Memory.Length ? readBuffer.Memory.Length : (int)numToRead;

                    contentStringBuilder.Append(Encoding.Default.GetString(readBuffer.Memory[..sz].Span));

                    if (sz >= readBuffer.Memory.Length)
                    {
                        readBuffer.Dispose();
                        readBuffer = null;
                    }
                    else
                    {
                        int newLen = readBuffer.Memory.Length - sz;
                        var newBuf = ExactMemoryPool<byte>.Shared.Rent(newLen);
                        readBuffer.Memory.Slice(sz, newLen).CopyTo(newBuf.Memory);
                        readBuffer.Dispose();

                        readBuffer = newBuf;
                    }

                    totalRead += sz;
                    numToRead -= sz;
                }

                while (numToRead > 0)
                {
                    tarBuffer.ReadBlock(recBuffer);

                    var sz = (int)numToRead;

                    if (TarBuffer.BlockSize > sz)
                    {
                        var recBuf = recBuffer.AsSpan();

                        contentStringBuilder.Append(Encoding.Default.GetString(recBuf[..sz]));
                        readBuffer?.Dispose();

                        readBuffer = ExactMemoryPool<byte>.Shared.Rent(TarBuffer.BlockSize - sz);
                        recBuf[sz..TarBuffer.BlockSize].CopyTo(readBuffer.Memory.Span);
                    }
                    else
                    {
                        sz = TarBuffer.BlockSize;
                        contentStringBuilder.Append(Encoding.Default.GetString(recBuffer.AsSpan()));
                    }

                    totalRead += sz;
                    numToRead -= sz;
                }

                entryOffset += totalRead;

                return contentStringBuilder.ToString();
            }
        }

        #endregion Stream Overrides

        /// <summary>
        /// Get the record size being used by this stream's TarBuffer.
        /// </summary>
        public int RecordSize
        {
            get { return tarBuffer.RecordSize; }
        }

        public void Reset()
        {
            entryOffset = 0;
            tarBuffer.Reset();
        }

        /// <summary>
        /// Get the next entry in this tar archive. This will skip
        /// over any remaining data in the current entry, if there
        /// is one, and place the input stream at the header of the
        /// next entry, and read the header and instantiate a new
        /// TarEntry from the header bytes and return that entry.
        /// If there are no more entries in the archive, null will
        /// be returned to indicate that the end of the archive has
        /// been reached.
        /// </summary>
        /// <returns>
        /// The next TarEntry in the archive, or null.
        /// </returns>
        public TarEntry? GetNextEntry()
        {
            if (hasHitEOF) return default;
            else
            {
                if (currentEntry is not null) SkipToNextEntry();

                tarBuffer.ReadBlock(headerBuffer);

                if (TarBuffer.IsEndOfArchiveBlock(headerBuffer))
                {
                    hasHitEOF = true;

                    // Read the second zero-filled block
                    tarBuffer.ReadBlock(headerBuffer);

                    currentEntry = default;
                    readBuffer?.Dispose();
                }
                else
                {
                    try
                    {
                        TarHeader.ValidateHeader(headerBuffer,this);
                        if (!IsChecksumOK)
                        {
                            throw new TarException("Header checksum is invalid");
                        }

                        entryOffset = 0;
                        entrySize = Size;
                        string longName = string.Empty;
                        long numToRead = 0;

                        switch (Type_Flag)
                        {
                            case TarHeader.LF_GNU_LONGNAME:
                                var arrayName = ArrayPool<byte>.Shared.Rent((int)entrySize);
                                numToRead = entrySize;
                                var offset = 0;
                                while (numToRead > 0)
                                {
                                    var length = numToRead > TarBuffer.BlockSize ? TarBuffer.BlockSize : (int)numToRead;
                                    int numRead = Read(arrayName, offset, length);

                                    numToRead -= numRead;
                                    offset += numRead;
                                }
                                longName = TarHeader.ParseName(arrayName.AsSpan()[..(int)entrySize],encoding);
                                ArrayPool<byte>.Shared.Return(arrayName);
                                SkipToNextEntry();
                                tarBuffer.ReadBlock(headerBuffer);
                                break;
                            case TarHeader.LF_GHDR:
                                // POSIX global extended header
                                // Ignore things we dont understand completely for now
                                SkipToNextEntry();
                                tarBuffer.ReadBlock(headerBuffer);
                                break;
                            case TarHeader.LF_XHDR:
                                // POSIX extended header
                                numToRead = entrySize;

                                var xhr = new TarExtendedHeaderReader();

                                while (numToRead > 0)
                                {
                                    var length = numToRead > nameBuffer.Length ? nameBuffer.Length : (int)numToRead;
                                    int numRead = Read(nameBuffer, 0, length);

                                    if (numRead == -1)
                                    {
                                        throw new InvalidHeaderException("Failed to read long name entry");
                                    }

                                    xhr.Read(nameBuffer, numRead);
                                    numToRead -= numRead;
                                }
                                if (xhr.Headers.TryGetValue("path", out string? name))
                                {
                                    longName = name ?? string.Empty;
                                }

                                SkipToNextEntry();
                                tarBuffer.ReadBlock(headerBuffer);
                                break;
                            case TarHeader.LF_GNU_VOLHDR:
                                // TODO: could show volume name when verbose
                                SkipToNextEntry();
                                tarBuffer.ReadBlock(headerBuffer);
                                break;
                            default:
                                if (Type_Flag is
                                    not TarHeader.LF_NORMAL and
                                    not TarHeader.LF_OLDNORM and
                                    not TarHeader.LF_LINK and
                                    not TarHeader.LF_SYMLINK and
                                    not TarHeader.LF_DIR)
                                {
                                    // Ignore things we dont understand completely for now
                                    SkipToNextEntry();
                                    tarBuffer.ReadBlock(headerBuffer);
                                }
                                break;
                        }

                        currentEntry = new TarEntry(headerBuffer, encoding);

                        if (!string.IsNullOrEmpty(longName))
                        {
                            currentEntry.Name = longName;
                        }

                        // Magic was checked here for 'ustar' but there are multiple valid possibilities
                        // so this is not done anymore.

                        entryOffset = 0;

                        // TODO: Review How do we resolve this discrepancy?!
                        entrySize = currentEntry.Size;
                    }
                    catch (InvalidHeaderException ex)
                    {
                        entrySize = 0;
                        entryOffset = 0;
                        currentEntry = null;

                        var errorText = string.Format(
                            "Bad header in record {0} block {1} {2}",
                            tarBuffer.CurrentRecord, tarBuffer.CurrentBlock,
                            ex.Message);
                        throw new InvalidHeaderException(errorText);
                    }
                    finally
                    {
                        readBuffer?.Dispose();
                    }
                }
                return currentEntry;
            }
        }

        private void SkipToNextEntry()
        {
            long numToSkip = entrySize - entryOffset;

            int i;
            for (i = 0; i < numToSkip; i += TarBuffer.BlockSize) 
            {
                tarBuffer.SkipBlock();
            }
            entryOffset += i;

            readBuffer?.Dispose();
            readBuffer = null;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return inputStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            inputStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            inputStream.Write(buffer, offset, count);
        }

        public void ReturnArrays()
        {
            ArrayPool<byte>.Shared.Return(headerBuffer);
            ArrayPool<byte>.Shared.Return(nameBuffer);
            ArrayPool<byte>.Shared.Return(recBuffer);

            tarBuffer.ReturnArrays();
        }

        #region Instance Fields

        private readonly byte[] headerBuffer;
        private readonly byte[] nameBuffer;
        private readonly byte[] recBuffer;

        private readonly StringBuilder contentStringBuilder = new();

        public bool IsChecksumOK = false;
        public byte Type_Flag = 0x0;
        public long Size = 0;

        /// <summary>
        /// Flag set when last block has been read
        /// </summary>
        protected bool hasHitEOF;

        /// <summary>
        /// Size of this entry as recorded in header
        /// </summary>
        protected long entrySize;

        /// <summary>
        /// Number of bytes read for this entry so far
        /// </summary>
        protected long entryOffset;

        /// <summary>
        /// Buffer used with calls to <code>Read()</code>
        /// </summary>
        protected IMemoryOwner<byte>? readBuffer;

        /// <summary>
        /// Working buffer
        /// </summary>
        protected TarBuffer tarBuffer;

        /// <summary>
        /// Current entry being read
        /// </summary>
        private TarEntry? currentEntry;

        /// <summary>
        /// Stream used as the source of input data.
        /// </summary>
        private readonly Stream inputStream;

        private readonly Encoding encoding;

        #endregion Instance Fields
    }
}
