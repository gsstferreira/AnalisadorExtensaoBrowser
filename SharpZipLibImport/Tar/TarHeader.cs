using SharpZipLib.Core;
using System.Buffers;
using System.Text;

namespace SharpZipLib.Tar
{
    /// <summary>
    /// This class encapsulates the Tar Entry Header used in Tar Archives.
    /// The class also holds a number of tar constants, used mostly in headers.
    /// </summary>
    /// <remarks>
    ///    The tar format and its POSIX successor PAX have a long history which makes for compatability
    ///    issues when creating and reading files.
    ///
    ///    This is further complicated by a large number of programs with variations on formats
    ///    One common issue is the handling of names longer than 100 characters.
    ///    GNU style long names are currently supported.
    ///
    /// This is the ustar (Posix 1003.1) header.
    ///
    /// struct header
    /// {
    /// 	char t_name[100];          //   0 Filename
    /// 	char t_mode[8];            // 100 Permissions
    /// 	char t_uid[8];             // 108 Numerical User ID
    /// 	char t_gid[8];             // 116 Numerical Group ID
    /// 	char t_size[12];           // 124 Filesize
    /// 	char t_mtime[12];          // 136 st_mtime
    /// 	char t_chksum[8];          // 148 Checksum
    /// 	char t_typeflag;           // 156 Type of File
    /// 	char t_linkname[100];      // 157 Target of Links
    /// 	char t_magic[6];           // 257 "ustar" or other...
    /// 	char t_version[2];         // 263 Version fixed to 00
    /// 	char t_uname[32];          // 265 User Name
    /// 	char t_gname[32];          // 297 Group Name
    /// 	char t_devmajor[8];        // 329 Major for devices
    /// 	char t_devminor[8];        // 337 Minor for devices
    /// 	char t_prefix[155];        // 345 Prefix for t_name
    /// 	char t_mfill[12];          // 500 Filler up to 512
    /// };
    /// </remarks>
    public class TarHeader
    {
        #region Constants

        /// <summary>
        /// The length of the name field in a header buffer.
        /// </summary>
        public const int NAMELEN = 100;

        /// <summary>
        /// The length of the mode field in a header buffer.
        /// </summary>
        public const int MODELEN = 8;

        /// <summary>
        /// The length of the user id field in a header buffer.
        /// </summary>
        public const int UIDLEN = 8;

        /// <summary>
        /// The length of the group id field in a header buffer.
        /// </summary>
        public const int GIDLEN = 8;

        /// <summary>
        /// The length of the checksum field in a header buffer.
        /// </summary>
        public const int CHKSUMLEN = 8;

        /// <summary>
        /// Offset of checksum in a header buffer.
        /// </summary>
        public const int CHKSUMOFS = 148;

        /// <summary>
        /// The length of the size field in a header buffer.
        /// </summary>
        public const int SIZELEN = 12;

        /// <summary>
        /// The length of the magic field in a header buffer.
        /// </summary>
        public const int MAGICLEN = 6;

        /// <summary>
        /// The length of the version field in a header buffer.
        /// </summary>
        public const int VERSIONLEN = 2;

        /// <summary>
        /// The length of the modification time field in a header buffer.
        /// </summary>
        public const int MODTIMELEN = 12;

        /// <summary>
        /// The length of the user name field in a header buffer.
        /// </summary>
        public const int UNAMELEN = 32;

        /// <summary>
        /// The length of the group name field in a header buffer.
        /// </summary>
        public const int GNAMELEN = 32;

        /// <summary>
        /// The length of the devices field in a header buffer.
        /// </summary>
        public const int DEVLEN = 8;

        /// <summary>
        /// The length of the name prefix field in a header buffer.
        /// </summary>
        public const int PREFIXLEN = 155;

        //
        // LF_ constants represent the "type" of an entry
        //

        /// <summary>
        ///  The "old way" of indicating a normal file.
        /// </summary>
        public const byte LF_OLDNORM = 0;

        /// <summary>
        /// Normal file type.
        /// </summary>
        public const byte LF_NORMAL = (byte)'0';

        /// <summary>
        /// Link file type.
        /// </summary>
        public const byte LF_LINK = (byte)'1';

        /// <summary>
        /// Symbolic link file type.
        /// </summary>
        public const byte LF_SYMLINK = (byte)'2';

        /// <summary>
        /// Character device file type.
        /// </summary>
        public const byte LF_CHR = (byte)'3';

        /// <summary>
        /// Block device file type.
        /// </summary>
        public const byte LF_BLK = (byte)'4';

