import React from "react";
import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen, fireEvent, waitFor, cleanup } from "@testing-library/react";
import Articles from "./articles";
import * as reactRouter from "react-router";
import * as errorMessages from "../components/use-error-messages";
import type { ArticlesPageDto } from "../model";
import type { Result } from "../result";

const mockLoad = vi.fn();

vi.mock("react-router", async (importOriginal) => {
  const actual = await importOriginal<typeof reactRouter>();
  return {
    ...actual,
    useLoaderData: vi.fn(),
    useFetcher: vi.fn(),
  };
});

vi.mock("../components/use-error-messages", () => ({
  useErrorMessages: vi.fn(),
}));

describe("Articles Page", () => {
  const mockInitialData: ArticlesPageDto = {
    items: [
      { id: "1", description: "Article 1", price: 10.0 },
      { id: "2", description: "Article 2", price: 20.0 },
    ],
    next: "cursor-1",
    hasNext: true,
  };

  const mockInitialResult: Result<ArticlesPageDto> = {
    ok: true,
    value: mockInitialData,
  };

  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(reactRouter.useLoaderData).mockReturnValue(mockInitialResult);
    vi.mocked(reactRouter.useFetcher).mockReturnValue({
      state: "idle",
      data: undefined,
      load: mockLoad,
      Form: ({ children }: { children: React.ReactNode }) => <form>{children}</form>,
    } as unknown as reactRouter.FetcherWithComponents<Result<ArticlesPageDto>>);
    vi.mocked(errorMessages.useErrorMessages).mockReturnValue({
      errors: [],
      addError: vi.fn(),
      dismissError: vi.fn(),
    });

    // Mock HTMLDialogElement methods for Modal component
    HTMLDialogElement.prototype.showModal = vi.fn();
    HTMLDialogElement.prototype.close = vi.fn();
  });

  afterEach(() => {
    cleanup();
  });

  it("renders initial articles from loader", () => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    render(<Articles loaderData={mockInitialResult} /> as any);

    expect(screen.getByText("Article 1")).toBeDefined();
    expect(screen.getByText("Article 2")).toBeDefined();
    expect(screen.getByRole("button", { name: "Load More" })).toHaveProperty("disabled", false);
  });

  it("calls fetcher.load when clicking Load More", () => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    render(<Articles loaderData={mockInitialResult} /> as any);

    const loadMoreButton = screen.getByRole("button", { name: "Load More" });
    fireEvent.click(loadMoreButton);

    expect(mockLoad).toHaveBeenCalledWith("/articles?after=cursor-1");
  });

  it("appends articles when fetcher.data changes", async () => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const { rerender } = render(<Articles loaderData={mockInitialResult} /> as any);

    const mockNextData: ArticlesPageDto = {
      items: [{ id: "3", description: "Article 3", price: 30.0 }],
      next: null,
      hasNext: false,
    };

    // Simulate fetcher finished loading
    vi.mocked(reactRouter.useFetcher).mockReturnValue({
      state: "idle",
      data: { ok: true, value: mockNextData },
      load: mockLoad,
      Form: ({ children }: { children: React.ReactNode }) => <form>{children}</form>,
    } as unknown as reactRouter.FetcherWithComponents<Result<ArticlesPageDto>>);

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    rerender(<Articles loaderData={mockInitialResult} /> as any);

    await waitFor(() => {
      expect(screen.getByText("Article 3")).toBeDefined();
    });

    expect(screen.getByRole("button", { name: "Load More" })).toHaveProperty("disabled", true);
  });

  it("disables Load More button if hasNext is false", () => {
    const data: Result<ArticlesPageDto> = {
      ok: true,
      value: {
        ...mockInitialData,
        hasNext: false,
      },
    };

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    render(<Articles loaderData={data} /> as any);

    expect(screen.getByRole("button", { name: "Load More" })).toHaveProperty("disabled", true);
  });

  it("shows loading state when fetcher is loading", () => {
    vi.mocked(reactRouter.useFetcher).mockReturnValue({
      state: "loading",
      data: undefined,
      load: mockLoad,
      Form: ({ children }: { children: React.ReactNode }) => <form>{children}</form>,
    } as unknown as reactRouter.FetcherWithComponents<Result<ArticlesPageDto>>);

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    render(<Articles loaderData={mockInitialResult} /> as any);

    expect(screen.getByText("Loading...")).toBeDefined();
    expect(screen.getByRole("button", { name: "Loading..." })).toHaveProperty("disabled", true);
  });
});
