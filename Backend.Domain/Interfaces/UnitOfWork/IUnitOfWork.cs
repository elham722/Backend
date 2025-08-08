namespace Backend.Domain.Interfaces.UnitOfWork
{
  
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
} 