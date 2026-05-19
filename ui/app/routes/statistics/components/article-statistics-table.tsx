import type {
  ArticleStatisticsItemDto,
  ArticleStatisticsTotalsDto,
} from "@cashregister/model";
import { formatPrice } from "@cashregister/money";

interface ArticleStatisticsTableProps {
  articles: ArticleStatisticsItemDto[];
  totals: ArticleStatisticsTotalsDto;
}

export function ArticleStatisticsTable({
  articles,
  totals,
}: ArticleStatisticsTableProps) {
  return (
    <table className="w-full border-collapse">
      <thead>
        <tr className="border-b bg-gray-100 text-left">
          <th className="p-2 font-semibold">Article</th>
          <th className="p-2 font-semibold text-right">Sold Units</th>
          <th className="p-2 font-semibold text-right">Orders</th>
          <th className="p-2 font-semibold text-right">Volume</th>
        </tr>
      </thead>
      <tbody>
        {articles.map((article, index) => (
          <tr
            key={article.articleId}
            className={index % 2 === 1 ? "bg-gray-50" : undefined}
          >
            <td className="p-2">{article.description}</td>
            <td className="p-2 text-right">{article.soldUnits}</td>
            <td className="p-2 text-right">{article.ordersIncluded}</td>
            <td className="p-2 text-right">{formatPrice(article.volumeInCents)}</td>
          </tr>
        ))}
        {articles.length === 0 && (
          <tr>
            <td colSpan={4} className="p-4 text-center text-gray-500 text-sm italic">
              No article statistics found.
            </td>
          </tr>
        )}
      </tbody>
      <tfoot>
        <tr className="border-t bg-gray-100 font-semibold">
          <th scope="row" className="p-2 text-left">Total</th>
          <td className="p-2 text-right">{totals.soldUnits}</td>
          <td className="p-2 text-right">{totals.ordersIncluded}</td>
          <td className="p-2 text-right">{formatPrice(totals.volumeInCents)}</td>
        </tr>
      </tfoot>
    </table>
  );
}
