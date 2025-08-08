using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Backend.Domain.Specifications
{
    public interface ISpecification<T>
    {
        Expression<Func<T, bool>> Criteria { get; }
        List<Expression<Func<T, object>>> Includes { get; }
        List<string> IncludeStrings { get; }
        Expression<Func<T, object>>? OrderBy { get; }
        Expression<Func<T, object>>? OrderByDescending { get; }
        int Take { get; }
        int Skip { get; }
        bool IsPagingEnabled { get; }

        Expression<Func<T, bool>> ToExpression();
        bool IsSatisfiedBy(T entity) => ToExpression().Compile()(entity);
        
        // Builder methods
        ISpecification<T> AddInclude(Expression<Func<T, object>> includeExpression);
        ISpecification<T> AddInclude(string includeString);
        ISpecification<T> AddOrderBy(Expression<Func<T, object>> orderByExpression);
        ISpecification<T> AddOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression);
        ISpecification<T> ApplyPaging(int skip, int take);
    }
} 