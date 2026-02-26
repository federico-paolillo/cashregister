import type { ArticleListItemDto } from "@cashregister/model";
import { ArticleRow } from "@cashregister/routes/articles/components/article-row";

interface ArticlesTableProps {
  articles: ArticleListItemDto[];
  onEdit: (article: ArticleListItemDto) => void;
}

export function ArticlesTable({ articles, onEdit }: ArticlesTableProps) {
  return (
    <table className="w-full border-collapse">
      <thead>
        <tr className="border-b bg-gray-100 text-left">
          <th className="p-2 font-semibold">Name</th>
          <th className="p-2 font-semibold text-right">Price</th>
          <th className="p-2 font-semibold text-right">Actions</th>
        </tr>
      </thead>
      <tbody>
        {articles.map((article, index) => (
          <ArticleRow
            key={article.id}
            article={article}
            striped={index % 2 === 1}
            onEdit={onEdit}
          />
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
