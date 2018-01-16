using StandardStorage;
using System.Threading.Tasks;

namespace DSLink.VFS
{
    public class SystemVFS : IVFS
    {
        public async Task<bool> Exists(string folderPath, string filePath)
        {
            var folder = await FileSystem.Current.GetFolderFromPathAsync(folderPath);
            return await folder.CheckExistsAsync(filePath) == ExistenceCheckResult.FileExists;
        }

        public async Task WriteString(string folderPath, string filePath)
        {

        }
    }
}
