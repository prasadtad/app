using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RecipeShelf.Common.Proxies
{
    public sealed class S3FileProxy : IFileProxy, IDisposable
    {
        private TransferUtility _transferUtility = new TransferUtility();

        private readonly ILogger<S3FileProxy> _logger;
        private readonly CommonSettings _settings;

        public S3FileProxy(ILogger<S3FileProxy> logger, IOptions<CommonSettings> optionsAccessor)
        {
            _logger = logger;
            _settings = optionsAccessor.Value;
        }

        public Task<bool> CanConnectAsync()
        {
            _logger.LogDebug("Checking if {S3Bucket} exists", _settings.S3FileProxyBucket);
            return _transferUtility.S3Client.DoesS3BucketExistAsync(_settings.S3FileProxyBucket);
        }

        public async Task<IEnumerable<string>> ListKeysAsync(string folder)
        {
            _logger.LogDebug("Listing keys in {Folder}", folder);
            var allKeys = new List<string>();
            ListObjectsV2Request request = new ListObjectsV2Request
            {
                BucketName = _settings.S3FileProxyBucket,
                MaxKeys = 100,
                Prefix = folder + "/"
            };
            ListObjectsV2Response response;
            do
            {
                response = await _transferUtility.S3Client.ListObjectsV2Async(request);

                // Process response.
                foreach (S3Object entry in response.S3Objects)
                {
                    if (entry.Key.Equals(folder + "/")) continue;
                    allKeys.Add(entry.Key);
                }

                request.ContinuationToken = response.NextContinuationToken;
            } while (response.IsTruncated == true);
            return allKeys;
        }

        public async Task<string> GetTextAsync(string key)
        {
            _logger.LogDebug("Reading {Key} as text", key);
            var request = new GetObjectRequest
            {
                BucketName = _settings.S3FileProxyBucket,
                Key = key
            };
            using (GetObjectResponse response = await _transferUtility.S3Client.GetObjectAsync(request))
            using (Stream responseStream = response.ResponseStream)
            using (StreamReader reader = new StreamReader(responseStream))
                return reader.ReadToEnd();
        }

        public async Task PutTextAsync(string key, string text)
        {
            _logger.LogDebug("Putting text at {Key}", key);
            var request = new PutObjectRequest
            {
                BucketName = _settings.S3FileProxyBucket,
                Key = key,
                ContentBody = text
            };
            await _transferUtility.S3Client.PutObjectAsync(request);
        }

        public void Dispose()
        {
            if (_transferUtility != null)
            {
                _transferUtility.Dispose();
                _transferUtility = null;
            }
        }
    }
}
