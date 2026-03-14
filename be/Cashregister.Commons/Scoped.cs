using Microsoft.Extensions.DependencyInjection;

namespace Cashregister.Factories;

public sealed class Scoped<TService>(IServiceProvider serviceProvider) where TService : notnull
{
    public async Task<TResult> ExecuteAsync<TResult>(Func<TService, Task<TResult>> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var scope = serviceProvider.CreateScope();

        var service = scope.ServiceProvider.GetRequiredService<TService>();

        return await action(service);
    }

    public async Task ExecuteAsync(Func<TService, Task> action)
    {
        async Task<Unit> ExecuteReturningNothingAsync(TService service)
        {
            await action(service);

            return default;
        }

        _ = await ExecuteAsync(ExecuteReturningNothingAsync);
    }
}