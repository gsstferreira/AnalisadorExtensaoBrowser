namespace SharpZipLib.Core
{
    #region EventArgs

    /// <summary>
    /// Event arguments for scanning.
    /// </summary>
    /// <remarks>
    /// Initialise a new instance of <see cref="ScanEventArgs"/>
    /// </remarks>
    /// <param name="name">The file or directory name.</param>
    public class ScanEventArgs(string name) : EventArgs
    {

        #region Constructors

        #endregion Constructors

        /// <summary>
        /// The file or directory name for this event.
        /// </summary>
        public string Name
        {
            get { return name_; }
        }

        /// <summary>
        /// Get set a value indicating if scanning should continue or not.
        /// </summary>
        public bool ContinueRunning
        {
            get { return continueRunning_; }
            set { continueRunning_ = value; }
        }

        #region Instance Fields

#pragma warning disable IDE0044 // Add readonly modifier
        private string name_ = name;
#pragma warning restore IDE0044 // Add readonly modifier
        private bool continueRunning_ = true;

        #endregion Instance Fields
    }

    /// <summary>
    /// Event arguments during processing of a single file or directory.
    /// </summary>
    /// <remarks>
    /// Initialise a new instance of <see cref="ScanEventArgs"/>
    /// </remarks>
    /// <param name="name">The file or directory name if known.</param>
    /// <param name="processed">The number of bytes processed so far</param>
    /// <param name="target">The total number of bytes to process, 0 if not known</param>
    public class ProgressEventArgs(string name, long processed, long target) : EventArgs
    {

        #region Constructors

        #endregion Constructors

        /// <summary>
        /// The name for this event if known.
        /// </summary>
        public string Name
        {
            get { return name_; }
        }

        /// <summary>
        /// Get set a value indicating whether scanning should continue or not.
        /// </summary>
        public bool ContinueRunning
        {
            get { return continueRunning_; }
            set { continueRunning_ = value; }
        }

        /// <summary>
        /// Get a percentage representing how much of the <see cref="Target"></see> has been processed
        /// </summary>
        /// <value>0.0 to 100.0 percent; 0 if target is not known.</value>
        public float PercentComplete
        {
            get
            {
                float result = target_ <= 0 ? 0 : processed_ / (float)target_ * 100.0f;
                return result;
            }
        }

        /// <summary>
        /// The number of bytes processed so far
        /// </summary>
        public long Processed
        {
            get { return processed_; }
        }

        /// <summary>
        /// The number of bytes to process.
        /// </summary>
        /// <remarks>Target may be 0 or negative if the value isnt known.</remarks>
        public long Target
        {
            get { return target_; }
        }

        #region Instance Fields

#pragma warning disable IDE0044 // Add readonly modifier
        private string name_ = name;
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning disable IDE0044 // Add readonly modifier
        private long processed_ = processed;
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning disable IDE0044 // Add readonly modifier
        private long target_ = target;
#pragma warning restore IDE0044 // Add readonly modifier
        private bool continueRunning_ = true;

        #endregion Instance Fields
    }

    /// <summary>
    /// Event arguments for directories.
    /// </summary>
    /// <remarks>
    /// Initialize an instance of <see cref="DirectoryEventArgs"></see>.
    /// </remarks>
    /// <param name="name">The name for this directory.</param>
    /// <param name="hasMatchingFiles">Flag value indicating if any matching files are contained in this directory.</param>
    public class DirectoryEventArgs(string name, bool hasMatchingFiles) : ScanEventArgs(name)
    {

        #region Constructors

        #endregion Constructors

        /// <summary>
        /// Get a value indicating if the directory contains any matching files or not.
        /// </summary>
        public bool HasMatchingFiles
        {
            get { return hasMatchingFiles_; }
        }

        private readonly

        #region Instance Fields

        bool hasMatchingFiles_ = hasMatchingFiles;

        #endregion Instance Fields
    }

    /// <summary>
    /// Arguments passed when scan failures are detected.
    /// </summary>
    /// <remarks>
    /// Initialise a new instance of <see cref="ScanFailureEventArgs"></see>
    /// </remarks>
    /// <param name="name">The name to apply.</param>
    /// <param name="e">The exception to use.</param>
    public class ScanFailureEventArgs(string name, Exception e) : EventArgs
    {

        #region Constructors

        #endregion Constructors

        /// <summary>
        /// The applicable name.
        /// </summary>
        public string Name
        {
            get { return name_; }
        }

        /// <summary>
        /// The applicable exception.
        /// </summary>
        public Exception Exception
        {
            get { return exception_; }
        }

        /// <summary>
        /// Get / set a value indicating whether scanning should continue.
        /// </summary>
        public bool ContinueRunning
        {
            get { return continueRunning_; }
            set { continueRunning_ = value; }
        }

        #region Instance Fields

#pragma warning disable IDE0044 // Add readonly modifier
        private string name_ = name;
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning disable IDE0044 // Add readonly modifier
        private Exception exception_ = e;
#pragma warning restore IDE0044 // Add readonly modifier
        private bool continueRunning_ = true;

        #endregion Instance Fields
    }

    #endregion EventArgs

    #region Delegates

    /// <summary>
    /// Delegate invoked before starting to process a file.
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="e">The event arguments.</param>
    public delegate void ProcessFileHandler(object sender, ScanEventArgs e);

    /// <summary>
    /// Delegate invoked during processing of a file or directory
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="e">The event arguments.</param>
    public delegate void ProgressHandler(object sender, ProgressEventArgs e);

    /// <summary>
    /// Delegate invoked when a file has been completely processed.
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="e">The event arguments.</param>
    public delegate void CompletedFileHandler(object sender, ScanEventArgs e);

    /// <summary>
    /// Delegate invoked when a directory failure is detected.
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="e">The event arguments.</param>
    public delegate void DirectoryFailureHandler(object sender, ScanFailureEventArgs e);

    /// <summary>
    /// Delegate invoked when a file failure is detected.
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="e">The event arguments.</param>
    public delegate void FileFailureHandler(object sender, ScanFailureEventArgs e);

    #endregion Delegates

    /// <summary>
    /// FileSystemScanner provides facilities scanning of files and directories.
    /// </summary>
    public class FileSystemScanner
    {
        #region Constructors

        /// <summary>
        /// Initialise a new instance of <see cref="FileSystemScanner"></see>
        /// </summary>
        /// <param name="filter">The <see cref="PathFilter">file filter</see> to apply when scanning.</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public FileSystemScanner(string filter)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            fileFilter_ = new PathFilter(filter);
        }

        /// <summary>
        /// Initialise a new instance of <see cref="FileSystemScanner"></see>
        /// </summary>
        /// <param name="fileFilter">The <see cref="PathFilter">file filter</see> to apply.</param>
        /// <param name="directoryFilter">The <see cref="PathFilter"> directory filter</see> to apply.</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public FileSystemScanner(string fileFilter, string directoryFilter)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            fileFilter_ = new PathFilter(fileFilter);
            directoryFilter_ = new PathFilter(directoryFilter);
        }

        /// <summary>
        /// Initialise a new instance of <see cref="FileSystemScanner"></see>
        /// </summary>
        /// <param name="fileFilter">The file <see cref="IScanFilter">filter</see> to apply.</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public FileSystemScanner(IScanFilter fileFilter)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            fileFilter_ = fileFilter;
        }

        /// <summary>
        /// Initialise a new instance of <see cref="FileSystemScanner"></see>
        /// </summary>
        /// <param name="fileFilter">The file <see cref="IScanFilter">filter</see>  to apply.</param>
        /// <param name="directoryFilter">The directory <see cref="IScanFilter">filter</see>  to apply.</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public FileSystemScanner(IScanFilter fileFilter, IScanFilter directoryFilter)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            fileFilter_ = fileFilter;
            directoryFilter_ = directoryFilter;
        }

        #endregion Constructors

        #region Delegates

        /// <summary>
        /// Delegate to invoke when a directory is processed.
        /// </summary>
        public event EventHandler<DirectoryEventArgs> ProcessDirectory;

        /// <summary>
        /// Delegate to invoke when a file is processed.
        /// </summary>
        public ProcessFileHandler ProcessFile;

        /// <summary>
        /// Delegate to invoke when processing for a file has finished.
        /// </summary>
        public CompletedFileHandler CompletedFile;

        /// <summary>
        /// Delegate to invoke when a directory failure is detected.
        /// </summary>
        public DirectoryFailureHandler DirectoryFailure;

        /// <summary>
        /// Delegate to invoke when a file failure is detected.
        /// </summary>
        public FileFailureHandler FileFailure;

        #endregion Delegates

        /// <summary>
        /// Raise the DirectoryFailure event.
        /// </summary>
        /// <param name="directory">The directory name.</param>
        /// <param name="e">The exception detected.</param>
        private bool OnDirectoryFailure(string directory, Exception e)
        {
            DirectoryFailureHandler handler = DirectoryFailure;
            bool result = handler != null;
            if (result)
            {
                var args = new ScanFailureEventArgs(directory, e);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                handler(this, args);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                alive_ = args.ContinueRunning;
            }
            return result;
        }

        /// <summary>
        /// Raise the FileFailure event.
        /// </summary>
        /// <param name="file">The file name.</param>
        /// <param name="e">The exception detected.</param>
        private bool OnFileFailure(string file, Exception e)
        {
            FileFailureHandler handler = FileFailure;

            bool result = handler != null;

            if (result)
            {
                var args = new ScanFailureEventArgs(file, e);
                FileFailure(this, args);
                alive_ = args.ContinueRunning;
            }
            return result;
        }

        /// <summary>
        /// Raise the ProcessFile event.
        /// </summary>
        /// <param name="file">The file name.</param>
        private void OnProcessFile(string file)
        {
            ProcessFileHandler handler = ProcessFile;

            if (handler != null)
            {
                var args = new ScanEventArgs(file);
                handler(this, args);
                alive_ = args.ContinueRunning;
            }
        }

        /// <summary>
        /// Raise the complete file event
        /// </summary>
        /// <param name="file">The file name</param>
        private void OnCompleteFile(string file)
        {
            CompletedFileHandler handler = CompletedFile;

            if (handler != null)
            {
                var args = new ScanEventArgs(file);
                handler(this, args);
                alive_ = args.ContinueRunning;
            }
        }

        /// <summary>
        /// Raise the ProcessDirectory event.
        /// </summary>
        /// <param name="directory">The directory name.</param>
        /// <param name="hasMatchingFiles">Flag indicating if the directory has matching files.</param>
        private void OnProcessDirectory(string directory, bool hasMatchingFiles)
        {
            EventHandler<DirectoryEventArgs> handler = ProcessDirectory;

            if (handler != null)
            {
                var args = new DirectoryEventArgs(directory, hasMatchingFiles);
                handler(this, args);
                alive_ = args.ContinueRunning;
            }
        }

        /// <summary>
        /// Scan a directory.
        /// </summary>
        /// <param name="directory">The base directory to scan.</param>
        /// <param name="recurse">True to recurse subdirectories, false to scan a single directory.</param>
        public void Scan(string directory, bool recurse)
        {
            alive_ = true;
            ScanDir(directory, recurse);
        }

        private void ScanDir(string directory, bool recurse)
        {
            try
            {
                string[] names = Directory.GetFiles(directory);
                bool hasMatch = false;
                for (int fileIndex = 0; fileIndex < names.Length; ++fileIndex)
                {
                    if (!fileFilter_.IsMatch(names[fileIndex]))
                    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                        names[fileIndex] = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                    }
                    else
                    {
                        hasMatch = true;
                    }
                }

                OnProcessDirectory(directory, hasMatch);

                if (alive_ && hasMatch)
                {
                    foreach (string fileName in names)
                    {
                        try
                        {
                            if (fileName != null)
                            {
                                OnProcessFile(fileName);
                                if (!alive_)
                                {
                                    break;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            if (!OnFileFailure(fileName, e))
                            {
                                throw;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (!OnDirectoryFailure(directory, e))
                {
                    throw;
                }
            }

            if (alive_ && recurse)
            {
                try
                {
                    string[] names = Directory.GetDirectories(directory);
                    foreach (string fulldir in names)
                    {
                        if (directoryFilter_ == null || directoryFilter_.IsMatch(fulldir))
                        {
                            ScanDir(fulldir, true);
                            if (!alive_)
                            {
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (!OnDirectoryFailure(directory, e))
                    {
                        throw;
                    }
                }
            }
        }

        #region Instance Fields

        /// <summary>
        /// The file filter currently in use.
        /// </summary>
#pragma warning disable IDE0044 // Add readonly modifier
        private IScanFilter fileFilter_;
#pragma warning restore IDE0044 // Add readonly modifier

        /// <summary>
        /// The directory filter currently in use.
        /// </summary>
#pragma warning disable IDE0044 // Add readonly modifier
        private IScanFilter directoryFilter_;
#pragma warning restore IDE0044 // Add readonly modifier

        /// <summary>
        /// Flag indicating if scanning should continue running.
        /// </summary>
        private bool alive_;

        #endregion Instance Fields
    }
}
