import { formatPrice } from "@cashregister/money";
import type { ArticleListItemDto } from "@cashregister/model";

interface ArticleSelectorProps {
  articles: ArticleListItemDto[];
  onSelect: (article: ArticleListItemDto) => void;
}

export function ArticleSelector({ articles, onSelect }: ArticleSelectorProps) {
  return (
    <div className="grid grid-cols-3 gap-3">
      {articles.map((article) => (
        <button
          key={article.id}
          type="button"
          onClick={() => onSelect(article)}
          className="rounded border border-gray-200 p-4 text-left hover:bg-blue-50 active:bg-blue-100"
        >
          <div className="font-medium text-sm">{article.description}</div>
          <div className="text-sm text-gray-500">
            {formatPrice(article.priceInCents)}
          </div>
        </button>
      ))}
      {articles.length === 0 && (
        <p className="col-span-3 text-sm italic text-gray-500">
          No articles available.
        </p>
      )}
    </div>
  );
}
