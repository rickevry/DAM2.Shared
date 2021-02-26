using MongoDB.Driver;
using Proto.Cluster.Identity;
using Proto.Cluster.Identity.MongoDb;

namespace DAM2.Core.Shared
{
    public class MongoIdentityLookup
    {
        public static IIdentityLookup GetIdentityLookup(string clusterName, string connectionString, string pidCollection, string pidDatabaseName)
        {
            var db = GetMongo(connectionString, pidDatabaseName);
            var identity = new IdentityStorageLookup(
                new MongoIdentityStorage(clusterName, db.GetCollection<PidLookupEntity>(pidCollection))
            );
            return identity;
        }

        private static IMongoDatabase GetMongo(string connectionString, string databaseName)
        {
            var url = MongoUrl.Create(connectionString);
            var settings = MongoClientSettings.FromUrl(url);
            var client = new MongoClient(settings);
            var database = client.GetDatabase(databaseName);
            return database;
        }
    }
}
