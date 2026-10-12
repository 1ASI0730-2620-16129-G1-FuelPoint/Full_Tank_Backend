namespace TechnoSac.FullTank.Platform.Shared.Domain.Repositories;

/// <summary>
///     Unit of work interface for all repositories
/// </summary>
public interface IUnitOfWork
{
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(System.Data.IsolationLevel isolationLevel, CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Save changes to the repository
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns></returns>
    Task CompleteAsync(CancellationToken cancellationToken = default);
}
