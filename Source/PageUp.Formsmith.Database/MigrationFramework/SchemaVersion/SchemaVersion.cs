using System;
using System.Collections.Generic;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

namespace PageUp.Formsmith.Database.MigrationFramework.SchemaVersion
{
    public class SchemaVersion
    {
        private string SchemaVersionTableName = AWSConfigs.DynamoDBConfig.Context.TableNamePrefix + "SchemaVersion";
        private AmazonDynamoDBClient _client;
        private DynamoDBContext _dbContext;

        public SchemaVersion(AmazonDynamoDBClient client)
        {
            _client = client;
            _dbContext = new DynamoDBContext(client);
        }

        public void Initialise()
        {
            CreateTable();
        }

        public bool IsScriptApplied(string scriptName)
        {
            var schemaVersionTable = Table.LoadTable(_client, SchemaVersionTableName);

            Document itemGet = schemaVersionTable.GetItem(scriptName);

            var script = _dbContext.FromDocument<SchemaVersionDataModel>(itemGet);

            return script != null;
        }

        private void CreateTable()
        {
            List<string> currentTables = _client.ListTables().TableNames;

            if (!currentTables.Contains(SchemaVersionTableName))
            {
                #region Form table creation

                var schema = new List<KeySchemaElement>
                                 {
                                     new KeySchemaElement {AttributeName = "ScriptName", KeyType = "HASH"}
                                 };

                // The key attributes "FormId" is a string type and "InstanceId" is numeric type
                var definitions = new List<AttributeDefinition>
                                      {
                                          new AttributeDefinition { AttributeName = "ScriptName", AttributeType = "S" }
                                      };

                var throughput = new ProvisionedThroughput { ReadCapacityUnits = 1, WriteCapacityUnits = 1 };

                // Configure the CreateTable request
                var request = new CreateTableRequest
                {
                    TableName = SchemaVersionTableName,
                    KeySchema = schema,
                    ProvisionedThroughput = throughput,
                    AttributeDefinitions = definitions
                };

                CreateTableResponse response = _client.CreateTable(request);
                string currentStatus = response.TableDescription.TableStatus;
                WaitUntilTableReady(SchemaVersionTableName, currentStatus);

                #endregion
            }
        }

        private void WaitUntilTableReady(string tableName, string currentStatus)
        {
            if (!currentStatus.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase))
            {
                string status = null;
                // Let us wait until table is created. Call DescribeTable.
                do
                {
                    System.Threading.Thread.Sleep(3000); // Wait 3 seconds.
                    try
                    {
                        var res = _client.DescribeTable(new DescribeTableRequest { TableName = tableName });

                        Console.WriteLine("Table name: {0}, status: {1}",
                          res.Table.TableName,
                          res.Table.TableStatus);
                        status = res.Table.TableStatus;
                    }
                    catch (ResourceNotFoundException)
                    {
                        // Table is eventually consistent. So you might  get resource not found. So we handle the potential exception.
                    }
                } while (status != "ACTIVE");
            }
        }

        public void SetScriptAsApplied(string scriptName)
        {
            var data = new SchemaVersionDataModel
            {
                ScriptName = scriptName,
                Applied = DateTime.Now.ToUniversalTime().ToString()
            };
            _dbContext.Save(data);
        }
    }
}