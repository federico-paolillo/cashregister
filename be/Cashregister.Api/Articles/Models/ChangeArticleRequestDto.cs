namespace Cashregister.Api.Articles.Models;

public sealed record ChangeArticleRequestDto(
    string Description,
    long PriceInCents
);
