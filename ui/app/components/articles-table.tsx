import type { ArticleListItemDto } from "../model";

interface ArticlesTableProps {
  articles: ArticleListItemDto[];
}

export function ArticlesTable({ articles }: ArticlesTableProps) {
  return (
    <table className="w-full border-collapse">
      <thead>
        <tr className="border-b bg-gray-100 text-left">
          <th className="p-2 font-semibold">ID</th>
          <th className="p-2 font-semibold">Description</th>
          <th className="p-2 font-semibold text-right">Price</th>
        </tr>
      </thead>
      <tbody>
        {articles.map((article) => (
          <tr key={article.id} className="border-b hover:bg-gray-50">
            <td className="p-2 text-sm text-gray-500">{article.id}</td>
            <td className="p-2">{article.description}</td>
            <td className="p-2 text-right">
              {article.price.toLocaleString(undefined, {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2,
              })}
            </td>
          </tr>
        ))}
        {articles.length === 0 && (
          <tr>
            <td colSpan={3} className="p-4 text-center text-gray-500 text-sm italic">
              No articles found.
            </td>
          </tr>
        )}
      </tbody>
    </table>
  );
}