        /// <summary>
        /// Directory file type.
        /// </summary>
        public const byte LF_DIR = (byte)'5';

        /// <summary>
        /// FIFO (pipe) file type.
        /// </summary>
        public const byte LF_FIFO = (byte)'6';

        /// <summary>
        /// Contiguous file type.
        /// </summary>
        public const byte LF_CONTIG = (byte)'7';

        /// <summary>
        /// Posix.1 2001 global extended header
        /// </summary>
        public const byte LF_GHDR = (byte)'g';

        /// <summary>
        /// Posix.1 2001 extended header
        /// </summary>
        public const byte LF_XHDR = (byte)'x';

        // POSIX allows for upper case ascii type as extensions

        /// <summary>
        /// Solaris access control list file type
        /// </summary>
        public const byte LF_ACL = (byte)'A';

        /// <summary>
        /// GNU dir dump file type
        /// This is a dir entry that contains the names of files that were in the
        /// dir at the time the dump was made
        /// </summary>
        public const byte LF_GNU_DUMPDIR = (byte)'D';

        /// <summary>
        /// Solaris Extended Attribute File
        /// </summary>
        public const byte LF_EXTATTR = (byte)'E';

        /// <summary>
        /// Inode (metadata only) no file content
        /// </summary>
        public const byte LF_META = (byte)'I';

        /// <summary>
        /// Identifies the next file on the tape as having a long link name
        /// </summary>
        public const byte LF_GNU_LONGLINK = (byte)'K';

        /// <summary>
        /// Identifies the next file on the tape as having a long name
        /// </summary>
        public const byte LF_GNU_LONGNAME = (byte)'L';

        /// <summary>
        /// Continuation of a file that began on another volume
        /// </summary>
        public const byte LF_GNU_MULTIVOL = (byte)'M';

        /// <summary>
        /// For storing filenames that dont fit in the main header (old GNU)
        /// </summary>
        public const byte LF_GNU_NAMES = (byte)'N';

        /// <summary>
        /// GNU Sparse file
        /// </summary>
        public const byte LF_GNU_SPARSE = (byte)'S';

        /// <summary>
        /// GNU Tape/volume header ignore on extraction
        /// </summary>
        public const byte LF_GNU_VOLHDR = (byte)'V';

        /// <summary>
        /// The magic tag representing a POSIX tar archive.  (would be written with a trailing NULL)
        /// </summary>
        public const string TMAGIC = "ustar";

        /// <summary>
        /// The magic tag representing an old GNU tar archive where version is included in magic and overwrites it
        /// </summary>
        public const string GNU_TMAGIC = "ustar  ";

        private const long timeConversionFactor = 10000000L; // 1 tick == 100 nanoseconds
        private static readonly DateTime dateTime1970 = new(1970, 1, 1, 0, 0, 0, 0);

        public const int CHECKSUM_OFFSET = NAMELEN + MODELEN + UIDLEN + GIDLEN + SIZELEN + MODTIMELEN;
        public const int SIZE_OFFSET = NAMELEN + MODELEN + UIDLEN + GIDLEN;
        public const int TYPE_FLAG_OFFSET = CHECKSUM_OFFSET + 1;
        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initialise a default TarHeader instance
        /// </summary>
        public TarHeader()
        {
            Name = string.Empty;
            Magic = TMAGIC;

            Size = 0;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Get/set the entry's Unix style permission mode.
        /// </summary>
        public int Mode
        {
            get { return mode; }
            set { mode = value; }
        }

        /// <summary>
        /// Get/set the entry's size.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when setting the size to less than zero.</exception>
        public long Size
        {
            get { return size; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Cannot be less than zero");
                }

                size = value;
            }
        }

        /// <summary>
        /// Get/set the entry's modification time.
        /// </summary>
        /// <remarks>
        /// The modification time is only accurate to within a second.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when setting the date time to less than 1/1/1970.</exception>
        public DateTime ModTime
        {
            get { return modTime; }
            set
            {
                if (value < dateTime1970)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "ModTime cannot be before Jan 1st 1970");
                }

