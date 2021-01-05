using MongoDB.Driver;
using Polly;
using System;
using System.IO;
using System.Net.Sockets;

namespace Lab08.Repository
{
    internal static class MongoRetryPolicy
    {
        public static TResult Retry<TResult>(Func<TResult> action, int retries = 3)
        {
            return Policy
                .Handle<MongoConnectionException>(i =>
                    i.InnerException.GetType() == typeof(IOException) ||
                    i.InnerException.GetType() == typeof(SocketException))
                .Retry(retries)
                .Execute(action);
        }
    }
}
