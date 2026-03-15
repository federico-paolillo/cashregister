import { describe, it, expect } from "vitest";
import { formatPrice, centsToDecimal, decimalToCents } from "@cashregister/money";

describe("formatPrice", () => {
  it("formats integer price with two decimals", () => {
    expect(formatPrice(3)).toBe("3.00");
  });

  it("formats decimal price with two decimals", () => {
    expect(formatPrice(4.5)).toBe("4.50");
  });

  it("formats zero", () => {
    expect(formatPrice(0)).toBe("0.00");
  });

  it("formats large numbers without thousand separator", () => {
    expect(formatPrice(1234.5)).toBe("1234.50");
  });
});

describe("centsToDecimal", () => {
  it("converts cents to decimal", () => {
    expect(centsToDecimal(999)).toBe(9.99);
  });

  it("converts zero", () => {
    expect(centsToDecimal(0)).toBe(0);
  });

  it("converts round amount", () => {
    expect(centsToDecimal(1500)).toBe(15);
  });
});

describe("decimalToCents", () => {
  it("converts decimal to cents", () => {
    expect(decimalToCents(9.99)).toBe(999);
  });

  it("converts zero", () => {
    expect(decimalToCents(0)).toBe(0);
  });

  it("rounds to nearest cent", () => {
    expect(decimalToCents(1.5)).toBe(150);
  });
});
