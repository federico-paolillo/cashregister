namespace Cashregister.Api.Articles.Models;

internal sealed record RegisterArticleRequestDto(
    string Description,
    long PriceInCents
);