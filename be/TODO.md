# TODO

## Done: Extract abstract PaginationQuery base class in Database layer

Extracted `PaginationQuery<TEntity, TListItem>` abstract base class in `Cashregister.Database/Queries/PaginationQuery.cs`. Both `FetchArticlesListQuery` and `FetchOrdersListQuery` are now thin subclasses that only provide the queryable source and projection expression.
