using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using System.Text.Json;

namespace Common.Handlers
{
    public class DynamoDBHandler
    {
        private static readonly string? isLambda = Environment.GetEnvironmentVariable("LAMBDA_TASK_ROOT");

        private static AmazonDynamoDBClient GetClient()
        {
            if(isLambda is null)
            {
                return new AmazonDynamoDBClient(new AmazonDynamoDBConfig
                {
                    RegionEndpoint = RegionEndpoint.SAEast1,
                    Profile = new Profile("BrowserExtensionAnalysis")
                });
            }
            else
            {
                return new AmazonDynamoDBClient();
            }
        }

        private static readonly AmazonDynamoDBClient DBClient = GetClient();
        public static void UpdateEntry(string tableName, object entity)
        {
            var json = JsonSerializer.SerializeToDocument(entity).RootElement.ToString();

            var table = Table.LoadTable(DBClient, tableName);
            table.PutItemAsync(Document.FromJson(json)).Wait();
        }

        public static T GetEntry<T>(string tableName, string key) where T : new()
        {
            try
            {
                var table = Table.LoadTable(DBClient, tableName);
                var doc = table.GetItemAsync(key).Result;

                if(doc != null)
                {
                    var entityJson = doc.ToJson();
                    return JsonSerializer.Deserialize<T>(entityJson) ?? new();
                }
                else
                {
                    return new();
                }
            }
            catch (Exception ex) 
            {
                var log = string.Format("{0} - id = {1}: {2}", tableName, key, ex.Message);
                Console.WriteLine(log);
                return new();
            }
        }
        public static List<T> GetEntries<T>(string tableName, int quantity, int offset)
        {
            var list = new List<T>();

            var table = Table.LoadTable(DBClient, tableName);

            var scan = table.Scan(new ScanFilter());
            var results = scan.GetNextSetAsync().Result;

            foreach (var entry in results) 
            {
                var jsonEntry = entry.ToJson();
                var item = JsonSerializer.Deserialize<T>(jsonEntry);

                if(item != null)
                {
                    list.Add(item);
                }
            }
            return list;
        }
    }
}
