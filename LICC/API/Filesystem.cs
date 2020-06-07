using System.IO;
using System.Text;

namespace LICC.API
{
    /// <summary>
    /// Represents a virtual file system.
    /// </summary>
    public interface IFileSystem
    {
        bool FileExists(string path);
        StreamReader OpenRead(string path);
    }

    /// <summary>
    /// File system with System.IO as backend.
    /// </summary>
    public class SystemIOFilesystem : IFileSystem
    {
        private readonly string RootPath;

        public SystemIOFilesystem(string rootPath)
        {
            this.RootPath = Path.GetFullPath(rootPath);
        }

        private string FilePath(string path) => Path.Combine(RootPath, path);

        public bool FileExists(string path) => File.Exists(FilePath(path));

        public StreamReader OpenRead(string path)
            => new StreamReader(File.OpenRead(FilePath(path)), Encoding.UTF8, true, 4096, false);
    }
}
