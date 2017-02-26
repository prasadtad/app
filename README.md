# app
Recipe Shelf is hosted on netlify using gohugo to compile the site. An AWS Ec2 server is used to push markdown to the repository connected to netlify, update recipes data and cache. Data is stored in S3 buckets and DynamoDB, images in cloudinary and a Redis cache is used to filter recipes.
Use [ElasticMq](https://github.com/adamw/elasticmq) and [Local DynamoDB](http://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.html) for the local environment

The following environment variables are needed to run everything:
  -  MARKDOWN_FOLDER = C:\\Src\\site\\site\\content\\recipe
  -  FILE_PROXY_TYPE = Local
  -  SQS_URL_PREFIX = http://localhost:9324/queue/
  -  USE_LOCAL_QUEUE = True
  -  LOG_LEVEL = Debug
  -  LOCAL_FILE_PROXY_FOLDER = C:\\Users\\prasad\\Dropbox\\recipeshelf\\Data
  -  CACHE_ENDPOINT = localhost:6379
  -  COMMIT_AND_PUSH = True
  -  USE_LOCAL_DYNAMODB = True
  -  MARKDOWN_ROOT = C:\\Src\\site
  -  S3_FILE_PROXY_BUCKET = recipeshelf
