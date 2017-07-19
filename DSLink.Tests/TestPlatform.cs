using DSLink.NET;
using Moq;
using PCLStorage;

namespace DSLink.Tests
{
    public class TestPlatform : NETPlatform
    {
        private readonly Mock<IFolder> _mockFolder;

        public TestPlatform(Mock<IFolder> mockFolder)
        {
            _mockFolder = mockFolder;
        }

        public override IFolder GetPlatformStorageFolder()
        {
            return _mockFolder.Object;
        }
    }
}
