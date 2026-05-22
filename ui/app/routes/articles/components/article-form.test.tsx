import { describe, it, expect, afterEach, vi } from "vitest";
import { render, screen, cleanup } from "@testing-library/react";
import { ArticleForm } from "@cashregister/routes/articles/components/article-form";

const mockFetcher = {
  state: "idle" as string,
  data: undefined as unknown,
  formData: undefined as FormData | undefined,
  Form: "form",
};

vi.mock("react-router", () => ({
  Link: ({ children, to, ...props }: { children: React.ReactNode; to: string } & React.AnchorHTMLAttributes<HTMLAnchorElement>) =>
    <a href={String(to)} {...props}>{children}</a>,
  useFetcher: () => mockFetcher,
}));

afterEach(() => {
  cleanup();
  mockFetcher.state = "idle";
  mockFetcher.data = undefined;
  mockFetcher.formData = undefined;
});

describe("ArticleForm", () => {
  it("renders inputs with correct name attributes for FormData serialization", () => {
    render(<ArticleForm articleId="abc-123" />);

    expect(screen.getByLabelText("Description")).toHaveProperty(
      "name",
      "description",
    );
    const priceField = document.querySelector<HTMLInputElement>(
      'input[type="hidden"][name="priceInCents"]',
    );
    expect(screen.getByLabelText("Price")).toBeDefined();
    expect(priceField).not.toBeNull();
    expect(screen.getByLabelText("Detail receipt")).toHaveProperty(
      "name",
      "printDetailReceipt",
    );
  });

  it("initializes fields from initialData", () => {
    render(
      <ArticleForm
        articleId="abc-123"
        initialData={{
          description: "Latte",
          priceInCents: 450,
          printDetailReceipt: false,
          quantityAvailable: 12,
        }}
      />,
    );

    expect(screen.getByLabelText("Description")).toHaveProperty(
      "value",
      "Latte",
    );
    expect(screen.getByLabelText("Price")).toHaveProperty(
      "value",
      "4.50",
    );
    expect(screen.getByLabelText("Detail receipt")).toHaveProperty(
      "checked",
      false,
    );
    expect(screen.getByLabelText("Quantity available")).toHaveProperty(
      "checked",
      true,
    );
    expect(screen.getByLabelText("Available pieces")).toHaveProperty(
      "value",
      "12",
    );
  });

  it("defaults detail receipts to enabled", () => {
    render(<ArticleForm articleId="abc-123" />);

    expect(screen.getByLabelText("Detail receipt")).toHaveProperty(
      "checked",
      true,
    );
  });

  it("defaults quantity available to disabled", () => {
    render(<ArticleForm articleId="abc-123" />);

    expect(screen.getByLabelText("Quantity available")).toHaveProperty(
      "checked",
      false,
    );
    expect(screen.getByLabelText("Available pieces")).toHaveProperty(
      "disabled",
      true,
    );
  });

  it("disables both buttons when fetcher is submitting", () => {
    mockFetcher.state = "submitting";

    render(<ArticleForm articleId="abc-123" />);

    expect(screen.getByRole("button", { name: "Save" })).toHaveProperty(
      "disabled",
      true,
    );
  });

  it("does not render cancel controls", () => {
    render(<ArticleForm articleId="abc-123" />);

    expect(screen.queryByRole("button", { name: "Cancel" })).toBeNull();
    expect(screen.queryByRole("link", { name: "Cancel" })).toBeNull();
  });

  it("posts to /articles", () => {
    render(<ArticleForm articleId="abc-123" />);

    const form = screen.getByRole("button", { name: "Save" }).closest("form");
    expect(form).not.toBeNull();
    expect(form!.getAttribute("action")).toBe("/articles");
  });

  it("includes articleId as a hidden field when provided", () => {
    render(<ArticleForm articleId="abc-123" />);

    const hidden = document.querySelector<HTMLInputElement>(
      'input[name="articleId"]',
    );
    expect(hidden).not.toBeNull();
    expect(hidden!.value).toBe("abc-123");
  });

  it("does not include articleId hidden field when not provided", () => {
    render(<ArticleForm />);

    const hidden = document.querySelector<HTMLInputElement>(
      'input[name="articleId"]',
    );
    expect(hidden).toBeNull();
  });

  it("calls onError when fetcher completes with a failure result", () => {
    const onSubmit = vi.fn();
    const onError = vi.fn();

    mockFetcher.state = "submitting";

    const { rerender } = render(
      <ArticleForm articleId="abc-123" onSubmit={onSubmit} onError={onError} />,
    );

    mockFetcher.state = "idle";
    mockFetcher.data = { ok: false, error: { message: "Server error", status: 500 } };

    rerender(
      <ArticleForm articleId="abc-123" onSubmit={onSubmit} onError={onError} />,
    );

    expect(onError).toHaveBeenCalledWith("Server error");
    expect(onSubmit).not.toHaveBeenCalled();
  });

  it("calls onSubmit with submitted data when fetcher completes", () => {
    const onSubmit = vi.fn();

    const formData = new FormData();
    formData.append("description", "Espresso");
    formData.append("priceInCents", "350");

    // Phase 1: Submitting — formData is available
    mockFetcher.state = "submitting";
    mockFetcher.formData = formData;

    const { rerender } = render(
      <ArticleForm articleId="abc" onSubmit={onSubmit} />,
    );

    // Phase 2: Idle with result — formData cleared
    mockFetcher.state = "idle";
    mockFetcher.data = { ok: true, value: undefined };
    mockFetcher.formData = undefined;

    rerender(
      <ArticleForm articleId="abc" onSubmit={onSubmit} />,
    );

    expect(onSubmit).toHaveBeenCalled();
  });
});
