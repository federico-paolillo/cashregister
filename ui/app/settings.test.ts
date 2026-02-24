import { describe, it, expect, vi } from "vitest";
import { mustParseConfiguration } from "./settings";

describe("mustParseConfiguration", () => {
  it("defaults apiBaseUrl to empty string when VITE_API_BASE_URL is undefined", () => {
    vi.stubEnv("VITE_API_BASE_URL", undefined as unknown as string);

    const settings = mustParseConfiguration();

    expect(settings.apiBaseUrl).toBe("");
    vi.unstubAllEnvs();
  });

  it("uses VITE_API_BASE_URL when set", () => {
    vi.stubEnv("VITE_API_BASE_URL", "http://localhost:5000");

    const settings = mustParseConfiguration();

    expect(settings.apiBaseUrl).toBe("http://localhost:5000");
    vi.unstubAllEnvs();
  });
});
