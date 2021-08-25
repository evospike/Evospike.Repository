using System;
namespace Evospike.Repository.Interfaces
{
    public interface IEntity
    {
        Guid Id { get; set; }
    }
}
