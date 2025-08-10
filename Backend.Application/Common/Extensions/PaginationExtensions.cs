using Backend.Application.Common.DTOs;
using Backend.Application.Common.Results;
using System.Linq.Expressions;

namespace Backend.Application.Common.Extensions;

/// <summary>
/// Extensions for pagination operations
/// </summary>
public static class PaginationExtensions
{
    /// <summary>
    /// Applies pagination to IQueryable
    /// </summary>
    /// <typeparam name="T">Type of the data</typeparam>
    /// <param name="query">The queryable source</param>
    /// <param name="paginationDto">Pagination parameters</param>
    /// <returns>Paginated result</returns>
    public static async Task<PaginatedResult<T>> ToPaginatedResultAsync<T>(
        this IQueryable<T> query,
        PaginationDto paginationDto)
    {
        var totalCount = await Task.FromResult(query.Count());
        
        var data = query
            .Skip(paginationDto.Skip)
            .Take(paginationDto.Take)
            .ToList();

        return PaginatedResult<T>.Success(data, totalCount, paginationDto.PageNumber, paginationDto.PageSize);
    }

    /// <summary>
    /// Applies pagination to IEnumerable
    /// </summary>
    /// <typeparam name="T">Type of the data</typeparam>
    /// <param name="data">The data source</param>
    /// <param name="paginationDto">Pagination parameters</param>
    /// <returns>Paginated result</returns>
    public static PaginatedResult<T> ToPaginatedResult<T>(
        this IEnumerable<T> data,
        PaginationDto paginationDto)
    {
        var totalCount = data.Count();
        
        var paginatedData = data
            .Skip(paginationDto.Skip)
            .Take(paginationDto.Take)
            .ToList();

        return PaginatedResult<T>.Success(paginatedData, totalCount, paginationDto.PageNumber, paginationDto.PageSize);
    }

    /// <summary>
    /// Applies search filter to IQueryable
    /// </summary>
    /// <typeparam name="T">Type of the data</typeparam>
    /// <param name="query">The queryable source</param>
    /// <param name="searchTerm">Search term</param>
    /// <param name="searchProperties">Properties to search in</param>
    /// <returns>Filtered queryable</returns>
    public static IQueryable<T> WhereSearch<T>(
        this IQueryable<T> query,
        string? searchTerm,
        params Expression<Func<T, object>>[] searchProperties)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || !searchProperties.Any())
            return query;

        // For now, use a simpler approach to avoid complex expression tree manipulation
        // This will be improved in the Infrastructure layer
        var searchTermLower = searchTerm.ToLower();
        
        // Use the first search property for now
        var firstProperty = searchProperties.First();
        var propertyName = GetPropertyName(firstProperty);
        
        // Create a simple contains expression
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, propertyName);
        var toStringMethod = typeof(object).GetMethod("ToString");
        var toStringCall = Expression.Call(property, toStringMethod);
        var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
        var toLowerCall = Expression.Call(toStringCall, toLowerMethod);
        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        var searchTermConstant = Expression.Constant(searchTermLower);
        var containsCall = Expression.Call(toLowerCall, containsMethod, searchTermConstant);
        
        var lambda = Expression.Lambda<Func<T, bool>>(containsCall, parameter);
        return query.Where(lambda);
    }

    /// <summary>
    /// Applies sorting to IQueryable
    /// </summary>
    /// <typeparam name="T">Type of the data</typeparam>
    /// <param name="query">The queryable source</param>
    /// <param name="sortBy">Sort field</param>
    /// <param name="sortDirection">Sort direction</param>
    /// <param name="defaultSort">Default sort expression</param>
    /// <returns>Sorted queryable</returns>
    public static IQueryable<T> OrderByField<T>(
        this IQueryable<T> query,
        string? sortBy,
        string? sortDirection,
        Expression<Func<T, object>>? defaultSort = null)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            if (defaultSort != null)
            {
                return query.OrderBy(defaultSort);
            }
            return query;
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, sortBy);
        var lambda = Expression.Lambda<Func<T, object>>(property, parameter);

        if (string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase))
        {
            return query.OrderByDescending(lambda);
        }

        return query.OrderBy(lambda);
    }

    /// <summary>
    /// Applies dynamic sorting to IQueryable
    /// </summary>
    /// <typeparam name="T">Type of the data</typeparam>
    /// <param name="query">The queryable source</param>
    /// <param name="sortBy">Sort field</param>
    /// <param name="sortDirection">Sort direction</param>
    /// <returns>Sorted queryable</returns>
    public static IQueryable<T> OrderByDynamic<T>(
        this IQueryable<T> query,
        string? sortBy,
        string? sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, sortBy);
        var lambda = Expression.Lambda(property, parameter);

        var methodName = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase)
            ? "OrderByDescending"
            : "OrderBy";

        var method = typeof(Queryable).GetMethods()
            .Where(m => m.Name == methodName && m.GetParameters().Length == 2)
            .Single()
            .MakeGenericMethod(typeof(T), property.Type);

        return (IQueryable<T>)method.Invoke(null, new object[] { query, lambda })!;
    }

    /// <summary>
    /// Gets property name from expression
    /// </summary>
    /// <typeparam name="T">Type of the data</typeparam>
    /// <param name="expression">Property expression</param>
    /// <returns>Property name</returns>
    private static string GetPropertyName<T>(Expression<Func<T, object>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }

        if (expression.Body is UnaryExpression unaryExpression && 
            unaryExpression.Operand is MemberExpression unaryMemberExpression)
        {
            return unaryMemberExpression.Member.Name;
        }

        throw new ArgumentException("Invalid property expression");
    }
} 