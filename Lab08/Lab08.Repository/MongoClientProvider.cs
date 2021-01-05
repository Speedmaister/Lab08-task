using Lab08.Repository.Configuration;
using MongoDB.Driver;
using System.Collections.Generic;

namespace Lab08.Repository
{
    internal class MongoClientProvider
    {
        private readonly MongoDbConfiguration mongoConfiguration;

        private readonly IDictionary<string, IMongoClient> clientsRepository;

        protected internal IMongoClient DefaultClient { get; private set; }

        public MongoClientProvider(MongoDbConfiguration configuration)
        {
            this.mongoConfiguration = configuration;
            this.clientsRepository = new Dictionary<string, IMongoClient>();

            this.SetDefaultClient();
        }

        public IMongoClient GetClient(string clientName)
        {
            return clientsRepository[clientName];
        }        

        private void SetDefaultClient()
        {
            string connectionString = this.mongoConfiguration.ConnectionString;
            MongoClientSettings settings = MongoClientSettings.FromConnectionString(connectionString);

            if (this.mongoConfiguration.MaxConnectionsPerPool > 0)
            {
                settings.MaxConnectionPoolSize = this.mongoConfiguration.MaxConnectionsPerPool;
            }

            this.DefaultClient = new MongoClient(settings);
            clientsRepository.Add("default", this.DefaultClient);
        }
    }
}
