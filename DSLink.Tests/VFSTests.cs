using NUnit.Framework;
using System;
using System.Threading.Tasks;
using DSLink.VFS;

namespace DSLink.Tests
{
    [TestFixture]
    public class VFSTests
    {
        [Test]
        public async Task SaveFile([Values(typeof(SystemVFS))] Type vfsType)
        {
            var vfs = (IVFS)Activator.CreateInstance(vfsType);

            Assert.AreEqual("Hello World!", vfs.Test());
        }
    }
}
