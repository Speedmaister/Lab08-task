using Lab08.Services.Contracts;
using System;

namespace Lab08.Services
{
    public class CurrentTimeProvider : ICurrentTimeProvider
    {
        public DateTime Now()
        {
            return DateTime.UtcNow;
        }
    }
}
