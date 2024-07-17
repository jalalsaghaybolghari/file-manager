using FileServer.ViewModels.Interface;
using Microsoft.EntityFrameworkCore.Storage;
namespace FileServer.Framework.Transaction
{
    public interface ITransactionManager : IScopedDependency
    {
        IDbContextTransaction Transaction { get; }
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}