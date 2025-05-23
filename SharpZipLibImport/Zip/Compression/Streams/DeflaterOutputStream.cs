using SharpZipLib.Core.Exceptions;
using SharpZipLib.Encryption;
using System.Security.Cryptography;
using System.Text;

namespace SharpZipLib.Zip.Compression.Streams
{
    /// <summary>
    /// A special stream deflating or compressing the bytes that are
    /// written to it.  It uses a Deflater to perform actual deflating.<br/>
    /// Authors of the original java version : Tom Tromey, Jochen Hoenicke
    /// </summary>
    public class DeflaterOutputStream : Stream
    {
        #region Constructors

        /// <summary>
        /// Creates a new DeflaterOutputStream with a default Deflater and default buffer size.
        /// </summary>
        /// <param name="baseOutputStream">
        /// the output stream where deflated output should be written.
        /// </param>
        public DeflaterOutputStream(Stream baseOutputStream)
            : this(baseOutputStream, new Deflater(), 512)
        {
        }

        /// <summary>
        /// Creates a new DeflaterOutputStream with the given Deflater and
        /// default buffer size.
        /// </summary>
        /// <param name="baseOutputStream">
        /// the output stream where deflated output should be written.
        /// </param>
        /// <param name="deflater">
        /// the underlying deflater.
        /// </param>
        public DeflaterOutputStream(Stream baseOutputStream, Deflater deflater)
            : this(baseOutputStream, deflater, 512)
        {
        }

        /// <summary>
        /// Creates a new DeflaterOutputStream with the given Deflater and
        /// buffer size.
        /// </summary>
        /// <param name="baseOutputStream">
        /// The output stream where deflated output is written.
        /// </param>
        /// <param name="deflater">
        /// The underlying deflater to use
        /// </param>
        /// <param name="bufferSize">
        /// The buffer size in bytes to use when deflating (minimum value 512)
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// bufsize is less than or equal to zero.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// baseOutputStream does not support writing
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// deflater instance is null
        /// </exception>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public DeflaterOutputStream(Stream baseOutputStream, Deflater deflater, int bufferSize)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            ArgumentNullException.ThrowIfNull(baseOutputStream);

            if (baseOutputStream.CanWrite == false)
            {
                throw new ArgumentException("Must support writing", nameof(baseOutputStream));
            }

            ArgumentOutOfRangeException.ThrowIfLessThan(bufferSize, 512);

            baseOutputStream_ = baseOutputStream;
            buffer_ = new byte[bufferSize];
            deflater_ = deflater ?? throw new ArgumentNullException(nameof(deflater));
        }

        #endregion Constructors

        #region Public API

        /// <summary>
        /// Finishes the stream by calling finish() on the deflater.
        /// </summary>
        /// <exception cref="SharpZipBaseException">
        /// Not all input is deflated
        /// </exception>
        public virtual void Finish()
        {
            deflater_.Finish();
            while (!deflater_.IsFinished)
            {
                int len = deflater_.Deflate(buffer_, 0, buffer_.Length);
                if (len <= 0)
                {
                    break;
                }

                EncryptBlock(buffer_, 0, len);

                baseOutputStream_.Write(buffer_, 0, len);
            }

            if (!deflater_.IsFinished)
            {
                throw new SharpZipBaseException("Can't deflate all input?");
            }

            baseOutputStream_.Flush();

            if (cryptoTransform_ != null)
            {
                if (cryptoTransform_ is ZipAESTransform)
                {
                    AESAuthCode = ((ZipAESTransform)cryptoTransform_).GetAuthCode();
                }
                cryptoTransform_.Dispose();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                cryptoTransform_ = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }
        }

        /// <summary>
        /// Finishes the stream by calling finish() on the deflater.
        /// </summary>
        /// <param name="ct">The <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
        /// <exception cref="SharpZipBaseException">
        /// Not all input is deflated
        /// </exception>
        public virtual async Task FinishAsync(CancellationToken ct)
        {
            deflater_.Finish();
            while (!deflater_.IsFinished)
            {
                int len = deflater_.Deflate(buffer_, 0, buffer_.Length);
                if (len <= 0)
                {
                    break;
                }

                EncryptBlock(buffer_, 0, len);

                await baseOutputStream_.WriteAsync(buffer_, 0, len, ct).ConfigureAwait(false);
            }

            if (!deflater_.IsFinished)
            {
                throw new SharpZipBaseException("Can't deflate all input?");
            }

            await baseOutputStream_.FlushAsync(ct).ConfigureAwait(false);

            if (cryptoTransform_ != null)
            {
                if (cryptoTransform_ is ZipAESTransform)
                {
                    AESAuthCode = ((ZipAESTransform)cryptoTransform_).GetAuthCode();
                }
                cryptoTransform_.Dispose();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                cryptoTransform_ = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }
        }

