using Backend.Domain.Aggregates.Common;
using Backend.Domain.Interfaces.Repositories;
using Backend.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Backend.Persistence.Repositories;

/// <summary>
/// Concrete generic repository implementation for factory creation
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
/// <typeparam name="TId">Entity ID type</typeparam>
public class GenericRepository<T, TId> : BaseRepository<T, TId>, IGenericRepository<T, TId>
    where T : BaseAggregateRoot<TId>
    where TId : struct
{
    public GenericRepository(ApplicationDbContext context) : base(context)
    {
    }
} 