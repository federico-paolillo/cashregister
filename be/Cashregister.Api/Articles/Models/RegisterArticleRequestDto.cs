namespace Cashregister.Api.Articles.Models;

public sealed record RegisterArticleRequestDto(
    string Description,
    long PriceInCents
);