        /// <summary>
        /// Gets or sets a flag indicating ownership of underlying stream.
        /// When the flag is true <see cref="Stream.Dispose()" /> will close the underlying stream also.
        /// </summary>
        /// <remarks>The default value is true.</remarks>
        public bool IsStreamOwner { get; set; } = true;

        ///	<summary>
        /// Allows client to determine if an entry can be patched after its added
        /// </summary>
        public bool CanPatchEntries
        {
            get
            {
                return baseOutputStream_.CanSeek;
            }
        }

        #endregion Public API

        #region Encryption

        /// <summary>
        /// The CryptoTransform currently being used to encrypt the compressed data.
        /// </summary>
        protected ICryptoTransform cryptoTransform_;

        /// <summary>
        /// Returns the 10 byte AUTH CODE to be appended immediately following the AES data stream.
        /// </summary>
        protected byte[] AESAuthCode;

        /// <inheritdoc cref="StringCodec.ZipCryptoEncoding"/>
        public Encoding ZipCryptoEncoding
        {
            get => _stringCodec.ZipCryptoEncoding;
            set
            {
                _stringCodec = _stringCodec.WithZipCryptoEncoding(value);
            }
        }

        /// <summary>
        /// Encrypt a block of data
        /// </summary>
        /// <param name="buffer">
        /// Data to encrypt.  NOTE the original contents of the buffer are lost
        /// </param>
        /// <param name="offset">
        /// Offset of first byte in buffer to encrypt
        /// </param>
        /// <param name="length">
        /// Number of bytes in buffer to encrypt
        /// </param>
        protected void EncryptBlock(byte[] buffer, int offset, int length)
        {
            if (cryptoTransform_ is null) return;
            cryptoTransform_.TransformBlock(buffer, 0, length, buffer, 0);
        }

        #endregion Encryption

        #region Deflation Support

        /// <summary>
        /// Deflates everything in the input buffers.  This will call
        /// <code>def.deflate()</code> until all bytes from the input buffers
        /// are processed.
        /// </summary>
        protected void Deflate()
            => DeflateSyncOrAsync(false, null).GetAwaiter().GetResult();

        private async Task DeflateSyncOrAsync(bool flushing, CancellationToken? ct)
        {
            while (flushing || !deflater_.IsNeedingInput)
            {
                int deflateCount = deflater_.Deflate(buffer_, 0, buffer_.Length);

                if (deflateCount <= 0)
                {
                    break;
                }

                EncryptBlock(buffer_, 0, deflateCount);

                if (ct.HasValue)
                {
                    await baseOutputStream_.WriteAsync(buffer_, 0, deflateCount, ct.Value).ConfigureAwait(false);
                }
                else
                {
                    baseOutputStream_.Write(buffer_, 0, deflateCount);
                }
            }

            if (!deflater_.IsNeedingInput)
            {
                throw new SharpZipBaseException("DeflaterOutputStream can't deflate all input?");
            }
        }

        #endregion Deflation Support

        #region Stream Overrides

        /// <summary>
        /// Gets value indicating stream can be read from
        /// </summary>
        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating if seeking is supported for this stream
        /// This property always returns false
        /// </summary>
        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Get value indicating if this stream supports writing
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                return baseOutputStream_.CanWrite;
            }
        }

        /// <summary>
        /// Get current length of stream
        /// </summary>
        public override long Length
        {
            get
            {
                return baseOutputStream_.Length;
            }
        }

        /// <summary>
        /// Gets the current position within the stream.
        /// </summary>
        /// <exception cref="NotSupportedException">Any attempt to set position</exception>
        public override long Position
        {
            get
            {
                return baseOutputStream_.Position;
            }
            set
            {
                throw new NotSupportedException("Position property not supported");
            }
        }

        /// <summary>
        /// Sets the current position of this stream to the given value. Not supported by this class!
        /// </summary>
        /// <param name="offset">The offset relative to the <paramref name="origin"/> to seek.</param>
        /// <param name="origin">The <see cref="SeekOrigin"/> to seek from.</param>
        /// <returns>The new position in the stream.</returns>
        /// <exception cref="NotSupportedException">Any access</exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("DeflaterOutputStream Seek not supported");
        }

        /// <summary>
        /// Sets the length of this stream to the given value. Not supported by this class!
        /// </summary>
        /// <param name="value">The new stream length.</param>
        /// <exception cref="NotSupportedException">Any access</exception>
        public override void SetLength(long value)
        {
            throw new NotSupportedException("DeflaterOutputStream SetLength not supported");
        }

        /// <summary>
        /// Read a byte from stream advancing position by one
        /// </summary>
        /// <returns>The byte read cast to an int.  THe value is -1 if at the end of the stream.</returns>
        /// <exception cref="NotSupportedException">Any access</exception>
        public override int ReadByte()
        {
            throw new NotSupportedException("DeflaterOutputStream ReadByte not supported");
        }

        /// <summary>
        /// Read a block of bytes from stream
        /// </summary>
        /// <param name="buffer">The buffer to store read data in.</param>
        /// <param name="offset">The offset to start storing at.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <returns>The actual number of bytes read.  Zero if end of stream is detected.</returns>
        /// <exception cref="NotSupportedException">Any access</exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("DeflaterOutputStream Read not supported");
        }

        /// <summary>
        /// Flushes the stream by calling <see cref="Flush">Flush</see> on the deflater and then
        /// on the underlying stream.  This ensures that all bytes are flushed.
        /// </summary>
        public override void Flush()
        {
            deflater_.Flush();
            DeflateSyncOrAsync(true, null).GetAwaiter().GetResult();
            baseOutputStream_.Flush();
        }

        /// <summary>
        /// Asynchronously clears all buffers for this stream, causes any buffered data to be written to the underlying device, and monitors cancellation requests.
        /// </summary>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            deflater_.Flush();
            await DeflateSyncOrAsync(true, cancellationToken).ConfigureAwait(false);
            await baseOutputStream_.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Calls <see cref="Finish"/> and closes the underlying
        /// stream when <see cref="IsStreamOwner"></see> is true.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (!isClosed_)
            {
                isClosed_ = true;

                try
                {
                    Finish();
                    if (cryptoTransform_ != null)
                    {
                        GetAuthCodeIfAES();
                        cryptoTransform_.Dispose();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                        cryptoTransform_ = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                    }
                }
                finally
                {
                    if (IsStreamOwner)
                    {
                        baseOutputStream_.Dispose();
                    }
                }
            }
        }

