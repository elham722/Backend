using System;
using System.Linq.Expressions;

namespace Backend.Domain.Specifications
{
    public interface ISpecification<T>
    {
        Expression<Func<T, bool>> ToExpression();
        bool IsSatisfiedBy(T entity) => ToExpression().Compile()(entity); 
    }
} 