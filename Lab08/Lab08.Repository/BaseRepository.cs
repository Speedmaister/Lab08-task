using Lab08.Data;
using Lab08.Repository.Enums;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace Lab08.Repository
{
    internal class BaseRepository<TEntity> where TEntity : Entity
    {
        /// <summary>
        /// Check if the repository for the current entity should keep track of historical changes.
        /// </summary>
        protected readonly bool trackChanges;

        protected IMongoCollection<TEntity> Collection { get; set; }

        public async Task<CollectionUpdateResult> UpdateConcurrentlyAsync(TEntity entity, FilterDefinition<TEntity> criteria, params UpdateDefinition<TEntity>[] updates)
        {
            FilterDefinition<TEntity> filter = null;
            if (criteria == null)
            {
                filter = Builders<TEntity>.Filter.And(
                                                    Builders<TEntity>.Filter.Eq(x => x.Id, entity.Id),
                                                    Builders<TEntity>.Filter.Eq(x => x.ModifiedOn, entity.ModifiedOn));
            }
            else
            {
                filter = Builders<TEntity>.Filter.And(criteria,
                                                    Builders<TEntity>.Filter.Eq(x => x.Id, entity.Id),
                                                    Builders<TEntity>.Filter.Eq(x => x.ModifiedOn, entity.ModifiedOn));
            }

            var result = await UpdateFieldsByFilter(filter, updates);
            if (result.IsAcknowledged)
            {
                if (result.MatchedCount > 0)
                {
                    return CollectionUpdateResult.Success;
                }
                else
                {
                    return CollectionUpdateResult.Conflict;
                }
            }
            else
            {
                return CollectionUpdateResult.Failed;
            }
        }

        private async Task<UpdateResult> UpdateFieldsByFilter(FilterDefinition<TEntity> filter, params UpdateDefinition<TEntity>[] updates)
        {
            var update = Builders<TEntity>.Update.Combine(updates).CurrentDate(i => i.ModifiedOn);
            var updateResult = await Collection.UpdateManyAsync(filter, update.CurrentDate(i => i.ModifiedOn));
            return updateResult;
        }

        /// <summary>
        /// Called before an insert.
        /// </summary>
        /// <param name="entity">The entity.</param>
        protected void BeforeInsert(TEntity entity)
        {
            entity.CreatedOn = DateTime.Now;
            entity.ModifiedOn = DateTime.Now;
        }

        /// <summary>
        /// Called before an update.
        /// </summary>
        /// <param name="entity">The entity.</param>
        protected void BeforeUpdate(TEntity entity)
        {
            if (entity.CreatedOn == null || entity.CreatedOn == DateTime.MinValue)
            {
                entity.CreatedOn = DateTime.Now;
            }

            entity.ModifiedOn = DateTime.Now;
        }
    }
}
