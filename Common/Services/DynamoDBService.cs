using Amazon.DynamoDBv2;
using Amazon;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;
using System.Text.Json;

namespace Common.Services
{
    public  class DynamoDBService
    {
        private static readonly AmazonDynamoDBClient DBClient = new(new AmazonDynamoDBConfig()
        {
            RegionEndpoint = RegionEndpoint.SAEast1,
            Profile = new Profile("BrowserExtensionAnalysis")
        });
        public static void SaveItemToDB(string tableName, object entity)
        {
            var json = JsonSerializer.SerializeToDocument(entity).RootElement.ToString();

            var table = Table.LoadTable(DBClient, tableName);
            table.PutItemAsync(Document.FromJson(json)).Wait();
        }

        public static T? GetItemFromDB<T>(string tableName, string key)
        {
            var table = Table.LoadTable(DBClient, tableName);
            var doc = table.GetItemAsync(key).Result;
            var entityJson = doc.ToJson();

            return JsonSerializer.Deserialize<T>(entityJson);
        }
        public static List<T> GetEntriesFromDB<T>(string tableName, int quantity, int offset)
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
