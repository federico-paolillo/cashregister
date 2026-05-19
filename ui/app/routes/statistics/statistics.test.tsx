import { cleanup, render, screen, waitFor } from "@testing-library/react";
import * as errorMessages from "@cashregister/components/use-error-messages";
import { deps } from "@cashregister/deps";
import type { StatisticsDto } from "@cashregister/model";
import type { Result } from "@cashregister/result";
import Statistics, { clientLoader } from "@cashregister/routes/statistics/statistics";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import type { Route } from "./+types/statistics";

vi.mock("@cashregister/components/use-error-messages", () => ({
  useErrorMessages: vi.fn(),
}));

vi.mock("@cashregister/deps", () => ({
  deps: {
    apiClient: {
      get: vi.fn(),
      buildUrl: vi.fn(),
    },
  },
}));

const statistics: StatisticsDto = {
  articles: {
    items: [
      {
        articleId: "article-1",
        description: "Espresso",
        soldUnits: 3,
        ordersIncluded: 2,
        volumeInCents: 450,
      },
      {
        articleId: "article-2",
        description: "Tea",
        soldUnits: 1,
        ordersIncluded: 1,
        volumeInCents: 200,
      },
    ],
    totals: {
      soldUnits: 4,
      ordersIncluded: 3,
      volumeInCents: 650,
    },
  },
  orders: {
    orderCount: 2,
    nominalVolumeInCents: 750,
    realVolumeInCents: 650,
    deltaInCents: -100,
  },
  ordersTotals: {
    orderCount: 2,
    nominalVolumeInCents: 750,
    realVolumeInCents: 650,
    deltaInCents: -100,
  },
};

const statisticsResult: Result<StatisticsDto> = {
  ok: true,
  value: statistics,
};

function renderStatistics(result: Result<StatisticsDto> = statisticsResult) {
  const props = {
    loaderData: {
      statistics: result,
      articlesCsvUrl: "/api/statistics/articles.csv",
      ordersCsvUrl: "/api/statistics/orders.csv",
    },
  } as Route.ComponentProps;

  return render(
    <Statistics {...props} />,
  );
}

describe("Statistics", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(errorMessages.useErrorMessages).mockReturnValue({
      errors: [],
      addError: vi.fn(),
      dismissError: vi.fn(),
    });
  });

  afterEach(() => {
    cleanup();
  });

  it("renders article and order statistics with footer totals", () => {
    renderStatistics();

    expect(screen.getByText("Espresso")).toBeDefined();
    expect(screen.getByText("Tea")).toBeDefined();
    expect(screen.getByRole("heading", { name: "Article Sales" })).toBeDefined();
    expect(screen.getByRole("heading", { name: "Order Volume" })).toBeDefined();
    const articlesCsvLink = screen.getByRole("link", { name: "Articles CSV" });
    const ordersCsvLink = screen.getByRole("link", { name: "Orders CSV" });

    expect(articlesCsvLink).toHaveProperty(
      "href",
      "http://localhost:3000/api/statistics/articles.csv",
    );
    expect(articlesCsvLink.getAttribute("download")).toBe("statistics-articles.csv");
    expect(ordersCsvLink).toHaveProperty(
      "href",
      "http://localhost:3000/api/statistics/orders.csv",
    );
    expect(ordersCsvLink.getAttribute("download")).toBe("statistics-orders.csv");
    expect(screen.getAllByText("Total")).toHaveLength(2);
    expect(screen.getAllByText("6.50")).toHaveLength(3);
    expect(screen.getAllByText("7.50")).toHaveLength(2);
    expect(screen.getAllByText("-1.00")).toHaveLength(2);
  });

  it("surfaces loader failures through the error-message system", async () => {
    const addError = vi.fn();

    vi.mocked(errorMessages.useErrorMessages).mockReturnValue({
      errors: [],
      addError,
      dismissError: vi.fn(),
    });

    renderStatistics({
      ok: false,
      error: { message: "Statistics unavailable", status: 500 },
    });

    expect(screen.getByText("Statistics could not be loaded.")).toBeDefined();

    await waitFor(() => {
      expect(addError).toHaveBeenCalledWith("Statistics unavailable");
    });
  });
});

describe("clientLoader", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("fetches statistics and builds CSV URLs", async () => {
    vi.mocked(deps.apiClient.get).mockResolvedValue(statisticsResult);
    vi.mocked(deps.apiClient.buildUrl)
      .mockReturnValueOnce("/api/statistics/articles.csv")
      .mockReturnValueOnce("/api/statistics/orders.csv");

    const result = await clientLoader();

    expect(result).toEqual({
      statistics: statisticsResult,
      articlesCsvUrl: "/api/statistics/articles.csv",
      ordersCsvUrl: "/api/statistics/orders.csv",
    });
    expect(deps.apiClient.get).toHaveBeenCalledWith("/statistics");
    expect(deps.apiClient.buildUrl).toHaveBeenNthCalledWith(
      1,
      "/statistics/articles.csv",
    );
    expect(deps.apiClient.buildUrl).toHaveBeenNthCalledWith(
      2,
      "/statistics/orders.csv",
    );
  });
});
