using FluentValidation;
using TaskTracker.Application.Common.Interfaces;

namespace TaskTracker.Application.Common.Behaviors;

public sealed class ValidationDecorator<TCommand, TResult>(
    ICommandHandler<TCommand, TResult> inner,
    IEnumerable<IValidator<TCommand>> validators)
    : ICommandHandler<TCommand, TResult>
{
    public async Task<TResult> HandleAsync(TCommand command, CancellationToken ct)
    {
        foreach (var validator in validators)
            await validator.ValidateAndThrowAsync(command, ct);

        return await inner.HandleAsync(command, ct);
    }
}