using System;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace PageUp.Formsmith.Database.Helpers
{
    public static class AmazonDynamoDbClientExtensions
    {
        public static void WaitUntilTableReady(this AmazonDynamoDBClient client, string tableName, string currentStatus = null)
        {
            int attempts = 0;
            if (!string.IsNullOrEmpty(currentStatus) && !currentStatus.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase))
            {
                string status = null;
                // Let us wait until table is created. Call DescribeTable.
                do
                {
                    if (attempts > 10)
                        throw new TimeoutException("Waited for 30 seconds for table " + tableName + " to be created.");

                    System.Threading.Thread.Sleep(3000); // Wait 3 seconds.
                    try
                    {
                        attempts++;
                        var res = client.DescribeTable(new DescribeTableRequest { TableName = tableName });

                        Console.WriteLine("Table name: {0}, status: {1}",
                          res.Table.TableName,
                          res.Table.TableStatus);
                        status = res.Table.TableStatus;
                    }
                    catch (ResourceNotFoundException)
                    {
                        // Table is eventually consistent. So you might get resource not found. So we handle the potential exception.
                    }
                } while (status != "ACTIVE");
            }
        }

        public static void WaitUntilTableDeleted(this AmazonDynamoDBClient client, string tableName)
        {
            int attempts = 0;
            string status = null;
            // Let us wait until table is deleted. Call DescribeTable.
            do
            {
                if (attempts > 10)
                    throw new TimeoutException("Waited for 30 seconds for table " + tableName + " to be deleted.");

                attempts++;
                System.Threading.Thread.Sleep(3000); // Wait 5 seconds.
                try
                {
                    var res = client.DescribeTable(new DescribeTableRequest
                    {
                        TableName = tableName
                    });

                    status = res.Table.TableStatus;
                }
                catch (ResourceNotFoundException)
                {
                    // Table not found. It is deleted
                    return;
                }
            } while (status == "DELETING");
        }
    }
}