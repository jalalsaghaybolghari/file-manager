using FileServer.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FileServer.Framework.Transaction
{
    public class TransactionManager : ITransactionManager
    {
        DbContext transactionDbContext;
        public IDbContextTransaction Transaction { get; private set; }
        public TransactionManager(ApplicationDbContext transactionDbContext)
        {
            this.transactionDbContext = transactionDbContext;
        }
        public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            var task = this.transactionDbContext.Database.BeginTransactionAsync(cancellationToken: cancellationToken);
            this.Transaction = task.Result;
            return task;
        }
        public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            this.transactionDbContext.SaveChangesAsync(cancellationToken: cancellationToken);
            return this.Transaction.CommitAsync(cancellationToken: cancellationToken);
        }
        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            return this.Transaction.RollbackAsync(cancellationToken: cancellationToken);
        }
    }
}