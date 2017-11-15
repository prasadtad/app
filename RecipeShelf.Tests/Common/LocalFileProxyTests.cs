using RecipeShelf.Common;
using RecipeShelf.Common.Proxies;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace RecipeShelf.Tests.Common
{
    public class LocalFileProxyTests : IDisposable
    {
        private readonly string _localFileProxyFolder = Path.Combine(Directory.GetCurrentDirectory(), "FileProxyTestData");

        private const string _folder = "Folder", _file1 = "File1.txt", _file2 = "File2.txt";
        private readonly string _folderPath, _file1Path, _file2Path;

        private readonly LocalFileProxy _localFileProxy;

        public LocalFileProxyTests()
        {
            _folderPath = Path.Combine(_localFileProxyFolder, _folder);
            _file1Path = Path.Combine(_folderPath, _file1);
            _file2Path = Path.Combine(_folderPath, _file2);

            _localFileProxy = new LocalFileProxy(new MockLogger<LocalFileProxy>(),
                                     new MockOptions<CommonSettings>(
                                         new CommonSettings
                                         {
                                             FileProxyType = FileProxyTypes.Local,
                                             LocalFileProxyFolder = _localFileProxyFolder
                                         }));
            if (!Directory.Exists(_localFileProxyFolder)) Directory.CreateDirectory(_localFileProxyFolder);
            if (!Directory.Exists(_folderPath)) Directory.CreateDirectory(_folderPath);
            if (!File.Exists(_file1Path)) File.WriteAllText(_file1Path, "TestData1");
            if (!File.Exists(_file2Path)) File.WriteAllText(_file2Path, "TestData2");
        }

        [Fact]
        public async Task CanConnectTestAsync()
        {
            Directory.Delete(_localFileProxyFolder, true);
            Assert.False(await _localFileProxy.CanConnectAsync());

            Directory.CreateDirectory(_localFileProxyFolder);
            Assert.True(await _localFileProxy.CanConnectAsync());
        }

        [Fact]
        public async Task DeleteTestAsync()
        {
            await _localFileProxy.DeleteAsync(Path.Combine(_folder, _file1));
            Assert.False(File.Exists(_file1Path));
            Assert.True(File.Exists(_file2Path));
        }

        [Fact]
        public async Task GetTextTestAsync()
        {
            var filename = Path.Combine(_folder, "File1.txt");
            var fileText = await _localFileProxy.GetTextAsync(filename);
            Assert.Equal("TestData1", fileText.Text);
            var fileText2 = await _localFileProxy.GetTextAsync(filename, fileText.LastModified);
            Assert.Null(fileText2.Text);
            Assert.Equal(fileText.LastModified, fileText2.LastModified);
            await File.WriteAllTextAsync(_file1Path, "TestData1x");
            var fileText3 = await _localFileProxy.GetTextAsync(filename, fileText2.LastModified);
            Assert.NotNull(fileText3.Text);
            Assert.Equal("TestData1x", fileText3.Text);
            Assert.NotEqual(fileText2.LastModified, fileText3.LastModified);
            await File.WriteAllTextAsync(_file1Path, "TestData1");
        }

        [Fact]
        public async Task ListKeysTestAsync()
        {
            var keys = await _localFileProxy.ListKeysAsync(_folder);
            Assert.Contains(_file1Path, keys);
            Assert.Contains(_file2Path, keys);
        }

        [Fact]
        public async Task PutTextTestAsync()
        {
            File.Delete(_file1Path);
            await _localFileProxy.PutTextAsync(Path.Combine(_folder, "File1.txt"), "TestData1");
            Assert.True(File.Exists(_file1Path));
            Assert.Equal("TestData1", File.ReadAllText(_file1Path));
        }

        public void Dispose()
        {
            if (Directory.Exists(_localFileProxyFolder))
                Directory.Delete(_localFileProxyFolder, true);
        }
    }
}
