using DSLink.Platform;
using Moq;
using StandardStorage;

namespace DSLink.Tests
{
    public class TestPlatform : BasePlatform
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
