import { describe, it, expect, vi, afterEach } from "vitest";
import { render, screen, cleanup } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { ErrorMessageItem } from "@cashregister/components/error-message-item";

afterEach(cleanup);

describe("ErrorMessageItem", () => {
  it("displays the error message", () => {
    render(
      <ErrorMessageItem
        error={{ id: 1, message: "broken pipe", severity: "error" }}
        onDismiss={() => { }}
      />,
    );

    expect(screen.getByText("broken pipe")).toBeDefined();
  });

  it("calls onDismiss when dismiss is clicked", async () => {
    const onDismiss = vi.fn();
    const user = userEvent.setup();

    render(
      <ErrorMessageItem
        error={{ id: 42, message: "something", severity: "error" }}
        onDismiss={onDismiss}
      />,
    );

    await user.click(screen.getByLabelText("Dismiss"));

    expect(onDismiss).toHaveBeenCalledOnce();
  });

  it("does not carry role=alert (items live inside a role=log container)", () => {
    const { container } = render(
      <ErrorMessageItem
        error={{ id: 1, message: "alert me", severity: "error" }}
        onDismiss={() => { }}
      />,
    );

    expect(container.querySelector("[role='alert']")).toBeNull();
  });

  it("applies error styles for error severity", () => {
    const { container } = render(
      <ErrorMessageItem
        error={{ id: 1, message: "oops", severity: "error" }}
        onDismiss={() => { }}
      />,
    );

    expect(container.firstChild).toHaveProperty("className");
    expect((container.firstChild as HTMLElement).className).toContain("border-red-300");
  });

  it("applies info styles for info severity", () => {
    const { container } = render(
      <ErrorMessageItem
        error={{ id: 2, message: "done", severity: "info" }}
        onDismiss={() => { }}
      />,
    );

    expect((container.firstChild as HTMLElement).className).toContain("border-green-300");
  });
});
