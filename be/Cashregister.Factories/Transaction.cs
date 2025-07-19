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
        finally
        {
            await unitOfWork.RollbackAsync(cancellationToken);
        }
    }

    protected abstract Task<Result<TOutput>> InternalExecuteAsync(TInput input);
}