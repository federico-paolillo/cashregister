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

## Consider: Migrate from Cysharp/Ulid to RobThree/NUlid for monotonic ULID generation

The current library (`Cysharp/Ulid`) uses cryptographic randomness for the random component of each ULID. When two ULIDs are generated within the same millisecond, their relative sort order is non-deterministic. This caused flaky integration tests that assumed creation order equals ULID sort order.

The tests have been fixed to not rely on creation order, but migrating to `RobThree/NUlid` (https://github.com/RobThree/NUlid) would provide monotonic ULID generation via its `MonotonicUlidRng`. With monotonic ULIDs, sequentially generated IDs are guaranteed to sort in creation order even within the same millisecond, because the random component is incremented by 1 bit rather than regenerated.

Migration steps:
1. Replace the `Ulid` package reference with `NUlid` in `Cashregister.Domain`
2. Create a shared `MonotonicUlidRng` instance (e.g. a static field or DI singleton)
3. Change `Identifier.New()` to use `Ulid.NewUlid(rng)` with the monotonic RNG
4. Integration tests could then be simplified back to asserting creation-order descriptions directly, since ULID sort order would match creation order
