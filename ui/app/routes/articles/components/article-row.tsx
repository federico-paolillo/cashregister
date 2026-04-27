import { Link } from "react-router";
import type { ArticleListItemDto } from "@cashregister/model";
import { formatPrice } from "@cashregister/money";
import { buildArticlesSelectionLink } from "../url";

interface ArticleRowProps {
  article: ArticleListItemDto;
  striped: boolean;
  selected: boolean;
  until: string | null;
}

export function ArticleRow({ article, striped, selected, until }: ArticleRowProps) {
  const to = buildArticlesSelectionLink(article.id, until);
  const backgroundClass = selected ? "bg-blue-100" : striped ? "bg-gray-50" : "";

  return (
    <tr className={`border-b hover:bg-blue-50 ${backgroundClass}`}>
      <td className="p-2">
        <Link to={to} className="block">{article.description}</Link>
      </td>
      <td className="p-2 text-right">
        <Link to={to} className="block">{formatPrice(article.priceInCents)}</Link>
      </td>
    </tr>
  );
}
