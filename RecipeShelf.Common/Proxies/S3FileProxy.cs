using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RecipeShelf.Common.Proxies
{
    public sealed class S3FileProxy : IFileProxy, IDisposable
    {
        private TransferUtility _transferUtility = new TransferUtility();

        private readonly Logger<S3FileProxy> _logger = new Logger<S3FileProxy>();
        
        public async Task<IEnumerable<string>> ListKeysAsync(string folder)
        {
            _logger.Debug("ListKeys", $"Listing keys for {folder}");
            var allKeys = new List<string>();
            try
            {
                ListObjectsV2Request request = new ListObjectsV2Request
                {
                    BucketName = Settings.S3FileProxyBucket,
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
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    _logger.Error("ListKeys", "Check the provided AWS Credentials.");
                    _logger.Error("ListKeys", "To sign up for service, go to http://aws.amazon.com/s3");
                }
                else
                    _logger.Error("ListKeys", $"Error occurred. Message:'{amazonS3Exception.Message}' when listing objects");
                throw;
            }
            return allKeys;
        }

        public async Task<string> GetTextAsync(string key)
        {
            _logger.Debug("GetText", $"Getting {key} as text");
            var request = new GetObjectRequest
            {
                BucketName = Settings.S3FileProxyBucket,
                Key = key
            };
            using (GetObjectResponse response = await _transferUtility.S3Client.GetObjectAsync(request))
            using (Stream responseStream = response.ResponseStream)
            using (StreamReader reader = new StreamReader(responseStream))
                return reader.ReadToEnd();
        }

        public async Task PutTextAsync(string key, string text)
        {
            _logger.Debug("PutText", $"Putting text at {key}");
            var request = new PutObjectRequest
            {
                BucketName = Settings.S3FileProxyBucket,
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
