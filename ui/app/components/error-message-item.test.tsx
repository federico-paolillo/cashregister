import { describe, it, expect, vi, afterEach } from "vitest";
import { render, screen, cleanup } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { ErrorMessageItem } from "@cashregister/components/error-message-item";

afterEach(cleanup);

describe("ErrorMessageItem", () => {
  it("displays the error message", () => {
    render(
      <ErrorMessageItem
        error={{ id: 1, message: "broken pipe" }}
        onDismiss={() => { }}
      />,
    );

    expect(screen.getByText("broken pipe")).toBeDefined();
  });

  it("calls onDismiss with the error id when dismiss is clicked", async () => {
    const onDismiss = vi.fn();
    const user = userEvent.setup();

    render(
      <ErrorMessageItem
        error={{ id: 42, message: "something" }}
        onDismiss={onDismiss}
      />,
    );

    await user.click(screen.getByLabelText("Dismiss"));

    expect(onDismiss).toHaveBeenCalledOnce();
    expect(onDismiss).toHaveBeenCalledWith(42);
  });

  it("has role alert for accessibility", () => {
    render(
      <ErrorMessageItem
        error={{ id: 1, message: "alert me" }}
        onDismiss={() => { }}
      />,
    );

    expect(screen.getByRole("alert")).toBeDefined();
  });
});
