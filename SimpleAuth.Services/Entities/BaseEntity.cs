using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleAuth.Services.Entities
{
    public abstract class BaseEntity
    {
    }
    
    public abstract class BaseEntity<T> : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public T Id { get; set; }
    }

    public static class BaseEntityExtensions
    {
        public static TEntity WithRandomId<TEntity>(this TEntity entity) where TEntity : BaseEntity<Guid>
        {
            if (entity.Id != Guid.Empty)
                throw new InvalidOperationException("Id existing");
            entity.Id = Guid.NewGuid();
            return entity;
        }
    }
}