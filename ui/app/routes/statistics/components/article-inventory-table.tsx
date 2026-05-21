import type { ArticleInventoryItemDto } from "@cashregister/model";

interface ArticleInventoryTableProps {
  articles: ArticleInventoryItemDto[];
}

export function ArticleInventoryTable({ articles }: ArticleInventoryTableProps) {
  return (
    <table className="w-full border-collapse">
      <thead>
        <tr className="border-b bg-gray-100 text-left">
          <th className="p-2 font-semibold">Article</th>
          <th className="p-2 font-semibold">Status</th>
          <th className="p-2 font-semibold text-right">Sold Units</th>
          <th className="p-2 font-semibold">Article ID</th>
        </tr>
      </thead>
      <tbody>
        {articles.map((article, index) => (
          <tr
            key={article.articleId}
            className={index % 2 === 1 ? "bg-gray-50" : undefined}
          >
            <td className="p-2">{article.description}</td>
            <td className="p-2">{article.retired ? "Retired" : "Active"}</td>
            <td className="p-2 text-right">{article.soldUnits}</td>
            <td className="p-2 font-mono text-xs">{article.articleId}</td>
          </tr>
        ))}
        {articles.length === 0 && (
          <tr>
            <td
              colSpan={4}
              className="p-4 text-center text-gray-500 text-sm italic"
            >
              No sold articles found.
            </td>
          </tr>
        )}
      </tbody>
    </table>
  );
}
