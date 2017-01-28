# app
Recipe Shelf makes it easy to find the recipes you want

Recipe Shelf is hosted on netlify using gohugo to compile the site. An AWS Ec2 server is used to push markdown to the repository connected to netlify. Data is stored in S3 buckets and DynamoDB, images in cloudinary and a Redis cache is used to filter recipes.

The following settings are needed to run everything:
  - CacheEndpoint - localhost:6379
  - MarkdownFolder - C:\Src\site\site\content\recipe
  - MarkdownRoot - C:\Src\site
  - S3FileProxyBucket - recipeshelf
  - LocalFileProxyFolder - C:\Users\prasad\Dropbox\recipeshelf\Data
  - UseLocalDynamoDB - True
  - FileProxyType - Local
  - LogLevel - Debug
  - CommitAndPush - True
