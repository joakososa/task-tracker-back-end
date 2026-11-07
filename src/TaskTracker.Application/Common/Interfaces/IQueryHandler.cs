namespace TaskTracker.Application.Common.Interfaces;

/// A read-only use case. Separate from ICommandHandler so reads never get write-side behaviour (validation, saving).
public interface IQueryHandler<TQuery, TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken);
}
