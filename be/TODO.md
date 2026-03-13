# TODO

## Deferred: Extract abstract PaginationQuery base class in Database layer

When a second paginated entity is added (e.g. orders listing), the common EF Core cursor-based query pattern in `FetchArticlesListQuery` should be extracted into an abstract base class.

The pattern that repeats for each paginated entity is:
- `FetchAsync`: WHERE id > cursor, ORDER BY id, TAKE count, SELECT projection
- `FetchUntilAsync`: WHERE id <= cursor, ORDER BY id, SELECT projection

Proposed design:
- Create `PaginationQuery<TEntity, TListItem>` abstract class in `Cashregister.Database/Queries/`
- It takes `IApplicationDbContext` and provides the generic cursor WHERE/ORDER BY/TAKE logic
- Subclasses provide: (1) the `DbSet<TEntity>` to query, (2) the `Select` projection from `TEntity` to `TListItem`
- `FetchArticlesListQuery` becomes a thin subclass that only specifies `applicationDbContext.Articles` and the `ArticleEntity -> ArticleListItem` projection

This was deferred because with a single paginated entity there is no duplication to eliminate yet.
