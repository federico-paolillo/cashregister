using Cashregister.Factories.Problems;

namespace Cashregister.Factories;

public abstract class Transaction<TInput, TOutput>(
    IUnitOfWork unitOfWork
)
{
    public async Task<Result<TOutput>> ExecuteAsync(
        TInput input,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await unitOfWork.StartAsync(cancellationToken);

            var result = await InternalExecuteAsync(input);

            if (result.Ok)
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else
            {
                await unitOfWork.RollbackAsync(cancellationToken);
            }

            return result;
        }
#pragma warning disable CA1031 
        // We want to have a global unhandled exception handler
        catch(Exception ex)
#pragma warning restore CA1031
        {
            await unitOfWork.RollbackAsync(cancellationToken);

            return Result.Error<TOutput>(new UnhandledExceptionProblem(ex));
        }
    }

    protected abstract Task<Result<TOutput>> InternalExecuteAsync(TInput input);
}