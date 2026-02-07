import { describe, it, expect, afterEach, vi } from "vitest";
import { render, screen, cleanup } from "@testing-library/react";
import { ModalIdProvider } from "./use-modal";
import { ArticleForm } from "./article-form";

const mockFetcher = {
  state: "idle" as string,
  data: undefined as unknown,
  Form: "form",
};

vi.mock("react-router", () => ({
  useFetcher: () => mockFetcher,
}));

afterEach(() => {
  cleanup();
  mockFetcher.state = "idle";
  mockFetcher.data = undefined;
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
});