                modTime = new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second);
            }
        }
        /// <summary>
        /// Get value of true if the header checksum is valid, false otherwise.
        /// </summary>
        public bool IsChecksumValid
        {
            get { return isChecksumValid; }
        }

        /// <summary>
        /// Get/set the entry's type flag.
        /// </summary>
        public byte TypeFlag
        {
            get { return typeFlag; }
            set { typeFlag = value; }
        }
 
        #endregion Properties

        /// <summary>
        /// Parse TarHeader information from a header buffer.
        /// </summary>
        /// <param name = "header">
        /// The tar entry header buffer to get information from.
        /// </param>
        /// <param name = "nameEncoding">
        /// The <see cref="Encoding"/> used for the Name field, or null for ASCII only
        /// </param>
        public void ParseBuffer(byte[] header, Encoding nameEncoding)
        {
            int offset = 0;
            var headerSpan = header.AsSpan();

            Name = ParseName(headerSpan.Slice(offset, NAMELEN), nameEncoding);
            offset += NAMELEN;

            mode = (int)ParseOctal(header, offset, MODELEN);
            offset += MODELEN;

            offset += UIDLEN;

            offset += GIDLEN;

            Size = ParseBinaryOrOctal(header, offset, SIZELEN);
            offset += SIZELEN;

            ModTime = GetDateTimeFromCTime(ParseOctal(header, offset, MODTIMELEN));
            offset += MODTIMELEN;

            Checksum = (int)ParseOctal(header, offset, CHKSUMLEN);
            offset += CHKSUMLEN;

            TypeFlag = header[offset++];

            offset += NAMELEN;

            Magic = ParseName(headerSpan.Slice(offset, MAGICLEN), nameEncoding);
            offset += MAGICLEN;

            if (Magic == "ustar")
            {
                //Version = ParseName(headerSpan.Slice(offset, VERSIONLEN), nameEncoding);
                offset += VERSIONLEN;
                offset += UNAMELEN;
                offset += GNAMELEN;
                offset += DEVLEN;
                offset += DEVLEN;

                string prefix = ParseName(headerSpan.Slice(offset, PREFIXLEN), nameEncoding);
                if (!string.IsNullOrEmpty(prefix)) Name = prefix + '/' + Name;
            }

            isChecksumValid = Checksum == MakeCheckSum(header);
        }

        public static void ValidateHeader(byte[] header, TarInputStream tarStream)
        {
            tarStream.Size = ParseBinaryOrOctal(header, SIZE_OFFSET, SIZELEN);
            tarStream.Type_Flag = header[TYPE_FLAG_OFFSET];
            tarStream.IsChecksumOK = (int)ParseOctal(header, CHECKSUM_OFFSET, CHKSUMLEN) == MakeCheckSum(header);
        }

        // Return value that may be stored in octal or binary. Length must exceed 8.
        //
        private static long ParseBinaryOrOctal(byte[] header, int offset, int length)
        {
            if (header[offset] >= 0x80)
            {
                // File sizes over 8GB are stored in 8 right-justified bytes of binary indicated by setting the high-order bit of the leftmost byte of a numeric field.
                long result = 0;
                for (int pos = length - 8; pos < length; pos++)
                {
                    result = result << 8 | header[offset + pos];
                }

                return result;
            }

            return ParseOctal(header, offset, length);
        }

        /// <summary>
        /// Parse an octal string from a header buffer.
        /// </summary>
        /// <param name = "header">The header buffer from which to parse.</param>
        /// <param name = "offset">The offset into the buffer from which to parse.</param>
        /// <param name = "length">The number of header bytes to parse.</param>
        /// <returns>The long equivalent of the octal string.</returns>
        public static long ParseOctal(byte[] header, int offset, int length)
        {
            ArgumentNullException.ThrowIfNull(header);

            long result = 0;
            bool stillPadding = true;

            int end = offset + length;
            for (int i = offset; i < end; ++i)
            {
                if (header[i] == 0)
                {
                    break;
                }

                if (header[i] == (byte)' ' || header[i] == '0')
                {
                    if (stillPadding)
                    {
                        continue;
                    }

                    if (header[i] == (byte)' ')
                    {
                        break;
                    }
                }

                stillPadding = false;

                result = (result << 3) + (header[i] - '0');
            }

            return result;
        }

        /// <summary>
        /// Parse a name from a header buffer.
        /// </summary>
        /// <param name="header">
        /// The header buffer from which to parse.
        /// </param>
        /// <param name="encoding">
        /// name encoding, or null for ASCII only
        /// </param>
        /// <returns>
        /// The name parsed.
        /// </returns>
        public static string ParseName(ReadOnlySpan<byte> header, Encoding encoding)
        {
            for (int i = 0; i < header.Length; i++)
            {
                if (header[i] == 0)
                {
                    return encoding.GetString(header[..i]);
                }
            }
            return encoding.GetString(header);
        }


        /// <summary>
        /// Add <paramref name="name">name</paramref> to the buffer as a collection of bytes
        /// </summary>
        /// <param name="name">The name to add</param>
        /// <param name="nameOffset">The offset of the first character</param>
        /// <param name="buffer">The buffer to add to</param>
        /// <param name="bufferOffset">The index of the first byte to add</param>
        /// <param name="length">The number of characters/bytes to add</param>
        /// <param name="encoding">name encoding, or null for ASCII only</param>
        /// <returns>The next free index in the <paramref name="buffer"/></returns>
        public static int GetNameBytes(string name, int nameOffset, byte[] buffer, int bufferOffset, int length,
            Encoding encoding)
        {
            ArgumentNullException.ThrowIfNull(name);

            ArgumentNullException.ThrowIfNull(buffer);

            int i;
            if (encoding != null)
            {
                // it can be more sufficient if using Span or unsafe
                ReadOnlySpan<char> nameArray =
                    name.AsSpan().Slice(nameOffset, Math.Min(name.Length - nameOffset, length));
                var charArray = ArrayPool<char>.Shared.Rent(nameArray.Length);
                nameArray.CopyTo(charArray);

                // it can be more sufficient if using Span(or unsafe?) and ArrayPool for temporary buffer
                var bytesLength = encoding.GetBytes(charArray, 0, nameArray.Length, buffer, bufferOffset);
                ArrayPool<char>.Shared.Return(charArray);
                i = Math.Min(bytesLength, length);
            }
            else
            {
                for (i = 0; i < length && nameOffset + i < name.Length; ++i)
                {
                    buffer[bufferOffset + i] = (byte)name[nameOffset + i];
                }
            }

            for (; i < length; ++i)
            {
                buffer[bufferOffset + i] = 0;
            }

            return bufferOffset + length;
        }

        /// <summary>
        /// Put an octal representation of a value into a buffer
        /// </summary>
        /// <param name = "value">
        /// the value to be converted to octal
        /// </param>
        /// <param name = "buffer">
        /// buffer to store the octal string
        /// </param>
        /// <param name = "offset">
        /// The offset into the buffer where the value starts
        /// </param>
        /// <param name = "length">
        /// The length of the octal string to create
        /// </param>
        /// <returns>
        /// The offset of the character next byte after the octal string
        /// </returns>
        public static int GetOctalBytes(long value, byte[] buffer, int offset, int length)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            int localIndex = length - 1;

            // Either a space or null is valid here.  We use NULL as per GNUTar
            buffer[offset + localIndex] = 0;
            --localIndex;

            if (value > 0)
            {
                for (long v = value; localIndex >= 0 && v > 0; --localIndex)
                {
                    buffer[offset + localIndex] = (byte)((byte)'0' + (byte)(v & 7));
                    v >>= 3;
                }
            }

            for (; localIndex >= 0; --localIndex)
            {
                buffer[offset + localIndex] = (byte)'0';
            }

            return offset + length;
        }


        /// <summary>
        /// Compute the checksum for a tar entry header.
        /// The checksum field must be all spaces prior to this happening
        /// </summary>
        /// <param name = "buffer">The tar entry's header buffer.</param>
        /// <returns>The computed checksum.</returns>
        private static int ComputeCheckSum(byte[] buffer)
        {
            int sum = 0;
            for (int i = 0; i < buffer.Length; ++i)
            {
                sum += buffer[i];
            }

            return sum;
        }

        /// <summary>
        /// Make a checksum for a tar entry ignoring the checksum contents.
        /// </summary>
        /// <param name = "buffer">The tar entry's header buffer.</param>
        /// <returns>The checksum for the buffer</returns>
        private static int MakeCheckSum(byte[] buffer)
        {
            int sum = 0;
            for (int i = 0; i < CHKSUMOFS; ++i)
            {
                sum += buffer[i];
            }

            for (int i = 0; i < CHKSUMLEN; ++i)
            {
                sum += (byte)' ';
            }

            for (int i = CHKSUMOFS + CHKSUMLEN; i < buffer.Length; ++i)
            {
                sum += buffer[i];
            }

            return sum;
        }

        private static DateTime GetDateTimeFromCTime(long ticks)
        {
            DateTime result;

            try
            {
                result = new DateTime(dateTime1970.Ticks + ticks * timeConversionFactor);
            }
            catch (ArgumentOutOfRangeException)
            {
                result = dateTime1970;
            }

            return result;
        }

        #region Instance Fields

        public string Name { get; set; }
        private int mode;
        private long size;
        private DateTime modTime;
        private int Checksum { get; set; }
        private bool isChecksumValid;
        private byte typeFlag;
        private string Magic;

        #endregion Instance Fields
    }
}