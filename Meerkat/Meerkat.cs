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

        internal static IMongoCollection<TSchema> GetCollectionForType<TSchema>(TSchema model) where TSchema : Schema
        {
            if (_database == null)
                throw new InvalidOperationException(
                    $"The database connection has not been initialized. Call {nameof(Connect)}() before carrying out any operations.");
            
            var collectionName = model.GetType().GetCollectionName().ToLowerInvariant();
            return _database.GetCollection<TSchema>(collectionName);
        }

        internal static IMongoCollection<TSchema> GetCollectionForType<TSchema>() where TSchema : Schema
        {
            if (_database == null)
                throw new InvalidOperationException(
                    $"The database connection has not been initialized. Call {nameof(Connect)}() before carrying out any operations.");
            
            var collectionName = typeof(TSchema).GetCollectionName().ToLowerInvariant();
            return _database.GetCollection<TSchema>(collectionName);
        }
    }
}