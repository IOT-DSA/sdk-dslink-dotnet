using System.IO;
using PCLStorage;

namespace DSLink.Tests
{
    public static class Utilities
    {
        public static string CreateTempDirectory()
        {
            string tempDirectory = PortablePath.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }
    }
}
