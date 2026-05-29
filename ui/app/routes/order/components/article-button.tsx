import { formatPrice } from "@cashregister/money";
import type { ArticleListItemDto } from "@cashregister/model";
import { ArticleStatusIcons } from "./article-status-icons";

interface ArticleButtonProps {
  article: ArticleListItemDto;
  lowQuantity: boolean;
  onSelect: (article: ArticleListItemDto) => void;
}

export function ArticleButton({ article, lowQuantity, onSelect }: ArticleButtonProps) {
  return (
    <button
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
}
