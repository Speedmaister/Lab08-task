namespace Lab08.Repository.Configuration
{
    public class MongoDbConfiguration
    {
        public string ConnectionString { get; set; }

        public string Database { get; set; }

        public int MaxConnectionsPerPool { get; set; }
    }
}
