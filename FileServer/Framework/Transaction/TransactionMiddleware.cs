namespace FileServer.Framework.Transaction
{
    public class TransactionMiddleware
    {
        private readonly RequestDelegate next;
        public TransactionMiddleware(RequestDelegate next)
        {
            this.next = next;
        }
        public async Task Invoke(HttpContext context, ITransactionManager transactionManager)
        {
            if (context.Request.Method == "GET")
            {
                await next(context);
            }
            else
            {
                var cancellationToken = context.RequestAborted;
                await transactionManager.BeginTransactionAsync(cancellationToken);
                try
                {
                    await next(context);
                    await transactionManager.CommitTransactionAsync(cancellationToken);
                }
                catch
                {
                    await transactionManager.RollbackTransactionAsync(cancellationToken);
                    throw;
                }
            }
        }
    }
}