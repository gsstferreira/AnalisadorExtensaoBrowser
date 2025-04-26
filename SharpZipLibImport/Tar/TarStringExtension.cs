using SharpZipLib.Core;

namespace SharpZipLib.Tar
{
    internal static class TarStringExtension
    {
        public static string ToTarArchivePath(this string s)
        {
            return PathUtils.DropPathRoot(s).Replace(Path.DirectorySeparatorChar, '/');
        }
    }
}
