using Amazon.DynamoDBv2.DataModel;

namespace PageUp.Formsmith.Database.MigrationFramework.SchemaVersion
{
    [DynamoDBTable("SchemaVersion")]
    public class SchemaVersionDataModel
    {
        [DynamoDBHashKey]
        public string ScriptName { get; set; }

        [DynamoDBProperty]
        public string Applied { get; set; }
    }
}