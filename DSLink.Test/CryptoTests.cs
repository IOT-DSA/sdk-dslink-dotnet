using NUnit.Framework;
using StandardStorage;
using System;
using System.Threading.Tasks;
using DSLink.Connection;

namespace DSLink.Test
{
    [TestFixture]
    public class CryptoTests
    {
        private const string KnownSavedKeyPair =
            "AIjwCSLDZGF5G7BoncWBsBzIrAGhNNC+OJMbVm4Xirau " +
            "BGSjQgTqbifSDbVIgOyh8T6cI+GJ0WwCksKK+l3sWF0THm5QpeCfzmj8iYmdgC185c30zAdHg8DB5+Pf/fU6QV4=";
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

        [Test]
        public void Generate()
        {
            var keyPair = new KeyPair();
            keyPair.Generate();
            
            Assert.NotNull(keyPair.EncodedPublicKey);
            Assert.NotNull(keyPair.Save());
        }

        [Test]
        public void GenerateSaveAndLoad()
        {
            var keyPair = new KeyPair();
            keyPair.Generate();

            var savedKeyPair = keyPair.Save();
            var secondKeyPair = new KeyPair();
            secondKeyPair.LoadFrom(savedKeyPair);

            Assert.AreEqual(keyPair.EncodedPublicKey, secondKeyPair.EncodedPublicKey);
            Assert.AreEqual(keyPair.Save(), secondKeyPair.Save());
        }

        [Test]
        public void LoadKnownKeyPair()
        {
            var keyPair = new KeyPair();
            keyPair.LoadFrom(KnownSavedKeyPair);
            
            Assert.AreEqual(KnownSavedKeyPair, keyPair.Save());
        }
    }
}
