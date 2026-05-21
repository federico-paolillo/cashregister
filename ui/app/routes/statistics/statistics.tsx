import { useLoaderError } from "@cashregister/components/use-loader-error";
import { Tab, Tabber, TabList, TabPanel } from "@cashregister/components/tabber";
import { deps } from "@cashregister/deps";
import type { StatisticsDto } from "@cashregister/model";
import { formatPrice } from "@cashregister/money";
import { ArticleInventoryTable } from "@cashregister/routes/statistics/components/article-inventory-table";
import { OrderStatisticsTable } from "@cashregister/routes/statistics/components/order-statistics-table";
import { formatSignedPrice } from "@cashregister/routes/statistics/format";
import type { Route } from "./+types/statistics";

export async function clientLoader() {
  return {
    statistics: await deps.apiClient.get<StatisticsDto>("/statistics"),
    salesCsvUrl: deps.apiClient.buildUrl("/statistics/sales.csv"),
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
            download="statistics-sales.csv"
            href={loaderData.salesCsvUrl}
          >
            Sales CSV
          </a>
        </nav>
      </header>
      <main className="flex-1 overflow-auto p-4">
        {statistics ? (
          <Tabber defaultTab="articles">
            <TabList aria-label="Statistics views">
              <Tab id="articles">Articles</Tab>
              <Tab id="orders">Orders</Tab>
            </TabList>
            <TabPanel tabId="articles">
              <section>
                <h2 className="mb-3 text-lg font-semibold">
                  Article Inventory
                </h2>
                <ArticleInventoryTable articles={statistics.articles} />
              </section>
            </TabPanel>
            <TabPanel tabId="orders">
              <div className="grid gap-8">
                <section
                  aria-label="Statistics summary"
                  className="grid gap-3 md:grid-cols-5"
                >
                  <SummaryMetric
                    label="Produced Articles"
                    value={statistics.summary.producedArticles.toString()}
                  />
                  <SummaryMetric
                    label="Orders"
                    value={statistics.summary.orderCount.toString()}
                  />
                  <SummaryMetric
                    label="Expected Volume"
                    value={formatPrice(statistics.summary.expectedVolumeInCents)}
                  />
                  <SummaryMetric
                    label="Real Volume"
                    value={formatPrice(statistics.summary.realVolumeInCents)}
                  />
                  <SummaryMetric
                    label="Override Delta"
                    value={formatSignedPrice(statistics.summary.deltaInCents)}
                  />
                </section>
                <section>
                  <h2 className="mb-3 text-lg font-semibold">Order Volume</h2>
                  <OrderStatisticsTable orders={statistics.orders} />
                </section>
              </div>
            </TabPanel>
          </Tabber>
        ) : (
          <p className="text-sm text-gray-500">Statistics could not be loaded.</p>
        )}
      </main>
    </>
  );
}

interface SummaryMetricProps {
  label: string;
  value: string;
}

function SummaryMetric({ label, value }: SummaryMetricProps) {
  return (
    <div className="rounded border bg-white p-3">
      <div className="text-sm text-gray-500">{label}</div>
      <div className="mt-1 text-xl font-semibold">{value}</div>
    </div>
  );
}
