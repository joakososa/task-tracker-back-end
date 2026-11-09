using FluentValidation;
using TaskTracker.Application.Common.Interfaces;

namespace TaskTracker.Application.Common.Behaviors;

/// Same as <see cref="ValidationDecorator{TCommand, TResult}"/>, for commands that return nothing.
public sealed class ValidationDecorator<TCommand>(
    ICommandHandler<TCommand> inner,
    IEnumerable<IValidator<TCommand>> validators)
    : ICommandHandler<TCommand>
{
    public async Task HandleAsync(TCommand command, CancellationToken cancellationToken)
    {
        foreach (var validator in validators)
            await validator.ValidateAndThrowAsync(command, cancellationToken);

        await inner.HandleAsync(command, cancellationToken);
    }
}
