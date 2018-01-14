using System.Threading.Tasks;

namespace DSLink.VFS
{
    public interface IVFS
    {
        Task<bool> Exists(string folderPath, string filePath);
        Task WriteString(string folderPath, string filePath);
    }
}
