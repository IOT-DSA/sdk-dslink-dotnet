using DSLink.Platform;
using Moq;
using NUnit.Framework;
using PCLStorage;

namespace DSLink.Tests
{
    [SetUpFixture]
    public class RootTestsSetUp
    {
        [OneTimeSetUp]
        public void SetUpPlatform()
        {
            BasePlatform.SetPlatform(new TestPlatform(new Mock<IFolder>()));
        }
    }
}
