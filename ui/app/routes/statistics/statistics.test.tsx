import {
  cleanup,
  render,
  screen,
  waitFor,
  within,
} from "@testing-library/react";
import userEvent from "@testing-library/user-event";
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
  articles: [
    {
      articleId: "article-1",
      description: "Espresso",
      retired: false,
      soldUnits: 3,
    },
    {
      articleId: "article-2",
      description: "Tea",
      retired: true,
      soldUnits: 1,
    },
  ],
  orders: [
    {
      orderId: "order-1",
      orderNumber: "00000000000000000001",
      date: 1700000000,
      producedArticles: 3,
      expectedVolumeInCents: 550,
      realVolumeInCents: 450,
      deltaInCents: -100,
      hasOverride: true,
    },
    {
      orderId: "order-2",
      orderNumber: "00000000000000000002",
      date: 1700100000,
      producedArticles: 1,
      expectedVolumeInCents: 200,
      realVolumeInCents: 200,
      deltaInCents: 0,
      hasOverride: false,
    },
  ],
  summary: {
    orderCount: 2,
    producedArticles: 4,
    expectedVolumeInCents: 750,
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
      salesCsvUrl: "/api/statistics/sales.csv",
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

  it("shows article statistics first and keeps the CSV export visible", () => {
    renderStatistics();

    const articlePanel = screen.getByRole("tabpanel", { name: "Articles" });
    const articleView = within(articlePanel);

    expect(
      screen.getByRole("tab", { name: "Articles" }).getAttribute(
        "aria-selected",
      ),
    ).toBe("true");
    expect(articleView.getByText("Espresso")).toBeDefined();
    expect(articleView.getByText("Tea")).toBeDefined();
    expect(articleView.getByText("Retired")).toBeDefined();
    expect(
      articleView.getByRole("heading", { name: "Article Inventory" }),
    ).toBeDefined();
    expect(
      screen.queryByRole("tabpanel", { name: "Orders" }),
    ).toBeNull();
    const salesCsvLink = screen.getByRole("link", { name: "Sales CSV" });

    expect(salesCsvLink).toHaveProperty(
      "href",
      "http://localhost:3000/api/statistics/sales.csv",
    );
    expect(salesCsvLink.getAttribute("download")).toBe("statistics-sales.csv");
  });

  it("shows order summary and order statistics in the orders tab", async () => {
    const user = userEvent.setup();

    renderStatistics();

    await user.click(screen.getByRole("tab", { name: "Orders" }));

    const orderPanel = screen.getByRole("tabpanel", { name: "Orders" });
    const orderView = within(orderPanel);

    expect(orderView.getByText("Produced Articles")).toBeDefined();
    expect(orderView.getAllByText("Expected Volume")).toHaveLength(2);
    expect(orderView.getAllByText("4")).toHaveLength(1);
    expect(orderView.getAllByText("6.50")).toHaveLength(1);
    expect(orderView.getAllByText("7.50")).toHaveLength(1);
    expect(orderView.getAllByText("-1.00")).toHaveLength(2);
    expect(
      orderView.getByRole("heading", { name: "Order Volume" }),
    ).toBeDefined();
    expect(orderView.getByText("00000000000000000001")).toBeDefined();
    expect(orderView.getByText("Yes")).toBeDefined();
    expect(orderView.getByText("No")).toBeDefined();
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
      .mockReturnValueOnce("/api/statistics/sales.csv");

    const result = await clientLoader();

    expect(result).toEqual({
      statistics: statisticsResult,
      salesCsvUrl: "/api/statistics/sales.csv",
    });
    expect(deps.apiClient.get).toHaveBeenCalledWith("/statistics");
    expect(deps.apiClient.buildUrl).toHaveBeenNthCalledWith(
      1,
      "/statistics/sales.csv",
    );
  });
});
