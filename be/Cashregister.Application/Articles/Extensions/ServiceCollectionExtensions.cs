using Cashregister.Application.Articles.Handlers;
using Cashregister.Application.Articles.Handlers.Defaults;
using Cashregister.Application.Articles.Transactions;
using Cashregister.Application.Articles.Transactions.Defaults;

using Microsoft.Extensions.DependencyInjection;

namespace Cashregister.Application.Articles.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCashregisterArticles(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IRegisterArticleTransaction, RegisterArticleTransaction>();
        serviceCollection.AddScoped<IFetchArticlesPageHandler, FetchArticlesPageHandler>();
        serviceCollection.AddScoped<IRetireArticleTransaction, RetireArticleTransaction>();
        serviceCollection.AddScoped<IChangeArticleTransaction, ChangeArticleTransaction>();

        return serviceCollection;
    }
}