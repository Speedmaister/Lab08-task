using System;

namespace Lab08.Services.Contracts
{
    public interface ICurrentTimeProvider
    {
        DateTime Now();
    }
}
