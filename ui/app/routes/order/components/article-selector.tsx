import { formatPrice } from "@cashregister/money";
import type { ArticleListItemDto } from "@cashregister/model";

interface ArticleSelectorProps {
  articles: ArticleListItemDto[];
  lowQuantityArticleIds: Set<string>;
  onSelect: (article: ArticleListItemDto) => void;
}

export function ArticleSelector({ articles, lowQuantityArticleIds, onSelect }: ArticleSelectorProps) {
  return (
    <div className="grid grid-cols-3 gap-3">
      {articles.map((article) => {
        const lowQuantity = lowQuantityArticleIds.has(article.id);

        return (
          <button
            key={article.id}
            type="button"
            onClick={() => onSelect(article)}
            className={lowQuantity
              ? "rounded border border-orange-400 bg-orange-100 p-4 text-left text-orange-900 hover:border-orange-500 hover:bg-orange-200 hover:text-orange-900 active:border-orange-600 active:bg-orange-300 active:text-orange-900"
              : "rounded border border-gray-200 p-4 text-left hover:bg-blue-50 active:bg-blue-100"}
          >
            <div className="font-medium text-sm">{article.description}</div>
            <div className={lowQuantity ? "text-sm text-orange-700" : "text-sm text-gray-500"}>
              {formatPrice(article.priceInCents)}
            </div>
          </button>
        );
      })}
      {articles.length === 0 && (
        <p className="col-span-3 text-sm italic text-gray-500">
          No articles available.
        </p>
      )}
    </div>
  );
}
