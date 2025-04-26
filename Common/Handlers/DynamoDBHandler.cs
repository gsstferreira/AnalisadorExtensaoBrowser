using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime.CredentialManagement;
using Amazon.Util;
using Res;

namespace Common.Handlers
{
    public class DynamoDBHandler
    {
        private static readonly AmazonDynamoDBClient DBClient;
        private static readonly DynamoDBContext Context;
        static DynamoDBHandler()
        {
            if (new SharedCredentialsFile().TryGetProfile(Keys.AWSProfile, out _))
            {
                DBClient = new AmazonDynamoDBClient(new AmazonDynamoDBConfig
                {
                    RegionEndpoint = RegionEndpoint.SAEast1,
                    Profile = new Profile(Keys.AWSProfile)
                });
            }
            else
            {
                DBClient = new AmazonDynamoDBClient();
            }

            Context = new DynamoDBContext(DBClient);
        }
        public static Dictionary<string,AttributeValue> MapAsAttributes<T>(T entity)
        {
            return Context.ToDocument(entity).ToAttributeMap();
        }
        public static void PutEntry<T>(string tableName, T entity)
        {

            var doc = Context.ToDocument(entity);
            var table = Table.LoadTable(DBClient, tableName);

            table.PutItemAsync(doc).Wait();
        }
        public static void UpdateEntry(UpdateItemRequest request)
        {
            DBClient.UpdateItemAsync(request).Wait();
        }
        public static T? GetEntry<T>(string tableName, string key) where T : new()
        {
            try
            {
                var table = Table.LoadTable(DBClient, tableName);
                var doc = table.GetItemAsync(key).Result;

                if(doc is not null)
                {
                    return Context.FromDocument<T>(doc) ?? default;
                }
                else
                {
                    return default;
                }
            }
            catch (Exception ex) 
            {
                var log = string.Format("{0} - id = {1}: {2}", tableName, key, ex.Message);
                Console.WriteLine(log);
                return default;
            }
        }
        public static List<T> GetEntries<T>(string tableName)
        {
            var list = new List<T>();

            var table = Table.LoadTable(DBClient, tableName);

            var scan = table.Scan(new ScanFilter());
            var results = scan.GetNextSetAsync().Result;

            foreach (var entry in results) 
            {
                var item = Context.FromDocument<T>(entry) ?? default;

                if(item is not null) list.Add(item);
            }
            return list;
        }

        public static void DeleteEntry(string tableName, string key)
        {
            var table = Table.LoadTable(DBClient, tableName);
            table.DeleteItemAsync(key).Wait();
        }
    }
}
