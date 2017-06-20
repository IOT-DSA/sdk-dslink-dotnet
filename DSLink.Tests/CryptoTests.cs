using DSLink.Crypto;
using NUnit.Framework;
using PCLStorage;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DSLink.Tests
{
    [TestFixture]
    public class CryptoTests
    {
        private IFolder _tempFolder;

        [SetUp]
        public async Task SetUp()
        {
            var tempPath = GetTempDirectory();
            Console.WriteLine(tempPath);
            _tempFolder = await FileSystem.Current.GetFolderFromPathAsync(tempPath);
        }

        [TearDown]
        public async Task TearDown()
        {
            await _tempFolder.DeleteAsync();
        }

        [Test]
        public async Task LoadAndSaveKeyPair()
        {
            var keyPair1 = new KeyPair(_tempFolder, "keys.testing");
            await keyPair1.Load();

            var keyPairExists = await _tempFolder.CheckExistsAsync("keys.testing");
            Assert.AreEqual(ExistenceCheckResult.FileExists, keyPairExists);

            var keyPair2 = new KeyPair(_tempFolder, "keys.testing");
            await keyPair2.Load();

            Assert.AreEqual(keyPair1.EncodedPublicKey, keyPair2.EncodedPublicKey);
        }

        public static string GetTempDirectory()
        {
            string tempDirectory = PortablePath.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }
    }
}
