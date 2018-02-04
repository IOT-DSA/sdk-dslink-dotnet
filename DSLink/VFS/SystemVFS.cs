using StandardStorage;
using System.IO;
using System.Threading.Tasks;

namespace DSLink.VFS
{
    public class SystemVFS : IVFS
    {
        public SystemVFS(string basePath) : base(basePath)
        {
        }

        public override async Task<bool> ExistsAsync(string fileName)
        {
            var folder = await FileSystem.Current.GetFolderFromPathAsync(BasePath);
            return await folder.CheckExistsAsync(fileName) == ExistenceCheckResult.FileExists;
        }

        private async Task<IFile> _getFile(string fileName)
        {
            var folder = await FileSystem.Current.GetFolderFromPathAsync(BasePath);
            return await folder.GetFileAsync(fileName);
        }

        public override async Task CreateAsync(string fileName, bool replaceExisting)
        {
            var folder = await FileSystem.Current.GetFolderFromPathAsync(BasePath);
            var mode = replaceExisting ? CreationCollisionOption.ReplaceExisting : CreationCollisionOption.FailIfExists;
            var file = await folder.CreateFileAsync(fileName, mode);
        }

        public override async Task<Stream> WriteAsync(string fileName)
        {
            var file = await _getFile(fileName);
            return await file.OpenAsync(StandardStorage.FileAccess.ReadAndWrite);
        }

        public override async Task<Stream> ReadAsync(string fileName)
        {
            var file = await _getFile(fileName);
            return await file.OpenAsync(StandardStorage.FileAccess.Read);
        }
    }
}
