import { formatPrice } from "@cashregister/money";
import type { ArticleListItemDto } from "@cashregister/model";
import { ArticleStatusIcons } from "./article-status-icons";

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
            aria-label={article.description}
            onClick={() => onSelect(article)}
            className="relative min-h-24 rounded border border-gray-200 p-4 pr-12 text-left hover:bg-blue-50 active:bg-blue-100"
          >
            <ArticleStatusIcons
              lowQuantity={lowQuantity}
              printDetailReceipt={article.printDetailReceipt}
              className="absolute right-2 top-2"
            />
            <div className="font-medium text-sm">{article.description}</div>
            <div className="text-sm text-gray-500">
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
