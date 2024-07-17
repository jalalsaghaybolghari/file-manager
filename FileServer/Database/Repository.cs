using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
namespace FileServer.Database
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity
    {
        private readonly ApplicationDbContext dbContext;
        private DbSet<TEntity> entities { get; }
        public Repository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
            entities = this.dbContext.Set<TEntity>();
        }
        #region Async Method
        public virtual Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, IInclude<TEntity> include = null)
        {
            var query = GetQuery(include);
            query = query.Where(predicate);
            return query.FirstOrDefaultAsync(cancellationToken);
        }
        public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true)
        {
            await entities.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            if (saveNow)
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        public virtual async Task AddRangeAsync(IQueryable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true)
        {
            await this.entities.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
            if (saveNow)
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true)
        {
            entities.Update(entity);
            if (saveNow)
                await dbContext.SaveChangesAsync(cancellationToken);
        }
        public virtual async Task UpdateRangeAsync(IQueryable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true)
        {
            this.entities.UpdateRange(entities);
            if (saveNow)
                await dbContext.SaveChangesAsync(cancellationToken);
        }
        public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true)
        {
            entities.Remove(entity);
            if (saveNow)
                await dbContext.SaveChangesAsync(cancellationToken);
        }
        public async Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, bool saveNow = true)
        {
            var query = GetQuery().Where(predicate: predicate);
            await query.DeleteFromQueryAsync(cancellationToken);
            if (saveNow)
                await dbContext.SaveChangesAsync(cancellationToken);
        }
        public virtual async Task DeleteAsync(IQueryable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true)
        {
            this.entities.RemoveRange(entities);
            if (saveNow)
                await dbContext.SaveChangesAsync(cancellationToken);
        }
        #endregion
        #region Sync Methods
        public virtual TEntity Get(Expression<Func<TEntity, bool>> predicate, IInclude<TEntity> include = null)
        {
            var query = GetQuery(include);
            query = query.Where(predicate);
            return query.FirstOrDefault();
        }
        public virtual IQueryable<TEntity> GetQuery(IInclude<TEntity> include = null)
        {
            var query = entities.AsQueryable();
            if (include != null)
                query = include.Execute(query);
            return query;
        }
        public virtual void Add(TEntity entity, bool saveNow = true)
        {
            entities.Add(entity);
            if (saveNow)
                dbContext.SaveChanges();
        }
        public virtual void AddRange(IQueryable<TEntity> entities, bool saveNow = true)
        {
            this.entities.AddRange(entities);
            if (saveNow)
                dbContext.SaveChanges();
        }
        public virtual void Update(TEntity entity, bool saveNow = true)
        {
            entities.Update(entity);
            dbContext.SaveChanges();
        }
        public virtual void UpdateRange(IQueryable<TEntity> entities, bool saveNow = true)
        {
            this.entities.UpdateRange(entities);
            if (saveNow)
                dbContext.SaveChanges();
        }
        public void Delete(Expression<Func<TEntity, bool>> predicate, bool saveNow = true)
        {
            var query = GetQuery().Where(predicate: predicate);
            query.DeleteFromQuery();
            if (saveNow)
                dbContext.SaveChanges();
        }
        public virtual void Delete(TEntity entity, bool saveNow = true)
        {
            entities.Remove(entity);
            if (saveNow)
                dbContext.SaveChanges();
        }
        public virtual void Delete(IQueryable<TEntity> entities, bool saveNow = true)
        {
            this.entities.RemoveRange(entities);
            if (saveNow)
                dbContext.SaveChanges();
        }
        #endregion
        #region Attach & Detach
        public virtual void Detach(TEntity entity)
        {
            var entry = dbContext.Entry(entity);
            if (entry != null)
                entry.State = EntityState.Detached;
        }
        public virtual void Attach(TEntity entity)
        {
            if (dbContext.Entry(entity).State == EntityState.Detached)
                entities.Attach(entity);
        }
        #endregion
        #region Explicit Loading
        public virtual async Task LoadCollectionAsync<TProperty>(TEntity entity, Expression<Func<TEntity, IEnumerable<TProperty>>> collectionProperty, CancellationToken cancellationToken)
            where TProperty : class
        {
            Attach(entity);
            var collection = dbContext.Entry<TEntity>(entity).Collection(collectionProperty);
            if (!collection.IsLoaded)
                await collection.LoadAsync(cancellationToken).ConfigureAwait(false);
        }
        public virtual void LoadCollection<TProperty>(TEntity entity, Expression<Func<TEntity, IEnumerable<TProperty>>> collectionProperty)
            where TProperty : class
        {
            dbContext.Entry<TEntity>(entity).Collection(collectionProperty).Load();
            Attach(entity);
            var collection = dbContext.Entry(entity).Collection(collectionProperty);
            if (!collection.IsLoaded)
                collection.Load();
        }
        public virtual async Task LoadReferenceAsync<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> referenceProperty, CancellationToken cancellationToken)
            where TProperty : class
        {
            Attach(entity);
            var reference = dbContext.Entry(entity).Reference(referenceProperty);
            if (!reference.IsLoaded)
                await reference.LoadAsync(cancellationToken).ConfigureAwait(false);
        }
        public virtual void LoadReference<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> referenceProperty)
            where TProperty : class
        {
            Attach(entity);
            var reference = dbContext.Entry(entity).Reference(referenceProperty);
            if (!reference.IsLoaded)
                reference.Load();
        }
        public bool IsModified<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> property)
        {
            return dbContext.Entry<TEntity>(entity).Property(property).IsModified;
        }
        public bool IsModified(TEntity entity, params string[] properties)
        {
            foreach (var property in properties)
            {
                var result = dbContext.Entry<TEntity>(entity).Property(property).IsModified;
                if (result == true) return true;
            }
            return false;
        }
        public TProperty OrginalValue<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> property)
        {
            return dbContext.Entry<TEntity>(entity).Property(property).OriginalValue;
        }
        #endregion
    }
}