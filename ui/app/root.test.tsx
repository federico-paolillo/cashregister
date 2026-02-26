import { describe, it, expect, vi, afterEach, beforeEach } from "vitest";
import { render, screen, cleanup } from "@testing-library/react";
import Root, { ErrorBoundary, HydrateFallback, Layout } from "@cashregister/root";
import * as reactRouter from "react-router";

vi.mock("react-router", async (importOriginal) => {
  const actual = await importOriginal<typeof reactRouter>();
  return {
    ...actual,
    useRouteError: vi.fn(),
    isRouteErrorResponse: vi.fn(),
    Outlet: () => <div data-testid="outlet" />,
    Links: () => null,
    Meta: () => null,
    Scripts: () => null,
    ScrollRestoration: () => null,
  };
});

afterEach(cleanup);

describe("HydrateFallback", () => {
  it("renders the loading spinner", () => {
    const { container } = render(<HydrateFallback />);

    expect(container.querySelector(".animate-spin")).not.toBeNull();
  });
});

describe("ErrorBoundary", () => {
  beforeEach(() => {
    vi.mocked(reactRouter.isRouteErrorResponse).mockReturnValue(false);
    vi.mocked(reactRouter.useRouteError).mockReturnValue(null);
  });

  it("always shows the 'Something went wrong' heading", () => {
    render(<ErrorBoundary />);

    expect(screen.getByText("Something went wrong")).toBeDefined();
  });

  it("shows a 'Try Again' button", () => {
    render(<ErrorBoundary />);

    expect(screen.getByRole("button", { name: "Try Again" })).toBeDefined();
  });

  it("shows the message from an Error instance", () => {
    vi.mocked(reactRouter.useRouteError).mockReturnValue(new Error("network timeout"));

    render(<ErrorBoundary />);

    expect(screen.getByText("network timeout")).toBeDefined();
  });

  it("shows the fallback message for unknown error types", () => {
    vi.mocked(reactRouter.useRouteError).mockReturnValue("unexpected string");

    render(<ErrorBoundary />);

    expect(screen.getByText("An unexpected error occurred.")).toBeDefined();
  });

  it("shows error.data for a route error response", () => {
    vi.mocked(reactRouter.isRouteErrorResponse).mockReturnValue(true);
    vi.mocked(reactRouter.useRouteError).mockReturnValue({
      status: 404,
      statusText: "Not Found",
      data: "The page you requested does not exist.",
    });

    render(<ErrorBoundary />);

    expect(screen.getByText("The page you requested does not exist.")).toBeDefined();
  });

  it("falls back to status and statusText when route error has no data", () => {
    vi.mocked(reactRouter.isRouteErrorResponse).mockReturnValue(true);
    vi.mocked(reactRouter.useRouteError).mockReturnValue({
      status: 500,
      statusText: "Internal Server Error",
      data: "",
    });

    render(<ErrorBoundary />);

    expect(screen.getByText("500 Internal Server Error")).toBeDefined();
  });
});

describe("Root", () => {
  it("renders the outlet", () => {
    render(<Root />);

    expect(screen.getByTestId("outlet")).toBeDefined();
  });
});

describe("Layout", () => {
  it("renders children", () => {
    render(<Layout><span>child content</span></Layout>);

    expect(screen.getByText("child content")).toBeDefined();
  });
});
