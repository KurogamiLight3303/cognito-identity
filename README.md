# AWS Cognito Identity POC 

## Software Requirements
Programming languages: C#
Framework: ASP.NET / ASP.NET Core
Database: MSSQL
## Description
This is a POC project for handling basic user identity operation using AWS Cognito. 

### To deploy using AWS CodePipeline configure the following CodeBuild variables:
* ACCOUNT -> AWS Account ID
* REGION -> AWS Region
* REPOSITORY -> ECR Docker repository
* FUNCTION -> AWS Lambda Function
If the deploy target is not a Lambda function you can remove de *POST_BUILD* phase in the buildspec.yml file

### The Code use 3 main configurations variables
* APP_SECRETS_ID -> For getting the Secrets from AWS
* APP_KEYS_ID -> For the internal key storages of the WebServer App
* APP_LOGGING_GROUP -> For the CloudWatch Logging process
