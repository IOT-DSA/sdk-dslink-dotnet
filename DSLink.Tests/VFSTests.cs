using NUnit.Framework;
using System;
using System.IO;
using DSLink.VFS;
using System.Threading.Tasks;

namespace DSLink.Tests
{
    [TestFixture]
    public class VFSTests
    {
        public string rootDir;

        [SetUp]
        public void SetUp()
        {
            rootDir = Utilities.CreateTempDirectory();
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(rootDir, true);
        }

        [Test]
        public async Task FileExists([Values(typeof(SystemVFS))] Type vfsType)
        {
            var file = Path.Combine(rootDir, "fileExistsTest");
            var vfs = (IVFS)Activator.CreateInstance(vfsType);
            using (File.Create(file))
            {
                Assert.IsTrue(await vfs.Exists(rootDir, file));
            }
        }

        [Test]
        public async Task FileDoesNotExist([Values(typeof(SystemVFS))] Type vfsType)
        {
            var file = Path.Combine(rootDir, "fileDoesNotExistTest");
            var vfs = (IVFS)Activator.CreateInstance(vfsType);
            Assert.IsFalse(await vfs.Exists(rootDir, file));
        }
    }
}
