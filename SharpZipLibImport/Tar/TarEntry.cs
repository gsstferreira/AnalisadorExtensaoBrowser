using SharpZipLib.Core;
using System.Text;

namespace SharpZipLib.Tar
{
    /// <summary>
    /// This class represents an entry in a Tar archive. It consists
    /// of the entry's header, as well as the entry's File. Entries
    /// can be instantiated in one of three ways, depending on how
    /// they are to be used.
    /// <p>
    /// TarEntries that are created from the header bytes read from
    /// an archive are instantiated with the TarEntry( byte[] )
    /// constructor. These entries will be used when extracting from
    /// or listing the contents of an archive. These entries have their
    /// header filled in using the header bytes. They also set the File
    /// to null, since they reference an archive entry not a file.</p>
    /// <p>
    /// TarEntries that are created from files that are to be written
    /// into an archive are instantiated with the CreateEntryFromFile(string)
    /// pseudo constructor. These entries have their header filled in using
    /// the File's information. They also keep a reference to the File
    /// for convenience when writing entries.</p>
    /// <p>
    /// Finally, TarEntries can be constructed from nothing but a name.
    /// This allows the programmer to construct the entry by hand, for
    /// instance when only an InputStream is available for writing to
    /// the archive, and the header information is constructed from
    /// other information. In this case the header fields are set to
    /// defaults and the File is set to null.</p>
    /// <see cref="TarHeader"/>
    /// </summary>
    public class TarEntry
    {
        #region Constructors

        /// <summary>
        /// Initialise a default instance of <see cref="TarEntry"/>.
        /// </summary>
        private TarEntry()
        {
            file = string.Empty;
            header = new TarHeader();
        }

        /// <summary>
        /// Construct an entry from an archive's header bytes. File is set
        /// to null.
        /// </summary>
        /// <param name = "headerBuffer">
        /// The header bytes from a tar archive entry.
        /// </param>
        /// <param name = "nameEncoding">
        /// The <see cref="Encoding"/> used for the Name fields, or null for ASCII only
        /// </param>
        public TarEntry(byte[] headerBuffer, Encoding nameEncoding)
        {
            file = string.Empty;
            header = new TarHeader();
            header.ParseBuffer(headerBuffer, nameEncoding);
        }

        #endregion Constructors

        /// <summary>
        /// Construct an entry for a file. File is set to file, and the
        /// header is constructed from information from the file.
        /// </summary>
        /// <param name = "fileName">The file name that the entry represents.</param>
        /// <returns>Returns the newly created <see cref="TarEntry"/></returns>
        public static TarEntry CreateEntryFromFile(string fileName)
        {
            var entry = new TarEntry();
            entry.GetFileTarHeader(entry.header, fileName);
            return entry;
        }

        /// <summary>
        /// Get this entry's header.
        /// </summary>
        /// <returns>
        /// This entry's TarHeader.
        /// </returns>
        public TarHeader TarHeader
        {
            get { return header; }
        }

        /// <summary>
        /// Get/Set this entry's name.
        /// </summary>
        public string Name
        {
            get { return header.Name; }
            set { header.Name = value; }
        }

        ///// <summary>
        ///// Get/set this entry's user id.
        ///// </summary>
        //public int UserId
        //{
        //    get { return header.UserId; }
        //    set { header.UserId = value; }
        //}


        /// <summary>
        /// Get/set this entry's recorded file size.
        /// </summary>
        public long Size
        {
            get { return header.Size; }
            set { header.Size = value; }
        }

        /// <summary>
        /// Return true if this entry represents a directory, false otherwise
        /// </summary>
        /// <returns>
        /// True if this entry is a directory.
        /// </returns>
        public bool IsDirectory
        {
            get
            {
                if (header is not null)
                {
                    if (header.TypeFlag == TarHeader.LF_DIR || Name.EndsWith('/'))
                    {
                        return true;
                    }
                    else return false;
                }

                return Directory.Exists(file);
            }
        }

        /// <summary>
        /// Fill in a TarHeader with information from a File.
        /// </summary>
        /// <param name="header">
        /// The TarHeader to fill in.
        /// </param>
        /// <param name="file">
        /// The file from which to get the header information.
        /// </param>
        public void GetFileTarHeader(TarHeader header, string file)
        {
            ArgumentNullException.ThrowIfNull(header);

            ArgumentNullException.ThrowIfNull(file);

            this.file = file;

            // bugfix from torhovl from #D forum:
            string name = file;

            // 23-Jan-2004 GnuTar allows device names in path where the name is not local to the current directory
            if (name.StartsWith(Directory.GetCurrentDirectory(), StringComparison.Ordinal))
            {
                name = name[Directory.GetCurrentDirectory().Length..];
            }

            name = name.ToTarArchivePath();

            //header.LinkName = string.Empty;
            header.Name = name;

            if (Directory.Exists(file))
            {
                header.Mode = 1003; // Magic number for security access for a UNIX filesystem
                header.TypeFlag = TarHeader.LF_DIR;
                if (header.Name.Length == 0 || header.Name[^1] != '/')
                {
                    header.Name += "/";
                }

                header.Size = 0;
            }
            else
            {
                header.Mode = 33216; // Magic number for security access for a UNIX filesystem
                header.TypeFlag = TarHeader.LF_NORMAL;
                header.Size = new FileInfo(file.Replace('/', Path.DirectorySeparatorChar)).Length;
            }

            header.ModTime = File.GetLastWriteTime(file.Replace('/', Path.DirectorySeparatorChar)).ToUniversalTime();
        }

        /// <summary>
        /// Get entries for all files present in this entries directory.
        /// If this entry doesnt represent a directory zero entries are returned.
        /// </summary>
        /// <returns>
        /// An array of TarEntry's for this entry's children.
        /// </returns>
        public TarEntry[] GetDirectoryEntries()
        {
            if (file == null || !Directory.Exists(file))
            {
                return Empty.Array<TarEntry>();
            }

            string[] list = Directory.GetFileSystemEntries(file);
            TarEntry[] result = new TarEntry[list.Length];

            for (int i = 0; i < list.Length; ++i)
            {
                result[i] = CreateEntryFromFile(list[i]);
            }

            return result;
        }


        /// <summary>
        /// Fill in a TarHeader given only the entry's name.
        /// </summary>
        /// <param name="name">
        /// The tar entry name.
        /// </param>
        public void NameTarHeader(string name)
        {
            ArgumentNullException.ThrowIfNull(name);

            bool isDir = name.EndsWith('/');

            header.Name = name;
            header.Mode = isDir ? 1003 : 33216;
            header.Size = 0;

            header.ModTime = DateTime.UtcNow;

            header.TypeFlag = isDir ? TarHeader.LF_DIR : TarHeader.LF_NORMAL;
        }

        #region Instance Fields

        /// <summary>
        /// The name of the file this entry represents or null if the entry is not based on a file.
        /// </summary>
        private string file;

        /// <summary>
        /// The entry's header information.
        /// </summary>
        private TarHeader header;

        #endregion Instance Fields
    }
}
