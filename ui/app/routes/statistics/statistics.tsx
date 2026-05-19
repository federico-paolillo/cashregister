import { useLoaderError } from "@cashregister/components/use-loader-error";
import { deps } from "@cashregister/deps";
import type { StatisticsDto } from "@cashregister/model";
import { ArticleStatisticsTable } from "@cashregister/routes/statistics/components/article-statistics-table";
import { OrderStatisticsTable } from "@cashregister/routes/statistics/components/order-statistics-table";
import type { Route } from "./+types/statistics";

export async function clientLoader() {
  return {
    statistics: await deps.apiClient.get<StatisticsDto>("/statistics"),
    articlesCsvUrl: deps.apiClient.buildUrl("/statistics/articles.csv"),
    ordersCsvUrl: deps.apiClient.buildUrl("/statistics/orders.csv"),
  };
}

export default function Statistics({ loaderData }: Route.ComponentProps) {
  const statistics = loaderData.statistics.ok ? loaderData.statistics.value : null;

  useLoaderError(loaderData.statistics);

  return (
    <>
      <header className="flex items-center justify-between gap-4 border-b p-4">
        <h1 className="text-xl font-semibold">Statistics</h1>
        <nav aria-label="Statistics exports" className="flex gap-2">
          <a
            className="btn-outline"
            download="statistics-articles.csv"
            href={loaderData.articlesCsvUrl}
          >
            Articles CSV
          </a>
          <a
            className="btn-outline"
            download="statistics-orders.csv"
            href={loaderData.ordersCsvUrl}
          >
            Orders CSV
          </a>
        </nav>
      </header>
      <main className="flex-1 overflow-auto p-4">
        {statistics ? (
          <div className="grid gap-8">
            <section>
              <h2 className="mb-3 text-lg font-semibold">Article Sales</h2>
              <ArticleStatisticsTable
                articles={statistics.articles.items}
                totals={statistics.articles.totals}
              />
            </section>
            <section>
              <h2 className="mb-3 text-lg font-semibold">Order Volume</h2>
              <OrderStatisticsTable
                orders={statistics.orders}
                totals={statistics.ordersTotals}
              />
            </section>
          </div>
        ) : (
          <p className="text-sm text-gray-500">Statistics could not be loaded.</p>
        )}
      </main>
    </>
  );
}