#if NETSTANDARD2_1 || NETCOREAPP3_0_OR_GREATER
        /// <summary>
        /// Calls <see cref="FinishAsync"/> and closes the underlying
        /// stream when <see cref="IsStreamOwner"></see> is true.
        /// </summary>
        public override async ValueTask DisposeAsync()
        {
            if (!isClosed_)
            {
                isClosed_ = true;

                try
                {
                    await FinishAsync(CancellationToken.None).ConfigureAwait(false);
                    if (cryptoTransform_ != null)
                    {
                        GetAuthCodeIfAES();
                        cryptoTransform_.Dispose();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                        cryptoTransform_ = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                    }
                }
                finally
                {
                    if (IsStreamOwner)
                    {
                        await baseOutputStream_.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }
        }
#endif

        /// <summary>
        /// Get the Auth code for AES encrypted entries
        /// </summary>
        protected void GetAuthCodeIfAES()
        {
            if (cryptoTransform_ is ZipAESTransform)
            {
                AESAuthCode = ((ZipAESTransform)cryptoTransform_).GetAuthCode();
            }
        }

        /// <summary>
        /// Writes a single byte to the compressed output stream.
        /// </summary>
        /// <param name="value">
        /// The byte value.
        /// </param>
        public override void WriteByte(byte value)
        {
            byte[] b = [value];
            Write(b, 0, 1);
        }

        /// <summary>
        /// Writes bytes from an array to the compressed stream.
        /// </summary>
        /// <param name="buffer">
        /// The byte array
        /// </param>
        /// <param name="offset">
        /// The offset into the byte array where to start.
        /// </param>
        /// <param name="count">
        /// The number of bytes to write.
        /// </param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            deflater_.SetInput(buffer, offset, count);
            Deflate();
        }

        /// <summary>
        /// Asynchronously writes a sequence of bytes to the current stream, advances the current position within this stream by the number of bytes written, and monitors cancellation requests.
        /// </summary>
        /// <param name="buffer">
        /// The byte array
        /// </param>
        /// <param name="offset">
        /// The offset into the byte array where to start.
        /// </param>
        /// <param name="count">
        /// The number of bytes to write.
        /// </param>
        /// <param name="ct">
        /// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken ct)
        {
            deflater_.SetInput(buffer, offset, count);
            await DeflateSyncOrAsync(false, ct).ConfigureAwait(false);
        }

        #endregion Stream Overrides

        #region Instance Fields

        /// <summary>
        /// This buffer is used temporarily to retrieve the bytes from the
        /// deflater and write them to the underlying output stream.
        /// </summary>
#pragma warning disable IDE0044 // Add readonly modifier
        private byte[] buffer_;
#pragma warning restore IDE0044 // Add readonly modifier

        /// <summary>
        /// The deflater which is used to deflate the stream.
        /// </summary>
        protected Deflater deflater_;

        /// <summary>
        /// Base stream the deflater depends on.
        /// </summary>
        protected Stream baseOutputStream_;

        private bool isClosed_;

        /// <inheritdoc cref="StringCodec"/>
        protected StringCodec _stringCodec = ZipStrings.GetStringCodec();

        #endregion Instance Fields
    }
}
