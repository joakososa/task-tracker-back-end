namespace TaskTracker.Application.Common.Interfaces;

/// A command that only changes state, as CQS intends. Use the two-parameter version when the caller needs
/// something back (e.g. the created resource for a 201).
public interface ICommandHandler<TCommand>
{
    Task HandleAsync(TCommand command, CancellationToken cancellationToken);
}
