using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Lab08.Data;
using Lab08.Repository.Enums;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Lab08.Repository.Contracts
{
    

    /// <summary>
    /// An <c>interface</c> for common MongoDB data operations.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IRepository<TEntity> where TEntity : Entity
    {
        /// <summary>
        /// Gets the underling <see cref="IMongoCollection{TDocument}"/> used for queries.
        /// </summary>
        /// <value>
        /// The underling <see cref="IMongoCollection{TEntity}"/>.
        /// </value>
        IMongoCollection<TEntity> Collection { get; }

        /// <summary>
        /// Pass an array of all indexes for the entity that will be created. If the index is already created it won't be created again. 
        /// If a index for the same column but with different options is created it will throw an error
        /// </summary>
        /// <param name="fieldDefinitions"></param>
        /// <returns></returns>
        Task CreateIndexes(List<string> fields);

        #region Queries

        IMongoCollection<TEntity> GetCollection();

        /// <summary>
        /// Get the entity with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the entity to get.</param>
        /// <returns>An instance of TEnity that has the specified identifier if found, otherwise null.</returns>
        TEntity Get(string key);

        /// <summary>
        /// Finds the entity with the specified identifier.
        /// Projected to new type with only specified fields
        /// </summary>
        /// <param name="key">The entity identifier.</param>
        /// <param name="projection">Expression specifying the new projected type</param>
        /// <returns>The entity with the specified identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="projection"/>.</exception>
        TNewProjection Get<TNewProjection>(string key, Expression<Func<TEntity, TNewProjection>> projection);

        /// <summary>
        /// Finds the entity with the specified identifier.
        /// </summary>
        /// <param name="key">The entity identifier.</param>
        /// <param name="projection">The entity identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The entity with the specified identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="projection"/></exception>
        Task<TNewProjection> GetAsync<TNewProjection>(
            string key,
            Expression<Func<TEntity, TNewProjection>> projection,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get the entity with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the entity to get.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An instance of TEnity that has the specified identifier if found, otherwise null.</returns>
        Task<TEntity> GetAsync(string key, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get all entities as an <see cref="IQueryable{TEntity}"/>.
        /// </summary>
        /// <returns>An <see cref="IQueryable{TEntity}"/> of entities.</returns>
        IMongoQueryable<TEntity> GetAll();

        /// <summary>
        /// Get all entities async.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get the first entity using the specified <paramref name="criteria"/> expression.
        /// </summary>
        /// <param name="criteria">The criteria expression.</param>
        /// <returns>
        /// An instance of TEnity that matches the criteria if found, otherwise null.
        /// </returns>
        TEntity FirstOrDefault(Expression<Func<TEntity, bool>> criteria);

        /// <summary>
        /// Get the first entity using the specified <paramref name="criteria"/> expression.
        /// </summary>
        /// <param name="criteria">The criteria expression.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An instance of TEnity that matches the criteria if found, otherwise null.
        /// </returns>
        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> criteria, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Find the first entity using the specified <paramref name="criteria" /> expression.
        /// Projected to new type with only specified fields
        /// </summary>
        /// <param name="criteria">The criteria expression.</param>
        /// <param name="projection">Expression specifying the new projected type</param>
        /// <returns>The entity with the specified identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="criteria"/> or <paramref name="projection"/>.</exception>
        TNewProjection FirstOrDefault<TNewProjection>(
            Expression<Func<TEntity, bool>> criteria,
            Expression<Func<TEntity, TNewProjection>> projection);

        /// <summary>
        /// Find the first entity using the specified <paramref name="criteria" /> expression.
        /// Projected to new type with only specified fields
        /// </summary>
        /// <param name="criteria">The criteria expression.</param>
        /// <param name="projection">The projection expression.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An instance of TEnity that matches the criteria if found, otherwise null.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">criteria or projection</exception>
        Task<TNewProjection> FirstOrDefaultAsync<TNewProjection>(
            Expression<Func<TEntity, bool>> criteria,
            Expression<Func<TEntity, TNewProjection>> projection,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get all entities using the specified <paramref name="criteria"/> expression.
        /// </summary>
        /// <param name="criteria">The criteria expression.</param>
        /// <returns></returns>
        IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> criteria);

        /// <summary>
        /// Get all entities using the specified <paramref name="criteria"/> expression.
        /// </summary>
        /// <param name="criteria">The criteria expression.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<List<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> criteria, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get all entities using the specified <paramref name="criteria"/> expression.
        /// </summary>
        /// <param name="criteria">The criteria expression.</param>
        /// <param name="projection">The projection expression.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<List<TNewProjection>> WhereAsync<TNewProjection>(
            Expression<Func<TEntity, bool>> criteria,
            Expression<Func<TEntity, TNewProjection>> projection,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Determines if the specified <paramref name="criteria" /> exists.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns>
        ///   <c>true</c> if criteria expression is found; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">criteria</exception>
        bool Any(Expression<Func<TEntity, bool>> criteria);

        /// <summary>
        /// Determines if the specified <paramref name="criteria" /> exists async.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns>
        ///   <c>true</c> if criteria expression is found; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">criteria</exception>
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> criteria, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Counts the number of entities using the specified <paramref name="criteria" />.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns>Number of entities</returns>
        /// <exception cref="System.ArgumentNullException">criteria</exception>
        long Count(Expression<Func<TEntity, bool>> criteria);

        #endregion

        #region Insert

        /// <summary>
        /// Inserts the specified <paramref name="entity"/> to the underlying data repository.
        /// </summary>
        /// <param name="entity">The entity to be inserted.</param>
        /// <returns>The entity that was inserted.</returns>
        TEntity Insert(TEntity entity);

        /// <summary>
        /// Inserts the specified <paramref name="entities"/> to the underlying data repository.
        /// </summary>
        /// <param name="entities">The entities to be inserted.</param>
        void Insert(IEnumerable<TEntity> entities);

        /// <summary>
        /// Inserts the specified <paramref name="entities"/> in a batch operation to the underlying data repository.
        /// </summary>
        /// <param name="entities">The entities to be inserted.</param>
        void InsertBatch(IEnumerable<TEntity> entities);


        /// <summary>
        /// Inserts the specified <paramref name="entity" /> to the underlying data repository.
        /// </summary>
        /// <param name="entity">The entity to be inserted.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The entity that was inserted.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">entity</exception>
        Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Inserts the specified <paramref name="entities" /> in a batch operation to the underlying data repository.
        /// </summary>
        /// <param name="entities">The entities to be inserted.</param>
        /// <exception cref="System.ArgumentNullException">entities</exception>
        Task<List<TEntity>> InsertManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default(CancellationToken));
        #endregion

        #region Update
        /// <summary>
        /// Updates the specified <paramref name="entity"/> in the underlying data repository.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <returns>The entity that was updated.</returns>
        TEntity Update(TEntity entity);

        /// <summary>
        /// Updates the specified <paramref name="entities"/> in the underlying data repository.
        /// </summary>
        /// <param name="entities">The entities to be updated.</param>
        void Update(IEnumerable<TEntity> entities);

        /// <summary>
        /// Updates the specified <paramref name="entity" /> in the underlying data repository.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The entity that was updated.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">entity</exception>
        Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Updates entity by given criteria. If the state of the entity doesn't match the criteria, the result is a conflict or a failure. 
        /// This method doesn't keep history.
        /// </summary>
        /// <param name="entity">Entity that will be the target of the concurrent update.</param>
        /// <param name="criteria">Additional criteria for ensuring state. Filter by Id and ModifiedOn are included by default in the criteria. Pass null if no additional criteria is needed.</param>
        /// <param name="updates">Updates to apply once the state of the entities is ensured.</param>
        /// <returns>CollectionUpdateResult.Succes for sucessful update or CollectionUpdateResult.Conflict and CollectionUpdateResult.Failed for unsuccessful one.</returns>
        Task<CollectionUpdateResult> UpdateConcurrentlyAsync(TEntity entity, FilterDefinition<TEntity> criteria, params UpdateDefinition<TEntity>[] updates);

        Task UpdateManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default(CancellationToken));

        Task SetFieldAsync<TField1>(Expression<Func<TEntity, TField1>> expression, IDictionary<string, TField1> fieldValues);

        Task SetFieldAsync<TField>(Expression<Func<TEntity, TField>> expression, TField value, IEnumerable<string> ids);

        #endregion

        #region Delete

        /// <summary>
        /// Deletes an entity with the specified <paramref name="key"/> from the underlying data repository.
        /// </summary>
        /// <param name="key">The key of the entity to delete.</param>
        /// <returns>The number of documents deleted</returns>
        bool Delete(string key);

        /// <summary>
        /// Deletes the specified <paramref name="entity"/> from the underlying data repository.
        /// </summary>
        /// <param name="entity">The entity to be deleted.</param>
        /// <returns>The number of documents deleted</returns>
        bool Delete(TEntity entity);

        /// <summary>
        /// Deletes an entity with the specified <paramref name="id" /> from the underlying data repository.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The number of documents deleted</returns>
        /// <exception cref="ArgumentNullException"><paramref name="id"/> is <see langword="null" />.</exception>
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Deletes the specified <paramref name="entity" /> from the underlying data repository.
        /// </summary>
        /// <param name="entity">The entity to be deleted.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The number of documents deleted</returns>
        /// <exception cref="ArgumentNullException"><paramref name="entity"/> is <see langword="null" />.</exception>
        Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken));


        #endregion
    }
}