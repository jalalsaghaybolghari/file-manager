namespace FileServer.Database
{
    public interface IInclude<TEntity> where TEntity : class, IEntity
    {
        T Execute<T>(T query) where T : IQueryable<TEntity>;
    }
}