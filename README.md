# app
Recipe Shelf is hosted as a Redis application. An AWS Ec2 server is used to save recipe files and it's filtering and searching data to a redis cache. Data is stored in S3 buckets, images in cloudinary.
Use [Redis](https://redis.io/download) for development.

The following environment variables are needed to run RecipeShelf:
  -  LogLevel = Debug
  -  FileProxyType = Local
  -  LocalFileProxyFolder = C:\\Users\\prasad\\Dropbox\\recipeshelf\\Data
  -  S3FileProxyBucket = recipeshelf
  -  IngredientsCacheExpiration = 00:20:00
  -  CacheEndpoint = localhost:6379