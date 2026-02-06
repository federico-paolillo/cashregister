import { describe, it, expect } from "vitest";
import { success, failure } from "./result";

describe("result", () => {
  it("creates a success result", () => {
    const result = success(42);

    expect(result.ok).toBe(true);
    expect(result).toEqual({ ok: true, value: 42 });
  });

  it("creates a failure result", () => {
    const result = failure({ status: 404, message: "Not found" });

    expect(result.ok).toBe(false);
    expect(result).toEqual({
      ok: false,
      error: { status: 404, message: "Not found" },
    });
  });
});
