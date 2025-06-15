using Microsoft.Extensions.DependencyInjection;

namespace Cashregister.Factories;

public sealed class ScopeAwareFactory<TService>(IServiceProvider serviceProvider) where TService : notnull
{
    private readonly struct Nothing;

    public async Task<TResult> ExecuteAsync<TResult>(Func<TService, Task<TResult>> action)
    {
        using var scope = serviceProvider.CreateScope();

        var service = scope.ServiceProvider.GetRequiredService<TService>();

        return await action(service);
    }

    public async Task ExecuteAsync(Func<TService, Task> action)
    {
        async Task<Nothing> ExecuteReturningNothingAsync(TService service)
        {
            await action(service);

            return default;
        }

        _ = await this.ExecuteAsync(ExecuteReturningNothingAsync);
    }
}