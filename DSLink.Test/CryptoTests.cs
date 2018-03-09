using DSLink.Connection;
using NUnit.Framework;
using StandardStorage;
using System;
using System.Threading.Tasks;

namespace DSLink.Test
{
    [TestFixture]
    public class CryptoTests
    {
        private IFolder _tempFolder;

        [SetUp]
        public async Task SetUp()
        {
            var tempPath = Utilities.CreateTempDirectory();
            Console.WriteLine(tempPath);
            _tempFolder = await FileSystem.Current.GetFolderFromPathAsync(tempPath);
        }

        [TearDown]
        public async Task TearDown()
        {
            await _tempFolder.DeleteAsync();
        }

        // TODO: Write new crypto tests.
    }
}
