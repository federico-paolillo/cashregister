import { describe, it, expect, vi, beforeEach } from "vitest";
import { renderHook, act } from "@testing-library/react";
import { useArticlesPages } from "./use-articles-page";
import * as reactRouter from "react-router";
import type { ArticlesPageDto } from "../model";

vi.mock("react-router", () => ({
  useFetcher: vi.fn(),
}));

describe("useArticlesPages", () => {
  const initialData: ArticlesPageDto = {
    items: [{ id: "1", description: "A1", price: 10 }],
    next: "cursor-1",
    hasNext: true,
  };

  const mockLoad = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("initializes with provided data", () => {
    vi.mocked(reactRouter.useFetcher).mockReturnValue({
      state: "idle",
      data: undefined,
      load: mockLoad,
    } as unknown as reactRouter.FetcherWithComponents<ArticlesPageDto>);

    const { result } = renderHook(() => useArticlesPages(initialData));

    expect(result.current.articles).toEqual(initialData.items);
    expect(result.current.hasNext).toBe(true);
    expect(result.current.isLoadingMore).toBe(false);
  });

  it("calls fetcher.load when loadMore is called (with encodeURIComponent)", () => {
    vi.mocked(reactRouter.useFetcher).mockReturnValue({
      state: "idle",
      data: undefined,
      load: mockLoad,
    } as unknown as reactRouter.FetcherWithComponents<ArticlesPageDto>);

    const { result } = renderHook(() => useArticlesPages({
      ...initialData,
      next: "cursor with space",
    }));

    act(() => {
      result.current.loadMore();
    });

    expect(mockLoad).toHaveBeenCalledWith("/articles?after=cursor%20with%20space");
  });

  it("updates articles when fetcher data arrives", () => {
    const fetcherMock = {
      state: "idle",
      data: undefined as unknown as ArticlesPageDto | undefined,
      load: mockLoad,
    };
    vi.mocked(reactRouter.useFetcher).mockReturnValue(fetcherMock as unknown as reactRouter.FetcherWithComponents<ArticlesPageDto>);

    const { result, rerender } = renderHook(() => useArticlesPages(initialData));

    const nextData = {
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
    fetcherMock.data = nextData as ArticlesPageDto;
    rerender();

    expect(result.current.articles).toEqual([...initialData.items, ...nextData.items]);
    expect(result.current.hasNext).toBe(false);
    expect(result.current.isLoadingMore).toBe(false);
  });

  it("synchronizes state when initialData changes", () => {
    vi.mocked(reactRouter.useFetcher).mockReturnValue({
      state: "idle",
      data: undefined,
      load: mockLoad,
    } as unknown as reactRouter.FetcherWithComponents<ArticlesPageDto>);

    const { result, rerender } = renderHook(
      ({ data }) => useArticlesPages(data),
      { initialProps: { data: initialData } }
    );

    const newData: ArticlesPageDto = {
      items: [{ id: "new-1", description: "New A1", price: 100 }],
      next: "new-cursor",
      hasNext: true,
    };

    rerender({ data: newData });

    expect(result.current.articles).toEqual(newData.items);
    expect(result.current.hasNext).toBe(true);
  });
});
