using System;
using Meerkat.Extensions;
using MongoDB.Driver;
using MongoUrlParser;

namespace Meerkat
{
    public class Meerkat
    {
        private static IMongoDatabase _database;

        /// <summary>
        /// Connect to the database
        /// </summary>
        /// <param name="databaseConnectionString">A full mongodb connection string</param>
        public static void Connect(string databaseConnectionString)
        {
            var (dbUrl, dbName) = Parser.Parse(databaseConnectionString);

            var dbClient = new MongoClient(dbUrl);
            _database = dbClient.GetDatabase(dbName);
        }

        internal static IMongoCollection<T> GetCollectionForType<T>(T model) where T : Schema
        {
            if (_database == null)
                throw new InvalidOperationException(
                    $"The database connection has not been initialized. Call {nameof(Connect)}() before carrying out any operations.");
            
            var name = typeof(T).GetName();
            var collectionName = name.Pluralize().ToLowerInvariant();
            return _database.GetCollection<T>(collectionName);
        }
    }
}