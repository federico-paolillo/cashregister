import { describe, it, expect, afterEach } from "vitest";
import { render, screen, cleanup } from "@testing-library/react";
import { useLoaderError } from "@cashregister/components/use-loader-error";
import { ErrorMessagesProvider } from "@cashregister/components/use-error-messages";
import { ErrorMessageList } from "@cashregister/components/error-message-list";
import type { Result } from "@cashregister/result";

afterEach(cleanup);

function TestComponent({ loaderData }: { loaderData: Result<unknown> }) {
  useLoaderError(loaderData);
  return null;
}

function renderWithProvider(loaderData: Result<unknown>) {
  return render(
    <ErrorMessagesProvider autoDismissMs={0}>
      <ErrorMessageList />
      <TestComponent loaderData={loaderData} />
    </ErrorMessagesProvider>,
  );
}

describe("useLoaderError", () => {
  it("does nothing when result is ok", () => {
    renderWithProvider({ ok: true, value: "data" });
    expect(screen.queryByRole("log")).toBeNull();
  });

  it("adds an error toast when result is not ok", () => {
    renderWithProvider({ ok: false, error: { status: 500, message: "Server error" } });
    expect(screen.getByText("Server error")).toBeDefined();
  });
});
