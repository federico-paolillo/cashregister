import { describe, it, expect } from "vitest";
import { formatPrice, decimalToCents } from "@cashregister/money";

describe("formatPrice", () => {
  it("formats cents as decimal string", () => {
    expect(formatPrice(300)).toBe("3.00");
  });

  it("formats single cents", () => {
    expect(formatPrice(1)).toBe("0.01");
  });

  it("formats partial decimal amounts", () => {
    expect(formatPrice(450)).toBe("4.50");
  });

  it("formats zero cents", () => {
    expect(formatPrice(0)).toBe("0.00");
  });

  it("formats large cents amount", () => {
    expect(formatPrice(123450)).toBe("1234.50");
  });
});

describe("decimalToCents", () => {
  it("converts whole decimal strings to cents", () => {
    expect(decimalToCents("12")).toBe(1200);
  });

  it("converts one-decimal strings to cents", () => {
    expect(decimalToCents("12.3")).toBe(1230);
  });

  it("converts two-decimal strings to cents", () => {
    expect(decimalToCents("12.34")).toBe(1234);
  });

  it("handles zero", () => {
    expect(decimalToCents("0")).toBe(0);
  });

  it("rejects empty strings", () => {
    expect(decimalToCents("")).toBeNull();
  });

  it("rejects more than two decimal places", () => {
    expect(decimalToCents("12.345")).toBeNull();
  });

  it("rejects comma decimal separators", () => {
    expect(decimalToCents("12,34")).toBeNull();
  });

  it("rejects negative amounts", () => {
    expect(decimalToCents("-12.34")).toBeNull();
  });
});
