import { describe, it, expect } from "vitest";
import { formatPrice, decimalToCents } from "@cashregister/money";

describe("formatPrice", () => {
  it("formats cents as decimal string", () => {
    expect(formatPrice(300)).toBe("3.00");
  });

  it("formats partial cents as rounded decimal string", () => {
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
  it("converts decimal number to cents", () => {
    expect(decimalToCents(9.99)).toBe(999);
  });

  it("rounds when converting to cents", () => {
    expect(decimalToCents(9.999)).toBe(1000);
  });

  it("handles zero", () => {
    expect(decimalToCents(0)).toBe(0);
  });
});
