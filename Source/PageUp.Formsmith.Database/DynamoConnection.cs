using Amazon.DynamoDBv2;
using Amazon.Runtime;
using System.Configuration;

namespace PageUp.Formsmith.Database
{
    public class DynamoConnection
    {
        public static AmazonDynamoDBClient CreateConnection(string serviceUrl = null, string accessKey = null, string secretKey = null)
        {
            var AccessKey = accessKey ?? ConfigurationManager.AppSettings["AWSTempAccessKey"];
            var ServiceUrl = serviceUrl ?? ConfigurationManager.AppSettings["AWSServiceURL"];
            var SecretKey = secretKey ?? ConfigurationManager.AppSettings["AWSTempSecretKey"];

            var credentials = new BasicAWSCredentials(AccessKey, SecretKey);
            var config = new AmazonDynamoDBConfig
            {

                ServiceURL = ServiceUrl
            };

            return new AmazonDynamoDBClient(credentials, config);
        }
    }
}