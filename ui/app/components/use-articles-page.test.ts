import { describe, it, expect, vi, beforeEach } from "vitest";
import { renderHook, act } from "@testing-library/react";
import { useArticlesPages } from "./use-articles-page";
import * as reactRouter from "react-router";
import * as errorMessages from "./use-error-messages";
import type { ArticlesPageDto } from "../model";
import type { Result } from "../result";

vi.mock("react-router", () => ({
  useFetcher: vi.fn(),
}));

vi.mock("./use-error-messages", () => ({
  useErrorMessages: vi.fn(),
}));

describe("useArticlesPages", () => {
  const initialData: ArticlesPageDto = {
    items: [{ id: "1", description: "A1", price: 10 }],
    next: "cursor-1",
    hasNext: true,
  };

  const initialResult: Result<ArticlesPageDto> = {
    ok: true,
    value: initialData,
  };

  const mockLoad = vi.fn();
  const mockAddError = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(errorMessages.useErrorMessages).mockReturnValue({
      errors: [],
      addError: mockAddError,
      dismissError: vi.fn(),
    });
  });

  it("initializes with provided data", () => {
    vi.mocked(reactRouter.useFetcher).mockReturnValue({
      state: "idle",
      data: undefined,
      load: mockLoad,
    } as unknown as reactRouter.FetcherWithComponents<Result<ArticlesPageDto>>);

    const { result } = renderHook(() => useArticlesPages(initialResult));

    expect(result.current.articles).toEqual(initialData.items);
    expect(result.current.hasNext).toBe(true);
    expect(result.current.isLoadingMore).toBe(false);
  });

  it("calls addError when initialPageResult is an error", () => {
    vi.mocked(reactRouter.useFetcher).mockReturnValue({
      state: "idle",
      data: undefined,
      load: mockLoad,
    } as unknown as reactRouter.FetcherWithComponents<Result<ArticlesPageDto>>);

    const errorResult: Result<ArticlesPageDto> = {
      ok: false,
      error: { status: 500, message: "Server error" },
    };

    renderHook(() => useArticlesPages(errorResult));

    expect(mockAddError).toHaveBeenCalledWith("Server error");
  });

  it("calls fetcher.load when loadMore is called (with encodeURIComponent)", () => {
    vi.mocked(reactRouter.useFetcher).mockReturnValue({
      state: "idle",
      data: undefined,
      load: mockLoad,
    } as unknown as reactRouter.FetcherWithComponents<Result<ArticlesPageDto>>);

    const dataWithSpace: Result<ArticlesPageDto> = {
      ok: true,
      value: {
        ...initialData,
        next: "cursor with space",
      },
    };

    const { result } = renderHook(() => useArticlesPages(dataWithSpace));

    act(() => {
      result.current.loadMore();
    });

    expect(mockLoad).toHaveBeenCalledWith("/articles?after=cursor%20with%20space");
  });

  it("updates articles when fetcher data arrives", () => {
    const fetcherMock = {
      state: "idle",
      data: undefined as Result<ArticlesPageDto> | undefined,
      load: mockLoad,
    };
    vi.mocked(reactRouter.useFetcher).mockReturnValue(
      fetcherMock as unknown as reactRouter.FetcherWithComponents<Result<ArticlesPageDto>>
    );

    const { result, rerender } = renderHook(() => useArticlesPages(initialResult));

    const nextData: ArticlesPageDto = {
      items: [{ id: "2", description: "A2", price: 20 }],
      next: null,
      hasNext: false,
    };

    // Simulate loading
    fetcherMock.state = "loading";
    rerender();
    expect(result.current.isLoadingMore).toBe(true);

    // Simulate idle with data
    fetcherMock.state = "idle";
    fetcherMock.data = { ok: true, value: nextData };
    rerender();

    expect(result.current.articles).toEqual([...initialData.items, ...nextData.items]);
    expect(result.current.hasNext).toBe(false);
    expect(result.current.isLoadingMore).toBe(false);
  });

  it("calls addError when fetcher returns an error", () => {
    const fetcherMock = {
      state: "idle",
      data: undefined as Result<ArticlesPageDto> | undefined,
      load: mockLoad,
    };
    vi.mocked(reactRouter.useFetcher).mockReturnValue(
      fetcherMock as unknown as reactRouter.FetcherWithComponents<Result<ArticlesPageDto>>
    );

    const { rerender } = renderHook(() => useArticlesPages(initialResult));

    // Simulate fetcher returning an error
    fetcherMock.data = { ok: false, error: { status: 404, message: "Not found" } };
    rerender();

    expect(mockAddError).toHaveBeenCalledWith("Not found");
  });

  it("synchronizes state when initialData changes", () => {
    vi.mocked(reactRouter.useFetcher).mockReturnValue({
      state: "idle",
      data: undefined,
      load: mockLoad,
    } as unknown as reactRouter.FetcherWithComponents<Result<ArticlesPageDto>>);

    const { result, rerender } = renderHook(
      ({ data }) => useArticlesPages(data),
      { initialProps: { data: initialResult } }
    );

    const newData: ArticlesPageDto = {
      items: [{ id: "new-1", description: "New A1", price: 100 }],
      next: "new-cursor",
      hasNext: true,
    };

    const newResult: Result<ArticlesPageDto> = { ok: true, value: newData };

    rerender({ data: newResult });

    expect(result.current.articles).toEqual(newData.items);
    expect(result.current.hasNext).toBe(true);
  });
});
