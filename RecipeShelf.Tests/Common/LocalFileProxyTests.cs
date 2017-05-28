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
        public async Task CanConnectAsync()
        {
            Directory.Delete(_localFileProxyFolder, true);
            Assert.False(await _localFileProxy.CanConnectAsync());

            Directory.CreateDirectory(_localFileProxyFolder);
            Assert.True(await _localFileProxy.CanConnectAsync());
        }

        [Fact]
        public async Task DeleteAsync()
        {
            await _localFileProxy.DeleteAsync(_folder + "\\" + _file1);
            Assert.False(File.Exists(_file1Path));
            Assert.True(File.Exists(_file2Path));
        }

        [Fact]
        public async Task GetTextAsync()
        {
            Assert.Equal("TestData1", await _localFileProxy.GetTextAsync(_folder + "\\File1.txt"));
        }

        [Fact]
        public async Task ListKeysAsync()
        {
            var keys = await _localFileProxy.ListKeysAsync(_folder);
            Assert.Contains(_file1Path, keys);
            Assert.Contains(_file2Path, keys);
        }

        [Fact]
        public async Task PutTextAsync()
        {
            File.Delete(_file1Path);
            await _localFileProxy.PutTextAsync(_folder + "\\File1.txt", "TestData1");
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
