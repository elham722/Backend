using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Backend.Domain.Specifications
{
    public abstract class BaseSpecification<T> : ISpecification<T>
    {
        private readonly List<Expression<Func<T, bool>>> _criteria = new();
        public List<Expression<Func<T, object>>> Includes { get; } = new();
        public List<string> IncludeStrings { get; } = new();
        public Expression<Func<T, object>>? OrderBy { get; private set; }
        public Expression<Func<T, object>>? OrderByDescending { get; private set; }
        public int Take { get; private set; }
        public int Skip { get; private set; }
        public bool IsPagingEnabled { get; private set; }

        protected BaseSpecification() { }

        protected BaseSpecification(Expression<Func<T, bool>> criteria)
        {
            AddCriteria(criteria);
        }

        public Expression<Func<T, bool>> Criteria => CombineCriteria();

        public Expression<Func<T, bool>> ToExpression()
        {
            return Criteria;
        }

        public ISpecification<T> AddInclude(Expression<Func<T, object>> includeExpression)
        {
            Includes.Add(includeExpression);
            return this;
        }

        public ISpecification<T> AddInclude(string includeString)
        {
            IncludeStrings.Add(includeString);
            return this;
        }

        public ISpecification<T> AddOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
            return this;
        }

        public ISpecification<T> AddOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
        {
            OrderByDescending = orderByDescendingExpression;
            return this;
        }

        public ISpecification<T> ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
            return this;
        }

        protected void AddCriteria(Expression<Func<T, bool>> criteria)
        {
            _criteria.Add(criteria);
        }

        private Expression<Func<T, bool>> CombineCriteria()
        {
            if (!_criteria.Any())
                return x => true; // Return true for all items if no criteria

            if (_criteria.Count == 1)
                return _criteria.First();

            // For now, return the first criteria to avoid complex expression tree manipulation
            // This will be improved in the Infrastructure layer with proper query building
            return _criteria.First();
        }
    }
} 