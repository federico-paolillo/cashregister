import { describe, it, expect, afterEach, vi } from "vitest";
import { render, screen, cleanup } from "@testing-library/react";
import { ModalIdProvider } from "@cashregister/components/use-modal";
import { ArticleForm } from "@cashregister/components/article-form";

const mockFetcher = {
  state: "idle" as string,
  data: undefined as unknown,
  formData: undefined as FormData | undefined,
  Form: "form",
};

vi.mock("react-router", () => ({
  useFetcher: () => mockFetcher,
}));

afterEach(() => {
  cleanup();
  mockFetcher.state = "idle";
  mockFetcher.data = undefined;
  mockFetcher.formData = undefined;
});

function renderInModal(ui: React.ReactNode) {
  return render(<ModalIdProvider id="dlg-1">{ui}</ModalIdProvider>);
}

describe("ArticleForm", () => {
  it("renders inputs with correct name attributes for FormData serialization", () => {
    renderInModal(<ArticleForm intent="create" />);

    expect(screen.getByLabelText("Description")).toHaveProperty(
      "name",
      "description",
    );
    expect(screen.getByLabelText("Price (cents)")).toHaveProperty(
      "name",
      "priceInCents",
    );
  });

  it("sends the intent as a hidden field", () => {
    renderInModal(<ArticleForm intent="create" />);

    const hidden = document.querySelector<HTMLInputElement>(
      'input[name="intent"]',
    );
    expect(hidden).not.toBeNull();
    expect(hidden!.value).toBe("create");
  });

  it("initializes fields from initialData", () => {
    renderInModal(
      <ArticleForm
        intent="create"
        initialData={{ description: "Latte", priceInCents: 450 }}
      />,
    );

    expect(screen.getByLabelText("Description")).toHaveProperty(
      "value",
      "Latte",
    );
    expect(screen.getByLabelText("Price (cents)")).toHaveProperty(
      "value",
      "450",
    );
  });

  it("disables both buttons when fetcher is submitting", () => {
    mockFetcher.state = "submitting";

    renderInModal(<ArticleForm intent="create" />);

    expect(
      screen.getByRole("button", { name: "Cancel" }),
    ).toHaveProperty("disabled", true);
    expect(screen.getByRole("button", { name: "Save" })).toHaveProperty(
      "disabled",
      true,
    );
  });

  it("sets command and commandfor on the cancel button", () => {
    renderInModal(<ArticleForm intent="create" />);

    const cancel = screen.getByRole("button", { name: "Cancel" });
    expect(cancel.getAttribute("command")).toBe("close");
    expect(cancel.getAttribute("commandfor")).toBe("dlg-1");
  });

  it("posts to /articles", () => {
    renderInModal(<ArticleForm intent="create" />);

    const form = screen.getByRole("button", { name: "Save" }).closest("form");
    expect(form).not.toBeNull();
    expect(form!.getAttribute("action")).toBe("/articles");
  });

  it("includes articleId as a hidden field when provided", () => {
    renderInModal(<ArticleForm intent="edit" articleId="abc-123" />);

    const hidden = document.querySelector<HTMLInputElement>(
      'input[name="articleId"]',
    );
    expect(hidden).not.toBeNull();
    expect(hidden!.value).toBe("abc-123");
  });

  it("does not include articleId hidden field when not provided", () => {
    renderInModal(<ArticleForm intent="create" />);

    const hidden = document.querySelector<HTMLInputElement>(
      'input[name="articleId"]',
    );
    expect(hidden).toBeNull();
  });

  it("calls onError when fetcher completes with a failure result", () => {
    const onSubmit = vi.fn();
    const onError = vi.fn();

    mockFetcher.state = "submitting";

    const { rerender } = renderInModal(
      <ArticleForm intent="create" onSubmit={onSubmit} onError={onError} />,
    );

    mockFetcher.state = "idle";
    mockFetcher.data = { ok: false, error: { message: "Server error", status: 500 } };

    rerender(
      <ModalIdProvider id="dlg-1">
        <ArticleForm intent="create" onSubmit={onSubmit} onError={onError} />
      </ModalIdProvider>,
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

    const { rerender } = renderInModal(
      <ArticleForm intent="edit" articleId="abc" onSubmit={onSubmit} />,
    );

    // Phase 2: Idle with result — formData cleared
    mockFetcher.state = "idle";
    mockFetcher.data = { ok: true, value: undefined };
    mockFetcher.formData = undefined;

    rerender(
      <ModalIdProvider id="dlg-1">
        <ArticleForm intent="edit" articleId="abc" onSubmit={onSubmit} />
      </ModalIdProvider>,
    );

    expect(onSubmit).toHaveBeenCalled();
  });
});
