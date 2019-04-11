using System.IO;
using System.Threading.Tasks;

namespace DSLink.VFS
{
    public abstract class IVFS
    {
        protected string BasePath;

        public IVFS(string basePath)
        {
            BasePath = basePath;
        }

        public abstract Task<bool> ExistsAsync(string fileName);
        public abstract Task CreateAsync(string fileName, bool replaceExisting);
        public abstract Task<Stream> WriteAsync(string fileName);
        public abstract Task<Stream> ReadAsync(string fileName);
    }
}