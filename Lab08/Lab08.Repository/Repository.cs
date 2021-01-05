using Lab08.Data;
using Lab08.Repository.Configuration;
using Lab08.Repository.Contracts;
using Lab08.Repository.Enums;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Lab08.Repository
{
    /// <summary>
    /// A MongoDB data repository base class.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    internal class Repository<TEntity> : BaseRepository<TEntity>, IRepository<TEntity> where TEntity : Entity
    {
        private readonly IMongoClient client;

        protected readonly MongoDbConfiguration settings;

        public Repository(MongoDbConfiguration settings, MongoClientProvider mongoClientProvider)
        {
            this.settings = settings;
            this.client = mongoClientProvider.GetClient("default");

            this.SetCollection();
        }

        public new IMongoCollection<TEntity> Collection { get => base.Collection; protected set => base.Collection = value; }

        /// <summary>
        /// Sets the collections of the repository to ones for the provided tenant.
        /// </summary>
        /// <param name="tenantId"></param>
        protected void SetCollection()
        {
            string collectionName = GetCollectionName();
            IMongoDatabase database = this.client.GetDatabase(this.settings.Database);

            this.Collection = database.GetCollection<TEntity>(collectionName);
        }

        private static string GetCollectionName()
        {
            Type entityType = typeof(TEntity);
            string collectionName = entityType.Name;

            return collectionName;
        }

        #region Queries

        public IMongoCollection<TEntity> GetCollection()
        {
            return this.Collection;
        }

        /// <summary>
        /// Finds the entity with the specified identifier.
        /// </summary>
        /// <param name="key">The entity identifier.</param>
        /// <returns>The entity with the specified identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null" />.</exception>
        public TEntity Get(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return MongoRetryPolicy.Retry(() => this.Collection.Find(this.KeyExpression(key)).FirstOrDefault());
        }

        /// <summary>
        /// Finds the entity with the specified identifier.
        /// Projected to new type with only specified fields
        /// </summary>
        /// <param name="key">The entity identifier.</param>
        /// <param name="projection">Expression specifying the new projected type</param>
        /// <returns>The entity with the specified identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="projection"/>.</exception>
        public TNewProjection Get<TNewProjection>(string key, Expression<Func<TEntity, TNewProjection>> projection)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (projection == null)
            {
                throw new ArgumentNullException(nameof(projection));
            }

            return MongoRetryPolicy.Retry(() => this.Collection
                                  .Find(this.KeyExpression(key))
                                  .Project(projection)
                                  .FirstOrDefault());
        }

        /// <summary>
        /// Finds the entity with the specified identifier.
        /// </summary>
        /// <param name="key">The entity identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The entity with the specified identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null" />.</exception>
        public Task<TEntity> GetAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return MongoRetryPolicy.Retry(() => this.Collection.Find(this.KeyExpression(key)).FirstOrDefaultAsync(cancellationToken));
        }

        /// <summary>
        /// Finds the entity with the specified identifier.
        /// </summary>
        /// <param name="key">The entity identifier.</param>
        /// <param name="projection">The entity identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The entity with the specified identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="projection"/></exception>
        public Task<TNewProjection> GetAsync<TNewProjection>(
            string key,
            Expression<Func<TEntity, TNewProjection>> projection,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (projection == null)
            {
                throw new ArgumentNullException(nameof(projection));
            }

            return MongoRetryPolicy.Retry(() => this.Collection
                                  .Find(this.KeyExpression(key))
                                  .Project(projection)
                                  .FirstOrDefaultAsync(cancellationToken));
        }

        /// <summary>
        /// Get all <typeparamref name="TEntity" /> entities as an IQueryable
        /// </summary>
        /// <returns>
        /// IQueryable of <typeparamref name="TEntity" />.
        /// </returns>
        public IMongoQueryable<TEntity> GetAll()
        {
            return MongoRetryPolicy.Retry(() => this.Collection.AsQueryable().Where(x => !x.IsDeleted));
        }

        /// <summary>
        /// Get all entities async
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">criteria</exception>
        public Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return MongoRetryPolicy.Retry(() => this.Collection.Find(x => !x.IsDeleted).ToListAsync(cancellationToken));
        }

        /// <summary>
        /// Find the first entity using the specified <paramref name="criteria" /> expression.
        /// </summary>
        /// <param name="criteria">The criteria expression.</param>
        /// <returns>
        /// An instance of TEnity that matches the criteria if found, otherwise null.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">criteria</exception>
        public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            Expression<Func<TEntity, bool>> querry = criteria.And(x => !x.IsDeleted);
            return MongoRetryPolicy.Retry(() => this.Collection.Find(querry).FirstOrDefault());
        }

        /// <summary>
        /// Find the first entity using the specified <paramref name="criteria" /> expression.
        /// Projected to new type with only specified fields
        /// </summary>
        /// <param name="criteria">The criteria expression.</param>
        /// <param name="projection">Expression specifying the new projected type</param>
        /// <returns>The entity with the specified identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="criteria"/> or <paramref name="projection"/>.</exception>
        public TNewProjection FirstOrDefault<TNewProjection>(
            Expression<Func<TEntity, bool>> criteria,
            Expression<Func<TEntity, TNewProjection>> projection)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            if (projection == null)
            {
                throw new ArgumentNullException(nameof(projection));
            }

            return MongoRetryPolicy.Retry(() => this.Collection
                                  .Find(criteria)
                                  .Project(projection)
                                  .FirstOrDefault());
        }

        /// <summary>
        /// Find the first entity using the specified <paramref name="criteria" /> expression.
        /// </summary>
        /// <param name="criteria">The criteria expression.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An instance of TEnity that matches the criteria if found, otherwise null.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">criteria</exception>
        public Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> criteria, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            Expression<Func<TEntity, bool>> querry = criteria.And(x => !x.IsDeleted);
            return MongoRetryPolicy.Retry(() => this.Collection.Find(querry).FirstOrDefaultAsync(cancellationToken));
        }

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
        public Task<TNewProjection> FirstOrDefaultAsync<TNewProjection>(
            Expression<Func<TEntity, bool>> criteria,
            Expression<Func<TEntity, TNewProjection>> projection,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            if (projection == null)
            {
                throw new ArgumentNullException(nameof(projection));
            }

            Expression<Func<TEntity, bool>> querry = criteria.And(x => !x.IsDeleted);
            return MongoRetryPolicy.Retry(() => this.Collection
                                  .Find(querry)
                                  .Project(projection)
                                  .FirstOrDefaultAsync(cancellationToken));
        }

        /// <summary>
        /// Find all entities using the specified <paramref name="criteria" /> expression.
        /// </summary>
        /// <param name="criteria">The criteria expression.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">criteria</exception>
        public IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            Expression<Func<TEntity, bool>> querry = criteria.And(x => !x.IsDeleted);
            return MongoRetryPolicy.Retry(() => this.Collection.AsQueryable().Where(querry));
        }

        /// <summary>
        /// Find all entities using the specified <paramref name="criteria" /> expression.
        /// </summary>
        /// <param name="criteria">The criteria expression.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">criteria</exception>
        public Task<List<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> criteria, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            Expression<Func<TEntity, bool>> querry = criteria.And(x => !x.IsDeleted);
            return MongoRetryPolicy.Retry(() => this.Collection.Find(querry).ToListAsync(cancellationToken));
        }

        /// <inheritdoc />
        public Task<List<TNewProjection>> WhereAsync<TNewProjection>(
            Expression<Func<TEntity, bool>> criteria,
            Expression<Func<TEntity, TNewProjection>> projection,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            Expression<Func<TEntity, bool>> querry = criteria.And(x => !x.IsDeleted);
            return MongoRetryPolicy.Retry(() => this.Collection
                                  .Find(querry)
                                  .Project(projection)
                                  .ToListAsync(cancellationToken));
        }

        /// <summary>
        /// Determines if the specified <paramref name="criteria" /> exists.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns>
        ///   <c>true</c> if criteria expression is found; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">criteria</exception>
        public bool Any(Expression<Func<TEntity, bool>> criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            Expression<Func<TEntity, bool>> querry = criteria.And(x => !x.IsDeleted);
            return MongoRetryPolicy.Retry(() => this.Collection.AsQueryable().Any(querry));
        }

        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> criteria, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            Expression<Func<TEntity, bool>> querry = criteria.And(x => !x.IsDeleted);
            return MongoRetryPolicy.Retry(() => this.Collection.Find(querry).AnyAsync(cancellationToken));
        }

        /// <summary>
        /// Counts the number of entities using the specified <paramref name="criteria" />.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns>Number of entities</returns>
        /// <exception cref="System.ArgumentNullException">criteria</exception>
        public long Count(Expression<Func<TEntity, bool>> criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            Expression<Func<TEntity, bool>> query = criteria.And(x => !x.IsDeleted);
            return MongoRetryPolicy.Retry(() => this.Collection.AsQueryable().LongCount(query));
        }

        #endregion

        #region CRUD

        #region Insert

        /// <summary>
        /// Inserts the specified <paramref name="entity" /> to the underlying data repository.
        /// </summary>
        /// <param name="entity">The entity to be inserted.</param>
        /// <returns>
        /// The entity that was inserted.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">entity</exception>
        public TEntity Insert(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            this.BeforeInsert(entity);



            MongoRetryPolicy.Retry(() =>
            {
                this.CreateIndexes(this.GetDefaultIndexProperties());
                this.Collection.InsertOne(entity);
                return true;
            });

            return entity;
        }

        /// <summary>
        /// Inserts the specified <paramref name="entities" /> to the underlying data repository.
        /// </summary>
        /// <param name="entities"></param>
        /// <exception cref="System.ArgumentNullException">entities</exception>
        public void Insert(IEnumerable<TEntity> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            foreach (TEntity entity in entities)
            {
                this.Insert(entity);
            }
        }

        /// <summary>
        /// Inserts the specified <paramref name="entities" /> in a batch operation to the underlying data repository.
        /// </summary>
        /// <param name="entities">The entities to be inserted.</param>
        /// <exception cref="System.ArgumentNullException">entities</exception>
        public void InsertBatch(IEnumerable<TEntity> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            List<TEntity> list = entities.ToList();
            list.ForEach(this.BeforeInsert);

            MongoRetryPolicy.Retry(() =>
            {
                this.CreateIndexes(this.GetDefaultIndexProperties());
                this.Collection.InsertMany(list);

                return true;
            });
        }

        /// <summary>
        /// Inserts the specified <paramref name="entity" /> to the underlying data repository.
        /// </summary>
        /// <param name="entity">The entity to be inserted.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The entity that was inserted.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">entity</exception>
        public Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            this.BeforeInsert(entity);


            return MongoRetryPolicy.Retry(async () =>
            {
                await this.CreateIndexes(this.GetDefaultIndexProperties());
                return await this.Collection.InsertOneAsync(entity, cancellationToken: cancellationToken).ContinueWith(t =>
                {
                    return entity;
                }, cancellationToken);
            });
        }

        /// <summary>
        /// Inserts the specified <paramref name="entities" /> in a batch operation to the underlying data repository.
        /// </summary>
        /// <param name="entities">The entities to be inserted.</param>
        /// <exception cref="System.ArgumentNullException">entities</exception>
        public Task<List<TEntity>> InsertManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            List<TEntity> list = entities.ToList();
            list.ForEach(this.BeforeInsert);

            return MongoRetryPolicy.Retry(async () =>
            {
                await this.CreateIndexes(this.GetDefaultIndexProperties());
                return await this.Collection.InsertManyAsync(list, cancellationToken: cancellationToken).ContinueWith(x =>
                {
                    return list;
                }, cancellationToken);
            });
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates the specified <paramref name="entity" /> in the underlying data repository.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <returns>
        /// The entity that was updated.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">entity</exception>
        public TEntity Update(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            this.BeforeUpdate(entity);

            var updateOptions = new UpdateOptions
            {
                IsUpsert = true
            };

            string key = entity.Id;

            MongoRetryPolicy.Retry(() =>
            {
                ReplaceOneResult replaceOneResult = this.Collection.ReplaceOne(this.KeyExpression(key), entity, updateOptions);
                return replaceOneResult;
            });

            return entity;
        }

        /// <summary>
        /// Updates the specified <paramref name="entities" /> in the underlying data repository.
        /// </summary>
        /// <param name="entities"></param>
        /// <exception cref="System.ArgumentNullException">entities</exception>
        public void Update(IEnumerable<TEntity> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            foreach (TEntity entity in entities)
            {
                this.Update(entity);
            }
        }

        /// <summary>
        /// Updates the specified <paramref name="entity" /> in the underlying data repository.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The entity that was updated.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">entity</exception>
        public Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            this.BeforeUpdate(entity);

            var updateOptions = new UpdateOptions
            {
                IsUpsert = true
            };
            string key = entity.Id;

            return MongoRetryPolicy.Retry(() =>
            {
                return this.Collection
                    .ReplaceOneAsync(this.KeyExpression(key), entity, updateOptions, cancellationToken)
                    .ContinueWith(t =>
                    {
                        return entity;
                    },
                    cancellationToken);
            });
        }

        public Task SetFieldAsync<TField>(Expression<Func<TEntity, TField>> expression, TField value, IEnumerable<string> ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            var find = new FilterDefinitionBuilder<TEntity>().In(f => f.Id, ids);
            var update = new UpdateDefinitionBuilder<TEntity>()
                .Set(expression, value)
                .Set(e => e.ModifiedOn, DateTime.UtcNow);

            return MongoRetryPolicy.Retry(() =>
            {
                return this.Collection.BulkWriteAsync(new List<WriteModel<TEntity>>
                {
                    new UpdateManyModel<TEntity>(find,update)
                });
            });
        }

        public Task SetFieldAsync<TField1>(Expression<Func<TEntity, TField1>> expression, IDictionary<string, TField1> fieldValues)
        {
            var updateDefinitions = new ConcurrentBag<WriteModel<TEntity>>();

            Parallel.ForEach(fieldValues, (pair) =>
            {
                var find = new FilterDefinitionBuilder<TEntity>().Eq(f => f.Id, pair.Key);
                var update = new UpdateDefinitionBuilder<TEntity>()
                    .Set(expression, fieldValues[pair.Key])
                    .Set(e => e.ModifiedOn, DateTime.UtcNow);

                updateDefinitions.Add(new UpdateOneModel<TEntity>(find, update));
            });

            return MongoRetryPolicy.Retry(() =>
            {
                return this.Collection.BulkWriteAsync(updateDefinitions);
            });
        }

        /// <summary>
        /// Inserts the specified <paramref name="entities" /> in a batch operation to the underlying data repository.
        /// </summary>
        /// <param name="entities">The entities to be inserted.</param>
        /// <exception cref="System.ArgumentNullException">entities</exception>
        public async Task UpdateManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            List<TEntity> list = entities.ToList();
            list.ForEach(this.BeforeUpdate);

            var models = new WriteModel<TEntity>[entities.Count()];

            Parallel.For(0, models.Count(), (index) =>
            {
                var entity = list[index];
                string key = entity.Id;
                models[index] = new ReplaceOneModel<TEntity>(this.KeyExpression(key), entity) { IsUpsert = true };
            });

            await MongoRetryPolicy.Retry(async () =>
            {
                await this.Collection.BulkWriteAsync(models);
            });
        }

        #endregion

        #region Delete
        /// <summary>
        /// Deletes the specified <paramref name="entity" /> from the underlying data repository.
        /// </summary>
        /// <param name="entity">The entity to be deleted.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The number of documents deleted</returns>
        /// <exception cref="ArgumentNullException"><paramref name="entity"/> is <see langword="null" />.</exception>
        public Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            entity.IsDeleted = true;
            this.BeforeUpdate(entity);

            return MongoRetryPolicy.Retry(() =>
            {
                return this.Collection.ReplaceOneAsync(this.KeyExpression(entity.Id), entity, cancellationToken: cancellationToken)
                .ContinueWith(x =>
                {
                    bool resultIsAcknowledged = x.Result.IsAcknowledged;
                    return resultIsAcknowledged;
                },
                cancellationToken);
            });
        }

        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The number of documents deleted</returns>
        /// <exception cref="ArgumentNullException"><paramref name="id"/> is <see langword="null" />.</exception>
        public bool Delete(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            UpdateDefinition<TEntity> updateDefinition = Builders<TEntity>.Update.Set(x => x.IsDeleted, true);
            UpdateDefinition<TEntity> modifyDefinition = Builders<TEntity>.Update.Set(x => x.ModifiedOn, DateTime.Now);

            bool result = MongoRetryPolicy.Retry(() =>
            {
                UpdateResult deleteResult = this.Collection.UpdateOne(this.KeyExpression(id), updateDefinition);

                return deleteResult.IsAcknowledged && this.Collection.UpdateOne(this.KeyExpression(id), modifyDefinition).IsAcknowledged;
            });

            return result;
        }

        /// <summary>
        /// Deletes the specified <paramref name="entity" /> from the underlying data repository.
        /// </summary>
        /// <param name="entity">The entity to be deleted.</param>
        /// <returns>The number of documents deleted</returns>
        /// <exception cref="ArgumentNullException"><paramref name="entity"/> is <see langword="null" />.</exception>
        public bool Delete(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            string key = entity.Id;

            return MongoRetryPolicy.Retry(() => this.Delete(key));
        }

        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The number of documents deleted</returns>
        /// <exception cref="ArgumentNullException"><paramref name="id"/> is <see langword="null" />.</exception>
        public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var entity = await this.GetAsync(id, cancellationToken);
            if (entity == null)
            {
                return true;
            }

            this.BeforeUpdate(entity);
            entity.IsDeleted = true;

            return await MongoRetryPolicy.Retry(async () =>
            {
                return await this.Collection
                    .ReplaceOneAsync(this.KeyExpression(entity.Id), entity, cancellationToken: cancellationToken)
                    .ContinueWith(t =>
                    {
                        bool resultIsAcknowledged = t.Result.IsAcknowledged;
                        return resultIsAcknowledged;
                    },
                    cancellationToken);
            });
        }

        #endregion

        #endregion

        #region Entity

        /// <summary>
        /// Pass an array of all indexes for the entity that will be created. If the index is already created it won't be created again. 
        /// If a index for the same column but with different options is created it will throw an error
        /// </summary>
        /// <param name="fieldDefinitions"></param>
        /// <returns></returns>
        public async Task CreateIndexes(List<string> fields)
        {
            List<CreateIndexModel<TEntity>> indexModels = new List<CreateIndexModel<TEntity>>();

            foreach (var fieldDefinition in fields)
            {
                var indeKeysDefinitionBuilder = new IndexKeysDefinitionBuilder<TEntity>();
                var indexModel = new CreateIndexModel<TEntity>(indeKeysDefinitionBuilder.Ascending(fieldDefinition));
                indexModels.Add(indexModel);
            }

            await this.Collection.Indexes.CreateManyAsync(indexModels);
        }

        /// <summary>
        /// Gets the key expression with the specified <paramref name="key" />.
        /// </summary>
        /// <param name="key">The key to get expression with.</param>
        /// <returns>
        /// The key expression for the specified key.
        /// </returns>
        /// <example>
        ///   <code>
        /// Example xpression for an entity key.
        /// <![CDATA[entity => entity.Id == key]]></code>
        /// </example>
        protected Expression<Func<TEntity, bool>> KeyExpression(string key)
        {
            return entity => entity.Id == key && !entity.IsDeleted;
        }

        #endregion

        private List<string> GetDefaultIndexProperties()
        {
            return new List<string>() { nameof(Entity.IsDeleted) };
        }

        private string GetExpressionFiledName<TField>(Expression<Func<TEntity, TField>> expression)
        {
            if (!(expression.Body is MemberExpression member))
            {
                throw new ArgumentException();
            }

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
            {
                throw new Exception();
            }

            return propInfo.Name;
        }
    }
}