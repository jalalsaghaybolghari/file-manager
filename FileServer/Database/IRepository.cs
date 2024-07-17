using System.Linq.Expressions;
namespace FileServer.Database
{
    public interface IRepository<TEntity> where TEntity : class, IEntity
    {
        void Add(TEntity entity, bool saveNow = true);
        Task AddAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true);
        void AddRange(IQueryable<TEntity> entities, bool saveNow = true);
        Task AddRangeAsync(IQueryable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true);
        void Attach(TEntity entity);
        void Delete(TEntity entity, bool saveNow = true);
        Task DeleteAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true);
        void Delete(Expression<Func<TEntity, bool>> predicate, bool saveNow = true);
        Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, bool saveNow = true);
        void Delete(IQueryable<TEntity> entities, bool saveNow = true);
        Task DeleteAsync(IQueryable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true);
        void Detach(TEntity entity);
        TEntity Get(Expression<Func<TEntity, bool>> predicate, IInclude<TEntity> include = null);
        Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, IInclude<TEntity> include = null);
        IQueryable<TEntity> GetQuery(IInclude<TEntity> include = null);
        void LoadCollection<TProperty>(TEntity entity, Expression<Func<TEntity, IEnumerable<TProperty>>> collectionProperty) where TProperty : class;
        Task LoadCollectionAsync<TProperty>(TEntity entity, Expression<Func<TEntity, IEnumerable<TProperty>>> collectionProperty, CancellationToken cancellationToken) where TProperty : class;
        void LoadReference<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> referenceProperty) where TProperty : class;
        Task LoadReferenceAsync<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> referenceProperty, CancellationToken cancellationToken) where TProperty : class;
        void Update(TEntity entity, bool saveNow = true);
        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true);
        void UpdateRange(IQueryable<TEntity> entities, bool saveNow = true);
        Task UpdateRangeAsync(IQueryable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true);
        bool IsModified<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> property);
        bool IsModified(TEntity entity, params string[] properties);
        TProperty OrginalValue<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> property);
    }
}