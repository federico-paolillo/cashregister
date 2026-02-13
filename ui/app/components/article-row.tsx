import type { ArticleListItemDto } from "../model";

interface ArticleRowProps {
  article: ArticleListItemDto;
  striped: boolean;
  onEdit: (article: ArticleListItemDto) => void;
}

export function ArticleRow({ article, striped, onEdit }: ArticleRowProps) {
  return (
    <tr
      className={`border-b hover:bg-blue-50 ${striped ? "bg-gray-50" : ""}`}
    >
      <td className="p-2">{article.description}</td>
      <td className="p-2 text-right">
        {article.price.toLocaleString(undefined, {
          minimumFractionDigits: 2,
          maximumFractionDigits: 2,
        })}
      </td>
      <td className="p-2 text-right">
        <button
          type="button"
          aria-label={`Edit ${article.description}`}
          className="mr-2 cursor-pointer text-gray-500 hover:text-blue-600"
          onClick={() => onEdit(article)}
        >
          ✎
        </button>
        <button
          type="button"
          aria-label={`Delete ${article.description}`}
          className="cursor-pointer text-gray-500 hover:text-red-600"
          disabled
        >
          ✕
        </button>
      </td>
    </tr>
  );
}
