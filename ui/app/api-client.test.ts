import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { ApiClient } from "@cashregister/api-client";

describe("ApiClient", () => {
  beforeEach(() => {
    vi.useFakeTimers();
    vi.spyOn(Math, "random").mockReturnValue(0);
    vi.stubGlobal("fetch", vi.fn());
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    vi.restoreAllMocks();
    vi.useRealTimers();
  });

  it("does not resolve a get before the selected delay elapses", async () => {
    vi.mocked(fetch).mockResolvedValue(jsonResponse({ id: "article-1" }));

    const result = new ApiClient("/api").get("/articles/article-1");
    let settled = false;
    result.then(() => {
      settled = true;
    });

    await vi.advanceTimersByTimeAsync(49);

    expect(settled).toBe(false);

    await vi.advanceTimersByTimeAsync(1);

    await expect(result).resolves.toEqual({
      ok: true,
      value: { id: "article-1" },
    });
  });

  it("uses a random delay between 50ms and 150ms", async () => {
    vi.mocked(fetch).mockResolvedValue(jsonResponse({ ok: true }));

    vi.mocked(Math.random).mockReturnValue(0.999999);

    const result = new ApiClient("/api").get("/devices");
    let settled = false;
    result.then(() => {
      settled = true;
    });

    await vi.advanceTimersByTimeAsync(149);

    expect(settled).toBe(false);

    await vi.advanceTimersByTimeAsync(1);

    await expect(result).resolves.toEqual({
      ok: true,
      value: { ok: true },
    });
  });

  it("returns successful JSON responses", async () => {
    vi.mocked(fetch).mockResolvedValue(jsonResponse({ name: "Printer" }));

    const result = new ApiClient("/api").get("/devices");

    await vi.advanceTimersByTimeAsync(50);

    await expect(result).resolves.toEqual({
      ok: true,
      value: { name: "Printer" },
    });
  });

  it("returns undefined for 204 responses", async () => {
    vi.mocked(fetch).mockResolvedValue(new Response(null, { status: 204 }));

    const result = new ApiClient("/api").del("/articles/article-1");

    await vi.advanceTimersByTimeAsync(50);

    await expect(result).resolves.toEqual({ ok: true, value: undefined });
  });

  it("returns HTTP failures after the delay", async () => {
    vi.mocked(fetch).mockResolvedValue(new Response(null, { status: 404 }));

    const result = new ApiClient("/api").get("/articles/missing");
    let settled = false;
    result.then(() => {
      settled = true;
    });

    await vi.advanceTimersByTimeAsync(49);

    expect(settled).toBe(false);

    await vi.advanceTimersByTimeAsync(1);

    await expect(result).resolves.toEqual({
      ok: false,
      error: { status: 404, message: "/api/articles/missing" },
    });
  });

  it("builds public API URLs for non-JSON links", () => {
    const url = new ApiClient("/api/").buildUrl(
      "statistics/articles.csv",
      { scope: "all" },
    );

    expect(url).toBe("/api/statistics/articles.csv?scope=all");
  });
});

function jsonResponse(body: unknown): Response {
  return new Response(JSON.stringify(body), {
    status: 200,
    headers: { "Content-Type": "application/json" },
  });
}
