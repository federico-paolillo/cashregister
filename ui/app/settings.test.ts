import { describe, it, expect, vi } from "vitest";
import { mustParseConfiguration } from "./settings";

describe("mustParseConfiguration", () => {
  it("defaults apiBaseUrl to empty string when VITE_API_BASE_URL is undefined", () => {
    vi.stubEnv("VITE_API_BASE_URL", undefined as unknown as string);

    const settings = mustParseConfiguration();

    expect(settings.apiBaseUrl).toBe("/api");
    expect(settings.lowQuantityWarningThreshold).toBe(5);
    vi.unstubAllEnvs();
  });

  it("uses VITE_API_BASE_URL when set", () => {
    vi.stubEnv("VITE_API_BASE_URL", "http://localhost:5000");

    const settings = mustParseConfiguration();

    expect(settings.apiBaseUrl).toBe("http://localhost:5000");
    vi.unstubAllEnvs();
  });

  it("uses VITE_LOW_QUANTITY_WARNING_THRESHOLD when set", () => {
    vi.stubEnv("VITE_LOW_QUANTITY_WARNING_THRESHOLD", "2");

    const settings = mustParseConfiguration();

    expect(settings.lowQuantityWarningThreshold).toBe(2);
    vi.unstubAllEnvs();
  });
});
