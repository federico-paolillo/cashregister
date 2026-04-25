import { afterEach, describe, expect, it } from "vitest";
import { cleanup, fireEvent, render, screen } from "@testing-library/react";
import { MoneyInput } from "@cashregister/components/money-input";

afterEach(cleanup);

function hiddenInput(name = "amount") {
  return document.querySelector<HTMLInputElement>(`input[type="hidden"][name="${name}"]`);
}

describe("MoneyInput", () => {
  it("defaults required values to 0.00 and submits zero cents", () => {
    render(<MoneyInput name="amount" label="Amount" required />);

    expect(screen.getByLabelText("Amount")).toHaveProperty("value", "0.00");
    expect(hiddenInput()).toHaveProperty("value", "0");
  });

  it("converts whole decimal values to cents", () => {
    render(<MoneyInput name="amount" label="Amount" required />);

    fireEvent.change(screen.getByLabelText("Amount"), { target: { value: "12" } });

    expect(hiddenInput()).toHaveProperty("value", "1200");
  });

  it("converts one-decimal values to cents", () => {
    render(<MoneyInput name="amount" label="Amount" required />);

    fireEvent.change(screen.getByLabelText("Amount"), { target: { value: "12.3" } });

    expect(hiddenInput()).toHaveProperty("value", "1230");
  });

  it("converts two-decimal values to cents", () => {
    render(<MoneyInput name="amount" label="Amount" required />);

    fireEvent.change(screen.getByLabelText("Amount"), { target: { value: "12.34" } });

    expect(hiddenInput()).toHaveProperty("value", "1234");
  });

  it("submits no hidden cents field when optional input is empty", () => {
    render(<MoneyInput name="amount" label="Amount" />);

    expect(screen.getByLabelText("Amount")).toHaveProperty("value", "");
    expect(hiddenInput()).toBeNull();
  });

  it("rejects values with more than two decimals", () => {
    render(<MoneyInput name="amount" label="Amount" required />);

    const input = screen.getByLabelText("Amount");
    fireEvent.change(input, { target: { value: "12.345" } });

    expect(input.getAttribute("aria-invalid")).toBe("true");
    expect(hiddenInput()).toBeNull();
  });

  it("normalizes valid values to two decimals on blur", () => {
    render(<MoneyInput name="amount" label="Amount" required />);

    const input = screen.getByLabelText("Amount");
    fireEvent.change(input, { target: { value: "12.3" } });
    fireEvent.blur(input);

    expect(input).toHaveProperty("value", "12.30");
    expect(hiddenInput()).toHaveProperty("value", "1230");
  });
});
